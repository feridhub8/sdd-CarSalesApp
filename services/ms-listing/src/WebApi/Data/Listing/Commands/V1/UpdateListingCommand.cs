using MediatR;
using WebApi.Data.Listing.Commands.V1.Base;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Dtos.V1.Base;

namespace WebApi.Data.Listing.Commands.V1;

public sealed record UpdateListingCommand(Guid Id, UpdateListingDto Dto) : IRequest<GetListingOkResponseDto>, IListingFieldsCommand
{
    IListingFieldsDto IListingFieldsCommand.Dto => this.Dto;
}
