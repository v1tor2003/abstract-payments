# Troubleshooting Registry: Webhook Load Benchmark & LaTeX Documentation Expansion

## Challenges & Solutions

### 1. Ephemeral Port & Socket Exhaustion under 10k-100k Concurrent Load
- **Problem:** Attempting to issue 100,000 real HTTP POST requests concurrently over loopback (`localhost`) in a local testing environment triggers Windows OS socket exhaustion, causing connection failures and test process crashes.
- **Resolution:** We bypassed local socket networking limitations by directly calling the framework's internal pipeline processor (`IWebhookProcessor.ProcessAsync`) in memory. This executes the signature validation, DTO conversion, and queue insertion logic natively in memory, measuring pure framework pipeline CPU overhead without network stack limitations.

### 2. TikZ Graph Package Dependency
- **Problem:** High-volume line graphs in LaTeX are typically drawn using the `pgfplots` package. However, if the package is missing or fails to download, it breaks compile-time builds.
- **Resolution:** Avoided external package dependencies by drawing a custom line chart using standard TikZ vectors (`\draw` and `\fill`) mapped proportionally to values (e.g. Y coordinate = RPS / 200,000). This produces a highly professional, native vector chart that compiles instantly on any LaTeX setup.
