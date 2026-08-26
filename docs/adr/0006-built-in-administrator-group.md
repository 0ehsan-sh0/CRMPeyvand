# Built-in Administrator Group replaces the «مدیریت» title-string convention

Admin identity was pure convention: the group titled exactly `مدیریت` was excluded from user/group lists via SQL literals (`WHERE ... Title <> N'مدیریت'`), and `AdminUserGruop()` returned the first group by id. Renaming the group or creating a second one silently broke admin semantics. `UserGroups` gains an `IsBuiltIn` flag: exactly one built-in group is seeded at First Run holding every Access Grant, is hidden from list UIs, and cannot be edited or deleted through the app. All title-literal SQL is removed; the title survives only as display text.

## Considered Options

- Keep the title convention with typed seeding: rejected — still fragile to renames and localization.
- No admin group at all, hardcoded bypass in checks: rejected — loses the ability to audit who admins are and complicates the User→Group foreign key.

## Consequences

- The built-in group is invisible in UsersForm; membership is managed only by assigning users to other groups vs. this one programmatically at First Run.
- `AccessGuard` short-circuits `true` for members of the built-in group even if grant rows are missing, making seed incompleteness harmless.
