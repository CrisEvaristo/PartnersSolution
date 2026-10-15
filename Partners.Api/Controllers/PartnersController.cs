using Microsoft.AspNetCore.Mvc;
using Partners.Application.DTOs.Partners.Requests;
using Partners.Application.UseCases.Partners;


namespace Partners.Api.Controllers;

[ApiController]
[Route("api/partners")]
public sealed class PartnersController : ControllerBase
{
    private readonly CreatePartnerUseCase _createPartner;
    private readonly GetPartnerByIdUseCase _getPartnerById;
    private readonly GetPartnersUseCase _getPartners;
    private readonly UpdatePartnerUseCase _updatePartner;

    public PartnersController(
        CreatePartnerUseCase createPartner,
        GetPartnerByIdUseCase getPartnerById,
        GetPartnersUseCase getPartners,
        UpdatePartnerUseCase updatePartner)
    {
        _createPartner = createPartner;
        _getPartnerById = getPartnerById;
        _getPartners = getPartners;
        _updatePartner = updatePartner;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var partner = await _createPartner.ExecuteAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = partner.Id },
            partner);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var partner = await _getPartnerById.ExecuteAsync(
            id,
            cancellationToken);

        return Ok(partner);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PartnerQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _getPartners.ExecuteAsync(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var partner = await _updatePartner.ExecuteAsync(
            id,
            request,
            cancellationToken);

        return Ok(partner);
    }
}