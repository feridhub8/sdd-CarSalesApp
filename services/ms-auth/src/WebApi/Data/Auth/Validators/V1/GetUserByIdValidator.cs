using FluentValidation;
using WebApi.Data.Auth.Queries.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
