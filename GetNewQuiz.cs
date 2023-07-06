using Npgsql;

/* TODOS:

1. What if no new questions exists ie user has done all the quiz.

*/
internal partial class Program
{
    public static string? GetUnattemptedQuiz(string loginid, string connectionString)
    {
        // Check that a value is supplied before attempting to execute the query
        if (string.IsNullOrEmpty(loginid))
            return ErrorHandler("Please supply a Login ID");

        // Establish a connection to the PostgreSQL database using the provided connection string
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open(); // Open the database connection

            // Define the SQL statement to retrieve unattempted quiz data
            string sqlstatement = @"SELECT qq.question_id, qq.question_text, qo.option_name, qo.option_text
                                    FROM quiz_questions qq
                                    LEFT JOIN quiz_history qh ON qq.question_id = qh.question_id AND qh.login_id ILIKE @loginid
                                    INNER JOIN quiz_options qo ON qq.question_id = qo.question_id
                                    WHERE qh.question_id IS NULL;";

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