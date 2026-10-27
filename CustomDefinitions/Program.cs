using System.Globalization;
using XmlAuditfile;
using XmlAuditfile.Memory;

// Adds your own dialect and view, in the format of auditfile-spec, reads an audit file and runs the view.
// Usage: CustomDefinitions <file.xaf> [continuation files...]

// The definitions sit next to the built-in ones of the package; a file is refused as a whole when anything is wrong.
// The reader never opens files itself, so read the JSON and hand over the text (or a stream).
// They are added before the arguments are checked, so a run without arguments still checks definitions/.
var definitions = Path.Combine(AppContext.BaseDirectory, "definitions");
var dialects = Auditfile.Dialects.Add(AuditfileVersion.Financial.V3_2, File.ReadAllText(Path.Combine(definitions, "dialects.json")));
IReadOnlyList<XmlAuditfile.Definitions.ViewDefinition> views;
using (var json = File.OpenRead(Path.Combine(definitions, "views.json")))
{
    views = Auditfile.Views.Add(json);
}

Console.WriteLine($"Added    : dialect {string.Join(", ", dialects.Select(d => d.Name))}; view {string.Join(", ", views.Select(v => v.Name))}");

if (args.Length == 0)
{
    Console.WriteLine("Usage: CustomDefinitions <file.xaf> [continuation files...]");
    return 2;
}

var sink = new MemorySink();
var source = AuditfileSource.FromFactories(args.Select(path => (Path.GetFileName(path), (Func<Stream>)(() => File.OpenRead(path)))).ToArray());
var report = await Auditfile.ReadAsync(source, sink);

// An added dialect is recognized like a built-in one: by namespace or by header/softwareDesc.
Console.WriteLine($"Version  : {report.Schema?.VersionKey ?? "-"} ({report.Schema?.Source})");
Console.WriteLine($"Dialect  : {report.Schema?.Dialect ?? "-"}");
Console.WriteLine($"Loaded   : {report.Loaded}");

// An added view is found like a built-in one; a view that does not fit the version reports XAF-Q001.
if (report.Loaded && report.Schema?.Version?.FindView("account_activity") is { } view)
{
    // Parameters filter the input rows; null leaves a filter out.
    var parameters = new Dictionary<string, object?> { ["period_from"] = null, ["period_to"] = null };
    var result = sink.Run(view, parameters, TextLanguage.English);
    Console.WriteLine();
    Console.WriteLine($"{view.Label?.Get(TextLanguage.English) ?? view.Name} ({result.Rows.Count} rows)");
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"  {diagnostic}");
    }

    PrintTable(result, maxRows: 20);
}

Console.WriteLine();
foreach (var diagnostic in report.Diagnostics)
{
    Console.WriteLine(diagnostic);
}

Console.WriteLine(report.IsValid ? "Valid." : "Not valid.");
return report.IsValid ? 0 : 1;

static void PrintTable(ViewResult result, int maxRows)
{
    const int width = 16;
    var numeric = result.Columns.Select((_, i) => result.Rows.Any(r => r[i] is decimal or long)).ToArray();

    string Line(IEnumerable<string> values) => "  " + string.Join("  ", values.Select((v, i) =>
    {
        v = v.Length > width ? v[..(width - 1)] + "…" : v;
        return numeric[i] ? v.PadLeft(width) : v.PadRight(width);
    }));

    Console.WriteLine(Line(result.Columns.Select(c => c.Label?.Get(TextLanguage.English) ?? c.Name)));
    foreach (var row in result.Rows.Take(maxRows))
    {
        Console.WriteLine(Line(row.Select(v => v is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : v?.ToString() ?? "")));
    }

    if (result.Rows.Count > maxRows)
    {
        Console.WriteLine($"  … {result.Rows.Count - maxRows} more");
    }
}
