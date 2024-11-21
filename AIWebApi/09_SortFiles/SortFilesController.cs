using AIWebApi.Core;

namespace AIWebApi._09_SortFiles;

public interface ISortFilesController
{
    Task<ResponseDto> RunSortFiles();

    Task<bool> ClearSortFiles();
}

public class SortFilesController(
    IAudioAIService audioAIService,
    IConfiguration configuration,
    IFileService fileService,
    IGPT4AIService chatService,
    IHttpService httpService,
    IJsonService jsonService,
    ILogger<SortFilesController> logger) : ISortFilesController
{
    private readonly string DataPath = "ExternalData";
    private readonly string WorkPath = "WorkData";
    public const string FileName = "pliki_z_fabryki.zip";
    private readonly Uri PostDataUrl = new("https://centrala.ag3nts.org/report");
    private readonly string ApiKey = configuration.GetStrictValue<string>("ApiKey");

    private readonly IAudioAIService _audioAIService = audioAIService;
    private readonly IFileService _fileService = fileService;
    private readonly IGPT4AIService _chatService = chatService;
    private readonly IHttpService _httpService = httpService;
    private readonly IJsonService _jsonService = jsonService;
    private readonly ILogger<SortFilesController> _logger = logger;

    public async Task<ResponseDto> RunSortFiles()
    {
        UnzipData();
        List<FileDto> files = await ReadFiles();

        List<FileTypeDto> fileTypes = [];
        foreach (FileDto file in files)
        {
            fileTypes.Add(await GetInformation(file));
        }

        return await SendResponse(fileTypes);
    }

    public Task<bool> ClearSortFiles()
    {
        _fileService.SetFolder([DataPath, WorkPath]);
        _fileService.ClearDataFolder();
        return Task.FromResult(true);
    }

    private void UnzipData()
    {
        _fileService.SetFolder(DataPath);
        _fileService.UnzipFileToFolder(FileName, WorkPath);
    }

    private async Task<List<FileDto>> ReadFiles()
    {
        _fileService.SetFolder([DataPath, WorkPath]);
        IEnumerable<string> files = _fileService.GetFileNames();
        List<FileDto> fileData = [];
        foreach (string file in files)
        {
            string description;
            switch (_fileService.GetFileType(file))
            {
                case "txt":
                    description = await _fileService.ReadTextFile(file) ?? throw new Exception();
                    break;
                case "mp3":
                    description = await _audioAIService.AudioTranscription(file); ;
                    break;
                case "png":
                    description = await RecognizeImage(file);
                    break;
                default:
                    continue;
            }
            fileData.Add(new FileDto(file, description!));
        }
        return fileData;
    }

    private async Task<string> RecognizeImage(string fileName)
    {
        string prompt = "Read and write text from image.";
        MessageDto response = await _chatService.ReadImageChat(fileName, prompt);
        return response.Message;
    }

    private async Task<FileTypeDto> GetInformation(FileDto file)
    {
        string prompt = Prompts.SortFilesPrompt();
        List<MessageDto> messages = [new(Role.System, prompt), new(Role.User, file.Description)];
        MessageDto response = await _chatService.JsonChat(messages);

        _logger.LogInformation("Categorization source: {file} and message: {message}", file.FileName, file.Description);
        _logger.LogInformation("Categorization output: {output}", response.Message);
        OutputMessageDto outputMessage = _jsonService.Deserialize<OutputMessageDto>(response.Message);

        return new FileTypeDto(file.FileName, outputMessage.Category.CreateByDescription<FileType>());
    }

    private async Task<ResponseDto> SendResponse(List<FileTypeDto> files)
    {
        List<string> people = [];
        List<string> hardware = [];

        foreach (FileTypeDto file in files)
        {
            switch (file.Type)
            {
                case FileType.People:
                    people.Add(file.FileName);
                    break;
                case FileType.Machines:
                    hardware.Add(file.FileName);
                    break;
                default:
                    break;
            }
        }

        _logger.LogInformation("People files: {peopleFiles}", string.Join(", ", people));
        _logger.LogInformation("Hardware files: {hardwareFiles}", string.Join(", ", hardware));

        SortFilesRequestDto request = new("kategorie", ApiKey, new SortFilesDto(people, hardware));
        return await _httpService.PostJson<ResponseDto>(PostDataUrl, request);
    }
}
