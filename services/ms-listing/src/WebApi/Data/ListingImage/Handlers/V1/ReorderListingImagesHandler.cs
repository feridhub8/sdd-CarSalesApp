using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.ListingImage.Commands.V1;
using WebApi.Data.ListingImage.Dtos.V1;
using WebApi.Data.ListingImage.Services.V1.Base;

namespace WebApi.Data.ListingImage.Handlers.V1;

public sealed class ReorderListingImagesHandler : IRequestHandler<ReorderListingImagesCommand, List<GetListingImageOkResponseDto>>
{
    private readonly IValidator<ReorderListingImagesCommand> validator;
    private readonly IMapper mapper;
    private readonly IListingImageService listingImageService;

    public ReorderListingImagesHandler(IValidator<ReorderListingImagesCommand> validator, IMapper mapper, IListingImageService listingImageService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingImageService = listingImageService;
    }

    public async Task<List<GetListingImageOkResponseDto>> Handle(ReorderListingImagesCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var images = await this.listingImageService.ReorderAsync(request.ListingId, request.Dto.ImageIds!, ct);

        return this.mapper.Map<List<GetListingImageOkResponseDto>>(images);
    }
}
