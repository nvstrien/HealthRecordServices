# SNOMED2SQLITE

A tool to convert SNOMED-CT RF2 release files to a SQLite database.

## Features

1. Supports conversion of the following RF2 files:
    - Concept (Full/Snapshot)
    - Description (Full/Snapshot; including localised files)
    - Relationship (Full/Snapshot)
    - Language Refset (Full/Snapshot)

2. Automatically generates a transitive closure table for the |is a| relationships (Snapshot release only).

3. Provides a user-friendly console-based interface.

## Prerequisites

- .NET 7.0 SDK or later

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
#### Descendent Example (using Descriptions in release sct2_Description_Snapshot-nl_NL1000146_20230331.txt):
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

#### Descendent Example with | Preferred (foundation metadata concept) | terms only (using Descriptions in release sct2_Description_Snapshot-nl_NL1000146_20230331 + Refset der2_cRefset_LanguageSnapshot-nl_NL1000146_20230331.txt):
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
LEFT JOIN LanguageRefset ls_source
ON ds.Id = ls_source.ReferencedComponentId
LEFT JOIN LanguageRefset ls_dest
ON dt.Id = ls_dest.ReferencedComponentId
WHERE
tc.DestinationId =  74732009
AND ds.LanguageCode = "nl"
AND dt.LanguageCode = "nl"
AND SourceTerm LIKE "%depr%"
AND ls_source.AcceptabilityId = 900000000000548007
Group BY SourceTerm
```

Note: You can view the contents of the SQLite database and execute queries using a SQLite database viewer such as [DB Browser for SQLite](https://sqlitebrowser.org/).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.txt) file for details.

---

