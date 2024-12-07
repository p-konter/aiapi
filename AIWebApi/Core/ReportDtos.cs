namespace AIWebApi.Core;

public record ResponseDto(int Code, string Message);

public record RequestDto<T>(string Task, string Apikey, T Answer);

public record FillFormResponseDto(string Flag, string FileUrl);

public record VerifyDto(string Text, int MsgID);
