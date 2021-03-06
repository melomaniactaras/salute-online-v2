﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using SaluteOnline.API.DAL;
using SaluteOnline.Shared.Common;
using System;

namespace SaluteOnline.API.Migrations
{
    [DbContext(typeof(SaluteOnlineDbContext))]
    [Migration("20180225145751_removeIsActiveFromClub")]
    partial class removeIsActiveFromClub
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SaluteOnline.API.Domain.Club", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City")
                        .HasMaxLength(50);

                    b.Property<string>("Country")
                        .HasMaxLength(50);

                    b.Property<int>("CreatorId");

                    b.Property<string>("Description");

                    b.Property<Guid>("Guid");

                    b.Property<DateTimeOffset>("LastUpdate");

                    b.Property<string>("Logo");

                    b.Property<DateTimeOffset>("Registered");

                    b.Property<int>("Status");

                    b.Property<string>("Title")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("Club");
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.LinkEntities.ClubUserAdministrator", b =>
                {
                    b.Property<int>("ClubId");

                    b.Property<int>("UserId");

                    b.Property<bool>("IsActive");

                    b.Property<DateTimeOffset>("Registered");

                    b.HasKey("ClubId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ClubUserAdministrator");
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.MembershipRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ClubId");

                    b.Property<DateTimeOffset>("Created");

                    b.Property<Guid>("Guid");

                    b.Property<DateTimeOffset>("LastActivity");

                    b.Property<string>("Nickname");

                    b.Property<bool>("SelectedFromExisting");

                    b.Property<int>("Status");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ClubId");

                    b.HasIndex("UserId");

                    b.ToTable("MembershipRequest");
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Avatar");

                    b.Property<int>("ClubId");

                    b.Property<Guid>("Guid");

                    b.Property<bool>("IsActive");

                    b.Property<DateTimeOffset>("LastChanged");

                    b.Property<string>("Nickname");

                    b.Property<DateTimeOffset>("Registered");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ClubId");

                    b.HasIndex("UserId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .HasMaxLength(50);

                    b.Property<string>("AlternativeEmail")
                        .HasMaxLength(50);

                    b.Property<string>("Avatar");

                    b.Property<string>("City")
                        .HasMaxLength(50);

                    b.Property<string>("Country")
                        .HasMaxLength(50);

                    b.Property<DateTimeOffset>("DateOfBirth");

                    b.Property<string>("Email")
                        .HasMaxLength(50);

                    b.Property<string>("Facebook")
                        .HasMaxLength(50);

                    b.Property<string>("FirstName")
                        .HasMaxLength(50);

                    b.Property<Guid>("Guid");

                    b.Property<string>("Instagram")
                        .HasMaxLength(50);

                    b.Property<bool>("IsActive");

                    b.Property<DateTimeOffset>("LastActivity");

                    b.Property<string>("LastName")
                        .HasMaxLength(50);

                    b.Property<string>("Nickname")
                        .HasMaxLength(100);

                    b.Property<string>("Phone")
                        .HasMaxLength(50);

                    b.Property<DateTimeOffset>("Registered");

                    b.Property<int>("Role");

                    b.Property<string>("Skype")
                        .HasMaxLength(50);

                    b.Property<string>("Twitter")
                        .HasMaxLength(50);

                    b.Property<string>("Vk")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.ToTable("User");
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.Club", b =>
                {
                    b.HasOne("SaluteOnline.API.Domain.User", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.LinkEntities.ClubUserAdministrator", b =>
                {
                    b.HasOne("SaluteOnline.API.Domain.Club", "Club")
                        .WithMany("Administrators")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("SaluteOnline.API.Domain.User", "User")
                        .WithMany("ClubsAdministrated")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.MembershipRequest", b =>
                {
                    b.HasOne("SaluteOnline.API.Domain.Club", "Club")
                        .WithMany("MembershipRequests")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("SaluteOnline.API.Domain.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("SaluteOnline.API.Domain.Player", b =>
                {
                    b.HasOne("SaluteOnline.API.Domain.Club", "Club")
                        .WithMany("Players")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("SaluteOnline.API.Domain.User", "User")
                        .WithMany("PlayersAccounts")
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}
