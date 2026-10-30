using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.Model.Commands.V1;
using WebApi.Data.Model.Dtos.V1;
using WebApi.Data.Model.Queries.V1;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Model.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ModelController : ControllerBase
{
    private readonly IMediator mediator;

    public ModelController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType<List<GetModelOkResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GetModelOkResponseDto>>> GetAll([FromQuery] Guid? brandId, CancellationToken ct)
    {
        return await this.mediator.Send(new GetAllModelsQuery(brandId), ct);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetModelOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetModelOkResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetModelByIdQuery(id), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted")]
    [ProducesResponseType<List<GetModelWithDeletedOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<GetModelWithDeletedOkResponseDto>>> GetAllWithDeleted([FromQuery] Guid? brandId, CancellationToken ct)
    {
        return await this.mediator.Send(new GetAllModelsWithDeletedQuery(brandId), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted/{id:guid}")]
    [ProducesResponseType<GetModelWithDeletedOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetModelWithDeletedOkResponseDto>> GetByIdWithDeleted(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetModelByIdWithDeletedQuery(id), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("Create")]
    [ProducesResponseType<GetModelOkResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetModelOkResponseDto>> Create([FromBody] CreateModelDto dto, CancellationToken ct)
    {
        var model = await this.mediator.Send(new CreateModelCommand(dto), ct);
        return this.StatusCode(StatusCodes.Status201Created, model);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("Update/{id:guid}")]
    [ProducesResponseType<GetModelOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetModelOkResponseDto>> Update(Guid id, [FromBody] UpdateModelDto dto, CancellationToken ct)
    {
        return await this.mediator.Send(new UpdateModelCommand(id, dto), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("Delete/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await this.mediator.Send(new DeleteModelCommand(id), ct);
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
        await this.mediator.Send(new RestoreModelCommand(id), ct);
        return this.NoContent();
    }
}
