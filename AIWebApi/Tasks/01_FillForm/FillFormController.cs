using AIWebApi.Core;

using HtmlAgilityPack;

namespace AIWebApi.Tasks._01_FillForm;

public interface IFillFormController
{
    Task<FillFormResponseDto> RunFillForm();
}

public class FillFormController(IConfiguration configuration, IHttpService httpService, IKernelService kernelService, ILogger<FillFormController> logger)
    : BaseController(configuration, httpService), IFillFormController
{
    public const string Username = "tester";
    public const string Password = "574e112a";

    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<FillFormController> _logger = logger;

    public async Task<FillFormResponseDto> RunFillForm()
    {
        string question = await GetQuestion();
        _logger.LogInformation("Question is: {question}", question);

        string aiAnswer = await AskQuestion(question);
        _logger.LogInformation("AI api answer is: {aiAnswer}", aiAnswer);

        string formAnswer = await SendForm(aiAnswer);
        _logger.LogInformation("Form answer is: {formAnswer}", formAnswer);

        Uri filename = GetFilename(formAnswer);
        _logger.LogInformation("Filename from page is: {filename}", filename.ToString());

        string flag = GetFlag(formAnswer);
        _logger.LogInformation("Flag is: {flag}", flag);

        return new FillFormResponseDto(flag, filename.ToString());
    }

    private static HtmlDocument CreateHtmlDocument(string form)
    {
        HtmlDocument document = new();
        document.LoadHtml(form);
        return document;
    }

    private async Task<string> GetQuestion()
    {
        string page = await _httpService.GetString(GetUrl("XYZ"));
        HtmlDocument document = CreateHtmlDocument(page);

        HtmlNode node = document.DocumentNode.SelectSingleNode($"//*[@id='human-question']");
        return node.InnerText;
    }

    private async Task<string> AskQuestion(string question)
    {
        string message = $"You are a historic expert. Answer this: {question} Write only year, without any additional text or formatting!";
        return await _kernelService.SimpleChat(AIModel.Gpt4oMini, message);
    }

    private async Task<string> SendForm(string answer)
    {
        Dictionary<string, string> formData = new()
        {
            { "username", Username },
            { "password", Password },
            { "answer", answer }
        };

        FormUrlEncodedContent form = new(formData);
        return await _httpService.PostContent(GetUrl("XYZ"), form);
    }

    private Uri GetFilename(string form)
    {
        HtmlDocument document = CreateHtmlDocument(form);
        HtmlNode link = document.DocumentNode.SelectSingleNode("//a");
        string href = link.GetAttributeValue("href", null);
        return new Uri(GetUrl("XYZ"), href);
    }

    private static string GetFlag(string form)
    {
        HtmlDocument document = CreateHtmlDocument(form);
        HtmlNode node = document.DocumentNode.SelectSingleNode("//h2");
        return node.InnerText;
    }
}
