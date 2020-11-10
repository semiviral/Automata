using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Automata.Engine.Extensions
{
    public static class ImageSharpExtensions
    {
        public static Span<TPixel> GetPixelSpan<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (image.TryGetSinglePixelSpan(out Span<TPixel> pixels)) return pixels;
            else throw new Exception("Failed to get image data.");
        }
    }
}
