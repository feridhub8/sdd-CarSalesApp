using FluentValidation;
using FluentValidation.Results;
using SkiaSharp;
using WebApi.Data.ListingImage.Models.V1;
using WebApi.Data.ListingImage.Services.V1.Base;

namespace WebApi.Data.ListingImage.Services.V1;

// Only the pixels of an upload are kept, which is what removes its metadata, such as a GPS location.
public sealed class ImageProcessingService : IImageProcessingService
{
    private const long MaxPixels = 50_000_000;
    private const int LargeMaxSide = 1600;
    private const int ThumbnailMaxSide = 400;
    private const int WebpQuality = 80;
    private const string FileField = "file";

    private static readonly SKSamplingOptions Sampling = new(SKCubicResampler.Mitchell);

    // A 50 MP image takes about 200 MB once decoded, so a replica processes only a few at a time.
    private readonly SemaphoreSlim slots = new(2);

    public string ContentType => "image/webp";

    public async Task<ProcessedImage> ProcessAsync(Stream file, CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);

        await this.slots.WaitAsync(ct);
        try
        {
            using var original = Decode(buffer.ToArray());
            return new ProcessedImage(Encode(original, LargeMaxSide), Encode(original, ThumbnailMaxSide));
        }
        finally
        {
            this.slots.Release();
        }
    }

    private static SKBitmap Decode(byte[] bytes)
    {
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data) ?? throw InvalidFile("The file is not an image.");

        if (codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
        {
            throw InvalidFile("The image must be JPEG, PNG, or WebP.");
        }

        // Checked from the header, before any memory is taken for the pixels.
        if ((long)codec.Info.Width * codec.Info.Height > MaxPixels)
        {
            throw InvalidFile($"The image must have at most {MaxPixels / 1_000_000} megapixels.");
        }

        // Converted to sRGB, since the stored files carry no color profile.
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        using var decoded = new SKBitmap(info);
        if (codec.GetPixels(info, decoded.GetPixels()) != SKCodecResult.Success)
        {
            throw InvalidFile("The image is damaged.");
        }

        return ApplyOrigin(decoded, codec.EncodedOrigin);
    }

    // The orientation metadata is dropped with the rest, so the pixels themselves are turned upright.
    private static SKBitmap ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
    {
        var swapsSides = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var width = swapsSides ? source.Height : source.Width;
        var height = swapsSides ? source.Width : source.Height;

        var upright = new SKBitmap(source.Info.WithSize(width, height));
        using var canvas = new SKCanvas(upright);

        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Translate(width, 0);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.Translate(width, height);
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, height);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.Translate(width, 0);
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.Translate(width, height);
                canvas.RotateDegrees(90);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, height);
                canvas.RotateDegrees(270);
                break;
        }

        canvas.DrawBitmap(source, 0, 0, SKSamplingOptions.Default);
        return upright;
    }

    private static byte[] Encode(SKBitmap original, int maxSide)
    {
        // Never enlarges: a small photo keeps its size.
        var scale = Math.Min(1d, (double)maxSide / Math.Max(original.Width, original.Height));
        var width = Math.Max(1, (int)Math.Round(original.Width * scale));
        var height = Math.Max(1, (int)Math.Round(original.Height * scale));

        using var resized = original.Resize(original.Info.WithSize(width, height), Sampling)
            ?? throw new InvalidOperationException($"Could not resize an image to {width}x{height}.");
        using var image = SKImage.FromBitmap(resized);
        using var encoded = image.Encode(SKEncodedImageFormat.Webp, WebpQuality)
            ?? throw new InvalidOperationException("Could not encode an image as WebP.");

        return encoded.ToArray();
    }

    private static ValidationException InvalidFile(string message)
    {
        return new ValidationException([new ValidationFailure(FileField, message)]);
    }
}
