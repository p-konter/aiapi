namespace AIWebApi._00_PreWork;

public record ResponseDto(int Code, string Message) { }

public record RequestDto(string Task, string Apikey, IList<string> Answer) { }
