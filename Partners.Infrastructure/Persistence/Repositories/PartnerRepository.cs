using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Partners.Application.DTOs.Partners.Requests;
using Partners.Application.Exceptions;
using Partners.Application.Interfaces.Repositories;
using Partners.Domain.Entities;
using Partners.Domain.Enums;
using Partners.Infrastructure.Persistence;

namespace Partners.Infrastructure.Persistence.Repositories;

public sealed class PartnerRepository : IPartnerRepository
{
    private readonly AppDbContext _context;

    public PartnerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Partners
            .FirstOrDefaultAsync(partner => partner.Id == id,cancellationToken);
    }

    public async Task<Partner?> GetByTaxNumberAsync(string taxNumber,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taxNumber);

        var normalizedTaxNumber = NormalizeTaxNumber(taxNumber);

        return await _context.Partners
            .FirstOrDefaultAsync(
                partner => partner.TaxNumber == normalizedTaxNumber,
                cancellationToken);
    }

    public async Task<bool> ExistsByTaxNumberAsync(
        string taxNumber,
        Guid? excludePartnerId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taxNumber);

        var normalizedTaxNumber = NormalizeTaxNumber(taxNumber);

        var query = _context.Partners
            .Where(partner =>
                partner.TaxNumber == normalizedTaxNumber);

        if (excludePartnerId.HasValue)
        {
            query = query.Where(
                partner => partner.Id != excludePartnerId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        Partner partner,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(partner);

        await _context.Partners.AddAsync(
            partner,
            cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateTaxNumberException();
        }
    }

    public async Task UpdateAsync(Partner partner,byte[] expectedVersion,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(partner);
        ArgumentNullException.ThrowIfNull(expectedVersion);

        if (expectedVersion.Length != 8)
        {
            throw new ArgumentException("A versão enviada é inválida.",nameof(expectedVersion));
        }

        var entry = _context.Entry(partner);

        if (entry.State == EntityState.Detached)
        {
            _context.Partners.Attach(partner);
            entry = _context.Entry(partner);
            entry.State = EntityState.Modified;
        }

        // A versão enviada pelo cliente é a versão original
        // utilizada na condição WHERE do UPDATE.
        entry.Property(p => p.Version).OriginalValue =
            expectedVersion.ToArray();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new PartnerConcurrencyException();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateTaxNumberException();
        }
    }

    public async Task<(IReadOnlyList<Partner> Items, int TotalCount)>
        GetAllAsync(
            PartnerQueryParameters query,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Page),"A página deve ser maior ou igual a 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(query.PageSize),"O tamanho da página deve estar entre 1 e 100.");
        }

        IQueryable<Partner> partners = _context.Partners.AsNoTracking();

        // 1. Filtrar entidades inativas.
        if (!query.IncludeInactive)
        {
            partners = partners.Where(partner => partner.IsActive);
        }

        // 2. Filtrar por tipo.
        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            var type = query.Type.Trim().ToUpperInvariant();

            partners = type switch
            {
                "CUSTOMER" => partners.Where(
                    partner =>partner.Type == PartnerType.CUSTOMER || partner.Type == PartnerType.BOTH),

                "SUPPLIER" => partners.Where(
                    partner =>partner.Type == PartnerType.SUPPLIER || partner.Type == PartnerType.BOTH),

                "BOTH" => partners.Where(partner => partner.Type == PartnerType.BOTH),

                _ => throw new ArgumentException("O Type deve ser CUSTOMER, SUPPLIER ou BOTH.",nameof(query.Type))
            };
        }

        // 3. Pesquisar por nome ou NIF.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var normalizedTaxSearch = NormalizeTaxNumber(search);

            var nameSearch = partners.Where(
                partner => EF.Functions
                    .Collate(
                        partner.Name,
                        "Latin1_General_100_CI_AS")
                    .Contains(search));

            if (normalizedTaxSearch.Length == 0)
            {
                // Uma pesquisa de NIF vazia nunca deve corresponder
                // a todos os registos.
                partners = nameSearch;
            }
            else
            {
                partners = partners.Where(
                    partner =>
                        EF.Functions
                            .Collate(
                                partner.Name,
                                "Latin1_General_100_CI_AS")
                            .Contains(search)
                        ||
                        partner.TaxNumber.Contains(
                            normalizedTaxSearch));
            }
        }

        // 4. Contar depois de aplicar todos os filtros,
        // mas antes da paginação.
        var totalCount = await partners.CountAsync(
            cancellationToken);

        // 5. Calcular o offset sem overflow.
        var skip = ((long)query.Page - 1) * query.PageSize;

        if (skip > int.MaxValue)
        {
            return (Array.Empty<Partner>(), totalCount);
        }

        // 6. Ordenação determinística e paginação no SQL Server.
        var items = await partners
            .OrderBy(partner => partner.Name)
            .ThenBy(partner => partner.Id)
            .Skip((int)skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static string NormalizeTaxNumber(string value)
    {
        return new string(
            value
                .Where(character =>
                    !char.IsWhiteSpace(character) &&
                    character != '.' &&
                    character != '-')
                .ToArray());
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        for (Exception? current = exception;
             current is not null;
             current = current.InnerException)
        {
            if (current is SqlException sqlException &&
                sqlException.Number is 2601 or 2627)
            {
                return true;
            }
        }

        return false;
    }
}