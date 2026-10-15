using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partners.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxNumber = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_TaxNumber",
                table: "Partners",
                column: "TaxNumber",
                unique: true);

            migrationBuilder.InsertData(
                table: "Partners",
                columns: new[]
                {
                    "Id", "Name", "TaxNumber", "Type", "Email",
                    "CreditLimit", "IsActive", "CreatedAt", "UpdatedAt"
                },
                values: new object[,]
                {
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91101"),
                        "Ana Silva", "001234567", "CUSTOMER", null,
                        15000.00m, true,
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91102"),
                        "Mercearia do Bairro", "900000001", "SUPPLIER",
                        "compras@example.test", 25000.00m, true,
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91103"),
                        "Parceiro Integral", "900000002", "BOTH", null,
                        50000.00m, true,
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partners");
        }
    }
}
