using AIWebApi.Core;

namespace AIWebApi.Tasks._17_FineTuning;

public interface IFineTuningController
{
    Task<string> PrepareData();

    Task<ResponseDto> ValidateData();
}

public class FineTuningController(
    IConfiguration configuration,
    IFileService fileService,
    IHttpService httpService,
    IJsonService jsonService,
    IKernelService kernelService) : BaseController(configuration, httpService), IFineTuningController
{
    private readonly IFileService _fileService = fileService;
    private readonly IJsonService _jsonService = jsonService;
    private readonly IKernelService _kernelService = kernelService;

    private const string TaskName = "Validate data";
    private const string FileName = "training_data.jsonl";

    public async Task<string> PrepareData()
    {
        _fileService.SetFolder("ExternalData");
        List<FineTuning> messages = [];

        messages = await ProcessFile(messages, "correct.txt", Category.Valid);
        messages = await ProcessFile(messages, "incorrect.txt", Category.Invalid);

        await WriteFile(messages);
        return FileName;
    }

    private async Task<List<FineTuning>> ProcessFile(List<FineTuning> fineTuning, string fileName, string category)
    {
        string file = await _fileService.ReadTextFile(fileName) ?? throw new Exception("Wrong file");
        List<string> lines = [.. file.Split("\n", StringSplitOptions.RemoveEmptyEntries)];
        foreach (string line in lines)
        {
            List<FineTuningMessage> messages = [new(TuningRole.System, TaskName), new(TuningRole.User, line), new(TuningRole.Assistant, category)];
            fineTuning.Add(new FineTuning(messages));
        }
        return fineTuning;
    }

    private async Task WriteFile(List<FineTuning> messages)
    {
        string filePath = "ExternalData" + Path.DirectorySeparatorChar + FileName;
        using StreamWriter writer = new(filePath);
        foreach (FineTuning data in messages)
        {
            string jsonLine = _jsonService.Serialize(data);
            await writer.WriteLineAsync(jsonLine);
        }
    }

    public async Task<ResponseDto> ValidateData()
    {
        List<DataDto> data = await ReadFile();
        List<string> answer = await Validate(data);
        return await SendAnswer("research", "Report", answer);
        //return new ResponseDto(0, "Ok");
    }

    private async Task<List<DataDto>> ReadFile()
    {
        _fileService.SetFolder("ExternalData");
        string file = await _fileService.ReadTextFile("verify.txt") ?? throw new Exception("Wrong file");
        return file
            .Split("\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split("="))
            .Select(parts => new DataDto(parts[0], parts[1]))
            .ToList();
    }

    private async Task<List<string>> Validate(List<DataDto> data)
    {
        List<string> response = [];
        foreach (DataDto item in data)
        {
            List<MessageDto> messages = [new(Role.System, TaskName), new(Role.User, item.Content)];
            MessageDto reply = await _kernelService.Chat(AIModel.FineTuning, messages);
            _kernelService.ClearHistory();
            if (reply.Message == Category.Valid)
            {
                response.Add(item.Id);
            }
        }
        return response;
    }
}

public static class Category
{
    public const string Valid = "valid";
    public const string Invalid = "invalid";
}

public static class TuningRole
{
    public const string System = "system";
    public const string User = "user";
    public const string Assistant = "assistant";
}

public record FineTuning(List<FineTuningMessage> Messages);

public record FineTuningMessage(string Role, string Content);

public record DataDto(string Id, string Content);
