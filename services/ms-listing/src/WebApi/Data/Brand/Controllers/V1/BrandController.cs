using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.Brand.Commands.V1;
using WebApi.Data.Brand.Dtos.V1;
using WebApi.Data.Brand.Queries.V1;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Brand.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class BrandController : ControllerBase
{
    private readonly IMediator mediator;

    public BrandController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType<List<GetBrandOkResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GetBrandOkResponseDto>>> GetAll(CancellationToken ct)
    {
        return await this.mediator.Send(new GetAllBrandsQuery(), ct);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetBrandOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetBrandOkResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetBrandByIdQuery(id), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted")]
    [ProducesResponseType<List<GetBrandWithDeletedOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<GetBrandWithDeletedOkResponseDto>>> GetAllWithDeleted(CancellationToken ct)
    {
        return await this.mediator.Send(new GetAllBrandsWithDeletedQuery(), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted/{id:guid}")]
    [ProducesResponseType<GetBrandWithDeletedOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetBrandWithDeletedOkResponseDto>> GetByIdWithDeleted(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetBrandByIdWithDeletedQuery(id), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("Create")]
    [ProducesResponseType<GetBrandOkResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetBrandOkResponseDto>> Create([FromBody] CreateBrandDto dto, CancellationToken ct)
    {
        var brand = await this.mediator.Send(new CreateBrandCommand(dto), ct);
        return this.StatusCode(StatusCodes.Status201Created, brand);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("Update/{id:guid}")]
    [ProducesResponseType<GetBrandOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetBrandOkResponseDto>> Update(Guid id, [FromBody] UpdateBrandDto dto, CancellationToken ct)
    {
        return await this.mediator.Send(new UpdateBrandCommand(id, dto), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("Delete/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await this.mediator.Send(new DeleteBrandCommand(id), ct);
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
        await this.mediator.Send(new RestoreBrandCommand(id), ct);
        return this.NoContent();
    }
}
