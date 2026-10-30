using FluentValidation;
using WebApi.Data.Model.Queries.V1;

namespace WebApi.Data.Model.Validators.V1;

public sealed class GetModelByIdWithDeletedValidator : AbstractValidator<GetModelByIdWithDeletedQuery>
{
    public GetModelByIdWithDeletedValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
