using Npgsql;
using BCrypt.Net;
internal partial class Program
{
    public static async Task<bool> UserLogin(string LoginId, string Password, string connectionString)
    {

        try
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                //Hash the password using bcrypt
                string hashedPassword =BCrypt.Net.BCrypt.HashPassword(Password, BCrypt.Net.BCrypt.GenerateSalt(12));
    

                string SqlStatement = "SELECT COUNT(*) FROM quiz_users WHERE login_id = @LoginId AND password_hash = @Password";

                using (NpgsqlCommand command = new NpgsqlCommand(SqlStatement, connection))
                {
                    command.Parameters.AddWithValue("@LoginId", NpgsqlTypes.NpgsqlDbType.Varchar, LoginId);
                    command.Parameters.AddWithValue("@Password", NpgsqlTypes.NpgsqlDbType.Varchar,hashedPassword);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        // Login successful
                        return true;
                    }
                    else
                    {
                        // Login failed
                        return false;
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            // Handle PostgreSQL-related exceptions
            ErrorHandler($"An error occurred during login: {ex.Message}");
            throw; // Re-throw the exception if necessary
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            ErrorHandler($"An unexpected error occurred during login: {ex.Message}");
            throw; // Re-throw the exception if necessary
        }
    }
}

