using FluentValidation;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class GetMyListingsValidator : AbstractValidator<GetMyListingsQuery>
{
    public GetMyListingsValidator()
    {
        RuleFor(query => query.Page).PageNumber();
        RuleFor(query => query.PageSize).PageSize();
    }
}
