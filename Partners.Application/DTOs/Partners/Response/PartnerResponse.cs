using Partners.Domain.Entities;

namespace Partners.Application.DTOs.Partners.Responses;

public sealed record PartnerResponse(
    Guid Id,
    string Name,
    string TaxNumber,
    string Type,
    string? Email,
    decimal CreditLimit,
    bool IsActive,
    string Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static PartnerResponse FromEntity(Partner partner)
    {
        return new PartnerResponse(
            partner.Id,
            partner.Name,
            partner.TaxNumber,
            partner.Type.ToString(),
            partner.Email,
            partner.CreditLimit,
            partner.IsActive,
            Convert.ToBase64String(partner.Version),
            new DateTimeOffset(
                DateTime.SpecifyKind(partner.CreatedAt, DateTimeKind.Utc)),
            new DateTimeOffset(
                DateTime.SpecifyKind(partner.UpdatedAt, DateTimeKind.Utc)));
    }
}