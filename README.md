# SNOMED2SQLITE

A tool to convert SNOMED-CT RF2 release files to a SQLite database.

## Features

1. Supports conversion of the following RF2 files:
    - Concept Full
    - Description Full (including localised files)
    - Relationship Full

2. Automatically generates a transitive closure table for the relationships.

3. Provides a user-friendly console-based interface.

## Prerequisites

- .NET 5.0 SDK or later

## Usage

### Console Application

1. Clone the repository: `git clone https://github.com/yourusername/SNOMED2SQLITE.git`
2. Navigate to the project directory: `cd SNOMED2SQLITE`
3. Build the project: `dotnet build`
4. Run the application: `dotnet run --project SnomedToSQLite/SnomedToSQLite.csproj`

The console will guide you through the process. You need to provide the path to your RF2 release files when prompted. These files are not included in the distribution (see: https://www.snomed.org/get-snomed if you don't have them already).

### As a library

You can also use this project as a library in your own .NET project. Simply add a reference to the SnomedToSQLite project, and you can call its public methods in your own code.

```csharp
var converter = new SnomedToSQLiteConverter();
await converter.ConvertRf2ToSQLite("path/to/your/rf2/files");
```

## Contributing

Contributions are welcome! Please read the [contributing guide](CONTRIBUTING.md) to get started.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

---
