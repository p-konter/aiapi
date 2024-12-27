namespace AIWebApi.Core;

public record ResponseDto(int Code, string Message, List<string>? Hints);

public record RequestDto<T>(string Task, string Apikey, T Answer);

public record FillFormResponseDto(string Flag, string FileUrl);

public record VerifyDto(string Text, int MsgID);

public record SqlRequestDto(string Task, string Apikey, string Query);

public record SqlResponseDto(string Error, IList<IDictionary<string, string>> Reply);
