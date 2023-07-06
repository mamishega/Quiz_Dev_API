using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;

internal partial class Program
{
    public static async Task<dynamic> GenerateQuiz(string apiKey, string apiURL, string prompt, string model, int max_tokens, double temperature, int first_run, string connectionString)
    {
    // Variable to store the generated quiz content
        string? getQuizContent = string.Empty;
    // Variable to store the response status code
        string? responseStatusCode = string.Empty;

    // Create an AuthenticationHeaderValue with the API key
        var headers = new AuthenticationHeaderValue("Bearer", apiKey);

    // Create data object to hold prompt, model, max_tokens, and temperature
        var data = new
        {
            prompt,
            model,
            max_tokens,
            temperature
        };

    // Serialize the data object to JSON
        string json = JsonConvert.SerializeObject(data);

    // Create a new HttpClient
        using (var client = new HttpClient()) 
        {
        // Set the Authorization header for the HttpClient
            client.DefaultRequestHeaders.Authorization = headers; 

        // Send a POST request to the OpenAI API with the JSON data
            var response = await client.PostAsync(apiURL, new StringContent(json, Encoding.UTF8, "application/json"));

        // Get the response status code
            responseStatusCode = response.StatusCode.ToString(); 

        // If the response is successful
            if (response.IsSuccessStatusCode)
            {

            // Read the response content as string
                string responseContent =  CheckAndFixJson(await response.Content.ReadAsStringAsync());

            // Try to deserialise the response content
                try
                {
                // Deserialize the response content as dynamic object, or use a new ExpandoObject if null
                    dynamic result = JsonConvert.DeserializeObject(responseContent) ?? new System.Dynamic.ExpandoObject();

                // Check generated quiz is present in result and get the content if it does
                    if (result.choices != null && result.choices.Count > 0 && result.choices[0].text != null) getQuizContent = result.choices[0].text;
                }
            // Handle the exception during deserialisation
                catch (Exception ex)
                {
                // Return friendly error message using the error handler routine to return error in JSON format
                    return ErrorHandler($"An error occurred during deserialization from OpenAI: {ex.Message}");
                }
            }
        // Response is not succesful. Statuscocde indicates an error. Display the statuscode
            else
            {
            // Return friendly error message using the error handler routine to return error in JSON format
                return ErrorHandler($"There was a problem communicating with the API. Status code: {responseStatusCode}");
            }
        }

    // Create tables if first_run is set to 1 ie tables does not exist
        if (first_run==1) CreateTablesIfNotExists(connectionString);

    // Parse the JSON output and add it to Postgres
        AddQuiz(getQuizContent, prompt, connectionString);

    // Mark the duplicate entries as true
        MarkDuplicate(connectionString);

    // return the Quiz Content
        return getQuizContent;
    }
}