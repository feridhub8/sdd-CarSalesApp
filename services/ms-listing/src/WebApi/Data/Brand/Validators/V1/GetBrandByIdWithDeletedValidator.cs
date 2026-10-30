using FluentValidation;
using WebApi.Data.Brand.Queries.V1;

namespace WebApi.Data.Brand.Validators.V1;

public sealed class GetBrandByIdWithDeletedValidator : AbstractValidator<GetBrandByIdWithDeletedQuery>
{
    public GetBrandByIdWithDeletedValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
