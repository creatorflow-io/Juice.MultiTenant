﻿// <auto-generated />
using System;
using Juice.MultiTenant.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Juice.MultiTenant.Tests.Migrations.TenantContentPostgreSQL
{
    [DbContext(typeof(TenantContentPostgreDbContext))]
    partial class TenantContentPostgreDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Juice.MultiTenant.Tests.Domain.TenantContent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedUser")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("Disabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ModifiedUser")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Properties")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("jsonb")
                        .HasDefaultValue("{}");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.ToTable("TenantContent", "Contents");

                    b
                        .HasAnnotation("Finbuckle:MultiTenant", true)
                        .HasAnnotation("Juice:Auditable", true)
                        .HasAnnotation("Juice:Expandable", true);
                });
#pragma warning restore 612, 618
        }
    }
}
