﻿using System.ComponentModel;

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
    IGPT4AIService chatService,
    IHttpService httpService,
    ILogger<SortFilesController> logger,
    IZipService zipService) : ISortFilesController
{
    public const string FileName = "pliki_z_fabryki.zip";
    private readonly Uri PostDataUrl = new("https://centrala.ag3nts.org/report");
    private readonly string ApiKey = configuration.GetStrictValue<string>("ApiKey");

    private readonly IAudioAIService _audioAIService = audioAIService;
    private readonly IGPT4AIService _chatService = chatService;
    private readonly IHttpService _httpService = httpService;
    private readonly ILogger<SortFilesController> _logger = logger;
    private readonly IZipService _zipService = zipService;

    public async Task<ResponseDto> RunSortFiles()
    {
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
        _zipService.ClearDataFolder();
        return Task.FromResult(true);
    }

    private async Task<List<FileDto>> ReadFiles()
    {
        if (!_zipService.CheckDataFolderExists())
        {
            _zipService.UnzipFile(FileName);
        }

        IEnumerable<string> files = _zipService.GetFileNames();
        List<FileDto> fileData = [];
        foreach (string file in files)
        {
            string description;
            switch (_zipService.GetFileType(file))
            {
                case "txt":
                    description = await _zipService.ReadTextFile(file) ?? throw new Exception();
                    break;
                case "mp3":
                    description = await TranscriptAudioFile(file);
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
        string textFileName = _zipService.ChangeExtension(fileName, ".log");
        string? textFile = await _zipService.ReadTextFile(textFileName);
        if (textFile is not null)
        {
            return textFile;
        }

        BinaryData map = await _zipService.ReadBinaryFile(fileName);
        string prompt = "Read and write text from image.";
        MessageDto message = new(Role.User, prompt, [new ImageDto(map, ImageType.Png)]);
        MessageDto response = await _chatService.ThreadChat([message]);

        await _zipService.WriteTextFile(textFileName, response.Message);
        return response.Message;
    }

    private async Task<string> TranscriptAudioFile(string fileName)
    {
        string textFileName = _zipService.ChangeExtension(fileName, ".log");
        string? textFile = await _zipService.ReadTextFile(textFileName);
        if (textFile is not null)
        {
            return textFile;
        }

        string filePath = _zipService.CheckFileExists(fileName);
        string transcription = await _audioAIService.AudioTranscription(filePath);

        await _zipService.WriteTextFile(textFileName, transcription);
        return transcription;
    }

    private async Task<FileTypeDto> GetInformation(FileDto file)
    {
        string prompt = """
        <objective>
        You are an assistant. Your job is to sort information.
        </objective>
        
        <rules>
        - Read user message.
        - *Thinkink*. Decide the message contains information about people, machines, or something else.
        - Don't write people when the message contains pineapple pizza
        - People have names and fingerprints.
        - Abandoned cities have no people.
        - Algorithms or AI or communication systems or QII or temperature scanners are not machines.
        - Write one of three words: people, machines, others.
        </rules>

        <examples>
        User: He only eats vegetarian food
        Assistant: people

        User: The screws are loose
        Assistant: machines

        User: Today is a sunny day
        Assistant: others
        </examples>
        """;
        List<MessageDto> messages = [new(Role.System, prompt), new(Role.User, file.Description)];
        MessageDto response = await _chatService.ThreadChat(messages);

        _logger.LogInformation("Categorization: {category} from file: {file} and message: {message}", response.Message, file.FileName, file.Description);
        return new FileTypeDto(file.FileName, response.Message.Trim().ToLower().CreateByDescription<FileType>());
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

public record FileDto(string FileName, string Description);

public record FileTypeDto(string FileName, FileType Type);

public record SortFilesDto(IList<string> People, IList<string> Hardware);

public record SortFilesRequestDto(string Task, string Apikey, SortFilesDto Answer);

public enum FileType
{
    [Description("people")]
    People,
    [Description("machines")]
    Machines,
    [Description("others")]
    Others
};
