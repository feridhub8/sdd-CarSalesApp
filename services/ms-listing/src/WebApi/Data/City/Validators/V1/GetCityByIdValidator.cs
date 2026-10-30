using FluentValidation;
using WebApi.Data.City.Queries.V1;

namespace WebApi.Data.City.Validators.V1;

public sealed class GetCityByIdValidator : AbstractValidator<GetCityByIdQuery>
{
    public GetCityByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
