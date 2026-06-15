using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace HungryHub.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ── Save restaurant logo ─────────────────
        // Output: 400x400 square crop (like app icons)
        public async Task<string> SaveRestaurantImageAsync(
            IFormFile file)
        {
            return await SaveImageAsync(
                file,
                "images/restaurants",
                400, 400,
                "rest_");
        }

        // ── Save food/menu item image ────────────
        // Output: 600x400 landscape (like food cards)
        public async Task<string> SaveMenuImageAsync(
            IFormFile file)
        {
            return await SaveImageAsync(
                file,
                "images/menu",
                600, 400,
                "food_");
        }

        // ── Core save + resize method ────────────
        private async Task<string> SaveImageAsync(
            IFormFile file,
            string folder,
            int width,
            int height,
            string prefix)
        {
            // Validate file
            if (file == null || file.Length == 0)
                return string.Empty;

            var ext = Path.GetExtension(
                file.FileName).ToLower();
            var allowed = new[]
            {
                ".jpg", ".jpeg", ".png",
                ".gif", ".webp"
            };
            if (!allowed.Contains(ext))
                return string.Empty;

            // Generate unique filename
            string fileName = prefix +
                Guid.NewGuid().ToString("N")
                    .Substring(0, 12) + ".jpg";

            string folderPath = Path.Combine(
                _env.WebRootPath, folder);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(
                folderPath, fileName);

            // Read, resize and save with ImageSharp
            using var stream = file.OpenReadStream();
            using var image =
                await Image.LoadAsync(stream);

            // Smart crop to fill exact dimensions
            // (same as object-fit: cover in CSS)
            image.Mutate(x => x
                .Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Crop,
                    Position =
                        AnchorPositionMode.Center
                }));

            // Save as high quality JPEG
            await image.SaveAsJpegAsync(
                filePath,
                new JpegEncoder { Quality = 88 });

            // Return the web-accessible path
            return "/" + folder.Replace("\\", "/")
                + "/" + fileName;
        }

        // ── Delete old image file ────────────────
        public void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string fullPath = Path.Combine(
                _env.WebRootPath,
                imagePath.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
