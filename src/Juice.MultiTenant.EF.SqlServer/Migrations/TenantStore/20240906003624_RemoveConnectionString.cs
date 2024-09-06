using Juice.EF;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Juice.MultiTenant.EF.SqlServer.Migrations.TenantStore
{
    /// <inheritdoc />
    public partial class RemoveConnectionString : Migration
    {
        private readonly string _schema = "App";

        public RemoveConnectionString() { }

        public RemoveConnectionString(ISchemaDbContext schema)
        {
            _schema = schema.Schema;
        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionString",
                schema: _schema,
                table: "Tenant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionString",
                schema: _schema,
                table: "Tenant",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
