using MediatR;
using WebApi.Data.ListingImage.Dtos.V1;

namespace WebApi.Data.ListingImage.Commands.V1;

public sealed record CreateListingImageCommand(Guid ListingId, CreateListingImageDto Dto) : IRequest<GetListingImageOkResponseDto>;
