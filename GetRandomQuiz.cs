using Newtonsoft.Json;
using Npgsql;

internal partial class Program
{
    public static string? GetRandomQuiz(string connectionString)
    {
        // Establish a connection to the PostgreSQL database using the provided connection string
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open(); // Open the database connection

            // Define the SQL statement to retrieve the question IDs from the quiz_questions table where NOT duplicate
            string sqlstatement = "SELECT question_id FROM quiz_questions WHERE NOT duplicate;";

            // Create a new NpgsqlCommand object with the SQL statement and the database connection
            using (NpgsqlCommand command = new NpgsqlCommand(sqlstatement, connection))
            {
                // Execute the command and retrieve a data reader
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    // Create a list to store the question IDs
                    List<object> idList = new List<object>();

                    // Iterate through each row in the result set
                    while (reader.Read())
                    {
                        // Retrieve the question ID from the reader
                        int id = reader.GetInt32(0);

                        // Create a JSON object for each question ID
                        var idObject = new
                        {
                            ID = id
                        };

                        // Add the question ID object to the list
                        idList.Add(idObject);
                    }

                    // Pick a random question ID from the list
                    var random = new Random();
                    int index = random.Next(idList.Count);

                    // Serialize the randomly chosen question ID object to a JSON string
                    string jsonResponse = JsonConvert.SerializeObject(idList[index]);

                    // Return the JSON response
                    return jsonResponse;
                }
            }
        }
    }

}