# Migrate to modern .NET runtime after behavior-preserving refactor

The app leaves .NET Framework 4.7.2 for the modern .NET runtime (start on .NET 8 LTS for maximum package compatibility with HandyControl/LiveCharts/Stimulsoft; retarget to the newest LTS afterwards if dependency checks pass), moving EF6 to EF Core in the same phase. Driver: maintainability and tech modernization. It is sequenced *after* the schema and permission refactor so that any breakage isolates to the platform change, not to simultaneous logic changes.

## Considered Options

- Staying on .NET Framework 4.7.2: rejected — no further runtime investment from Microsoft; contradicts the stated modernization goal.
- Upgrading first: rejected in favor of ADR-0001 ordering.
