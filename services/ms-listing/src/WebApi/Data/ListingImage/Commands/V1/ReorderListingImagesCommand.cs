using MediatR;
using WebApi.Data.ListingImage.Dtos.V1;

namespace WebApi.Data.ListingImage.Commands.V1;

public sealed record ReorderListingImagesCommand(Guid ListingId, ReorderListingImagesDto Dto) : IRequest<List<GetListingImageOkResponseDto>>;
