﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;

namespace SaluteOnline.API.Security
{
    public class CustomAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly AuthSettings _authSettings;

        public CustomAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock, IMemoryCache memoryCache, IOptions<AuthSettings> settings) :
            base(options, logger, encoder, clock)
        {
            _memoryCache = memoryCache;
            _authSettings = settings.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string token;
            // ReSharper disable once CollectionNeverUpdated.Local
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            {
                if (Request.Path.HasValue && !Request.Path.Value.Contains("soMessageHub"))
                {
                    return Task.FromResult(AuthenticateResult.Fail("Missing or malformed Authorization header."));
                }
                var found = Request.Query.TryGetValue("Bearer", out var bearer);
                if (!found)
                    return Task.FromResult(AuthenticateResult.Fail("Missing or malformed Authorization header."));
                token = bearer;
            }
            else
            {
                token = authorization.First();
            }
            var payload = GetUser(token, true);
            if (string.IsNullOrEmpty(payload.Role))
            {
                _memoryCache.Remove(token);
                return Task.FromResult(AuthenticateResult.Fail("Missing user role."));
            }
            var identity = new ClaimsIdentity("Auth");
            identity.AddClaim(new Claim("role", payload.Role));
            identity.AddClaim(new Claim("subjectId", payload.Subject));
            identity.AddClaim(new Claim("email", payload.Email));
            identity.AddClaim(new Claim("avatar", payload.Picture));
            identity.AddClaim(new Claim("userId", payload.UserId));
            identity.AddClaim(new Claim("userName", payload.UserName));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, "Auth");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private AuthPayload GetUser(string token, bool tryReadFromCache)
        {
            AuthPayload payload = null;
            var cacheExists = false;
            if (tryReadFromCache)
            {
                cacheExists = _memoryCache.TryGetValue(token, out payload);
            }
            if (cacheExists)
                return payload;
            payload = AuthInfrastructure.Decode(token, _authSettings.Domain);
            _memoryCache.Set(token, payload, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5)});
            return payload;
        }
    }
}
