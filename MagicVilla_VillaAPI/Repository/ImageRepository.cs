using MagicVilla_VillaAPI.Repository.IRepository;

namespace MagicVilla_VillaAPI.Repository
{
    public class ImageRepository : IImageRepository
    {
        private const long MaxFileSize = 5 * 1024 * 1024; 
        private readonly string[] AlloweExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ImageRepository(IWebHostEnvironment webHostEnvironment)
        {

            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return false;
                }
                var filName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "villas", filName);
                if (File.Exists(filePath))
                {

                   await Task.Run(()=> File.Delete(filePath));
                    return true;


                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return false;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (!ValidateImage(file))
                {
                   
                    throw new InvalidOperationException("Invalid image file.");
                }
                var UploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "villas");
                if (!Directory.Exists(UploadFolder))
                {
                    Directory.CreateDirectory(UploadFolder);
                }
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(UploadFolder, uniqueFileName);

                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(filestream);
                }

                return $"/images/villas/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }
            if (file.Length > MaxFileSize)
            {
                return false;
            }
            var externsion = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AlloweExtensions.Contains(externsion))
            {
                return false;
            }
            return true;
        }
    }
}
