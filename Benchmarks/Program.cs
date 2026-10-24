using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(XmlAuditfile.Benchmarks.ReadBenchmarks).Assembly).Run(args);
