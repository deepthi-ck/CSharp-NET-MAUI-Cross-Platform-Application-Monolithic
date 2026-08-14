# C# .NET MAUI Cross-Platform Application — Scenario 1 Monolithic

Minimal MAUI-shaped monolith: one repo, one customer .NET version per branch, flat single module. UI/ViewModels and shared service use the **same** version. This is **not** Scenario 2, **not** gRPC, **not** Blazor WASM.

Inspired conceptually by [dotnet/maui](https://github.com/dotnet/maui). The MAUI product tree is **not** cloned. Headless host runs when a display runtime is unavailable.

C# BCL built-ins (`Dictionary` / `ConcurrentDictionary` / `ObservableCollection`, LINQ, `DateTimeOffset` / `TimeSpan`, `string.IsNullOrWhiteSpace`, `Task`, exceptions, `System.Text.Json`) are **required** for PUT/GET/DELETE/TTL/eviction/UI binding.

## Branches (exactly 8)

| Branch | Customer Version | MSBuild TFM |
|---|---|---|
| `C#_net6.0` | 6 | net6.0 |
| `C#_net7.0` | 7 | net7.0 |
| `C#_net8.0` | 8 | net8.0 |
| `C#_net9.0` | 9 | net9.0 |
| `C#_net10.0` | 10 | net10.0 |
| `C#_net42.0` | 42.0 (named) | net462 |
| `C#_net472.0` | 4.7.2 | net472 |
| `C#_net648.0` | 648.0 (named, Framework 4.8) | net48 |

## Build

```bash
python build.py
dotnet run --project src/CSharpMauiMonolith.csproj
```

Operator UI (page-to-page, also mirrors AppShell flyout routes):

- `http://127.0.0.1:5082/` Dashboard
- `/resources.html` Resource PUT/GET/DELETE (`product:1001` / `Visvantha`)
- `/stats.html` Statistics
- `/nodes.html` Partition nodes
- `/health.html` Health
- `/version.html` Customer version

```bash
dotnet run --project src/CSharpMauiMonolith.csproj -- --self-test
```

See `quality/README.md` and `docs/version-matrix.md`.
