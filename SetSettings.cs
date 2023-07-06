internal partial class Program
{   
    public static dynamic SetSettings()
    {

    // Settings file name
        string settingsFile = "Settings.user";
    // Set the friendly error message in case file is not found
        string fileNotFoundError = $">>> ERROR: {settingsFile} not found.\nPlease create {settingsFile} file and enter all required information.";


    // Check that the file exists
        if (!File.Exists(settingsFile)) 
        {
            Console.WriteLine(fileNotFoundError);
            return false;
        }

    // Read all lines from the settings file
        string[] lines = File.ReadAllLines(settingsFile);

    // Parse the lines into a dictionary of settings
        var settings = lines
            .Select(l => l.Split(new[] { '=' }))
            .ToDictionary(s => s[0].Trim(), s => s[1].Trim());
        
    // Update the value of FIRST_RUN
        settings["FIRST_RUN"] = "0";

    // Create a new list to store the updated settings
        var updatedLines = settings.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList();

    // Write the updated settings back to the file
        File.WriteAllLines(settingsFile, updatedLines);

    return true;

    }

}
/*
In this code, after updating the FIRST_RUN value to 0, it creates a new list called updatedLines to store the updated settings. 
It iterates over the settings dictionary, converting each key-value pair back into the original format (key=value), and adds them to the updatedLines list.
Finally, it writes the updatedLines list back to the file, replacing the previous content with only the updated FIRST_RUN value while preserving the rest of the settings as they were.
*/