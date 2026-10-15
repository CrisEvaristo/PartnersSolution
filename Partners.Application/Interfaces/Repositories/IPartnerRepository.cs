
using Partners.Application.DTOs.Partners.Requests;
using Partners.Domain.Entities;

namespace Partners.Application.Interfaces.Repositories;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Partner?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken);

    Task<bool> ExistsByTaxNumberAsync(string taxNumber,Guid? excludePartnerId,CancellationToken cancellationToken);

    Task AddAsync(Partner partner,CancellationToken cancellationToken);

    Task UpdateAsync(Partner partner,byte[] expectedVersion, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Partner> Items, int TotalCount)> GetAllAsync(PartnerQueryParameters query,CancellationToken cancellationToken);
}