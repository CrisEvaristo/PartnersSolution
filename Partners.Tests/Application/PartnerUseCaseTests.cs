using Microsoft.VisualStudio.TestTools.UnitTesting;
using Partners.Application.DTOs.Partners.Requests;
using Partners.Application.Exceptions;
using Partners.Application.Interfaces.Repositories;
using Partners.Application.UseCases.Partners;
using Partners.Domain.Entities;

namespace Partners.Tests.Application;

[TestClass]
public sealed class PartnerUseCaseTests
{
    [TestMethod]
    public async Task Create_trims_name_normalizes_tax_number_and_empty_email()
    {
        var repository = new FakePartnerRepository();
        var useCase = new CreatePartnerUseCase(repository);

        var response = await useCase.ExecuteAsync(
            new CreatePartnerRequest
            {
                Name = " Ana Silva ",
                TaxNumber = "001.234.567",
                Type = "CUSTOMER",
                Email = "  ",
                CreditLimit = 10.25m
            },
            CancellationToken.None);

        Assert.AreEqual("Ana Silva", response.Name);
        Assert.AreEqual("001234567", response.TaxNumber);
        Assert.IsNull(response.Email);
        Assert.IsTrue(response.IsActive);
        Assert.AreEqual(1, repository.Partners.Count);
    }

    [TestMethod]
    public async Task Create_rejects_invalid_tax_number()
    {
        var repository = new FakePartnerRepository();
        var useCase = new CreatePartnerUseCase(repository);

        await Assert.ThrowsExceptionAsync<ApplicationValidationException>(async () =>
            await useCase.ExecuteAsync(
                new CreatePartnerRequest
                {
                    Name = "Empresa",
                    TaxNumber = "12345678A",
                    Type = "CUSTOMER",
                    CreditLimit = 10.00m
                },
                CancellationToken.None));

        Assert.AreEqual(0, repository.Partners.Count);
    }

    [TestMethod]
    public async Task Create_rejects_credit_limit_with_excess_precision()
    {
        var useCase = new CreatePartnerUseCase(new FakePartnerRepository());

        await Assert.ThrowsExceptionAsync<ApplicationValidationException>(() =>
            useCase.ExecuteAsync(
                new CreatePartnerRequest
                {
                    Name = "Empresa",
                    TaxNumber = "123456789",
                    Type = "CUSTOMER",
                    CreditLimit = 10.999m
                },
                CancellationToken.None));
    }

    [TestMethod]
    public async Task Update_rejects_malformed_version_before_mutating_entity()
    {
        var repository = new FakePartnerRepository();
        var partner = new Partner(
            "Nome original",
            "123456789",
            Partners.Domain.Enums.PartnerType.CUSTOMER,
            null,
            50m);
        repository.Partners.Add(partner);
        var useCase = new UpdatePartnerUseCase(repository);

        await Assert.ThrowsExceptionAsync<ApplicationValidationException>(async () =>
            await useCase.ExecuteAsync(
                partner.Id,
                new UpdatePartnerRequest
                {
                    Name = "Nome alterado",
                    TaxNumber = "123456789",
                    Type = "CUSTOMER",
                    CreditLimit = 75m,
                    IsActive = false,
                    Version = "not-base64"
                },
                CancellationToken.None));

        Assert.AreEqual("Nome original", partner.Name);
        Assert.IsTrue(partner.IsActive);
    }

    [TestMethod]
    public async Task List_defaults_to_first_page_of_twenty()
    {
        var useCase = new GetPartnersUseCase(new FakePartnerRepository());

        var response = await useCase.ExecuteAsync(
            new PartnerQueryParameters(),
            CancellationToken.None);

        Assert.AreEqual(1, response.Page);
        Assert.AreEqual(20, response.PageSize);
    }

    private sealed class FakePartnerRepository : IPartnerRepository
    {
        public List<Partner> Partners { get; } = [];

        public Task<Partner?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(Partners.SingleOrDefault(partner => partner.Id == id));

        public Task<Partner?> GetByTaxNumberAsync(
            string taxNumber,
            CancellationToken cancellationToken) =>
            Task.FromResult(Partners.SingleOrDefault(partner => partner.TaxNumber == taxNumber));

        public Task<bool> ExistsByTaxNumberAsync(
            string taxNumber,
            Guid? excludePartnerId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Partners.Any(partner =>
                partner.TaxNumber == taxNumber && partner.Id != excludePartnerId));

        public Task AddAsync(Partner partner, CancellationToken cancellationToken)
        {
            Partners.Add(partner);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Partner partner,
            byte[] expectedVersion,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<(IReadOnlyList<Partner> Items, int TotalCount)> GetAllAsync(
            PartnerQueryParameters query,
            CancellationToken cancellationToken) =>
            Task.FromResult<(IReadOnlyList<Partner>, int)>((Partners, Partners.Count));
    }
}