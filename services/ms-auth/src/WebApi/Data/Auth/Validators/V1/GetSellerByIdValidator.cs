using FluentValidation;
using WebApi.Data.Auth.Queries.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class GetSellerByIdValidator : AbstractValidator<GetSellerByIdQuery>
{
    public GetSellerByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
