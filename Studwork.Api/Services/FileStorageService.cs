namespace Studwork.Api.Services;

public class FileStorageService(IWebHostEnvironment env, IConfiguration configuration)
{
    private readonly string _uploadPath = Path.Combine(env.ContentRootPath, "uploads");
    private readonly long _maxFileSize = configuration.GetValue<long>("FileStorage:MaxSizeBytes", 10 * 1024 * 1024);
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar"];

    public async Task<(string storedName, string contentType, long size)> SaveAsync(IFormFile file)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("Файл пустой");

        if (file.Length > _maxFileSize)
            throw new InvalidOperationException($"Максимальный размер файла: {_maxFileSize / 1024 / 1024} МБ");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Недопустимый тип файла");

        Directory.CreateDirectory(_uploadPath);

        var storedName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_uploadPath, storedName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return (storedName, file.ContentType, file.Length);
    }

    public string GetUrl(string storedName) => $"/uploads/{storedName}";

    public (string fullPath, string contentType, string fileName)? GetFile(string storedName)
    {
        var fullPath = Path.Combine(_uploadPath, storedName);
        if (!File.Exists(fullPath)) return null;
        return (fullPath, "application/octet-stream", storedName);
    }

    public bool IsImage(string contentType) =>
        contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
