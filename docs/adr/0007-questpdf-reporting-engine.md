# QuestPDF replaces Stimulsoft for invoice printing and reporting

CRMPeyvand migrates from Stimulsoft Reports to QuestPDF for all document generation and printable reporting (sales invoices, customer lists, activity logs, and periodic sales summaries).

## Context & Problem

The legacy Stimulsoft reporting engine encountered multiple friction points on modern .NET (`net10.0-windows`):
1. Runtime C# `CodeDomProvider` dynamic script compilation is deprecated and prohibited in modern .NET runtimes, requiring fragile interpretation mode flags.
2. Hardcoded local directory references (`C:\Program Files (x86)\Stimulsoft Designer...`) in project files broken portability across developer machines and build environments.
3. Embedded SQL connections (`StiSqlDatabase`) bypassed the application's Data Access Layer (DAL) and Business Logic Layer (BLL), creating hidden schema dependencies and connection-string vulnerabilities.
4. Binary serialization dependencies (`BinaryFormatter`) conflict with modern .NET security defaults.

## Decision Summary

1. **Adopt QuestPDF**: Replace Stimulsoft with QuestPDF (Community License, free for revenue < $1M USD).
2. **Code-First Document Architecture**: All reports and printable sales invoices are authored as strongly-typed C# classes implementing `QuestPDF.Infrastructure.IDocument`.
3. **Persian & RTL Typography**: Use HarfBuzz text shaping with embedded Persian fonts (such as Shabnam / Vazirmatn) and explicit right-to-left layout direction (`ContentDirection.RightToLeft`).
4. **Data Isolation via BLL**: Reports receive strongly-typed ViewModels/DTOs from BLL, completely eliminating embedded SQL and raw database connections in templates.
5. **Interactive Preview & Export**: Provide a unified WPF print preview and export capability (Save as PDF, Direct Print to Windows Printer Dialog).

## Considered Options

- **Keep Stimulsoft on .NET Core packages**: Rejected — proprietary licensing costs, closed XML/MRT template format, and recurring runtime quirks across major .NET version upgrades.
- **HTML/CSS with WebView2 Print-to-PDF**: Considered — highly customizable styling, but introduces heavy Chromium runtime overhead and less deterministic page-breaking control than native PDF layout trees.
- **WPF FlowDocument**: Considered — built-in, but cumbersome XAML pagination, lack of modern fluent layout primitives, and difficult headless export.
- **QuestPDF**: Selected — pure C#, zero external desktop software prerequisites, fast SkiaSharp rendering, native RTL/HarfBuzz, testable in standard xUnit unit tests, and 100% version-controllable in git.

## Consequences

- All 10 legacy `.mrt` files and Stimulsoft NuGet/DLL dependencies are retired.
- Report layouts are version-controlled C# source code and can be unit-tested in CI without external designers.
- No third-party software (like Stimulsoft Designer) needs to be installed on developer or client machines.
