namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IImageRepository
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string imageUrl);
        
        bool ValidateImage(IFormFile file);
    }
}
