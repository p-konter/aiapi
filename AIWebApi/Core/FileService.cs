using Aspose.Zip;

namespace AIWebApi.Core;

public interface IFileService
{
    string GetFolder();

    void SetFolder(string path);

    void SetFolder(IList<string> paths);

    void CreateFolder();

    string ChangeExtension(string fileName, string extension);

    string CheckFileExists(string fileName);

    Task<BinaryData> ReadBinaryFile(string fileName);

    Task<string?> ReadTextFile(string fileName);

    FileStream ReadStream(string fileName);

    Task WriteTextFile(string fileName, string content);

    Task WriteBinaryFile(string fileName, byte[] content);

    IEnumerable<string> GetFileNames();

    string GetFileType(string fileName);

    string GetFileName(string fileName);

    void DeleteFile(string fileName);

    void ClearDataFolder();

    void UnzipFileToFolder(string fileName, string unzipPath, string? password = null);
}

public class FileService : IFileService
{
    public string Folder { get; set; } = string.Empty;

    public string GetFolder() => Folder;

    public void SetFolder(string path) => Folder = path;

    public void SetFolder(IList<string> paths) => Folder = string.Join(Path.DirectorySeparatorChar, paths);

    public void CreateFolder() => Directory.CreateDirectory(Folder);

    private string SetFilePath(string fileName)
    {
        return string.IsNullOrEmpty(fileName)
            ? throw new ArgumentException("File path cannot be null or empty", nameof(fileName))
            : Path.Combine(Folder, fileName);
    }

    public async Task<string?> ReadTextFile(string fileName)
    {
        string filePath = SetFilePath(fileName);
        return File.Exists(filePath) ? await File.ReadAllTextAsync(filePath) : null;
    }

    public async Task<BinaryData> ReadBinaryFile(string fileName)
    {
        string filePath = CheckFileExists(fileName);
        Stream imageStream = File.OpenRead(filePath);
        return await BinaryData.FromStreamAsync(imageStream);
    }

    public FileStream ReadStream(string fileName)
    {
        string filePath = CheckFileExists(fileName);
        FileStream fileStream = File.OpenRead(filePath);
        return fileStream;
    }

    public async Task WriteTextFile(string fileName, string content)
    {
        string filePath = SetFilePath(fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task WriteBinaryFile(string fileName, byte[] content)
    {
        string filePath = SetFilePath(fileName);
        if (!File.Exists(filePath))
        {
            await File.WriteAllBytesAsync(filePath, content);
        }
    }

    public string CheckFileExists(string fileName)
    {
        string filePath = SetFilePath(fileName);
        return !File.Exists(filePath) ? throw new FileNotFoundException("File not found", fileName) : filePath;
    }

    public string ChangeExtension(string fileName, string extension) => Path.ChangeExtension(fileName, extension);

    public IEnumerable<string> GetFileNames()
    {
        foreach (string file in Directory.EnumerateFiles(Folder))
        {
            yield return Path.GetFileName(file);
        }
    }

    public string GetFileType(string fileName) => Path.GetExtension(fileName).TrimStart('.').ToLower();

    public string GetFileName(string fileName) => Path.GetFileNameWithoutExtension(fileName);

    public void UnzipFileToFolder(string fileName, string unzipPath, string? password = null)
    {
        string outputDirectory = Path.Combine(Folder, unzipPath);
        if (!Directory.Exists(outputDirectory))
        {
            string zipFilePath = SetFilePath(fileName);

            ArchiveLoadOptions options = new();
            if (password is not null)
            {
                options.DecryptionPassword = password;
            }

            using Archive archive = new(zipFilePath, options);
            archive.ExtractToDirectory(outputDirectory);
        }
    }

    public void DeleteFile(string fileName)
    {
        string filePath = SetFilePath(fileName);
        File.Delete(filePath);
    }

    public void ClearDataFolder()
    {
        DirectoryInfo directory = new(Folder);
        foreach (FileInfo file in directory.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in directory.GetDirectories())
        {
            dir.Delete(true);
        }

        directory.Delete();
    }
}
