# CRMPeyvand Architecture Documentation Suite

This directory contains the authoritative technical architecture and system design documentation for **CRMPeyvand (پیوند CRM)**.

## Documentation Index

| Document | Focus & Contents | Key Components |
|---|---|---|
| [System Overview](overview.md) | High-level architecture, design philosophy, multi-tier layer structure, solution map, and technology stack. | `CRMPeyvand.sln`, Presentation, BLL, DAL, BE |
| [C4 Architecture Model & Dynamic Flows](c4-model.md) | C4 Context, Container, Component diagrams, plus Mermaid sequence and state diagrams for core runtime interactions. | C4 Diagrams, Data Flow, Life Cycles |
| [Data & Storage Architecture](data-and-storage.md) | Dual-database engine (SQLite default & SQL Server opt-in), EF6 provider registration, schema management, live backup/restore mechanics, and ERD. | `DB`, `DataSource`, `SqliteSchema`, `SettingDAL` |
| [Domain & Business Logic](domain-and-business-logic.md) | Pure domain policies, inventory validation, invoice & payment settlement lifecycle, and the dual-currency (Rial/Toman) system. | `Pricing`, `StockPolicy`, `SettlementPolicy`, `Money` |
| [Security & Access Control (RBAC)](security-and-rbac.md) | Authentication, PBKDF2-SHA256 password hashing, persistent session tokens, strongly-typed permission matrix `(Section × Operation)`, and admin seeding. | `PasswordHasher`, `AccessGuard`, `AccessDecision` |
| [Reporting Subsystem](reporting-subsystem.md) | QuestPDF vector reporting architecture, right-to-left Persian typography, report models, factories, and print preview pipeline. | `QuestPDF`, `PersianReportStyle`, `InvoiceDocument` |

---

## Architectural Principles

1. **Behavior-Preserving & Incremental**: Legacy behavior (Persian solar calendar dates, SMS messaging workflows, UI responsiveness) is strictly preserved while modularizing structural code ([ADR-0001](../adr/0001-incremental-in-place-refactor.md)).
2. **Zero-Configuration Run**: The application boots out-of-the-box using an embedded SQLite database. SQL Server is an opt-in mode without requiring changes to code or domain entities ([ADR-0008](../adr/0008-dual-database-mode.md)).
3. **Canonical Domain Ubiquitous Language**: The system adheres strictly to the canonical vocabulary recorded in [CONTEXT.md](../../CONTEXT.md) (e.g. Catalog Item, Invoice Line, Outstanding Balance, Base Unit vs. Display Unit).
4. **Strong Typing Over Magic Values**: Permissions are represented as strongly-typed enums (`Section`, `Operation`) evaluated via pure decision logic rather than comma-separated strings or role tags ([ADR-0003](../adr/0003-typed-permission-matrix.md)).
5. **Authoritative Receipts & Balances**: Payments represent immutable historical events that dictate invoice checkout and customer debt states, preventing financial drift ([ADR-0009](../adr/0009-payments-authoritative-over-checkout-flag.md)).
