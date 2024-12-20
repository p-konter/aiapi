﻿using AIWebApi.Core;

namespace AIWebApi.Tasks._11_GenerateKeywords;

public interface IGenerateKeywordsController
{
    Task<ResponseDto> RunGenerateKeywords();
}

public class GenerateKeywordsController(
    IConfiguration configuration,
    IFileService fileService,
    IHttpService httpService,
    IJsonService jsonService,
    IKernelService kernelService,
    ILogger<GenerateKeywordsController> logger)
    : BaseController(configuration, httpService), IGenerateKeywordsController
{
    private readonly IFileService _fileService = fileService;
    private readonly IJsonService _jsonService = jsonService;
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<GenerateKeywordsController> _logger = logger;

    private readonly string DataPath = "ExternalData";
    private readonly string FactsPath = "facts";
    private readonly string WorkPath = "WorkData";
    public const string FileName = "pliki_z_fabryki.zip";

    public async Task<ResponseDto> RunGenerateKeywords()
    {
        UnzipData();
        List<FactsDto> facts = await GetFactsData();
        List<FileDto> files = await ReadFiles(facts);
        Dictionary<string, string> answer = PrepareAnswer(files);

        return await SendAnswer("dokumenty", "Report", answer);
    }

    private void UnzipData()
    {
        _fileService.SetFolder(DataPath);
        _fileService.UnzipFileToFolder(FileName, WorkPath);
    }

    private async Task<List<FactsDto>> GetFactsData()
    {
        _fileService.SetFolder([DataPath, WorkPath, FactsPath]);
        IEnumerable<string> files = _fileService.GetFileNames();
        List<FactsDto> factsData = [];
        foreach (string file in files)
        {
            string? text = await _fileService.ReadTextFile(file);
            if (text is null or "entry deleted\n")
            {
                continue;
            }

            string system = """
            <objective>
            You are an assistant copy editor. Write a short summary for the submitted document. Pay attention to names, professions and keywords.
            </objective>

            <rules>
            - Write a reply in Json format
            - In the NAME field, type the name of the character from the document
            - In the SUMMARY field, write keywords: focus on position, profession (eg. teacher, programmer), skills (e.g. programming languages)
            </rules>

            <output_format>
            {
                "name": "type name here",
                "summary": "type summary here"
            }
            </output_format>
            """;
            MessageDto response = await _kernelService.ProcessTextFile(AIModel.Gpt4o, file, [new(Role.System, system)], returnJson: true);
            _kernelService.ClearHistory();
            _logger.LogInformation("Facts summary: file: {file}, message: {message}", file, response.Message);
            FactsDto factsDto = _jsonService.Deserialize<FactsDto>(response.Message);
            factsData.Add(factsDto);
        }
        return factsData;
    }

    private async Task<string?> ExtractName(string document)
    {
        string prompt = """
        <objective>
        You are a detective. Check if the person's name appears in the document.
        </objective>

        <rules>
        - If you find a name, write it
        - If there is no name, write "No"
        - Don't write anything else
        </rules>
        """;
        MessageDto request = new(Role.User, $"{prompt}\n<document>\n{document}\n</document>");
        MessageDto response = await _kernelService.Chat(AIModel.Gpt4oMini, [request]);
        _kernelService.ClearHistory();
        return response.Message != "No" ? response.Message : null;
    }

    private async Task<List<FileDto>> ReadFiles(List<FactsDto> facts)
    {
        _fileService.SetFolder([DataPath, WorkPath]);
        IEnumerable<string> files = _fileService.GetFileNames();
        List<FileDto> fileData = [];
        foreach (string file in files)
        {
            if (_fileService.GetFileType(file) != "txt")
            {
                continue;
            }
            string text = await _fileService.ReadTextFile(file) ?? throw new Exception();

            string? name = await ExtractName(text);
            string context = string.Empty;

            if (name != null)
            {
                string summary = facts.Where(x => x.Name == name).Select(c => c.Summary).First();
                context = $"\n<context>\n{summary}\n</context>\n";
            }

            string prompt = """
            <objective>
            You are an assistant copy editor. List keywords from the uploaded document.
            </objective>

            <rules>
            - Write the words in Polish, in the nominative case
            - Write up to 25 most important keywords
            - Print a list of words separated by commas
            - Don't write anything more
            - Filename is important, extract and write sector
            </rules>

            <output_example>
            keyword1,keywords2,keyword3
            </output_example>
            """;
            string user = $"{prompt}\n{context}\n<filename>\n{file}\n</filename>\n\n<document>\n{text}\n</document>";
            MessageDto response = await _kernelService.Chat(AIModel.Gpt4o, [new(Role.User, user)]);
            _kernelService.ClearHistory();

            _logger.LogInformation("File keywords: file: {file}, message: {message}", file, response.Message);

            fileData.Add(new(file, response.Message.Split(",")));
        }
        return fileData;
    }

    private static Dictionary<string, string> PrepareAnswer(List<FileDto> files)
    {
        Dictionary<string, string> answer = [];
        foreach (FileDto file in files)
        {
            answer.Add(file.FileName, string.Join(",", file.Keywords.Select(k => k.Trim())));
        }

        return answer;
    }
}

public record FactsDto(string Name, string Summary);

public record FileDto(string FileName, IList<string> Keywords);

public record GenerateKeywordsRequestDto(string Task, string Apikey, IDictionary<string, string> Answer);
