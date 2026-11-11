# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Workspace-level setup (Docker dev container, commit/PR conventions, modernization plan) lives in
`../CLAUDE.md`. This file covers only what is specific to this repo.

## Commands

Run from the workspace folder (`dotnet/`), never with a host .NET install:

```bash
./dev build Infrastructure.Data.BlobStorage
./dev test  Infrastructure.Data.BlobStorage
./dev exec "dotnet test Infrastructure.Data.BlobStorage/Infrastructure.Data.BlobStorage.sln --filter FullyQualifiedName~WriteJsonAsyncTest"
./dev azurite   # Azurite on localhost:10000-10002, for tests that hit Blob Storage
```

- `GeneratePackageOnBuild` is on: every build also produces `bin/<Config>/Infrastructure.Data.BlobStorage.<Version>.nupkg`.
- No CI, no `publish.yml`, no CHANGELOG or community files yet (#13, #14). Model them on the CosmosDb or ValueObjects repos.

## Architecture

A single NuGet package (`Infrastructure.Data.BlobStorage`) with one class:

- `BlobStorageRepository` (`Repository.cs`) implements `IBlobStorage` and `IDownloadContent`. Both interfaces
  come from the **`Infrastructure.Data.Abstractions` NuGet package**, not from source. Changing a method
  signature means changing that package first (sibling repo `../Infrastructure.Data.Abstractions`).
- Configuration is `IOptions<Settings>` (`Settings.cs`). Two auth modes:
  - `UseDefaultAzureCredential = true` → `BlobServiceClient(new Uri(UrlContainer), new DefaultAzureCredential())`.
  - otherwise → `BlobServiceClient(ConnectionString)`.
  `Container` is the single container name used for all operations.
- `Upload<T>`: a `string` is written as UTF-8 bytes; any other `T` is serialized to JSON (camelCase, indented,
  `UnicodeRanges.All` encoder so accented Portuguese text is not escaped). Upload creates the container if it
  does not exist; `Download`/`DownloadContent`/`Exists` do not.
- Errors in `Download`/`DownloadContent` are logged and swallowed (return `null`). Logging goes through the
  `LoggerMessage.Define` helpers in `LogMessages.cs` (messages in Portuguese).
- `WriteJsonAsync` is `public static` so it can be unit tested. It is the only code covered by the single
  NUnit test; nothing tests against Blob Storage or Azurite yet.

## Known defects (milestone `v10.0.0`)

Remove each line when its issue is closed.

- #4: `Upload<T>` is not `async`: the `try/catch` never sees exceptions thrown by the returned task, and the catch
  path returns a `null` `Task` (awaiting it throws `NullReferenceException`).
- #5: `Exists` blocks on `ExistsAsync()` (sync-over-async); `CreateContainerIfNotExistsAsync` calls sync `Exists()`.
- #6: `MemoryStream`s in the upload path are never disposed.
- #7: Both `LogMessages` events use `EventId(1, "Upload Error")`; the download one is mislabeled.
- The test file has a mangled encoding in the Portuguese sample string (`Servi�o`).
