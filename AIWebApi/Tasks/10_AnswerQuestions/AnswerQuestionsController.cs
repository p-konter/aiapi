using System.Text;

using AIWebApi.Core;

using HtmlAgilityPack;

namespace AIWebApi.Tasks._10_AnswerQuestions;

public interface IAnswerQuestionsController
{
    Task<ResponseDto> RunAnswerQuestions();
}

public class AnswerQuestionsController : BaseController, IAnswerQuestionsController
{
    private readonly IFileService _fileService;
    private readonly IKernelService _kernelService;
    private readonly ILogger<AnswerQuestionsController> _logger;
    private readonly Uri DataUri;

    public AnswerQuestionsController(
        IConfiguration configuration,
        IFileService fileService,
        IHttpService httpService,
        IKernelService kernelService,
        ILogger<AnswerQuestionsController> logger) : base(configuration, httpService)
    {
        _fileService = fileService;
        _kernelService = kernelService;
        _logger = logger;
        DataUri = GetUrl("ArxivData");
    }

    public async Task<ResponseDto> RunAnswerQuestions()
    {
        IList<Question> questions = await GetQuestions();
        string document = await GetPage();

        Dictionary<string, string> answers = await AnswerQuestions(questions, document);
        return await SendAnswer("arxiv", "Report", answers);
    }

    private async Task<IList<Question>> GetQuestions()
    {
        Uri url = GetUrlWithKey("ArxivQuestions");
        string questions = await _httpService.GetString(url);

        return questions
            .Split("\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split("="))
            .Select(parts => new Question(parts[0], parts[1]))
            .ToList();
    }

    private async Task<Dictionary<string, string>> AnswerQuestions(IList<Question> questions, string document)
    {
        Dictionary<string, string> answers = [];
        foreach (Question question in questions)
        {
            string prompt = $"""
                You are a helpful assistant. Briefly answer the above question regarding the document you sent.
                Write short answer in Polish.

                <rules>
                The question about leftovers is not about beer or any fruits.
                </rules>

                <question>
                {question.Text}
                </question>
                """;
            List<MessageDto> messages = [new(Role.System, document), new(Role.User, prompt)];
            MessageDto response = await _kernelService.Chat(AIModel.Gpt4oMini, messages);
            _kernelService.ClearHistory();

            _logger.LogInformation("Question: {question}, answer: {answer}", question.Text, response.Message);
            answers.Add(question.Id, response.Message);
        }

        return answers;
    }

    private async Task<string> GetPage()
    {
        string page = await _httpService.GetString(GetUrl("Arxiv"));
        HtmlDocument html = CreateHtmlDocument(page);
        HtmlDocument document = Clean(html);

        _fileService.SetFolder(["ExternalData", "WorkData"]);
        _fileService.CreateFolder();
        List<Image> images = await ExtractImages(document);
        List<Audio> audios = await ExtractAudio(document);

        string header = ParseHeader(document);
        string footer = ParseFooter(document);
        string text = ParseText(document, images, audios.FirstOrDefault()!);

        string result = BuildDocument(header, text, footer);
        _logger.LogInformation("Text: {text}", result);
        return result;
    }

    private async Task<List<Image>> ExtractImages(HtmlDocument document)
    {
        IList<HtmlNode> imageNodes = document.DocumentNode.SelectNodes("//figure");
        List<Image> images = imageNodes
            .Select(figureNode =>
            {
                string imgSrc = figureNode.SelectSingleNode(".//img")?.GetAttributeValue("data-cfsrc", string.Empty)!;
                string caption = figureNode.SelectSingleNode(".//figcaption")?.InnerText.Trim()!;
                return new Image(_fileService.GetFileName(imgSrc), new Uri(DataUri, imgSrc), caption);
            })
            .ToList();

        foreach (Image image in images)
        {
            byte[] imageFile = await _httpService.GetBinaryFile(image.Uri);
            string fullName = _fileService.ChangeExtension(image.Name, "png");
            await _fileService.WriteBinaryFile(fullName, imageFile);

            string prompt = $"""
                You are an archivist. Describe photo. Focus on the object in the photo.
                If this is a view, try to find a location.
                Use the caption provided, it contains important information.
                Write your answer in Polish.

                <caption>
                {image.Caption}
                </caption>
                """;

            MessageDto transcription = await _kernelService.ImageChat(AIModel.Gpt4o, fullName, prompt);
            image.Transcription = transcription.Message;
        }

        return images;
    }

    private async Task<List<Audio>> ExtractAudio(HtmlDocument document)
    {
        IList<HtmlNode> audioNodes = document.DocumentNode.SelectNodes("//audio");
        List<Audio> audios = audioNodes
            .Select(figureNode =>
            {
                string imgSrc = figureNode.SelectSingleNode(".//source")?.GetAttributeValue("src", string.Empty)!;
                return new Audio(_fileService.GetFileName(imgSrc), new Uri(DataUri, imgSrc));
            })
            .ToList();

        foreach (Audio audio in audios)
        {
            byte[] imageFile = await _httpService.GetBinaryFile(audio.Uri);
            string fullName = _fileService.ChangeExtension(audio.Name, "mp3");
            await _fileService.WriteBinaryFile(fullName, imageFile);

            string transcription = await _kernelService.AudioTranscription(fullName);
            audio.Transcription = transcription;
        }

        return audios;
    }

    private static HtmlDocument Clean(HtmlDocument document)
    {
        IList<HtmlNode> nodesToRemove = document.DocumentNode.SelectNodes("//script|//style|//p[@data-wtf]");
        foreach (HtmlNode node in nodesToRemove)
        {
            node.Remove();
        }
        
        IList<HtmlNode> textNodesToRemove = document.DocumentNode.SelectNodes("//text()");
        foreach (HtmlNode node in textNodesToRemove)
        {
            if (string.IsNullOrEmpty(node.InnerText.Trim()))
            {
                node.Remove();
            }
        }

        return document;
    }

    private static string ParseHeader(HtmlDocument document)
    {
        StringBuilder builder = new();
        HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='title']");
        builder.Append($"<tytuł>\n{titleNode.InnerText.Trim()}\n</tytuł>\n");
        titleNode.Remove();
        HtmlNode authorNode = document.DocumentNode.SelectSingleNode("//div[@class='authors']");
        builder.Append($"<autor>\n{authorNode.InnerText.Trim()}\n</autor>\n");
        authorNode.Remove();
        HtmlNode abstractNode = document.DocumentNode.SelectSingleNode("//div[@id='abstract']");
        builder.Append($"<abstrakt>\n{abstractNode.InnerText.Trim()}\n</abstrakt>\n");
        abstractNode.Remove();
        return builder.ToString();
    }

