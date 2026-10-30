using MediatR;

namespace WebApi.Data.Model.Commands.V1;

public sealed record RestoreModelCommand(Guid Id) : IRequest;
