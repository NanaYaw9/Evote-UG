namespace EVoteUG.Infrastructure.Storage;

public class LocalFileStorageService
{
    private readonly string _baseUploadPath;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    public LocalFileStorageService(string baseUploadPath = "wwwroot/uploads")
    {
        _baseUploadPath = baseUploadPath;
        if (!Directory.Exists(_baseUploadPath))
        {
            Directory.CreateDirectory(_baseUploadPath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string subFolder = "")
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream cannot be empty.", nameof(fileStream));

        if (fileStream.Length > MaxFileSizeInBytes)
            throw new InvalidOperationException($"File size exceeds maximum limit of {MaxFileSizeInBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", _allowedExtensions)}");

        var targetFolder = string.IsNullOrWhiteSpace(subFolder)
            ? _baseUploadPath
            : Path.Combine(_baseUploadPath, subFolder);

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetFolder, uniqueFileName);

        using var outputStream = new FileStream(physicalPath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        var relativeUrl = string.IsNullOrWhiteSpace(subFolder)
            ? $"/uploads/{uniqueFileName}"
            : $"/uploads/{subFolder.Replace('\\', '/')}/{uniqueFileName}";

        return relativeUrl;
    }

    public bool DeleteFile(string relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return false;

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine("wwwroot", relativePath);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
            return true;
        }

        return false;
    }
}
