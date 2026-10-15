using Partners.Application.DTOs.Partners.Responses;
using Partners.Application.Exceptions;
using Partners.Application.Interfaces.Repositories;

namespace Partners.Application.UseCases.Partners;

public sealed class GetPartnerByIdUseCase
{
    private readonly IPartnerRepository _repository;

    public GetPartnerByIdUseCase(IPartnerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PartnerResponse> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            throw new ApplicationValidationException(
                "O ID fornecido é inválido.");
        }

        var partner = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (partner is null)
        {
            throw new PartnerNotFoundException(id);
        }

        return PartnerResponse.FromEntity(partner);
    }
}