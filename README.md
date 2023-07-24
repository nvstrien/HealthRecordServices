Certainly, here's the updated README with the SQL examples:

---

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

### SQL Examples

#### Subsumption Test Example
```sql
SELECT 
tc.SourceId,
ds.Term as SourceTerm,
tc.DestinationId,
dt.Term as TargetTerm
FROM TransitiveClosure tc
LEFT JOIN Description ds
ON
tc.SourceId = ds.ConceptId
LEFT JOIN Description dt
ON
tc.DestinationId = dt.ConceptId
WHERE
tc.SourceId = 10811121000119102
AND tc.DestinationId =  10811201000119102
```
#### Descendent Example (using Descriptions in release sct2_Description_Full-nl_NL1000146_20230331.txt):
```sql
SELECT 
tc.SourceId,
ds.Term as SourceTerm,
tc.DestinationId,
dt.Term as TargetTerm
FROM TransitiveClosure tc
LEFT JOIN Description ds
ON
tc.SourceId = ds.ConceptId
LEFT JOIN Description dt
ON
tc.DestinationId = dt.ConceptId
WHERE
tc.DestinationId =  74732009
AND ds.LanguageCode = "nl"
AND dt.LanguageCode = "nl"
AND SourceTerm LIKE "%depr%"
GROUP BY SourceTerm
```
Note: You can view the contents of the SQLite database using a SQLite database viewer such as [DB Browser for SQLite](https://sqlitebrowser.org/).

## Contributing

Contributions are welcome! Please read the [contributing guide](CONTRIBUTING.md) to get started.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

---

