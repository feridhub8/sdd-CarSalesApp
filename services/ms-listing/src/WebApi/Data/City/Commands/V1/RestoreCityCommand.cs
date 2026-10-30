using MediatR;

namespace WebApi.Data.City.Commands.V1;

public sealed record RestoreCityCommand(Guid Id) : IRequest;
