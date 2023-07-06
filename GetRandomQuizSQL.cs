using Newtonsoft.Json;
using Npgsql;

internal partial class Program
{
    public static string? GetRandomQuizSQL(string connectionString)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open(); // Establish a connection to the PostgreSQL database and open it

            using (NpgsqlCommand command = new NpgsqlCommand("SELECT question_id FROM quiz_questions WHERE NOT duplicate ORDER BY RANDOM() LIMIT 1;", connection))
            {
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read()) // Read the first row returned by the query
                    {
                        int id = reader.GetInt32(0); // Retrieve the question ID from the first column

                        var userObject = new // Create a JSON object
                        {
                            ID = id
                        };

                        string jsonResponse = JsonConvert.SerializeObject(userObject); // Serialize the user object to a JSON string
                        return jsonResponse; // Return the JSON response
                    }

                    return null; // If no row is read, return null
                }
            }
        }
    }

}