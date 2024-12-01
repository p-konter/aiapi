using AIWebApi.Core;

namespace AIWebApi.Tasks._12_DateFromVector;

public interface IDateFromVectorController
{
    Task<ResponseDto> GetDateFromVector();
}

public class DateFromVectorController(
    IConfiguration configuration,
    IFileService fileService,
    IHttpService httpService,
    IKernelService kernelService,
    ILogger<DateFromVectorController> logger,
    IQdrantService qdrantService) : BaseController(configuration, httpService), IDateFromVectorController
{
    private readonly IFileService _fileService = fileService;
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<DateFromVectorController> _logger = logger;
    private readonly IQdrantService _qdrantService = qdrantService;

    private readonly string DataPath = "ExternalData";

    public async Task<ResponseDto> GetDateFromVector()
    {
        UnzipData();
        await _qdrantService.CreateCollectionAsync();

        List<FileMetaDataDto> data = await ReadFiles();
        await AddDataToBase(data);

        FileMetaDataDto result = await SearchData();
        return await SendAnswer("wektory", "Report", result.Date);
    }

    private void UnzipData()
    {
        string workPath = "WorkData";
        string weaponPath = "Weapon";

        _fileService.SetFolder(DataPath);
        _fileService.UnzipFileToFolder("pliki_z_fabryki.zip", workPath);

        _fileService.SetFolder([DataPath, workPath]);
        _fileService.UnzipFileToFolder("weapons_tests.zip", weaponPath, "1670");

        _fileService.SetFolder([DataPath, workPath, weaponPath, "do-not-share"]);
    }

    private async Task<List<FileMetaDataDto>> ReadFiles()
    {
        IEnumerable<string> files = _fileService.GetFileNames();

        List<FileMetaDataDto> fileData = [];
        foreach (string file in files)
        {
            string text = await _fileService.ReadTextFile(file) ?? throw new Exception();
            _logger.LogInformation("Read file: {file}", file);

            float[] vectors = await _kernelService.CreateVector(text);

            string date = _fileService.GetFileName(file).Replace('_', '-');
            fileData.Add(new FileMetaDataDto(date, vectors, text));
        }

        return fileData;
    }

    private async Task AddDataToBase(List<FileMetaDataDto> data)
    {
        List<DataPointDto> points = data.Select(x => new DataPointDto(x.Vectors, new Dictionary<string, string>()
        {
            { "Date", x.Date },
            { "Content", x.Content }
        })).ToList();

        await _qdrantService.AddPointsAsync(points);
    }

    private async Task<FileMetaDataDto> SearchData()
    {
        float[] question = await _kernelService.CreateVector("W raporcie, z którego dnia znajduje się wzmianka o kradzieży prototypu broni?");
        IList<SearchResultDto> results = await _qdrantService.SearchAsync(question);

        FileMetaDataDto response = results.Select(x => new FileMetaDataDto(x.Data["Date"], [x.Score], x.Data["Content"])).First();
        _logger.LogInformation("Search result: date: {date}, content: {content}", response.Date, response.Content);

        return response;
    }
}

public record FileMetaDataDto(string Date, float[] Vectors, string Content);
