using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3BlobUploader
{
    public interface IArtOptimizer
    {
        Task OptimizeImage(Stream inputStream, Stream outputStream, int quality = 75, int? width = null, int? height = null);
    }
    public class SixLaborImageSharpOptimizer : IArtOptimizer
    {
        public async Task OptimizeImage(Stream inputStream, Stream outputStream, int quality = 75, int? width = null, int? height = null)
        {
            using (var image = await Image.LoadAsync(inputStream))
            {
                var webpEncoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder() { Quality = quality };
                var newWidth = width ?? image.Width;
                var newHeight = height ?? image.Height;
                if (newWidth != image.Width || newHeight != image.Height)
                {
                    image.Mutate(x => x.Resize(newWidth, newHeight, LanczosResampler.Lanczos3));
                }

                //todo: the source file that was uploaded to s3 should include its filename without extension.
                //ie: original/canvas_mockup.png
                await image.SaveAsync(outputStream, webpEncoder);
            }
        }
    }
}
