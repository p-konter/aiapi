﻿using AIWebApi.Core;

namespace AIWebApi.Tasks;

public interface ICleanController
{
    Task<bool> ClearLogFiles();

    Task<bool> ClearWorkDir();
}

public class CleanController(IFileService fileService) : ICleanController
{
    private readonly IFileService _fileService = fileService;

    private readonly string Path = "ExternalData";
    private readonly string WorkPath = "WorkData";

    public Task<bool> ClearLogFiles()
    {
        _fileService.SetFolder(Path);
        IEnumerable<string> files = _fileService.GetFileNames();

        foreach (string file in files)
        {
            if (_fileService.GetFileType(file) == "log")
            {
                _fileService.DeleteFile(file);
            }
        }

        return Task.FromResult(true);
    }

    public Task<bool> ClearWorkDir()
    {
        _fileService.SetFolder([Path, WorkPath]);
        _fileService.ClearDataFolder();
        return Task.FromResult(true);
    }
}
