using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.ListingImage.Commands.V1;
using WebApi.Data.ListingImage.Dtos.V1;
using WebApi.Data.ListingImage.Services.V1.Base;

namespace WebApi.Data.ListingImage.Handlers.V1;

public sealed class CreateListingImageHandler : IRequestHandler<CreateListingImageCommand, GetListingImageOkResponseDto>
{
    private readonly IValidator<CreateListingImageCommand> validator;
    private readonly IMapper mapper;
    private readonly IListingImageService listingImageService;

    public CreateListingImageHandler(IValidator<CreateListingImageCommand> validator, IMapper mapper, IListingImageService listingImageService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingImageService = listingImageService;
    }

    public async Task<GetListingImageOkResponseDto> Handle(CreateListingImageCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await using var file = request.Dto.File!.OpenReadStream();
        var created = await this.listingImageService.CreateAsync(request.ListingId, file, ct);

        return this.mapper.Map<GetListingImageOkResponseDto>(created);
    }
}
