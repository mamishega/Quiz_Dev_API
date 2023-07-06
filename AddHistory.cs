using Npgsql;

internal partial class Program
{
    public static async Task<string?> AddHistory(string loginid, int questionid, char optionname, string connectionString)
    {
    // NOTE: No need to check values are present in passed parameters as it's already done in the route statement

    // Establish a connection to the database using the provided connection string
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
        // Open the database connection asynchronously
            await connection.OpenAsync();

        // Define the SQL statement to insert the history into the 'quiz_history' table
            string sqlstatement = "INSERT INTO quiz_history (login_id, question_id, option_name) VALUES (@loginid, @questionid, @optionname)";

        // Create a new NpgsqlCommand object with the SQL statement and the database connection
            using (NpgsqlCommand command = new NpgsqlCommand(sqlstatement, connection))
            {
            // Add parameters to the command to prevent SQL injection
                command.Parameters.AddWithValue("@loginid", NpgsqlTypes.NpgsqlDbType.Varchar, loginid);
                command.Parameters.AddWithValue("@questionid", NpgsqlTypes.NpgsqlDbType.Integer, questionid);
                command.Parameters.AddWithValue("@optionname", NpgsqlTypes.NpgsqlDbType.Char, optionname);

            // Execute the command and retrieve the scalar result asynchronously
                await command.ExecuteScalarAsync();
            }

        // Call the CheckAnswer method to check the correctness of the answer
            return await CheckAnswer(questionid, optionname, connectionString);
        }
    }
    
}