using System.Data;

using AIWebApi._00_PreWork;
using AIWebApi.Core;

namespace AIWebApi._03_FileCorrection;

public interface IFileCorrectionController
{
    Task<ResponseDto> RunFileCorrection();
}

public class FileCorrectionController(IConfiguration configuration, IHttpService httpService, IJsonService jsonService, ILogger<FileCorrectionController> logger)
    : IFileCorrectionController
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpService _httpService = httpService;
    private readonly IJsonService _jsonService = jsonService;
    private readonly ILogger<FileCorrectionController> _logger = logger;
    private readonly OpenAIService _openAIService = new(ChatModel.GPT_40_Mini, configuration);

    private readonly string FilePath = "./ExternalData/FileCorrection.txt";
    private readonly Uri PostDataUrl = new("https://centrala.ag3nts.org/report");

    public async Task<ResponseDto> RunFileCorrection()
    {
        FileDto fileData = await _jsonService.LoadFromFile<FileDto>(FilePath);
        fileData.Apikey = _configuration.GetStrictValue<string>("ApiKey");

        foreach (TestDataDto testData in fileData.TestData)
        {
            int answer = Evaluate(testData.Question);
            if (answer != testData.Answer)
            {
                testData.Answer = answer;
            }

            if (testData.Test is not null)
            {
                string answerMessage = await AnswerQuestion(testData.Test.Q);
                testData.Test = new(testData.Test.Q, answerMessage);
            }
        }

        return await PostAnswer(fileData);
    }

    private async Task<ResponseDto> PostAnswer(FileDto fileData)
    {
        AnswerDto answer = new("JSON", fileData.Apikey, fileData);
        return await _httpService.PostJson<ResponseDto>(PostDataUrl, answer);
    }

    private async Task<string> AnswerQuestion(string question)
    {
        MessageDto questionMessage = new(Role.User, question);
        MessageDto response = await _openAIService.ThreadChat([CreateSystemPrompt(), questionMessage]);
        _logger.LogInformation("FileCorrection question: {question}, response: {response}", questionMessage.Message, response.Message);

        return response.Message;
    }

    private static MessageDto CreateSystemPrompt()
    {
        string prompt = $"""
            You are Robbo, a a helpful assistant who speaks using as few words as possible.
            
            <snippet_rules>
                - answer the question
                - use only one word
            </snippet_rules>
            """;

        return new MessageDto(Role.System, prompt);
    }

    private static int Evaluate(string expression)
    {
        DataTable table = new();
        return (int)table.Compute(expression, string.Empty);
    }
}
