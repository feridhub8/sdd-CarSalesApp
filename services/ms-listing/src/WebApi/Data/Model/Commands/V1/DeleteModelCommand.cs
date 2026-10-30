using MediatR;

namespace WebApi.Data.Model.Commands.V1;

public sealed record DeleteModelCommand(Guid Id) : IRequest;
