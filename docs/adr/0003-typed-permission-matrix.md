# Typed Section × Operation permission matrix replaces string keys and magic ints

Access control was enforced via boolean columns read back from checkbox-image visibility, sections identified by Persian UI-label strings compared in repeated if/else chains, and operations encoded as magic ints 1–4 at every call site. Permissions become **Access Grants** — one row per (User Group, Section, Operation) — with Sections and Operations defined as code enums seeded to the database, and the access-check API taking typed `(Section, Operation)` values. Compile-time safety eliminates the class of bug where a form checked against the wrong section string.

## Consequences

- Adding a new section or operation requires a code change (deliberate: sections map to features).
- The Users form is rebuilt around real CheckBox controls driven by the matrix, replacing ~44 image-visibility toggles.
