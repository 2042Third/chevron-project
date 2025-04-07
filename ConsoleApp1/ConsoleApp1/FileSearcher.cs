namespace ConsoleApp1;

public class FileSearcher
{
    private readonly string _initialPath;
    private Action<string> onFileFound;

    // Constructor that takes the initial directory path.
    public FileSearcher(string path, Action<string> onFileFoundIn = null)
    {
        // Validate that the provided path is not null or empty.
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The directory path cannot be null or empty.", nameof(path));
        }
        _initialPath = path;
        
        // Assign the callback function to be called when a file is found.
        onFileFound = onFileFoundIn;
    }

    // Recursively finds and prints all .cs files in the directory.
    public void PrintCsFiles()
    {
        if (!Directory.Exists(_initialPath))
        {
            Console.Error.WriteLine($"The directory \"{_initialPath}\" does not exist.");
            return;
        }

        try
        {
            // Get all files ending with .cs, searching recursively.
            foreach (string file in Directory.EnumerateFiles(_initialPath, "*.cs", SearchOption.AllDirectories))
            {
                // If a callback function is provided, call it with the file path.
                onFileFound?.Invoke(Path.GetFullPath(file));
            }
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Console.Error.WriteLine("Access error: " + uaEx.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("An error occurred: " + ex.Message);
        }
    }
}