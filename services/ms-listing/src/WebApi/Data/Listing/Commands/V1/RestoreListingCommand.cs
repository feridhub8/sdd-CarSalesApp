using MediatR;

namespace WebApi.Data.Listing.Commands.V1;

public sealed record RestoreListingCommand(Guid Id) : IRequest;
