using XmlAuditfile;

// Validates an audit file: XSD, structure, keys and business rules.
// Usage: Validate <file.xaf> [--en] [--unverified] [--force xaf/3.2]
// Exit code: 0 valid, 1 not valid, 2 usage error.

const string Usage = "Usage: Validate <file.xaf> [--en] [--unverified] [--force xaf/3.2]";

var forceIndex = Array.IndexOf(args, "--force");
var files = args.Where((a, index) => !a.StartsWith("--", StringComparison.Ordinal) && (forceIndex < 0 || index != forceIndex + 1)).ToArray();
if (files.Length == 0 || (forceIndex >= 0 && forceIndex + 1 >= args.Length))
{
    Console.Error.WriteLine(Usage);
    return 2;
}

AuditfileVersion? force = null;
if (forceIndex >= 0)
{
    force = AuditfileVersion.Find(args[forceIndex + 1]);
    if (force is null)
    {
        Console.Error.WriteLine($"Unknown version '{args[forceIndex + 1]}'. Known: {string.Join(", ", AuditfileVersion.All.Select(v => v.Key))}.");
        return 2;
    }
}

var options = new ReadOptions
{
    Language = args.Contains("--en") ? TextLanguage.English : TextLanguage.Dutch,
    Mode = args.Contains("--unverified") ? ReadMode.Unverified : ReadMode.Verified,
    Version = force,
};

// The reader never opens files itself: hand it a stream factory per part.
var source = AuditfileSource.FromFactories(files.Select(path => (Path.GetFileName(path), (Func<Stream>)(() => File.OpenRead(path)))).ToArray());

// Unverified skips the XSD but runs every other check.
var report = await Auditfile.ValidateAsync(source, options);

foreach (var diagnostic in report.Diagnostics)
{
    Console.WriteLine(diagnostic);
}

// The counts include diagnostics left out of a long list.
Console.WriteLine($"{report.Count(DiagnosticSeverity.Error) + report.Count(DiagnosticSeverity.Fatal)} errors, {report.Count(DiagnosticSeverity.Warning)} warnings");
return report.IsValid ? 0 : 1;
