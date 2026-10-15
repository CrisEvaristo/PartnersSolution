namespace Partners.Application.DTOs.Partners.Requests;

public sealed record UpdatePartnerRequest
{
    public required string Name { get; init; }

    public required string TaxNumber { get; init; }

    public required string Type { get; init; }

    public string? Email { get; init; }

    public required decimal CreditLimit { get; init; }

    public required bool IsActive { get; init; }

    public required string Version { get; init; }
}