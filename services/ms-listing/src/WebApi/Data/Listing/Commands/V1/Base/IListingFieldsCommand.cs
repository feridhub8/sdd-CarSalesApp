using WebApi.Data.Listing.Dtos.V1.Base;

namespace WebApi.Data.Listing.Commands.V1.Base;

public interface IListingFieldsCommand
{
    IListingFieldsDto Dto { get; }
}
