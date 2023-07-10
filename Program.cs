internal partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

    // Read the OpenAi Api Key and Database Password from environment variable that you set with dotnet user-secrets set command.
        string openAiApiKey = builder.Configuration["OpenAI:APIKey"] ?? String.Empty;
        string pgsqlDbPassword = builder.Configuration["PGSQL:DbPassword"] ?? String.Empty;

    // Declare a dictionary variable to load and store the settings from Settings.user file
        Dictionary<string, string>? settings = GetSettings(); 
    // Create Connection String for NPGSQL
        string connectionString = $"Host={settings["HOST"]};Username={settings["USERNAME"]};Password={pgsqlDbPassword};Database={settings["DATABASE"]};IncludeErrorDetail=true"; 

    // Root route. The readme file does not exist. This is just a placeholder to show you how to use error handling
        app.MapGet("/", () => GetReadme());


    // Add the OpenAI generated Quiz to database and return it as JSON as well
         app.MapGet("/generatequiz", () =>
        {
            return GenerateQuiz(
        openAiApiKey, 
        settings["APIURL"], 
        settings["PROMPT"], 
        settings["MODEL"],
        int.Parse(settings["MAX_TOKENS"]),
        double.Parse(settings["TEMPERATURE"]),
        int.Parse(settings["FIRST_RUN"]),
        connectionString);
        });



 
    // Display all the users in the table in JSON format eg: http://localhost:5000/getallusers
        app.MapGet("/getallusers", () => GetAllUsers(connectionString)); 

    //  Display specific user info. Using route parameters in the URL eg: http://localhost:5000/getuser/fredkhan
        app.MapGet("/getuser/{loginid}", (string loginid) => GetUserByLoginID(loginid ?? string.Empty, connectionString));

    // Same as above but Using query parameters in the URL eg: http://localhost:5000/getuser?loginid=fredkhan
        app.MapGet("/getuser", (string loginid) => GetUserByLoginID(loginid ?? string.Empty, connectionString));

    //  Insert user to quiz users table. Using query parameters in the URL eg: http://localhost:5000/adduser?loginid=anhnguyen&firstname=anh&lastname=nguyen
        app.MapPost("/adduser", async (context) =>
        {
            // Get the value of the "loginid" parameter from the request query string.
            string? loginid = context.Request.Query["login_id"];

            // Get the value of the "firstname" parameter from the request query string.
            string? firstname = context.Request.Query["first_name"];

            // Get the value of the "lastname" parameter from the request query string.
            string? lastname = context.Request.Query["last_name"];

            // Get the value of the "password" parameter from the request query string.
            string? Password = context.Request.Query["password_hash"];

            // Check if any of the required parameters (loginid, firstname, lastname) are missing or empty.
            if (string.IsNullOrEmpty(loginid) || string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname)|| string.IsNullOrEmpty(Password))
            {
                // Set the HTTP response status code to 400 (Bad Request).
                context.Response.StatusCode = 400;

                // Write an error message to the response.
                await context.Response.WriteAsync("One or more parameters are missing.");

                // Exit the function early.
                return;
            }

            // Call the "AddUser" method, passing the "loginid", "firstname", "lastname", and "connectionString" parameters.
            // Await the method's asynchronous execution and assign the returned value to "result".
            string? result = await AddUser(loginid, firstname, lastname, Password,connectionString);

            // Write the value of "result" to the response, or an empty string if "result" is null.
            await context.Response.WriteAsync(result ?? string.Empty);
        });


    // Display True or False. Using query parameters in the url eg: http://localhost:5000/checkanswer?questionid=21&optionname=b
        app.MapGet("/checkanswer", (int questionid, char optionname) =>
        {
            // Check for blank or null values
            if (questionid <= 0 || optionname == '\0')
            {
                return null;
            }

            // Convert optionname to uppercase
            optionname = Char.ToUpper(optionname);

            // Call the CheckAnswer method
            return CheckAnswer(questionid, optionname, connectionString);
        });



    //  Insert record to quiz history table and returns true or false. Using query parameters in the URL eg: http://localhost:5000/addhistory?loginid=anhnguyen&questionid=11&optionname=d
        app.MapPost("/addhistory", async (context) =>
        {
        // Get the value of the "loginid" parameter from the request query string.
            string? loginid = context.Request.Query["loginid"];

        // Try to parse the value of the "questionid" parameter from the request query string to an integer.
        // If successful, assign the parsed value to the "questionid" variable.
        // If parsing fails, assign a default value of 0 to "questionid".
            int questionid;
            if (!int.TryParse(context.Request.Query["questionid"], out questionid)) questionid = 0;

        // Get the value of the "optionname" parameter from the request query string.
        // If there are multiple values for "optionname", only the first value will be assigned to "optionname".
            string? optionname = context.Request.Query["optionname"].FirstOrDefault();

        // Convert the first character of "optionname" to uppercase and assign it to "optionChar".
        // If "optionname" is null or empty, assign '\0' (null character) to "optionChar".
            char optionChar = !string.IsNullOrEmpty(optionname) ? char.ToUpper(optionname[0]) : '\0';

        // Check that all parameters are provided and question id was successfully parsed and valid
            if (string.IsNullOrEmpty(loginid) || questionid <= 0  || optionChar == '\0')
            {
            // Set the HTTP response status code to 400 (Bad Request).
                context.Response.StatusCode = 400;

            // Write an error message to the response.
                await context.Response.WriteAsync("One or more parameters are missing.");

            // Exit the function early.
                return;
            }

        // Call the "RecordAnswer" method, passing the "loginid", "questionid", "optionChar", and "connectionString" parameters.
        // Await the method's asynchronous execution and assign the returned value to "result".
            string? result = await AddHistory(loginid, questionid, optionChar, connectionString);

        // Write the value of "result" to the response, or an empty string if "result" is null.
            await context.Response.WriteAsync(result ?? string.Empty);
        });


    //Deactivate users to get access to quiz http://localhost:5000/deactivateuser?loginid=moshsh
        app.MapPut("/deactivateuser", (string LoginId) => DeactivateUser(LoginId ?? string.Empty,connectionString));

    // Return json of all questions and options eg: http://localhost:5000/getallquiz
        app.MapGet("/getallquiz", () => GetAllQuiz(connectionString));

    // Return json of unattempted questions and options eg: http://localhost:5000/getnewquiz?loginid=fredkhan
        app.MapGet("/getunattemptedquiz", (string loginid) => GetUnattemptedQuiz(loginid ?? string.Empty, connectionString));

    // Return json of attempted questions and options eg: http://localhost:5000/getnewquiz?loginid=fredkhan
        app.MapGet("/getattemptedquiz", (string loginid) => GetAttemptedQuiz(loginid ?? string.Empty, connectionString));

    // Return json of 1 random question and options eg: http://localhost:5000/getrandomquiz
        app.MapGet("/getrandomquiz", () => GetRandomQuiz(connectionString));

    // Return json of 1 random question and options eg: http://localhost:5000/getrandomquiz
        app.MapGet("/getrandomquizsql", () => GetRandomQuizSQL(connectionString));



    

        //  Update user to quiz users table. Using query parameters in the URL eg: http://localhost:5000/updateuser?loginid=anhnguyen&firstname=anh&lastname=nguyen&password
            app.MapPut("/updateuser", async (context) =>
            {
            // Get the value of the "loginid" parameter from the request query string.
            string? LoginId = context.Request.Query["login_id"];

            // Get the value of the "firstname" parameter from the request query string.
            string? FirstName = context.Request.Query["first_name"];

            // Get the value of the "lastname" parameter from the request query string.
            string? LastName = context.Request.Query["last_name"];

            // Get the value of the "lastname" parameter from the request query string.
            string? Password = context.Request.Query["password_hash"];

            // Check if any of the required parameters (loginid, firstname, lastname) are missing or empty.
            if (string.IsNullOrEmpty(LoginId)||string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Password))
            {
                // Set the HTTP response status code to 400 (Bad Request).
                context.Response.StatusCode = 418;

                // Write an error message to the response.
                await context.Response.WriteAsync("One or more parameters are missing.");

                // Exit the function early.
                return;
            }

            // Call the "UpdateUser" method, passing the "loginid", "firstname", "lastname", and "connectionString" parameters.
            // Await the method's asynchronous execution and assign the returned value to "result".
            string? result = await UpdateUser( LoginId, FirstName, LastName, Password, connectionString);

            // Write the value of "result" to the response, or an empty string if "result" is null.
            await context.Response.WriteAsync(result ?? string.Empty);
        });
        




        app.Run();
    }
}