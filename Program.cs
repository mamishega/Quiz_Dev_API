internal partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        var app = builder.Build();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

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

  //  Insert user to quiz users table. Using query parameters in the URL eg: http://localhost:5000/adduser?loginid=&firstname=&lastname=
        app.MapPost("/adduser",(string LoginId, string FirstName, string LastName, string Password) => AddUser(LoginId ?? string.Empty, FirstName ?? string.Empty, LastName ?? string.Empty, Password ?? string.Empty, connectionString) );      

//Login endpoint URL eg: http://localhost:5000/userlogin?login_id=&password_hash=
        app.MapPost("/userlogin", (string Login_Id,string Password) => UserLogin(Login_Id ?? string.Empty, Password ?? string.Empty, connectionString));

//  Update user to quiz users table. Using query parameters in the URL eg: http://localhost:5000/updateuser?loginid=anhnguyen&firstname=anh&lastname=nguyen&password
        app.MapPut("/updateuser",(string LoginId, string FirstName, string LastName, string Password) => UpdateUser(LoginId ?? string.Empty, FirstName ?? string.Empty, LastName ?? string.Empty, Password ?? string.Empty, connectionString));
//activate users to get access to quiz http://localhost:5000/activateuserstatus?login_id=
        app.MapPut("/activateuserstatus", (string Login_Id) => ActivateUserStatus(Login_Id ?? string.Empty,connectionString));   
//deactivate users to get access to quiz http://localhost:5000/deactivateuserstatus?login_id=
        app.MapPut("/deactivateuserstatus", (string Login_Id) => DeactivateUserStatus(Login_Id ?? string.Empty,connectionString));   
        
        
        
        

        


        app.Run();
    }
}