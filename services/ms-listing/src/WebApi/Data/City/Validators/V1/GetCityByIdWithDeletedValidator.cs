using FluentValidation;
using WebApi.Data.City.Queries.V1;

namespace WebApi.Data.City.Validators.V1;

public sealed class GetCityByIdWithDeletedValidator : AbstractValidator<GetCityByIdWithDeletedQuery>
{
    public GetCityByIdWithDeletedValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
