using Newtonsoft.Json;
using Npgsql;
internal partial class Program
{

    public static string GetUserByLoginID(string loginid, string connectionString)
    {
        // Check that a value is supplied before attempting to execute the query
        if (string.IsNullOrEmpty(loginid))
        {
            return ErrorHandler("Please supply a Login ID");
        }

        // Establish a connection to the database using the provided connection string
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            // Open the database connection
            connection.Open();

            // Define the SQL statement to retrieve user information from the quiz_users table based on login ID
            string sqlstatement = "SELECT login_id, first_name, last_name FROM quiz_users WHERE login_id ILIKE @loginID";

            // Create a new NpgsqlCommand object with the SQL statement and the database connection
            using (NpgsqlCommand command = new NpgsqlCommand(sqlstatement, connection))
            {
                // Add the loginid as a parameter to the command
                command.Parameters.AddWithValue("@loginID", NpgsqlTypes.NpgsqlDbType.Text, loginid);

                // Execute the command and retrieve the data reader
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    // Check if there is a row in the result set
                    if (reader.Read())
                    {
                        // Retrieve the login ID, first name, and last name from the reader
                        string loginId = reader.GetString(0);
                        string firstName = reader.GetString(1);
                        string lastName = reader.GetString(2);

                        // Create an anonymous object to store the user information
                        var userObject = new
                        {
                            LoginID = loginId,
                            FirstName = firstName,
                            LastName = lastName
                        };

                        // Serialize the user object to JSON
                        string jsonResponse = JsonConvert.SerializeObject(userObject);

                        // Return the JSON response
                        return jsonResponse;
                    }
                }
            }
        }

        // Return an error message if the user with the given login ID was not found
        return ErrorHandler($"{loginid} not found.");
    }

}