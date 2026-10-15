using Partners.Application.DTOs.Partners.Requests;
using Partners.Application.DTOs.Partners.Responses;
using Partners.Application.Exceptions;
using Partners.Application.Interfaces.Repositories;
using Partners.Application.Validators;
using Partners.Domain.Entities;

namespace Partners.Application.UseCases.Partners;

public sealed class CreatePartnerUseCase
{
    private readonly IPartnerRepository _repository;

    public CreatePartnerUseCase(IPartnerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PartnerResponse> ExecuteAsync(
        CreatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = PartnerInputValidator.ValidateName(request.Name);
        var taxNumber = PartnerInputValidator.ValidateTaxNumber(
            request.TaxNumber);
        var type = PartnerInputValidator.ValidateType(request.Type);
        var email = PartnerInputValidator.ValidateEmail(request.Email);
        var creditLimit = PartnerInputValidator.ValidateCreditLimit(
            request.CreditLimit);

        var exists = await _repository.ExistsByTaxNumberAsync(
            taxNumber,
            excludePartnerId: null,
            cancellationToken);

        if (exists)
        {
            throw new DuplicateTaxNumberException();
        }

        var partner = new Partner(
            name,
            taxNumber,
            type,
            email,
            creditLimit);

        await _repository.AddAsync(partner, cancellationToken);

        return PartnerResponse.FromEntity(partner);
    }
}