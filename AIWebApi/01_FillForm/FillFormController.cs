using AIWebApi.Core;

using HtmlAgilityPack;

namespace AIWebApi._01_FillForm;

public interface IFillFormController
{
    Task<FillFormResponseDto> RunFillForm();
}

public class FillFormController(IHttpService httpService, ILogger<FillFormController> logger, IOpenAIService openAIService) : IFillFormController
{
    public const string Username = "tester";
    public const string Password = "574e112a";

    public readonly Uri BaseUrl = new("http://xyz.ag3nts.org");

    private readonly IHttpService _httpService = httpService;
    private readonly ILogger<FillFormController> _logger = logger;
    private readonly IOpenAIService _openAIService = openAIService;

    public async Task<FillFormResponseDto> RunFillForm()
    {
        string question = await GetQuestion();
        _logger.LogInformation("Question is: {question}", question);

        string aiAnswer = await AskQuestion(question);
        _logger.LogInformation("AI api answer is: {aiAnswer}", aiAnswer);

        string formAnswer = await SendForm(aiAnswer);
        _logger.LogInformation("Form answer is: {formAnswer}", formAnswer);

        Uri filename = GetUrl(formAnswer);
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
        string page = await _httpService.GetString(BaseUrl);
        HtmlDocument document = CreateHtmlDocument(page);

        HtmlNode node = document.DocumentNode.SelectSingleNode($"//*[@id='human-question']");
        return node.InnerText;
    }

    private async Task<string> AskQuestion(string question)
    {
        string message = $"You are a historic expert. Answer this: {question} Write only year, without any additional text or formatting!";
        return await _openAIService.SimpleChat(message);
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
        return await _httpService.PostContent(BaseUrl, form);
    }

    private Uri GetUrl(string form)
    {
        HtmlDocument document = CreateHtmlDocument(form);
        HtmlNode link = document.DocumentNode.SelectSingleNode("//a");
        string href = link.GetAttributeValue("href", null);
        return new Uri(BaseUrl, href);
    }

    private static string GetFlag(string form)
    {
        HtmlDocument document = CreateHtmlDocument(form);
        HtmlNode node = document.DocumentNode.SelectSingleNode("//h2");
        return node.InnerText;
    }
}

public record FillFormResponseDto(string Flag, string FileUrl) { }
