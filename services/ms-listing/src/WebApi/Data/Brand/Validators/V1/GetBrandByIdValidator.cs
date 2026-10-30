using FluentValidation;
using WebApi.Data.Brand.Queries.V1;

namespace WebApi.Data.Brand.Validators.V1;

public sealed class GetBrandByIdValidator : AbstractValidator<GetBrandByIdQuery>
{
    public GetBrandByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
