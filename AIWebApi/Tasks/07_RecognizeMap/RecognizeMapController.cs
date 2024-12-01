using AIWebApi.Core;

namespace AIWebApi.Tasks._07_RecognizeMap;

public interface IRecognizeMapController
{
    Task<string> RunRecognizeMap();
}

public class RecognizeMapController(IFileService fileService, IKernelService kernelService) : IRecognizeMapController
{
    private readonly string Path = "ExternalData";

    private readonly IFileService _fileService = fileService;
    private readonly IKernelService _kernelService = kernelService;

    public async Task<string> RunRecognizeMap()
    {
        List<ImageDto> images = await LoadImages();
        return await Recognize(images);
    }

    private async Task<List<ImageDto>> LoadImages()
    {
        _fileService.SetFolder(Path);
        IEnumerable<string> files = _fileService.GetFileNames();

        List<ImageDto> images = [];
        foreach (string file in files)
        {
            if (_fileService.GetFileType(file) == "png")
            {
                BinaryData map = await _fileService.ReadBinaryFile(file);
                images.Add(new ImageDto(map, ImageType.Png));
            }
        }
        return images;
    }

    private async Task<string> Recognize(List<ImageDto> images)
    {
        string prompt = "You have attached images of screenshots of a map of city in Poland. Three of them are maps of the same city. Recognize which city it is and write its name.";
        MessageDto message = new(Role.User, prompt, images);
        MessageDto response = await _kernelService.Chat(AIModel.Gpt4o, [message]);
        return response.Message;
    }
}
