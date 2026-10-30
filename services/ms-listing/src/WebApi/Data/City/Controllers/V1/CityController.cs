using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Queries.V1;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.City.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class CityController : ControllerBase
{
    private readonly IMediator mediator;

    public CityController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType<List<GetCityOkResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GetCityOkResponseDto>>> GetAll(CancellationToken ct)
    {
        return await this.mediator.Send(new GetAllCitiesQuery(), ct);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetCityOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCityOkResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetCityByIdQuery(id), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted")]
    [ProducesResponseType<List<GetCityWithDeletedOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<GetCityWithDeletedOkResponseDto>>> GetAllWithDeleted(CancellationToken ct)
    {
        return await this.mediator.Send(new GetAllCitiesWithDeletedQuery(), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("WithDeleted/{id:guid}")]
    [ProducesResponseType<GetCityWithDeletedOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCityWithDeletedOkResponseDto>> GetByIdWithDeleted(Guid id, CancellationToken ct)
    {
        return await this.mediator.Send(new GetCityByIdWithDeletedQuery(id), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("Create")]
    [ProducesResponseType<GetCityOkResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetCityOkResponseDto>> Create([FromBody] CreateCityDto dto, CancellationToken ct)
    {
        var city = await this.mediator.Send(new CreateCityCommand(dto), ct);
        return this.StatusCode(StatusCodes.Status201Created, city);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("Update/{id:guid}")]
    [ProducesResponseType<GetCityOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetCityOkResponseDto>> Update(Guid id, [FromBody] UpdateCityDto dto, CancellationToken ct)
    {
        return await this.mediator.Send(new UpdateCityCommand(id, dto), ct);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("Delete/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await this.mediator.Send(new DeleteCityCommand(id), ct);
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
        await this.mediator.Send(new RestoreCityCommand(id), ct);
        return this.NoContent();
    }
}
