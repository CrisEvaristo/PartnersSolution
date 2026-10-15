using Partners.Application.DTOs.Partners.Requests;
using Partners.Application.DTOs.Partners.Responses;
using Partners.Application.Exceptions;
using Partners.Application.Interfaces.Repositories;

namespace Partners.Application.UseCases.Partners;

public sealed class GetPartnersUseCase
{
    private readonly IPartnerRepository _repository;

    public GetPartnersUseCase(IPartnerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<PartnerResponse>> ExecuteAsync(
        PartnerQueryParameters query,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page < 1)
        {
            throw new ApplicationValidationException(
                "Page deve ser maior ou igual a 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new ApplicationValidationException(
                "PageSize deve estar entre 1 e 100.");
        }

        string? type = null;

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            type = query.Type.Trim().ToUpperInvariant();

            if (type is not ("CUSTOMER" or "SUPPLIER" or "BOTH"))
            {
                throw new ApplicationValidationException(
                    "O Type deve ser CUSTOMER, SUPPLIER ou BOTH.");
            }
        }

        var normalizedQuery = query with
        {
            Search = string.IsNullOrWhiteSpace(query.Search)
                ? null
                : query.Search.Trim(),
            Type = type
        };

        var (items, totalCount) = await _repository.GetAllAsync(
            normalizedQuery,
            cancellationToken);

        var responseItems = items
            .Select(PartnerResponse.FromEntity)
            .ToList();

        return new PagedResponse<PartnerResponse>(
            responseItems,
            totalCount,
            query.Page,
            query.PageSize);
    }
}