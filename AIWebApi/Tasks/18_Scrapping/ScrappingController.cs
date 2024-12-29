using AIWebApi.Core;

using HtmlAgilityPack;

namespace AIWebApi.Tasks._18_Scrapping;

public interface IScrappingController
{
    Task<ResponseDto> RunScrapping();
}

public class ScrappingController : BaseController, IScrappingController
{
    private readonly IJsonService _jsonService;
    private readonly IKernelService _kernelService;
    private readonly ILogger<ScrappingController> _logger;

    private readonly Uri _softoUrl;

    public ScrappingController(
        IConfiguration configuration,
        IHttpService httpService,
        IJsonService jsonService,
        ILogger<ScrappingController> logger,
        IKernelService kernelService) : base(configuration, httpService)
    {
        _jsonService = jsonService;
        _kernelService = kernelService;
        _logger = logger;

        _softoUrl = GetUrl("Softo");
    }

    public async Task<ResponseDto> RunScrapping()
    {
        Dictionary<string, string> questions = await GetQuestions();
        Dictionary<string, string> answer = await AnswerQuestions(questions);
        return await SendAnswer("softo", "Report", answer);
    }

    private async Task<Page> GetPage(Uri uri)
    {
        string page = await _httpService.GetString(uri);
        HtmlDocument document = CreateHtmlDocument(page);
        List<Link> links = ExtractLinks(document);
        List<string> content = ScrapPage(document);
        return new(links, content);
    }

    private List<Link> ExtractLinks(HtmlDocument document)
    {
        List<Link> links = [];
        HtmlNodeCollection linkNodes = document.DocumentNode.SelectNodes("//a[@href]");

        foreach (HtmlNode node in linkNodes)
        {
            string href = node.GetAttributeValue("href", string.Empty);
            string title = node.GetAttributeValue("title", string.Empty);
            string text = node.InnerText.Trim();

            Uri.TryCreate(href, UriKind.Absolute, out Uri? uri);
            uri ??= new(_softoUrl, href);

            links.Add(new(uri, title, text));
        }

        return links;
    }

    private static List<string> ScrapPage(HtmlDocument document)
    {
        List<string> texts = [];
        IList<HtmlNode> textNodes = document.DocumentNode.SelectNodes("//text()");
        foreach (HtmlNode node in textNodes)
        {
            if (node.NodeType == HtmlNodeType.Text && !node.InnerText.Contains("CDATA"))
            {
                string text = node.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    texts.Add(text);
                }
            }
        }
        return texts;
    }

    private async Task<Dictionary<string, string>> GetQuestions()
    {
        Uri url = GetUrlWithKey("SoftoQuestions");
        return await _httpService.GetJson<Dictionary<string, string>>(url);
    }

    private async Task<Dictionary<string, string>> AnswerQuestions(Dictionary<string, string> questions)
    {
        Dictionary<string, string> answers = [];
        foreach ((string key, string value) in questions)
        {
            AnswerDto answer = await AnswerQuestion(value);
            _kernelService.ClearHistory();
            answers.Add(key, answer.Answer!);
        }

        return answers;
    }

    private async Task<AnswerDto> AnswerQuestion(string question)
    {
        Page page = await GetPage(_softoUrl);
        while (true)
        {
            AnswerDto answer = await AskQuestion(page, question);
            if (answer.Link != null)
            {
                if (!answer.Link.StartsWith(_softoUrl.ToString()))
                {
                    answer.Answer = answer.Link;
                    return answer;
                }
                Uri url = new(answer.Link);
                page = await GetPage(url);
            }
            else
            {
                return answer;
            }
        }
    }

    private async Task<AnswerDto> AskQuestion(Page page, string question)
    {
        string prompt = $"""
            You are a helpful assistant. Try to answer the question asked using the downloaded content from the website.

            <rules>
            Please answer the following question using the content from the provided page.
            If you can't find the answer, choose a link, but only from the links section, and make sure it is from the '{_softoUrl}' domain.
            Make it clear that you should only select from this domain and not consider other links.
            The answer should be in JSON format containing the thinking process and either the answer or the selected link.
            Write only short text answer or link.
            </rules>

            <question>
            {question}
            </question>

            <page>
            {string.Join("\n", page.Content)}
            </page>

            <links>
            {string.Join("\n", page.Links.Select(x => $"{x.Href} - {x.Text} - {x.Title}"))}
            </links>

            """ + """

            <example_output>
            {
                "thinking": "type here yor thinking",
                "link": "link",
                "answer": "answer"
            }
            </example_output>
            """;
        _logger.LogInformation("Prompt: {prompt}", prompt);

        MessageDto message = new(Role.User, prompt);
        MessageDto response = await _kernelService.Chat(AIModel.Gpt4oMini, [message], true);
        return _jsonService.Deserialize<AnswerDto>(response.Message);
    }
}

public record Link(Uri Href, string Title, string Text);

public record Page(List<Link> Links, List<string> Content);

public record AnswerDto(string Thinking, string Link)
{
    public string? Answer { get; set; }
}
