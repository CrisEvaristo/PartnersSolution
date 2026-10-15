namespace Partners.Application.DTOs.Partners.Requests;

public sealed record PartnerQueryParameters
{
    public string? Search { get; init; }

    public string? Type { get; init; }

    public bool IncludeInactive { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}