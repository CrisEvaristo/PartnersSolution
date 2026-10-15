using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partners.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoPartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91104"),
                        "Padaria Central", "900000003", "CUSTOMER",
                        "contacto@padaria.example.test", 12000.00m, true,
                        new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91105"),
                        "Distribuidora do Norte", "900000004", "SUPPLIER",
                        "vendas@distribuidora.example.test", 35000.00m, true,
                        new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91106"),
                        "Oficina Horizonte", "900000005", "BOTH",
                        "geral@oficina.example.test", 18000.00m, true,
                        new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91107"),
                        "Farmacia Avenida", "900000006", "CUSTOMER",
                        "apoio@farmacia.example.test", 8000.00m, true,
                        new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91108"),
                        "Logistica Atlântico", "900000007", "SUPPLIER",
                        "operacoes@logistica.example.test", 42000.00m, true,
                        new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91109"),
                        "Hotel Jardim", "900000008", "BOTH",
                        "reservas@hotel.example.test", 27000.00m, true,
                        new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        new Guid("a8c4df67-6431-4d12-b450-6a72daf91110"),
                        "Livraria Estacao", "900000009", "CUSTOMER",
                        "encomendas@livraria.example.test", 9500.00m, true,
                        new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc)
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Partners",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91104"),
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91105"),
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91106"),
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91107"),
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91108"),
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91109"),
                    new Guid("a8c4df67-6431-4d12-b450-6a72daf91110")
                });
        }
    }
}
