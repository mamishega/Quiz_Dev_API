using Npgsql;

internal partial class Program
{

    /*
    This will only work if the question_text are exactly identical.
    The following example questions are not identical: "What is the capital of France?" and "What is the capital city of France?"
    Therefore, the ability to mark the question as a dupicate at the front end is still needed.
    */
    public static void MarkDuplicate(string connectionString)
    {
        using var con = new NpgsqlConnection(connectionString);
        con.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = con;

        cmd.CommandText = @"UPDATE quiz_questions
                        SET duplicate = TRUE
                        WHERE question_id NOT IN (
                            SELECT MIN(question_id)
                            FROM quiz_questions
                            GROUP BY question_text)";
        cmd.ExecuteNonQuery();
        con.Close();
        return;
    }
}