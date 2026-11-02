using WebApi.Data.Brand.Services.V1;
using WebApi.Data.Brand.Services.V1.Base;
using WebApi.Data.City.Services.V1;
using WebApi.Data.City.Services.V1.Base;
using WebApi.Data.Listing.Services.V1;
using WebApi.Data.Listing.Services.V1.Base;
using WebApi.Data.ListingImage.Services.V1;
using WebApi.Data.ListingImage.Services.V1.Base;
using WebApi.Data.Model.Services.V1;
using WebApi.Data.Model.Services.V1.Base;
using WebApi.Data.Shared.Services.V1;
using WebApi.Data.Shared.Services.V1.Base;

namespace WebApi.Extensions.V1;

public static class ServiceExtensions
{
    public static void InitServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IModelService, ModelService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IListingImageService, ListingImageService>();

        // One instance, so its limit on images processed at a time holds across requests.
        services.AddSingleton<IImageProcessingService, ImageProcessingService>();
    }
}
