namespace ConsoleApp1;

public class FileSearcher
{
    private readonly string _initialPath;
    private Action<string, string, string> onFileFound;

    // Statistics properties
    public int FoldersSearched { get; private set; } = 0;
    public int CsFilesFound { get; private set; } = 0;
    public List<string> SearchedFolders { get; private set; } = new List<string>();

    // Constructor that takes the initial directory path.
    public FileSearcher(string path, Action<string, string, string> onFileFoundIn = null)
    {
        // Validate that the provided path is not null or empty.
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The directory path cannot be null or empty.", nameof(path));
        }
        _initialPath = path;
        
        // Assign the callback function to be called when a file is found.
        onFileFound = onFileFoundIn;
        
        // Initialize statistics
        SearchedFolders = new List<string>();
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
            // Reset statistics
            FoldersSearched = 0;
            CsFilesFound = 0;
            SearchedFolders.Clear();
            
            // Add initial folder to the list of searched folders
            SearchedFolders.Add(_initialPath);
            FoldersSearched++;
            
            // Get all subdirectories to track them
            var allDirectories = Directory.GetDirectories(_initialPath, "*", SearchOption.AllDirectories);
            foreach (var dir in allDirectories)
            {
                SearchedFolders.Add(dir);
                FoldersSearched++;
            }

            // Get all files ending with .cs, searching recursively.
            foreach (string file in Directory.EnumerateFiles(_initialPath, "*.cs", SearchOption.AllDirectories))
            {
                // Increment file counter
                CsFilesFound++;
                
                // If a callback function is provided, call it with the file path.
                onFileFound?.Invoke(Path.GetFullPath(file), Path.GetFileName(file), Path.GetFileName(file));
            }
            
            // Report statistics
            Console.WriteLine($"Search completed: {FoldersSearched} folders searched, {CsFilesFound} .cs files found.");
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
    
    // Method to print detailed search statistics
    public void PrintSearchStatistics()
    {
        Console.WriteLine($"Total folders searched: {FoldersSearched}");
        Console.WriteLine($"Total .cs files found: {CsFilesFound}");
    }
}
