using MediatR;

namespace WebApi.Data.ListingImage.Commands.V1;

public sealed record DeleteListingImageCommand(Guid Id) : IRequest;
