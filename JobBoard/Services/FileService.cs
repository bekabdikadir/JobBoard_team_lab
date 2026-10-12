namespace JobBoard.Services;

public interface IFileService
{
    bool IsValidImage(IFormFile file);
    Task<string> SaveAvatarAsync(IFormFile file);
    void Delete(string? relativePath);
}

public class FileService : IFileService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxSize = 3 * 1024 * 1024;
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool IsValidImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return file.Length > 0 && file.Length <= MaxSize && AllowedExtensions.Contains(ext);
    }

    public async Task<string> SaveAvatarAsync(IFormFile file)
    {
        var directory = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(directory);
        var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        var fullPath = Path.Combine(directory, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return "/uploads/avatars/" + fileName;
    }

    public void Delete(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath) || !relativePath.StartsWith("/uploads/"))
        {
            return;
        }
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
