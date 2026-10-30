using FluentValidation;
using WebApi.Data.City.Commands.V1;

namespace WebApi.Data.City.Validators.V1;

public sealed class RestoreCityValidator : AbstractValidator<RestoreCityCommand>
{
    public RestoreCityValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
