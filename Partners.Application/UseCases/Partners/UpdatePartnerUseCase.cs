using Partners.Application.DTOs.Partners.Requests;
using Partners.Application.DTOs.Partners.Responses;
using Partners.Application.Exceptions;
using Partners.Application.Interfaces.Repositories;
using Partners.Application.Validators;

namespace Partners.Application.UseCases.Partners;

public sealed class UpdatePartnerUseCase
{
    private readonly IPartnerRepository _repository;

    public UpdatePartnerUseCase(IPartnerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PartnerResponse> ExecuteAsync(
        Guid id,
        UpdatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (id == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "O ID fornecido é inválido.");
        }

        // Validar todos os dados antes de alterar a entidade.
        var name = PartnerInputValidator.ValidateName(request.Name);
        var taxNumber = PartnerInputValidator.ValidateTaxNumber(
            request.TaxNumber);
        var type = PartnerInputValidator.ValidateType(request.Type);
        var email = PartnerInputValidator.ValidateEmail(request.Email);
        var creditLimit = PartnerInputValidator.ValidateCreditLimit(
            request.CreditLimit);
        var expectedVersion = PartnerInputValidator.ValidateVersion(
            request.Version);

        // Obter a entidade acompanhada pelo DbContext.
        var partner = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (partner is null)
        {
            throw new PartnerNotFoundException(id);
        }

        var exists = await _repository.ExistsByTaxNumberAsync(
            taxNumber,
            excludePartnerId: id,
            cancellationToken);

        if (exists)
        {
            throw new DuplicateTaxNumberException();
        }

        partner.Update(
            name,
            taxNumber,
            type,
            email,
            creditLimit,
            request.IsActive);

        await _repository.UpdateAsync(
            partner,
            expectedVersion,
            cancellationToken);

        return PartnerResponse.FromEntity(partner);
    }
}