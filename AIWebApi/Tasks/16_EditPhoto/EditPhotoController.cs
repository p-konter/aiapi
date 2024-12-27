using System.ComponentModel;

using AIWebApi.Core;

namespace AIWebApi.Tasks._16_EditPhoto;

public interface IEditPhotoController
{
    Task<ResponseDto> RunPhotoEditing();
}

public class EditPhotoController : BaseController, IEditPhotoController
{
    private readonly IFileService _fileService;
    private readonly IKernelService _kernelService;
    private readonly ILogger<EditPhotoController> _logger;

    private readonly Uri Barbara;

    public EditPhotoController(
        IConfiguration configuration,
        IFileService fileService,
        IHttpService httpService,
        IKernelService kernelService,
        ILogger<EditPhotoController> logger) : base(configuration, httpService)
    {
        _fileService = fileService;
        _kernelService = kernelService;
        _logger = logger;

        Barbara = GetUrl("Barbara");
    }

    public async Task<ResponseDto> RunPhotoEditing()
    {
        List<Uri> photosUrls = await GetPhotos();
        List<string> photos = await DownloadPhotos(photosUrls);

        List<string> analyzes = await AnalyzePhotos(photos);
        List<string> analizesAgain = await AnalyzePhotos(analyzes);

        string description = await MakeDescription(analizesAgain);

        return await SendAnswer("photos", "Report", description);
        //return new ResponseDto(0, "Ok");
    }

    private async Task<List<Uri>> GetPhotos()
    {
        ResponseDto photos = await SendAnswer("photos", "Report", "START");
        _logger.LogInformation("Photos: {photos}", photos.Message);

        string prompt = "You are a graphic assistant. You will receive a message containing the urls of the photos. " +
            "Write these addresses, separated by a comma. Do not write anything else, just the addresses.";
        List<MessageDto> messages = [new(Role.System, prompt), new(Role.User, photos.Message)];
        MessageDto answer = await _kernelService.Chat(AIModel.Gpt4oMini, messages);
        _kernelService.ClearHistory();

        return answer.Message.Split(", ").Select(x => new Uri(x)).ToList();
    }

    private static Uri GetSmallUri(Uri url)
    {

        string originalPath = url.ToString();
        string smallPath = originalPath.Insert(originalPath.LastIndexOf('.'), "-small");
        return new Uri(smallPath);
    }

    private async Task<List<string>> DownloadPhotos(List<Uri> photos)
    {
        _fileService.SetFolder(["ExternalData", "WorkData"]);
        _fileService.CreateFolder();
        List<string> fileNames = [];

        foreach (Uri photo in photos)
        {
            string fileName = photo.Segments.Last();
            Uri smallPhotoUri = GetSmallUri(photo);
            byte[] file = await _httpService.GetBinaryFile(smallPhotoUri);
            await _fileService.WriteBinaryFile(fileName, file);
            fileNames.Add(fileName);
        }

        return fileNames;
    }

    private async Task<List<string>> AnalyzePhotos(List<string> photos)
    {
        List<PhotoAnalyze> analyzes = [];
        foreach (string photo in photos)
        {
            string prompt = """
            You are a graphic designer's assistant. Analyze the photo from a technical perspective.

            <rules>
            - You have 3 commands available: REPAIR, DARKEN, BRIGHTEN.
            - If the photo has noise, visual artifacts or poor quality, use the command: REPAIR.
            - If the photo is very bright or overexposed, use the command: DARKEN.
            - If the photo is too dark, use the command: BRIGHTEN.
            - If you think the photo is technically OK, don't write anything.
            - Think about whether the photo really needs repair.
            - Write only one command or write nothing.
            </rules>
            """;

            MessageDto response = await _kernelService.ImageChat(AIModel.Gpt4oMini, photo, prompt);
            _kernelService.ClearHistory();

            PhotoAnalyze analyze = (string.IsNullOrEmpty(response.Message)) ? new(photo, null)
                : new(photo, response.Message.CreateByDescription<Command>());
            analyzes.Add(analyze);
        }

        return await RunAnalyze(analyzes);
    }

    private async Task<List<string>> RunAnalyze(List<PhotoAnalyze> analyzes)
    {
        List<string> photos = [];
        foreach (PhotoAnalyze analyze in analyzes)
        {
            if (analyze.Command is not null)
            {
                string? name = await ExecuteCommand(analyze.Command.Value, analyze.Name);
                if (name != null)
                {
                    Uri photoUrl = new(Barbara, name);
                    Uri smallPhotoUri = GetSmallUri(photoUrl);
                    byte[] file = await _httpService.GetBinaryFile(smallPhotoUri);
                    await _fileService.WriteBinaryFile(name, file);
                    photos.Add(name);
                    continue;
                }
            }

            photos.Add(analyze.Name);
        }
        return photos;
    }

    private async Task<string?> ExecuteCommand(Command command, string name)
    {
        ResponseDto response = await SendAnswer("photos", "Report", $"{command.GetDescription()} {name}");
        _logger.LogInformation("Api response {command}, {code}, answer: {}", command.GetDescription(), response.Code, response.Message);

        string prompt = "You are a graphic assistant. You will receive a message that may contain a photo address. If the message contains a name or url of " +
            "a photo, return only the file name and do not write anything else. If the message does not contain a photo address, do not write anything.";
        List<MessageDto> messages = [new(Role.System, prompt), new(Role.User, response.Message)];
        MessageDto answer = await _kernelService.Chat(AIModel.Gpt4oMini, messages);
        _kernelService.ClearHistory();

        return string.IsNullOrEmpty(answer.Message) ? null : answer.Message;
    }

    private async Task<string> MakeDescription(List<string> photos)
    {
        string prompt = """
            Jesteś asystentem grafika. Przeanalizuj zdjęcia i opisz osobę, która na nich występuje. Zwróc uwagę na:
            - Kształt twarzy
            - Kolor i fakturę włosów
            - Płeć i budowę
            - Znaki szczególne
            Podaj formalny, precyzyjny opis, tak jak profesjonalnie opisuje się zdjęcia..
            Skup się na wyglądzie, nie oceniaj, unikaj zbędnych szczegółów.
            Napisz sam opis, bez dodatkowych informacji i bez formatowania.
            """;

        MessageDto response = await _kernelService.MultipleImageChat(AIModel.Gpt4o, photos, prompt);
        return response.Message;
    }
}

public record PhotoAnalyze(string Name, Command? Command);

public enum Command
{
    [Description("REPAIR")]
    Repair,

    [Description("DARKEN")]
    Darken,

    [Description("BRIGHTEN")]
    Brighten
}
