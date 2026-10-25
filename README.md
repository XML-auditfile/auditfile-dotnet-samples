# auditfile-dotnet-samples

Example programs for [auditfile-dotnet](https://github.com/XML-auditfile/auditfile-dotnet), the streaming reader
for Dutch tax audit files (XAF and related).

The samples use the published NuGet packages, pinned to `0.0.4`.

| Project | Shows |
| ------- | ----- |
| `ReadToMemory/` | Read a file into `MemorySink` and print tables, a row with definitions, the views and the diagnostics |
| `Validate/` | Validate a file (XSD, structure, keys, business rules); exit code 0 when valid |
| `Benchmarks/` | Throughput and allocations of reading a synthetic XAF 3.2 file, unverified and verified (`dotnet run -c Release --project Benchmarks`) |

```
dotnet run --project ReadToMemory -- administratie.xaf
dotnet run --project Validate -- administratie.xaf --en
```

## Package versions

The samples use one fixed version of the packages, written in each `.csproj`. `ReadToMemory` references
`XmlAuditfile.Memory`, which depends on `XmlAuditfile`; the other samples reference `XmlAuditfile` directly.
Dependabot (`.github/dependabot.yml`) opens a pull request for each new release, and CI builds every sample
before it is merged, so the samples follow the latest version.

`ReadToMemory` publishes as a single native executable (`PublishAot`), which shows that the reader is
trimming and Native AOT compatible.

## License

Apache License 2.0.
