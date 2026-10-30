using MediatR;

namespace WebApi.Data.Listing.Commands.V1;

public sealed record DeleteListingCommand(Guid Id) : IRequest;
