namespace AIWebApi.Core;

public interface IFileService
{
    string ChangeExtension(string fileName, string extension);

    string CheckFileExists(string fileName);

    Task<BinaryData> ReadBinaryFile(string fileName);

    Task<string?> ReadTextFile(string fileName);

    FileStream ReadStream(string fileName);

    Task WriteTextFile(string fileName, string content);
}

public class FileService : IFileService
{
    private const string DataFolder = "ExternalData";

    private static string SetFilePath(string fileName)
    {
        return string.IsNullOrEmpty(fileName)
            ? throw new ArgumentException("File path cannot be null or empty", nameof(fileName))
            : Path.Combine(DataFolder, fileName);
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

    public string CheckFileExists(string fileName)
    {
        string filePath = SetFilePath(fileName);
        return !File.Exists(filePath) ? throw new FileNotFoundException("File not found", fileName) : filePath;
    }

    public string ChangeExtension(string fileName, string extension) => Path.ChangeExtension(fileName, extension);
}
