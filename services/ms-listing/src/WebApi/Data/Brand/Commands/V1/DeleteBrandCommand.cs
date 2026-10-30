using MediatR;

namespace WebApi.Data.Brand.Commands.V1;

public sealed record DeleteBrandCommand(Guid Id) : IRequest;
