internal partial class Program
{   
    public static string GetReadme()
    {
        string readmeFile = "README.txt";

        // Check if the file exists. If it does Read the file and return the value else provide the error message to the error handler.
        return File.Exists(readmeFile) ? File.ReadAllText(readmeFile) : ErrorHandler($"{readmeFile} not found!");

    }
}