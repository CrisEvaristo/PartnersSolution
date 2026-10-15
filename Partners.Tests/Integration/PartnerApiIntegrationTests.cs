using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Partners.Infrastructure.Persistence;

namespace Partners.Tests.Integration;

[TestClass]
[DoNotParallelize]
public sealed class PartnerApiIntegrationTests
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static HttpClient _client = null!;
    private static string _connectionString = string.Empty;
    private static DbContextOptions<AppDbContext> _dbOptions = null!;

    [ClassInitialize]
    public static async Task StartApiAndDatabase(TestContext _)
    {
        var configuredConnection = Environment.GetEnvironmentVariable(
            "PARTNERS_TEST_SQLSERVER");

        if (string.IsNullOrWhiteSpace(configuredConnection))
        {
            Assert.Inconclusive(
                "Configure PARTNERS_TEST_SQLSERVER para executar os testes SQL Server.");
        }

        var connectionBuilder = new SqlConnectionStringBuilder(configuredConnection)
        {
            InitialCatalog = $"PartnersTests_{Guid.NewGuid():N}"
        };
        _connectionString = connectionBuilder.ConnectionString;
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:DefaultConnection"] = _connectionString
                        }));

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<AppDbContext>();
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(_connectionString));
                });
            });

        _client = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await database.Database.MigrateAsync();
    }

    [TestInitialize]
    public async Task ClearPartners()
    {
        await using var context = CreateContext();
        await context.Partners.ExecuteDeleteAsync();
    }

    [ClassCleanup]
    public static async Task StopApiAndDropDatabase()
    {
        _client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        if (!string.IsNullOrWhiteSpace(_connectionString))
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
        }
    }

    [TestMethod]
    public async Task Create_normalizes_values_returns_location_and_reserves_inactive_tax_number()
    {
        var created = await CreatePartnerAsync(
            " Ana Silva ",
            "001.234.567",
            "CUSTOMER");

        Assert.AreEqual("Ana Silva", created.Name);
        Assert.AreEqual("001234567", created.TaxNumber);
        Assert.IsTrue(created.IsActive);
        Assert.AreEqual(8, Convert.FromBase64String(created.Version).Length);
        Assert.AreEqual(TimeSpan.Zero, created.CreatedAt.Offset);

        using (var getResponse = await _client.GetAsync($"/api/partners/{created.Id}"))
        {
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
        }

        using (var duplicate = await PostPartnerAsync(
                   "Outra Empresa",
                   "001 234 567",
                   "SUPPLIER"))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, duplicate.StatusCode);
        }

        using var deactivateResponse = await UpdatePartnerAsync(
            created,
            isActive: false);
        Assert.AreEqual(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var deactivated = await ReadPartnerAsync(deactivateResponse);

        using var reservedResponse = await PostPartnerAsync(
            "Outra Empresa",
            "001 234 567",
            "SUPPLIER");
        Assert.AreEqual(HttpStatusCode.Conflict, reservedResponse.StatusCode);

        var inactive = await GetPartnerAsync(created.Id);
        Assert.IsFalse(inactive.IsActive);
        Assert.AreEqual(created.CreatedAt, inactive.CreatedAt);
        Assert.AreNotEqual(created.Version, deactivated.Version);
    }

    [TestMethod]
    public async Task Type_filters_include_both_and_combine_with_search_and_pagination()
    {
        var customer = await CreatePartnerAsync("Ana Silva", "100000001", "CUSTOMER");
        var both = await CreatePartnerAsync("Ana Silva", "100000002", "BOTH");
        var supplier = await CreatePartnerAsync("Ana Silva", "100000003", "SUPPLIER");
        await CreatePartnerAsync("Bruno Costa", "100000004", "CUSTOMER");

        var customers = await GetListAsync("Search=Ana&Type=CUSTOMER&Page=1&PageSize=1");
        var secondCustomerPage = await GetListAsync("Search=Ana&Type=CUSTOMER&Page=2&PageSize=1");
        var suppliers = await GetListAsync("Type=SUPPLIER");
        var bothOnly = await GetListAsync("Type=BOTH");
        var beyondLastPage = await GetListAsync("Type=CUSTOMER&Page=99&PageSize=1");

        await using var context = CreateContext();
        var orderedExpectedIds = await context.Partners
            .Where(item => item.Id == customer.Id || item.Id == both.Id)
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Select(item => item.Id)
            .ToArrayAsync();

        Assert.AreEqual(2, customers.TotalCount);
        Assert.AreEqual(2, secondCustomerPage.TotalCount);
        Assert.AreEqual(1, customers.PageSize);
        CollectionAssert.AreEqual(
            orderedExpectedIds,
            new[] { customers.Items[0].Id, secondCustomerPage.Items[0].Id });
        CollectionAssert.AreEquivalent(
            new[] { both.Id, supplier.Id },
            suppliers.Items.Select(item => item.Id).ToArray());
        CollectionAssert.AreEqual(new[] { both.Id }, bothOnly.Items.Select(item => item.Id).ToArray());
        Assert.AreEqual(0, beyondLastPage.Items.Count);
        Assert.AreEqual(3, beyondLastPage.TotalCount);

        var defaults = await GetListAsync(string.Empty);
        Assert.AreEqual(1, defaults.Page);
        Assert.AreEqual(20, defaults.PageSize);
    }

    [TestMethod]
    public async Task Update_allows_own_tax_number_and_rejects_another_without_partial_changes()
    {
        var first = await CreatePartnerAsync("Primeira", "200000001", "CUSTOMER");
        var second = await CreatePartnerAsync("Segunda", "200000002", "SUPPLIER");

        using (var ownTaxNumber = await UpdatePartnerAsync(
                   first,
                   name: "Primeira Atualizada"))
        {
            Assert.AreEqual(HttpStatusCode.OK, ownTaxNumber.StatusCode);
        }

        using (var duplicateTaxNumber = await UpdatePartnerAsync(
                   second,
                   taxNumber: first.TaxNumber))
        {
            Assert.AreEqual(HttpStatusCode.Conflict, duplicateTaxNumber.StatusCode);
        }

        var unchanged = await GetPartnerAsync(second.Id);
        Assert.AreEqual("Segunda", unchanged.Name);
        Assert.AreEqual("200000002", unchanged.TaxNumber);
    }

    [TestMethod]
    public async Task Deactivation_hides_from_default_list_but_keeps_id_access_and_can_reactivate()
    {
        var created = await CreatePartnerAsync("Ativa", "300000001", "BOTH");

        using var deactivateResponse = await UpdatePartnerAsync(created, isActive: false);
        var deactivated = await ReadPartnerAsync(deactivateResponse);
        var activeList = await GetListAsync(string.Empty);
        var allList = await GetListAsync("IncludeInactive=true");
        var byId = await GetPartnerAsync(created.Id);

        Assert.AreEqual(0, activeList.Items.Count);
        CollectionAssert.AreEqual(new[] { created.Id }, allList.Items.Select(item => item.Id).ToArray());
        Assert.IsFalse(byId.IsActive);

        using var reactivateResponse = await UpdatePartnerAsync(deactivated, isActive: true);
        Assert.AreEqual(HttpStatusCode.OK, reactivateResponse.StatusCode);
        CollectionAssert.AreEqual(
            new[] { created.Id },
            (await GetListAsync(string.Empty)).Items.Select(item => item.Id).ToArray());
    }

    [TestMethod]
    public async Task Invalid_inputs_return_400_without_mutating_data()
    {
        using (var invalidCredit = await PostPartnerAsync(
                   "Crédito Inválido",
                   "400000001",
                   "CUSTOMER",
                   creditLimit: 10.999m))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, invalidCredit.StatusCode);
        }

        using (var invalidTaxNumber = await PostPartnerAsync(
                   "NIF Inválido",
                   "12345678A",
                   "CUSTOMER"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, invalidTaxNumber.StatusCode);
        }

        using (var invalidPage = await _client.GetAsync("/api/partners?Page=0"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, invalidPage.StatusCode);
        }

        using (var invalidType = await _client.GetAsync("/api/partners?Type=INVALID"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, invalidType.StatusCode);
        }

        var existing = await CreatePartnerAsync("Original", "400000002", "CUSTOMER");
        using (var invalidVersion = await UpdatePartnerAsync(
                   existing,
                   version: "invalid"))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, invalidVersion.StatusCode);
        }

        using var missingCreditLimit = await _client.PostAsync(
            "/api/partners",
            JsonContent.Create(new
            {
                name = "Sem limite",
                taxNumber = "400000003",
                type = "CUSTOMER"
            }));
        Assert.AreEqual(HttpStatusCode.BadRequest, missingCreditLimit.StatusCode);

        var unchanged = await GetPartnerAsync(existing.Id);
        Assert.AreEqual("Original", unchanged.Name);
        Assert.AreEqual("400000002", unchanged.TaxNumber);
        await using var context = CreateContext();
        Assert.AreEqual(1, await context.Partners.CountAsync());
    }

    [TestMethod]
    public async Task Concurrent_creates_with_equivalent_tax_numbers_leave_one_row()
    {
        var first = PostPartnerAsync("Concorrente A", "500.000.001", "CUSTOMER");
        var second = PostPartnerAsync("Concorrente B", "500 000 001", "SUPPLIER");
        var responses = await Task.WhenAll(first, second);

        CollectionAssert.AreEquivalent(
            new[] { HttpStatusCode.Created, HttpStatusCode.Conflict },
            responses.Select(response => response.StatusCode).ToArray());

        foreach (var response in responses)
        {
            response.Dispose();
        }

        await using var context = CreateContext();
        Assert.AreEqual(
            1,
            await context.Partners.CountAsync(partner => partner.TaxNumber == "500000001"));
    }

    [TestMethod]
    public async Task Concurrent_api_updates_with_same_version_return_one_ok_and_one_conflict()
    {
        var partner = await CreatePartnerAsync("Antes", "600000001", "CUSTOMER");
        var responses = await Task.WhenAll(
            UpdatePartnerAsync(partner, name: "Concorrente A"),
            UpdatePartnerAsync(partner, name: "Concorrente B"));

        try
        {
            CollectionAssert.AreEquivalent(
                new[] { HttpStatusCode.OK, HttpStatusCode.Conflict },
                responses.Select(response => response.StatusCode).ToArray());
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }

        var saved = await GetPartnerAsync(partner.Id);
        Assert.IsTrue(saved.Name is "Concorrente A" or "Concorrente B");
    }

    private async Task<PartnerDto> CreatePartnerAsync(
        string name,
        string taxNumber,
        string type,
        decimal creditLimit = 100m)
    {
        using var response = await PostPartnerAsync(name, taxNumber, type, creditLimit);
        Assert.AreEqual(
            HttpStatusCode.Created,
            response.StatusCode,
            await response.Content.ReadAsStringAsync());
        Assert.IsNotNull(response.Headers.Location);
        return await ReadPartnerAsync(response);
    }

    private Task<HttpResponseMessage> PostPartnerAsync(
        string name,
        string taxNumber,
        string type,
        decimal creditLimit = 100m) =>
        _client.PostAsJsonAsync("/api/partners", new
        {
            name,
            taxNumber,
            type,
            email = "  ",
            creditLimit
        });

    private Task<HttpResponseMessage> UpdatePartnerAsync(
        PartnerDto partner,
        string? name = null,
        string? taxNumber = null,
        bool? isActive = null,
        string? version = null) =>
        _client.PutAsJsonAsync($"/api/partners/{partner.Id}", new
        {
            name = name ?? partner.Name,
            taxNumber = taxNumber ?? partner.TaxNumber,
            type = partner.Type,
            email = partner.Email,
            creditLimit = partner.CreditLimit,
            isActive = isActive ?? partner.IsActive,
            version = version ?? partner.Version
        });

    private async Task<PartnerDto> GetPartnerAsync(Guid id)
    {
        using var response = await _client.GetAsync($"/api/partners/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        return await ReadPartnerAsync(response);
    }

    private async Task<PagedDto> GetListAsync(string query)
    {
        var path = string.IsNullOrEmpty(query)
            ? "/api/partners"
            : $"/api/partners?{query}";
        using var response = await _client.GetAsync(path);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<PagedDto>()
            ?? throw new InvalidOperationException("A resposta da listagem veio vazia.");
    }

    private static async Task<PartnerDto> ReadPartnerAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<PartnerDto>()
        ?? throw new InvalidOperationException("A resposta da entidade veio vazia.");

    private static AppDbContext CreateContext() => new(_dbOptions);

    private sealed record PartnerDto(
        Guid Id,
        string Name,
        string TaxNumber,
        string Type,
        string? Email,
        decimal CreditLimit,
        bool IsActive,
        string Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    private sealed record PagedDto(
        IReadOnlyList<PartnerDto> Items,
        int TotalCount,
        int Page,
        int PageSize);
}