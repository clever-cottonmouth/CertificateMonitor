# Certificate Monitor

Certificate Monitor is a .NET 6.0 application designed to monitor and log certificate information from the current user's certificate store. It periodically captures snapshots of active certificates and logs them into a MySQL database.

## Features

- Monitors certificates in the `CurrentUser` store.
- Logs certificate details such as timestamp, process name, window title, certificate subject, and thumbprint.
- Uses a MySQL database for storing logs.
- Configurable connection string via `appsettings.json`.

## Project Structure

- **CertificateMonitor**: Main application project.
  - `Program.cs`: Entry point of the application.
  - `Service/DatabaseService.cs`: Handles database operations.
  - `Service/CertificateService.cs`: Handles certificate monitoring and logging.
  - `Helpers/CertificateHelper.cs`: Utility methods for working with certificates.
  - `Models/CertificateLog.cs`: Data model for certificate logs.
  - `config/appsettings.json`: Configuration file for database connection.
- **CertificateMonitorDoc**: Documentation or additional project files.
- **Table/Table.sql**: SQL script for creating the database and table structure.

## Prerequisites

- .NET 6.0 SDK
- MySQL Server

## Setup

1. Clone the repository.
2. Open the solution file `CertificateMonitor.sln` in Visual Studio.
3. Update the connection string in `config/appsettings.json` to match your MySQL server credentials.
4. Execute the SQL script `Table/Table.sql` to create the database and table.
5. Build and run the project.

## Usage

1. The application starts a timer that checks certificates every 10 seconds.
2. Logs are saved to the `certificate_logs` table in the MySQL database.
3. Press `Enter` to stop the application.

## Dependencies

- [Microsoft.Diagnostics.Tracing.TraceEvent](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent)
- [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration)
- [MySql.Data](https://www.nuget.org/packages/MySql.Data)
- [PuppeteerSharp](https://www.nuget.org/packages/PuppeteerSharp)
- [Selenium.WebDriver](https://www.nuget.org/packages/Selenium.WebDriver)
- [Topshelf](https://www.nuget.org/packages/Topshelf)

## License

This project is licensed under the MIT License.