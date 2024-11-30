using System.ComponentModel;

namespace AIWebApi.Tasks._09_SortFiles;

public record OutputMessageDto(string Thinking, string Category);

public record FileDto(string FileName, string Description);

public record FileTypeDto(string FileName, FileType Type);

public record SortFilesDto(IList<string> People, IList<string> Hardware);

public record SortFilesRequestDto(string Task, string Apikey, SortFilesDto Answer);

public enum FileType
{
    [Description("people")]
    People,
    [Description("machines")]
    Machines,
    [Description("others")]
    Others
};
