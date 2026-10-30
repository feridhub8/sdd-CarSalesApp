using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.Listing.Commands.V1;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Shared.Models.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.Listing.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ListingController : ControllerBase
{
    private readonly IMediator mediator;

    public ListingController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<GetListingOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<GetListingOkResponseDto>>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = PagingRuleExtensions.DefaultPage,
        [FromQuery] int pageSize = PagingRuleExtensions.DefaultPageSize)
    {
        return await this.mediator.Send(new GetAllListingsQuery(page, pageSize), ct);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetListingOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetListingOkResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetListingByIdQuery(id), ct);
    }

    [Authorize]
    [HttpGet("Mine")]
    [ProducesResponseType<PagedResponse<GetListingOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<GetListingOkResponseDto>>> GetMine(
        CancellationToken ct,
        [FromQuery] int page = PagingRuleExtensions.DefaultPage,
        [FromQuery] int pageSize = PagingRuleExtensions.DefaultPageSize)
    {
        return await this.mediator.Send(new GetMyListingsQuery(page, pageSize), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted")]
    [ProducesResponseType<PagedResponse<GetListingWithDeletedOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<GetListingWithDeletedOkResponseDto>>> GetAllWithDeleted(
        CancellationToken ct,
        [FromQuery] int page = PagingRuleExtensions.DefaultPage,
        [FromQuery] int pageSize = PagingRuleExtensions.DefaultPageSize)
    {
        return await this.mediator.Send(new GetAllListingsWithDeletedQuery(page, pageSize), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted/{id:guid}")]
    [ProducesResponseType<GetListingWithDeletedOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetListingWithDeletedOkResponseDto>> GetByIdWithDeleted(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetListingByIdWithDeletedQuery(id), ct);
    }

    [Authorize]
    [HttpPost("Create")]
    [ProducesResponseType<GetListingOkResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetListingOkResponseDto>> Create([FromBody] CreateListingDto dto, CancellationToken ct)
    {
        var listing = await this.mediator.Send(new CreateListingCommand(dto), ct);
        return this.StatusCode(StatusCodes.Status201Created, listing);
    }

    [Authorize]
    [HttpPut("Update/{id:guid}")]
    [ProducesResponseType<GetListingOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetListingOkResponseDto>> Update(Guid id, [FromBody] UpdateListingDto dto, CancellationToken ct)
    {
        return await this.mediator.Send(new UpdateListingCommand(id, dto), ct);
    }

    [Authorize]
    [HttpDelete("Delete/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await this.mediator.Send(new DeleteListingCommand(id), ct);
        return this.NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("Restore/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await this.mediator.Send(new RestoreListingCommand(id), ct);
        return this.NoContent();
    }
}