    private static string ParseFooter(HtmlDocument document)
    {
        StringBuilder builder = new();
        HtmlNode bibliographyNode = document.DocumentNode.SelectSingleNode("//div[@class='chicago-bibliography']");
        builder.Append("<źródła>\n");
        foreach (HtmlNode node in bibliographyNode.SelectNodes(".//p"))
        {
            builder.Append($"<źródło>\n{node.InnerText.Trim()}\n</źródło>\n");
        }
        builder.Append("</źródła>\n");
        bibliographyNode.Remove();
        return builder.ToString();
    }

    private static string ParseText(HtmlDocument document, List<Image> images, Audio audio)
    {
        StringBuilder builder = new();
        HtmlNode textNode = document.DocumentNode.SelectSingleNode("//div[@class='container']");
        HtmlNodeCollection h2Nodes = textNode.SelectNodes(".//h2");

        for (int i = 1; i < h2Nodes.Count - 1; i++)
        {
            string title = h2Nodes[i].InnerText.Trim();
            builder.Append($"<sekcja>\n<tytuł>\n{title}\n</tytuł>\n<treść>\n");
            IEnumerable<HtmlNode> nodesBetween = h2Nodes[i].ParentNode.ChildNodes
                .SkipWhile(node => node != h2Nodes[i])
                .Skip(1)
                .TakeWhile(node => node != h2Nodes[i + 1]);

            foreach (HtmlNode node in nodesBetween)
            {
                builder.Append(ReadNode(node, images, audio));
            }
            builder.Append($"</treść>\n</sekcja>\n");
        }

        return builder.ToString();
    }

    private static string ReadNode(HtmlNode node, List<Image> images, Audio audio)
    {
        StringBuilder builder = new();
        switch (node.Name)
        {
            case "p":
                builder.Append(node.InnerText.Trim());
                builder.Append('\n');
                if (node.HasChildNodes)
                {
                    builder.Append(ReadNode(node.FirstChild, images, audio));
                }
                break;
            case "figure":
                Image image = images.First();
                builder.Append("<zdjęcie>\n");
                builder.Append($"<nazwa>\n{image.Name}\n</nazwa>\n");
                builder.Append($"<opis>\n{image.Transcription}\n</opis>\n");
                builder.Append($"<podpis>\n{image.Caption}\n</podpis>\n");
                builder.Append("</zdjęcie>\n");
                images.Remove(image);
                break;
            case "audio":
                builder.Append($"<transkrypcja>\n{audio.Transcription}\n</transkrypcja>\n");
                break;
            default:
                break;
        }
        return builder.ToString();
    }

    private static string BuildDocument(string header, string text, string footer)
    {
        StringBuilder builder = new();
        builder.Append("<dokument>\n");
        builder.Append(header);
        builder.Append("<zawartość>\n");
        builder.Append(text);
        builder.Append("</zawartość>\n");
        builder.Append(footer);
        builder.Append("</dokument>\n");
        return builder.ToString();
    }
}

public record Question(string Id, string Text);

public record Audio(string Name, Uri Uri)
{
    public string? Transcription { get; set; }
}

public record Image(string Name, Uri Uri, string Caption)
{
    public string? Transcription { get; set; }
}
