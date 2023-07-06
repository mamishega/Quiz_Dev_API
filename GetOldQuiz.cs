using Npgsql;

internal partial class Program
{
    public static string? GetAttemptedQuiz(string loginid, string connectionString)
    {
        // Check that a value is supplied before attempting to execute the query
        if (string.IsNullOrEmpty(loginid))
            return ErrorHandler("Please supply a Login ID");

        // Establish a connection to the PostgreSQL database using the provided connection string
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open(); // Open the database connection

            // Define the SQL statement to retrieve attempted quiz data
            string sqlstatement = @"SELECT qq.question_id, qq.question_text, qo.option_name, qo.option_text
                                    FROM quiz_questions qq
                                    INNER JOIN quiz_options qo ON qq.question_id = qo.question_id
                                    WHERE qq.question_id IN (SELECT question_id FROM quiz_history WHERE login_id ILIKE @loginid);";

            // Define the SQL query to retrieve quiz data from the database
            using (NpgsqlCommand command = new NpgsqlCommand(sqlstatement, connection))
            {
                command.Parameters.AddWithValue("@loginid", NpgsqlTypes.NpgsqlDbType.Text, loginid);

                // Send the command to execute and return the output in JSON format
                return FormatQuizToJson(command);
            }
        }
    }
}