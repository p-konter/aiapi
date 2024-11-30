using System.Text.Json.Serialization;

namespace AIWebApi.Tasks._03_FileCorrection;

public class TestDataDto(string question, int answer, TestDto? test)
{
    public string Question { get; } = question;
    public int Answer { get; set; } = answer;
    public TestDto? Test { get; set; } = test;
}

public record TestDto(string Q, string A);

public class FileDto(string apikey, string description, string copyright, List<TestDataDto> testData)
{
    public string Apikey { get; set; } = apikey;
    public string Description { get; } = description;
    public string Copyright { get; } = copyright;

    [JsonPropertyName("test-data")]
    public List<TestDataDto> TestData { get; } = testData;
}

public record AnswerDto(string Task, string Apikey, FileDto Answer);
