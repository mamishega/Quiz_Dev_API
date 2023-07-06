using Newtonsoft.Json;
using Npgsql;
internal partial class Program
{
    public static string GetAllUsers(string connectionString)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open(); // Establish a connection to the database and open it

            string sqlstatement = "SELECT login_id, first_name, last_name FROM quiz_users ORDER BY login_id";

            using (NpgsqlCommand command = new NpgsqlCommand(sqlstatement, connection))
            {
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    List<object> userList = new List<object>(); // Create a list to store user objects

                    while (reader.Read()) // Iterate through each row in the result set
                    {
                        string loginid = reader.GetString(0); // Retrieve the login ID from the first column
                        string firstName = reader.GetString(1); // Retrieve the first name from the second column
                        string lastName = reader.GetString(2); // Retrieve the last name from the third column

                        var userObject = new // Create a JSON object for each row
                        {
                            LoginID = loginid,
                            FirstName = firstName,
                            LastName = lastName
                        };

                        userList.Add(userObject); // Add the user object to the list
                    }

                    string jsonResponse = JsonConvert.SerializeObject(userList); // Serialize the list of user objects to a JSON array
                    return jsonResponse; // Return the JSON response
                }
            }
        }
    }

}