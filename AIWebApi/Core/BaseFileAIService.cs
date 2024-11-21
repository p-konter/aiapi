namespace AIWebApi.Core;

public class BaseFileAIService(IFileService fileService)
{
    protected readonly string TempDataExtension = ".log";
    protected readonly IFileService _fileService = fileService;

    public void SetFolder(string folder) => _fileService.SetFolder(folder);

    public void SetFolder(List<string> folder) => _fileService.SetFolder(folder);

    protected async Task<string?> LoadProcessedData(string fileName)
    {
        string textFileName = _fileService.ChangeExtension(fileName, TempDataExtension);
        return await _fileService.ReadTextFile(textFileName);
    }

    protected async Task<BinaryData> ReadBinaryFile(string fileName) => await _fileService.ReadBinaryFile(fileName);

    protected string ReturnFilePath(string fileName) => _fileService.CheckFileExists(fileName);

    protected async Task SaveProcessedData(string fileName, string content)
    {
        string textFileName = _fileService.ChangeExtension(fileName, TempDataExtension);
        await _fileService.WriteTextFile(textFileName, content);
    }
}
