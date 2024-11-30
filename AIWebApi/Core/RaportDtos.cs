namespace AIWebApi.Core;

public record RequestDto(string Task, string Apikey, string Answer);

public record RequestListDto(string Task, string Apikey, IList<string> Answer);

public record ResponseDto(int Code, string Message);

public record RequestDto<T>(string Task, string Apikey, T Answer);
