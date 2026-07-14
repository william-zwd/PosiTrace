# PosiTrace

PosiTrace is a .NET 10 web application that provides position-tracking functionality (API and/or web UI). This repository contains the source code, controllers, and configuration for building, running, and extending the PosiTrace service.

This README covers getting started, building, running, testing, and contributing. It is written to be usable from both Visual Studio and the dotnet CLI.

## Table of Contents
- Project overview
- Requirements
- Getting started
- Build & run (CLI)
- Run in Visual Studio
- Configuration
- API
- Project structure
- Testing
- Contributing
- License

## Project overview

PosiTrace exposes endpoints for recording and retrieving positional data. The primary API surface is implemented in the Controllers folder (for example: `src/Controllers/PosiTraceController.cs`). The project follows a typical ASP.NET Core Web API layout and targets .NET 10.

## Requirements

- .NET 10 SDK (matching your environment)
- Visual Studio 2026 (18.7.0) or later (optional) or any editor that supports .NET 10
- PowerShell (pwsh) or your preferred shell for CLI commands

Verify your SDK by running:

```pwsh
dotnet --info
```

## Getting started (clone)

Clone the repository (if you haven't already):

```pwsh
git clone https://github.com/william-zwd/PosiTrace.git
cd PosiTrace
```

Open the solution in Visual Studio by double-clicking `PosiTrace.sln` or use the CLI to list projects:

```pwsh
dotnet sln list
```

## Build & run (CLI)

From the repository root you can build and run using the dotnet CLI.

Build the solution:

```pwsh
dotnet build PosiTrace.sln -c Release
```

Run the API project (common project paths: `src/` folder contains application projects). Example:

```pwsh
cd src
dotnet run --project PosiTrace
```

By default ASP.NET Core launches and listens on configured URLs (see `appsettings.json` and `Properties/launchSettings.json` for local ports). Open a browser or use curl to verify the health endpoint or API controllers.

## Run in Visual Studio

1. Open `PosiTrace.sln` in Visual Studio 2026.
2. Set the startup project (right-click the desired project in Solution Explorer > "Set as Startup Project").
3. Press F5 to run with the debugger or Ctrl+F5 to run without debugging.

## Configuration

Configuration is handled via standard ASP.NET Core patterns (appsettings.json, environment variables, user secrets for local development). Typical configuration keys you may need to set:

- ConnectionStrings: for any database dependencies
- Logging: logging level and providers
- ASPNETCORE_ENVIRONMENT: Development/Production

If your project uses secrets or external services, add them to `appsettings.Development.json` or set environment variables before running.

Example (PowerShell):

```pwsh
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src/PosiTrace
```

## API

The primary API endpoints are implemented in `src/Controllers`. The main controller is `PosiTraceController` (src/Controllers/PosiTraceController.cs). The controller exposes a single POST endpoint that accepts an array of address strings and returns cached or freshly-resolved geocoding data.

Base route
- Route template: [Route("[controller]")] — the controller name is used as the segment. By default the controller is named PosiTrace, so the endpoint URL is:
  - POST /PosiTrace

Content-Type
- Request and response bodies use application/json

Authentication
- No authentication is implemented by default.

Detailed endpoint: POST /PosiTrace
- Purpose: accept a list of free-form address strings, attempt to resolve geocoding using the OpenStreetMap Nominatim service, cache results in a local SQLite database, and return an array of StreetAddress objects.

Request
- Body: JSON array of strings. Example:

```json
[
  "5172 Kingsway, Unit 250, Burnaby",
  "514-12677 110A Avenue"
]
```

Validation
- If the request body is null or the array is empty, the controller returns HTTP 400 Bad Request with the text "No address.".

Behavior / Processing
- The controller uses an EF Core DbContextFactory (AppDBContext) and ensures the database is created on demand. The StreetAddresses table uses Address (string) as the primary key.
- For each address in the request:
  1. The address is normalized using StreetAddress.RemoveAUS (removes Australian-style tokens such as Apt/Unit/Suite, trims numeric prefixes, and joins comma-separated tokens).
  2. The cache (SQLite via AppDBContext) is checked for an existing entry using the normalized address as the key.
  3. If found in cache, the cached StreetAddress is returned for that address.
  4. If not found, the controller calls NominatimService.GetGeoCodingAsync(normalizedAddress) to query Nominatim (OpenStreetMap) using geocodejson format. The service sets a User-Agent header and requests JSON.
  5. If Nominatim returns features, the controller serializes and stores the returned geocoding payload in the GeoCoding field.
  6. If Nominatim returns no features (empty string), the controller attempts to extract a Canadian postal code from the original address using StreetAddress.PostalCode and retries geocoding by postal code. The controller annotates GeoCoding with "Using PostalCode <code>" when applicable.
  7. The controller persists (context.StreetAddresses.Add) and SaveChanges() for each new record.
  8. The controller waits 1 second (Thread.Sleep(1000)) after performing the external request and also in certain fall-back branches to avoid rapid-fire requests.

Responses
- 200 OK: on success, returns a JSON array of StreetAddress objects. Each object has:
  - Address (string) — the normalized address used as the cache key
  - GeoCoding (string|null) — either an empty string, a text annotation (e.g., "Using PostalCode ..."), or a JSON-serialized array of geocodingInner objects from the Nominatim geocodejson response

Example response (cached or new):

```json
[
  {
	"Address": "5172 Kingsway, Unit 250, Burnaby",
	"GeoCoding": "[{\"place_id\":12345,\"label\":\"5172 Kingsway, Burnaby\", ...}]"
  },
  {
	"Address": "514-12677 110A Avenue",
	"GeoCoding": "Using PostalCode V5A 1A1 [{\"place_id\":12346,...}]"
  }
]
```

- 400 Bad Request: when the request body is missing or empty. Body contains the plain text message "No address.".

Notes about error handling
- NominatimService returns null when an HttpRequestException occurs. The controller does not explicitly check for null in all branches; a null GeoCoding may be stored in the database and returned. Consider adding explicit error handling if you need to distinguish transient network errors from "no results".

Rate limiting and usage policy
- The controller and service implement a simple 1-second delay between calls (Thread.Sleep). This is a minimal approach toward being polite with Nominatim. Nominatim usage policy requires a descriptive User-Agent and rate limiting for automated queries. See https://operations.osmfoundation.org/policies/nominatim/ for details. The code sets a User-Agent in NominatimService.

Models
- StreetAddress (src/Models/StreetAddress.cs)
  - Address (string) — primary key, not auto-generated
  - GeoCoding (string) — holds the geocoding result or annotation
  - Static helpers:
	- RemoveAUS(string) — normalizes address tokens
	- PostalCode(string) — extracts a Canadian postal code (pattern used in code)

- GEOCode / geocodingInner (src/Models/GEOCode.cs)
  - GEOCode mirrors the geocodejson structure returned by Nominatim: top-level object with features[]. Each feature contains properties.geocoding which is mapped to geocodingInner (fields include place_id, label, name, postcode, street, city, state, country, country_code, and admin objects).

Examples

Using curl (replace port / host as needed):

```bash
curl -X POST http://localhost:5000/PosiTrace \
  -H "Content-Type: application/json" \
  -d '["5172 Kingsway, Unit 250, Burnaby"]'
```

Using PowerShell Invoke-RestMethod:

```pwsh
Invoke-RestMethod -Uri "https://localhost:5001/PosiTrace" -Method Post -Body (@('5172 Kingsway, Unit 250, Burnaby') | ConvertTo-Json) -ContentType 'application/json'
```

Implementation notes and suggested improvements
- The controller currently creates an HttpClient directly (new HttpClient()) and constructs NominatimService with it. Prefer registering and injecting a typed or named HttpClient (Program.cs already defines a named client "PosiTraceClient" with resilience handlers) and inject NominatimService via DI.
- Replace Thread.Sleep with asynchronous delays (await Task.Delay(1000)) so request threads are not blocked.
- Add explicit handling for NominatimService returning null to distinguish network errors from "no results".
- Consider bulk caching logic to batch DB writes and reduce frequent SaveChanges calls.

Configuration keys referenced by the API
- Connection string: DefaultConnection (expected to be configured in appsettings.json). Program.cs registers an AddDbContextFactory<AppDBContext> using this connection string and UseSqlite.

Troubleshooting
- If you see repeated null GeoCoding or HttpRequestException messages, ensure outbound HTTPS access to nominatim.openstreetmap.org and confirm the User-Agent header complies with Nominatim policy.
- If the app fails to create the SQLite DB, verify the connection string and file system permissions.

## Project structure

- src/                - Application projects and source code
  - Controllers/      - API controllers (e.g. PosiTraceController.cs)
  - Models/           - Domain and DTO models
  - Services/         - Business logic and services
  - Program.cs        - Application bootstrap
  - appsettings.json  - Configuration
- tests/              - Unit and integration tests (if present)
- PosiTrace.sln       - Solution file
- README.md           - This file

Note: file names and folders may vary slightly; inspect the `src/` folder for actual project names and adjust commands to target the correct project.

## Testing

If the repository contains tests, run them with the dotnet test command:

```pwsh
dotnet test
```

To run a specific test project:

```pwsh
dotnet test tests/YourTestProject.csproj
```

## Contributing

Contributions are welcome. Please follow these guidelines:

1. Fork the repository and create a feature branch: `git checkout -b feat/your-feature`.
2. Write or update tests for any new behavior.
3. Run the solution and tests locally.
4. Create a pull request describing your changes.

Coding style: follow the existing style in the repository. Keep changes focused and well-documented.

## Common troubleshooting

- If build fails due to SDK mismatch, install the .NET 10 SDK or update global.json (if present) to match your installed version.
- If ports are in use, update `launchSettings.json` or the Kestrel configuration in `appsettings.json`.

## License

If this repository should include a license, add a LICENSE file at the repository root. If no license is present, the default is that all rights are reserved by the repository owner.

## Contact

For questions, open an issue in the repository.

---

If you would like the README to include specific API routes, sample request/response bodies, or developer notes extracted from the code (for example method names, DTO fields), tell me and I will scan the repository and generate more detailed API documentation.
