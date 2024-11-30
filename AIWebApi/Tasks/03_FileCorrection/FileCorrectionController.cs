using System.Data;

using AIWebApi.Core;

namespace AIWebApi.Tasks._03_FileCorrection;

public interface IFileCorrectionController
{
    Task<ResponseDto> RunFileCorrection();
}

public class FileCorrectionController(
    IConfiguration configuration,
    IHttpService httpService,
    IJsonService jsonService,
    IKernelService kernelService,
    ILogger<FileCorrectionController> logger) : BaseController(configuration, httpService), IFileCorrectionController
{
    private readonly IJsonService _jsonService = jsonService;
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<FileCorrectionController> _logger = logger;

    private readonly string FilePath = "FileCorrection.txt";

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

        return await SendAnswer("JSON", "Report", fileData);
    }

    private async Task<string> AnswerQuestion(string question)
    {
        MessageDto questionMessage = new(Role.User, question);
        MessageDto response = await _kernelService.Chat(AIModel.Gpt4oMini, [CreateSystemPrompt(), questionMessage]);
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
