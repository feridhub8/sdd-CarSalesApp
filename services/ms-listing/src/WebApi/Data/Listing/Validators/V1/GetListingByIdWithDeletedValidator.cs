using FluentValidation;
using WebApi.Data.Listing.Queries.V1;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class GetListingByIdWithDeletedValidator : AbstractValidator<GetListingByIdWithDeletedQuery>
{
    public GetListingByIdWithDeletedValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
