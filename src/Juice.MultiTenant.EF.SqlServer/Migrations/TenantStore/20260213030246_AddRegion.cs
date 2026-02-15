using System;
using Juice.EF;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Juice.MultiTenant.EF.SqlServer.Migrations.TenantStore
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
            migrationBuilder.DropIndex(
                schema: _schema,
                name: "IX_Tenant_Identifier",
                table: "Tenant");

            migrationBuilder.RenameColumn(
                schema: _schema,
                name: "TenantClass",
                table: "Tenant",
                newName: "Tier");

            migrationBuilder.AlterColumn<string>(
                schema: _schema,
                name: "Identifier",
                table: "Tenant",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                schema: _schema,
                name: "Region",
                table: "Tenant",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

          
            migrationBuilder.CreateIndex(
                schema: _schema,
                name: "IX_Tenant_Identifier",
                table: "Tenant",
                column: "Identifier",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                schema: _schema,
                name: "IX_Tenant_Identifier",
                table: "Tenant");

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
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.CreateIndex(
                schema: _schema,
                name: "IX_Tenant_Identifier",
                table: "Tenant",
                column: "Identifier",
                unique: true,
                filter: "[Identifier] IS NOT NULL");
        }
    }
}
