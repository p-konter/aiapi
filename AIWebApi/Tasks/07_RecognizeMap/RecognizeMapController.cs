using AIWebApi.Core;

namespace AIWebApi.Tasks._07_RecognizeMap;

public interface IRecognizeMapController
{
    Task<string> RunRecognizeMap();
}

public class RecognizeMapController(IFileService fileService, IGPT4AIService chatService) : IRecognizeMapController
{
    private readonly List<string> FileNames = ["map1.png", "map2.png", "map3.png", "map4.png"];
    private readonly string DataPath = "ExternalData";

    private readonly IGPT4AIService _chatService = chatService;
    private readonly IFileService _fileService = fileService;

    public async Task<string> RunRecognizeMap()
    {
        _fileService.SetFolder(DataPath);
        List<ImageDto> images = await LoadImages();
        return await Recognize(images);
    }

    private async Task<List<ImageDto>> LoadImages()
    {
        List<ImageDto> images = [];
        foreach (string filename in FileNames)
        {
            BinaryData map = await _fileService.ReadBinaryFile(filename);
            images.Add(new ImageDto(map, ImageType.Png));
        }
        return images;
    }

    private async Task<string> Recognize(List<ImageDto> images)
    {
        string prompt = "You have attached images of screenshots of a map of city in Poland. Three of them are maps of the same city. Recognize which city it is and write its name.";
        MessageDto message = new(Role.User, prompt, images);
        MessageDto response = await _chatService.Chat([message]);
        return response.Message;
    }
}
