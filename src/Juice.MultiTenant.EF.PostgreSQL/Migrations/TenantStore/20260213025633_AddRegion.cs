using System;
using Juice.EF;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Juice.MultiTenant.EF.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class AddRegion : Migration
    {
        private readonly string _schema = "App";

        public AddRegion() { }

        public AddRegion(ISchemaDbContext schema)
        {
            _schema = schema.Schema;
        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                schema: _schema,
                name: "TenantClass",
                table: "Tenant",
                newName: "Tier");

            migrationBuilder.AlterColumn<string>(
                schema: _schema,
                name: "Identifier",
                table: "Tenant",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                schema: _schema,
                name: "Region",
                table: "Tenant",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                schema: _schema,
                name: "Region",
                table: "Tenant");

            migrationBuilder.RenameColumn(
                schema: _schema,
                name: "Tier",
                table: "Tenant",
                newName: "TenantClass");

            migrationBuilder.AlterColumn<string>(
                schema: _schema,
                name: "Identifier",
                table: "Tenant",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16);
        }
    }
}
