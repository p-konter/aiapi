using System.IO.Compression;

namespace AIWebApi.Core;

public interface IZipService : IFileService
{
    void ClearDataFolder();

    void UnzipFile(string fileName);
}

public class ZipService : FileService, IZipService
{
    protected override string DataFolder { get; } = $".{Path.DirectorySeparatorChar}ExternalData{Path.DirectorySeparatorChar}ZipData";

    public void UnzipFile(string fileName)
    {
        string zipFilePath = Path.Combine(base.DataFolder, fileName);
        ZipFile.ExtractToDirectory(zipFilePath, DataFolder);
    }

    public void ClearDataFolder()
    {
        DirectoryInfo directory = new(DataFolder);
        foreach (FileInfo file in directory.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in directory.GetDirectories())
        {
            dir.Delete(true);
        }
    }
}
