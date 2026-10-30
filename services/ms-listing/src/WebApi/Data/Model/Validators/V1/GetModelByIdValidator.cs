using FluentValidation;
using WebApi.Data.Model.Queries.V1;

namespace WebApi.Data.Model.Validators.V1;

public sealed class GetModelByIdValidator : AbstractValidator<GetModelByIdQuery>
{
    public GetModelByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
