using Newtonsoft.Json;
using Npgsql;

internal partial class Program
{
    public static async Task<string?> CheckAnswer(int questionid, char optionname, string connectionString)
    {
    // NOTE: No need to check values are present in passed parameters as it's already done in the route statement and calling method

        // Establish a connection to the database using the provided connection string
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            // Open the database connection asynchronously
            await connection.OpenAsync();
            // Define the SQL statement to retrieve the answer name where the supplied question id matches from the the quiz_answers table
            string sqlstatement = "SELECT answer_name FROM quiz_questions questions INNER JOIN quiz_answers answers ON answers.question_id = @questionid";

            // Create a new NpgsqlCommand object with the SQL statement to retrieve the answer from the database
            using (NpgsqlCommand command = new NpgsqlCommand(sqlstatement, connection))
            {
                // Add the question ID as a parameter to the command
                command.Parameters.AddWithValue("@questionid", NpgsqlTypes.NpgsqlDbType.Integer, questionid);

                // Execute the command and retrieve the data reader asynchronously
                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    // Check if there is a row in the result set
                    if (reader.Read())
                    {
                        // Retrieve the answer name from the first column of the result
                        char answerName = reader.GetChar(0);

                        // Compare the retrieved answer name with the provided option name
                        bool result = answerName == optionname;

                        // Create an anonymous object to store the result
                        var answerObject = new
                        {
                            Result = result
                        };

                        // Serialize the answer object to JSON
                        string jsonResponse = JsonConvert.SerializeObject(answerObject);

                        // Return the JSON response
                        return jsonResponse;
                    }
                }
            }
        }

        // Return an error message if the question ID was not found
        return ErrorHandler($"Question ID {questionid} not found.");
    }


}