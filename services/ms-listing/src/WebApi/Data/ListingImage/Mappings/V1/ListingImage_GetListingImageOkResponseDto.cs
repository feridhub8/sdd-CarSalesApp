using Mapster;
using WebApi.Data.ListingImage.Dtos.V1;
using WebApi.Data.Shared.Repositories.V1.Base;

namespace WebApi.Data.ListingImage.Mappings.V1;

public sealed class ListingImage_GetListingImageOkResponseDto : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Built on every read and never stored, so moving the files needs no data change.
        config.NewConfig<Entities.V1.ListingImage, GetListingImageOkResponseDto>()
            .Map(destination => destination.Url, source => MapContext.Current!.GetService<IFileRepository>().GetPublicPath(source.LargeKey))
            .Map(destination => destination.ThumbnailUrl, source => MapContext.Current!.GetService<IFileRepository>().GetPublicPath(source.ThumbnailKey));
    }
}
