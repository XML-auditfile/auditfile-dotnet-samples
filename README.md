# auditfile-dotnet-samples

Example programs for [auditfile-dotnet](https://github.com/XML-auditfile/auditfile-dotnet), the streaming reader
for Dutch tax audit files (XAF and related).

The samples use the published NuGet packages, pinned to `0.0.5`.

| Project | Shows |
| ------- | ----- |
| `ReadToMemory/` | Read a file into `MemorySink` and print tables, a row with definitions, the views and the diagnostics |
| `Validate/` | Validate a file (XSD, structure, keys, business rules); exit code 0 when valid |
| `CustomDefinitions/` | Add your own dialect and view from `definitions/`, read a file and run the view |
| `Benchmarks/` | Throughput and allocations of reading a synthetic XAF 3.2 file, unverified and verified (`dotnet run -c Release --project Benchmarks`) |

```
dotnet run --project ReadToMemory -- administratie.xaf
dotnet run --project Validate -- administratie.xaf --en
dotnet run --project CustomDefinitions -- administratie.xaf
```

## Custom dialects and views

`definitions/` holds a dialect and a view in the same format as `dialects.json` and `views/<type>.json` of
[auditfile-spec](https://github.com/XML-auditfile/auditfile-spec): `dialects.json` reads files of a package that writes
XAF 3.2 in a deviating namespace with decimal commas, `views.json` adds the view `account_activity` (lines, debit and
credit per account, optionally over a range of periods). `CustomDefinitions` copies them next to the program and adds
them with `Auditfile.Dialects.Add(AuditfileVersion.Financial.V3_2, json)` and `Auditfile.Views.Add(json)`; after that
the package uses them like its built-in dialects and views. Change the files or add your own: a file with an error is
refused as a whole, with a message that names the problem. CI runs the sample without a file, which checks the
definitions against the package.

## Package versions

The samples use one fixed version of the packages, written in each `.csproj`. `ReadToMemory` references
`XmlAuditfile.Memory`, which depends on `XmlAuditfile`; the other samples reference `XmlAuditfile` directly.
Dependabot (`.github/dependabot.yml`) opens a pull request for each new release, and CI builds every sample
before it is merged, so the samples follow the latest version.

`ReadToMemory` publishes as a single native executable (`PublishAot`), which shows that the reader is
trimming and Native AOT compatible.

## License

Apache License 2.0.
