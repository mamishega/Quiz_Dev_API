using Npgsql;

internal partial class Program
{
    public static void CreateTablesIfNotExists(string connectionString)
    {

        using var con = new NpgsqlConnection(connectionString);
        con.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = con;

        cmd.CommandText = @"DROP TABLE IF EXISTS quiz_answers, quiz_history, quiz_options, quiz_questions, quiz_users, quiz_prompts CASCADE";
        cmd.ExecuteNonQuery();
        
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS quiz_prompts (prompt_id SERIAL NOT NULL PRIMARY KEY, 
                prompt_text VARCHAR NOT NULL)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS quiz_questions (question_id SERIAL NOT NULL PRIMARY KEY, 
                question_text VARCHAR NOT NULL,
                prompt_id INT NOT NULL,
                duplicate BOOL DEFAULT FALSE,
                FOREIGN KEY (prompt_id)
                    REFERENCES quiz_prompts (prompt_id)
                    ON DELETE CASCADE
                    ON UPDATE CASCADE)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS  quiz_options (question_id INTEGER NOT NULL, 
                option_name CHAR NOT NULL,
                option_text VARCHAR NOT NULL,
                FOREIGN KEY (question_id)
                    REFERENCES quiz_questions (question_id)
                    ON DELETE CASCADE
                    ON UPDATE CASCADE)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS  quiz_answers (question_id INTEGER NOT NULL,
                answer_name CHAR NOT NULL,
                FOREIGN KEY (question_id)
                    REFERENCES quiz_questions (question_id)
                    ON DELETE CASCADE
                    ON UPDATE CASCADE)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS  quiz_users (login_id VARCHAR NOT NULL UNIQUE,
                first_name VARCHAR NOT NULL,
                last_name VARCHAR NOT NULL)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS  quiz_history (login_id VARCHAR NOT NULL,
                question_id INTEGER NOT NULL,
                option_name CHAR NOT NULL,
                FOREIGN KEY (login_id)
                    REFERENCES quiz_users (login_id)
                    ON DELETE CASCADE
                    ON UPDATE CASCADE)";
        cmd.ExecuteNonQuery();

        con.Close();

    // Set FIRST_RUN=0
        SetSettings();

    }


}
