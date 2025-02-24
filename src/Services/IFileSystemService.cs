namespace Yorit.Api.Services
{
    public interface IFileSystemService
    {
        bool DirectoryExists(string path);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
        Stream OpenRead(string path);
    }
}
