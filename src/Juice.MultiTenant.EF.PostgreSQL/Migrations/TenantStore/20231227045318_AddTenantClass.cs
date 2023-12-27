using Juice.EF;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Juice.MultiTenant.EF.PostgreSQL.Migrations.TenantStore
{
    /// <inheritdoc />
    public partial class AddTenantClass : Migration
    {
        private readonly string _schema = "App";

        public AddTenantClass() { }

        public AddTenantClass(ISchemaDbContext schema)
        {
            _schema = schema.Schema;
        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantClass",
                schema: _schema,
                table: "Tenant",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantClass",
                schema: _schema,
                table: "Tenant");
        }
    }
}
