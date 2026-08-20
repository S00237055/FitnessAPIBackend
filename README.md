# Fuel Track API

Backend API for my fitness tracking app. Handles user accounts, workout logging,
food logging and AI coaching. Built with ASP.NET Core 8 and Entity Framework Core,
with a SQL Server database.

The mobile app that uses this API is in a separate repo (MobileApp).

## What it does

- User registration and login, with passwords hashed using Argon2id
- JWT authentication, so users can only access their own data
- Logging workouts with exercises, sets, weights and reps
- Logging food with calories and macros
- An exercise catalogue of over 1000 exercises, seeded on first run
- AI coaching endpoints that call Google Gemini

## Requirements

- .NET 8 SDK
- SQL Server (I used SQL Server Express locally)
- A Google Gemini API key if you want the AI features to work

## Running it locally

1. Clone the repo.

2. Open `FitnessAPI/appsettings.json` and set your connection string and keys
   (see the Configuration section below).

3. From the solution folder, run:

   ```
   dotnet restore
   dotnet run --project FitnessAPI
   ```

The database is created automatically the first time it runs. Migrations are
applied on startup, and the exercise catalogue is imported from `exercises.json`
if the table is empty.

Once it's running, open `/swagger` in a browser to see and test the endpoints.

## Configuration

| Setting | What it's for |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `GeminiApiKey` | API key for the Google Gemini AI endpoints |
| `Jwt:Key` | Secret used to sign tokens. Must be at least 32 characters |
| `Jwt:Issuer` | Token issuer. I use `FitnessAPI` |
| `Jwt:Audience` | Token audience. I use `FitnessAppClient` |
| `Jwt:ExpiryMinutes` | How long a token lasts. Set to 10080 (7 days) |

Don't commit real keys. On Azure these are set as application settings instead,
which override whatever is in `appsettings.json`.

## Tests

20 automated tests using xUnit. They run against an in-memory database so they
don't need SQL Server and don't touch any real data.

```
dotnet test
```

For coverage:

```
dotnet test --collect:"XPlat Code Coverage" --settings coverage.runsettings
```

The tests also run automatically on GitHub Actions every time I push. The
workflow is in `.github/workflows/dotnet-tests.yml`.

## Deploying to Azure

I deployed this to Azure App Service.

1. In Visual Studio, right click the FitnessAPI project and choose **Publish**,
   then follow the wizard to create or pick an App Service.

2. In the Azure Portal, go to your App Service, then **Settings > Environment
   variables**, and add these application settings:

   ```
   Jwt__Key
   Jwt__Issuer
   Jwt__Audience
   Jwt__ExpiryMinutes
   GeminiApiKey
   ConnectionStrings__DefaultConnection
   ```

   Note the double underscores. That's how .NET reads nested settings from
   environment variables.

3. Save. The app restarts on its own.

To check it worked, open `/swagger` on the deployed URL and call
`GET /api/Exercises` without logging in. You should get a 401. Log in through
`/api/User/login`, copy the token, click **Authorize**, paste it in, and try
again. This time you should get the exercise list.

### Logging

Application logging is turned on in the portal under **Monitoring > App Service
logs**. Unhandled exceptions are caught by a global handler and written to the
log rather than returned to the caller, so you can see what happened in **Log
stream**.

## Endpoints

| Endpoint | What it does |
|---|---|
| `POST /api/User/register` | Create an account, returns a token |
| `POST /api/User/login` | Log in, returns a token |
| `GET /api/User/{id}` | Get your own profile |
| `PUT /api/User/{id}/profile` | Update weight and goal |
| `GET /api/Exercises` | Get the exercise catalogue |
| `GET /api/Workouts` | Get your workouts |
| `POST /api/Workouts` | Save a workout with its sets |
| `GET /api/FoodLogs/user/{id}` | Get your food diary |
| `POST /api/FoodLogs` | Save a food entry |
| `POST /api/Ai/DietAdvice` | AI diet feedback |
| `POST /api/Ai/WorkoutAdvice` | AI training feedback |
| `POST /api/Ai/WeeklyComparison` | AI week on week comparison |

Everything except register and login needs a token in the `Authorization`
header as `Bearer <token>`. The server works out who you are from the token, so
you can't read or change another user's data by changing the ID in the URL.
