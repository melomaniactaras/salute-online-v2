﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RawRabbit;
using RawRabbit.Configuration;
using RawRabbit.DependencyInjection.ServiceCollection;
using RawRabbit.Instantiation;
using SaluteOnline.API.DAL;
using SaluteOnline.API.Domain;
using SaluteOnline.API.Domain.LinkEntities;
using SaluteOnline.API.DTO.Club;
using SaluteOnline.API.Handlers.Declaration;
using SaluteOnline.API.Handlers.Implementation;
using SaluteOnline.API.Infrastructure.Kafka;
using SaluteOnline.API.Services.Implementation;
using SaluteOnline.API.Services.Interface;
using SaluteOnline.Shared.Common;
using SaluteOnline.Shared.Events;
using Swashbuckle.AspNetCore.Swagger;

namespace SaluteOnline.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var authSettings = Configuration.GetSection("Auth");

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = authSettings["Authority"];
                    options.ApiName = authSettings["ApiName"];
                    options.RequireHttpsMetadata = false;
                });

            var connectionString = Configuration.GetConnectionString("SoConnection");
            services.AddDbContext<SaluteOnlineDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton(Configuration);

            services.AddCors(
                options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
                });

            services.AddSwaggerGen(t =>
            {
                t.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Salute Online API"
                });
            }).ConfigureSwaggerGen(t =>
            {
                t.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "Bearer",
                    Name = "Authorization",
                    Type = "apiKey"
                });
            });

            services.AddMvc(options =>
            {
                options.CacheProfiles.Add(new KeyValuePair<string, CacheProfile>("CachingProfile", new CacheProfile
                {
                    Duration = 360,
                    Location = ResponseCacheLocation.Any,
                    VaryByHeader = "user-agent"
                }));
            }).AddJsonOptions(jsonOptions =>
            {
                jsonOptions.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                jsonOptions.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                jsonOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                jsonOptions.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.Configure<KafkaSettings>(Configuration.GetSection("KafkaSettings"));
            services.AddRawRabbit(GetRabbitConfiguration);
            InitializeServices(services);
            SetPolicies(services);
            SubsribeToRabbit(services);
            SetMapProfiles();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseCors("CorsPolicy");
            app.UseSwagger();
            app.UseSwaggerUI(t =>
            {
                t.SwaggerEndpoint("/swagger/v1/swagger.json", "Salute Online API");
            });
            app.UseMvc();
        }

        private static void InitializeServices(IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IClubsService, ClubsService>();
            services.AddScoped<IUserHandler, UserHandler>();
            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IStatisticService, StatisticService>();
            services.AddScoped<IKafkaProducer, KafkaProducer>();
        }

        private static void SetPolicies(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.User.ToString(),
                    policyUser => policyUser.RequireClaim("role", Roles.User.ToString(), Roles.ClubAdmin.ToString(), Roles.GlobalAdmin.ToString(), Roles.SilentDon.ToString()));
                options.AddPolicy(Policies.ClubAdmin.ToString(),
                    policyUser => policyUser.RequireClaim("role", Roles.ClubAdmin.ToString(), Roles.GlobalAdmin.ToString(), Roles.SilentDon.ToString()));
                options.AddPolicy(Policies.GlobalAdmin.ToString(),
                    policyUser => policyUser.RequireClaim("role", Roles.GlobalAdmin.ToString(), Roles.SilentDon.ToString()));
                options.AddPolicy(Policies.SilendDon.ToString(),
                    policyUser => policyUser.RequireClaim("role", Roles.SilentDon.ToString()));
            });
        }

        private const string RabbitSectionName = "RabbitSettings";
        private RawRabbitOptions GetRabbitConfiguration => new RawRabbitOptions
        {
            ClientConfiguration = new RawRabbitConfiguration
            {
                Username = Configuration.GetSection(RabbitSectionName).GetValue<string>(nameof(RawRabbitConfiguration.Username)),
                Password = Configuration.GetSection(RabbitSectionName).GetValue<string>(nameof(RawRabbitConfiguration.Password)),
                VirtualHost = Configuration.GetSection(RabbitSectionName).GetValue<string>(nameof(RawRabbitConfiguration.VirtualHost)),
                Port = Configuration.GetSection(RabbitSectionName).GetValue<int>(nameof(RawRabbitConfiguration.Port)),
                Hostnames = Configuration.GetSection(RabbitSectionName).GetSection(nameof(RawRabbitConfiguration.Hostnames)).Get<List<string>>(),
                Ssl = new SslOption
                {
                    Enabled = Configuration.GetSection(RabbitSectionName).GetValue<bool>("SslEnabled")
                },
                AutomaticRecovery = Configuration.GetSection(RabbitSectionName).GetValue<bool>(nameof(RawRabbitConfiguration.AutomaticRecovery)),
                RecoveryInterval = TimeSpan.FromSeconds(Configuration.GetSection(RabbitSectionName).GetValue<int>(nameof(RawRabbitConfiguration.RecoveryInterval)))
            }
        };

        private async void SubsribeToRabbit(IServiceCollection services)
        {
            var bus = RawRabbitFactory.CreateSingleton(GetRabbitConfiguration);
            var handler = services.BuildServiceProvider().GetService<IUserHandler>();
            await bus.SubscribeAsync<UserRegisteredEvent>(msg => Task.FromResult(handler.HandleNewUser(msg)));
        }

        private static void SetMapProfiles()
        {
            TypeAdapterConfig<ClubUserAdministrator, ClubMemberSummary>.NewConfig()
                .Map(t => t.Avatar, t => t.User.Avatar);

            TypeAdapterConfig<Club, ClubAdministrationSummaryDto>.NewConfig()
                .Map(t => t.CreatorUsername, t => t.Creator.DisplayName);
        }
    }
}
