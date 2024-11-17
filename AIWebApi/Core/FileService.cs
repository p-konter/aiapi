namespace AIWebApi.Core;

public interface IFileService
{
    string ChangeExtension(string fileName, string extension);

    string CheckFileExists(string fileName);

    bool CheckDataFolderExists();

    Task<BinaryData> ReadBinaryFile(string fileName);

    Task<string?> ReadTextFile(string fileName);

    FileStream ReadStream(string fileName);

    Task WriteTextFile(string fileName, string content);

    IEnumerable<string> GetFileNames();

    string GetFileType(string fileName);
}

public class FileService : IFileService
{
    protected virtual string DataFolder { get; } = "ExternalData";

    private string SetFilePath(string fileName)
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

    public bool CheckDataFolderExists() => Directory.Exists(DataFolder);

    public string ChangeExtension(string fileName, string extension) => Path.ChangeExtension(fileName, extension);

    public IEnumerable<string> GetFileNames()
    {
        foreach (string file in Directory.EnumerateFiles(DataFolder))
        {
            yield return Path.GetFileName(file);
        }
    }

    public string GetFileType(string fileName) => Path.GetExtension(fileName).TrimStart('.').ToLower();
}
