using AIWebApi.Core;

namespace AIWebApi._00_PreWork;

public interface IPreWorkController
{
    Task<ResponseDto> RunPreWork();
}

public class PreWorkController(IConfiguration configuration, IHttpService httpService, ILogger<PreWorkController> logger)
    : BaseController(configuration, httpService), IPreWorkController
{
    private readonly ILogger<PreWorkController> _logger = logger;
    private static readonly string[] separator = ["\r\n", "\n"];

    private async Task<IList<string>> FetchData()
    {
        Uri getDataUrl = new(_configuration.GetSection("Urls").GetStrictValue<string>("PoligonData"));
        string data = await _httpService.GetString(getDataUrl);
        return new List<string>(data.Split(separator, StringSplitOptions.RemoveEmptyEntries));
    }

    public async Task<ResponseDto> RunPreWork()
    {
        IList<string> values = await FetchData();
        _logger.LogInformation("Fetched values are: {values}", values);

        ResponseDto answer = await SendAnswer("POLIGON", "PoligonVerify", values);
        _logger.LogInformation("Final answer is {answer}", answer.Message);

        return answer;
    }
}
