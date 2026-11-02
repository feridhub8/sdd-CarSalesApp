using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.ListingImage.Commands.V1;
using WebApi.Data.ListingImage.Dtos.V1;

namespace WebApi.Data.ListingImage.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ListingImageController : ControllerBase
{
    private readonly IMediator mediator;

    public ListingImageController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [Authorize]
    [HttpPost("Create/{listingId:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<GetListingImageOkResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<GetListingImageOkResponseDto>> Create(Guid listingId, [FromForm] CreateListingImageDto dto, CancellationToken ct)
    {
        var image = await this.mediator.Send(new CreateListingImageCommand(listingId, dto), ct);
        return this.StatusCode(StatusCodes.Status201Created, image);
    }

    [Authorize]
    [HttpPut("Reorder/{listingId:guid}")]
    [ProducesResponseType<List<GetListingImageOkResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GetListingImageOkResponseDto>>> Reorder(Guid listingId, [FromBody] ReorderListingImagesDto dto, CancellationToken ct)
    {
        return await this.mediator.Send(new ReorderListingImagesCommand(listingId, dto), ct);
    }

    [Authorize]
    [HttpDelete("Delete/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await this.mediator.Send(new DeleteListingImageCommand(id), ct);
        return this.NoContent();
    }
}
