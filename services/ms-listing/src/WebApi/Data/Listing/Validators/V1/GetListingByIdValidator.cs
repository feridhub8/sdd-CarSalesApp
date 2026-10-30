using FluentValidation;
using WebApi.Data.Listing.Queries.V1;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class GetListingByIdValidator : AbstractValidator<GetListingByIdQuery>
{
    public GetListingByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
