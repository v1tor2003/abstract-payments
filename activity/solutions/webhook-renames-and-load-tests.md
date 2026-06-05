# Troubleshooting Registry: Webhook Builder Renames and Concurrent Load Testing

## Challenges & Solutions

### 1. SQLite Database Concurrency Locking
- **Problem:** When writing a load test against SQLite, running concurrent writes across multiple threads typically results in `SQLiteException: database is locked` because SQLite does not support concurrent write operations by default.
- **Resolution:** We utilized the single-reader queue architecture of `InMemoryWebhookQueue` (which uses a `System.Threading.Channels` channel configured with `SingleReader = true`). While the HTTP requests are handled concurrently, their actual execution is serialized in a single background worker thread (`WebhookQueueProcessor`), meaning all database updates happen sequentially. In the Arrange phase of the test, initial transaction insertions are done sequentially in a single loop to completely avoid database locking issues.

### 2. CS0103 / Compiler Errors from API Renaming
- **Problem:** Renaming public builder extension APIs risks breaking references in both production configuration (`Program.cs`) and unit tests (`WebhookProcessorTests.cs`).
- **Resolution:** Conducted a comprehensive workspace grep search to identify all occurrences of `.Endpoint` and `.ListenFor` in C# files, updating them simultaneously to ensure that all projects build successfully in a single unified step.

### 3. LaTeX Verbatim Formatting
- **Problem:** Code block changes in LaTeX verbatim environments must match the C# API exactly.
- **Resolution:** Updated `3_development.tex` verbatims and ran two consecutive `pdflatex` compilation cycles to ensure all cross-references are fully updated and resolved.
