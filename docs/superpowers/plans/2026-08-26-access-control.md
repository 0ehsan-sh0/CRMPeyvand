# Access Control & Password Hashing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the legacy string-keyed permission system with a typed `(User Group, Section, Operation)` grant matrix, rebuild UsersForm around real checkboxes, and hash passwords with PBKDF2-SHA256 instead of reversible Base64.

**Architecture:** Three vertical slices landed additively then cleaned: (1) password hashing inside UserBLL behind a pure static hasher; (2) typed access model — `Section`/`Operation` enums, `AccessGrant` entity, `IsBuiltIn` flag — added alongside the legacy `UserAccessRole` until every consumer is switched, then deleted; (3) a centralized `AccessGuard.Can(User, Section, Operation)` replacing every scattered `Ubll.Access(u, "<persian string>", n)` call site, keeping enforcement UI-only. A single fresh EF6 baseline migration is regenerated at the end (dev-only DB, wipe allowed).

**Tech Stack:** WPF on .NET Framework 4.7.2, Entity Framework 6 code-first (`MigrateDatabaseToLatestVersion`, automatic migrations OFF), classic non-SDK csproj for BE/BLL/DAL/CRMPeyvand, SDK-style xUnit project `CRMPeyvand.Tests`.

**Spec:** No separate spec doc. This plan implements the rulings recorded in Global Constraints plus `docs/adr/0003-typed-permission-matrix.md`, `docs/adr/0005-pbkdf2-password-hashing.md`, `docs/adr/0006-built-in-administrator-group.md`, and the glossary in `CONTEXT.md`. Executors read all four before starting.

## Global Constraints

- Runtime .NET Framework 4.7.2. No new NuGet packages anywhere.
- BE/BLL/DAL/CRMPeyvand are **classic non-SDK csproj**: every created/deleted source file needs its `<Compile Include>` entry added/removed manually; the `.resx` of a migration is `EmbeddedResource`. `CRMPeyvand.Tests` is SDK-style (globs automatically).
- Work **in place** on branch `refactor/access-control` off `master`. **No git worktrees** (Ruling R3).
- Local commits only, conventional style (`feat:`, `refactor:`, `test:`, `docs:`, `chore:`). **Never push** (Ruling R2).
- Subagents are general-purpose only (Ruling R5).
- The dev database is disposable: wiping/recreating it is always acceptable; there are **no production deployments**.
- **From Task 4 until Task 9 completes, the app is intentionally UNRUNNABLE** (EF model diverges from the stored baseline migration). Do not try to launch it in that window; build + unit tests are the only verification. Unit tests never touch the database.
- Persian UI copy stays unchanged except the new Discounts row caption «بخش تخفیف ها». Persian strings may survive as *display text* (captions), never as *data keys*.
- Glossary terms are authoritative (`CONTEXT.md`): Catalog Item, Invoice Line, Discount Code, Access Grant, Built-in Administrator Group, First Run. Avoid legacy terms in code identifiers.
- Build/test verification command pair (run from repo root):
  - `dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal`
  - `dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal`
- Every task ends with build green + full test suite green + a commit.

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `BLL\PasswordHasher.cs` | Create | Pure PBKDF2-SHA256 hash/verify, versioned wire format |
| `BLL\UserBLL.cs` | Modify | Hash on Create/Update; Verify in Login; blank-password = unchanged |
| `BE\Section.cs` | Create | `enum Section` — 10 members |
| `BE\Operation.cs` | Create | `enum Operation` — View/Create/Edit/Delete |
| `BE\AccessGrant.cs` | Create | Grant entity: one row per (group, section, operation) |
| `BE\UserGroup.cs` | Modify | Add `IsBuiltIn`, add `AccessGrants`; (Task 8: drop `UserAccessRoles`) |
| `DAL\DB.cs` | Modify | Add `DbSet<AccessGrant>`; (Task 8: drop `DbSet<UserAccessRole>`) |
| `BLL\AccessDecision.cs` | Create | Pure grant evaluation logic (unit-tested without DB) |
| `BLL\AccessGuard.cs` | Create | One-round-trip typed check used by all forms |
| `BLL\UserGroupBLL.cs`, `DAL\UserGroupDAL.cs` | Modify | Persist/replace grant collections; `IsBuiltIn` filtering; `AdminUserGruop`→`AdminUserGroup` |
| `DAL\UserDAL.cs` | Modify | Login rewrite; `Read()` filter swap |
| `RegisterUC.xaml.cs` | Modify | Enum-driven seeding of Built-in Administrator Group |
| `MainWindow.xaml.cs`, `Customer/Product/Invoice/Activities/Reminder/UsersForm/Setting/SMS/OffCode` forms | Modify | Swap call sites to `AccessGuard`; close Invoice-Edit + Discounts gaps |
| `UsersForm.xaml(.cs)` | Modify | Full permission-area rebuild around real `CheckBox` matrix; bug fixes |
| `BE\UserAccessRole.cs`, `DAL\UserDAL.cs` legacy `Access`, `BLL\UserBLL.cs` legacy overload | Delete (Task 8) | Kill switch for the legacy shape |
| `DAL\Migrations\*InitialCreate*` | Replace (Task 9) | Fresh scaffolded baseline matching the final model |
| `CRMPeyvand.Tests\PasswordHasherTests.cs`, `AccessDecisionTests.cs` | Create | New unit tests |

---

### Task 1: Branch setup and documentation carry-over

**Files:**
- Modify: `CONTEXT.md`, `docs/adr/0005-pbkdf2-password-hashing.md`, `docs/adr/0006-built-in-administrator-group.md` (already edited in working tree — uncommitted)

**Interfaces:**
- Produces: branch `refactor/access-control` based on `master` containing the docs commit all later tasks assume.

- [ ] **Step 1: Create the branch carrying the pending docs**

```bash
git status --short          # expect CONTEXT.md modified + two new ADR files, nothing else
git checkout -b refactor/access-control
```

- [ ] **Step 2: Review the doc diff briefly** — confirm the glossary gained **Built-in Administrator Group**, First Run mentions seeding, and the two ADRs exist. No content edits expected.

- [ ] **Step 3: Commit**

```bash
git add CONTEXT.md docs/adr/0005-pbkdf2-password-hashing.md docs/adr/0006-built-in-administrator-group.md
git commit -m "docs(context): built-in administrator group term, PBKDF2 and admin-flag ADRs"
```

---

### Task 2: PasswordHasher (BLL) with unit tests — TDD

**Files:**
- Create: `BLL\PasswordHasher.cs`
- Test: `CRMPeyvand.Tests\PasswordHasherTests.cs`
- Modify: `BLL\BLL.csproj` (add Compile entry)

**Interfaces:**
- Produces: `BLL.PasswordHasher` with `const int Iterations = 100000;`, `static string Hash(string password)`, `static bool Verify(string password, string stored)`.
- Wire format produced/consumed: `pbkdf2-sha256:<iterations>:<saltBase64>:<hashBase64>` (32-byte key, 16-byte salt). Task 3 relies on these exact names and order of arguments: `Verify(password, stored)`.

- [ ] **Step 1: Write the failing tests**

Create `CRMPeyvand.Tests\PasswordHasherTests.cs`:

```csharp
using System;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class PasswordHasherTests
    {
        private const string Sample = "s3cret-Pass";

        [Fact]
        public void Hash_produces_versioned_format()
        {
            string stored = PasswordHasher.Hash(Sample);

            Assert.StartsWith("pbkdf2-sha256:", stored);
            Assert.Equal(4, stored.Split(':').Length);
        }

        [Fact]
        public void Hash_embeds_configured_iteration_count()
        {
            string stored = PasswordHasher.Hash(Sample);

            Assert.Equal(PasswordHasher.Iterations.ToString(),
                         stored.Split(':')[1]);
        }

        [Fact]
        public void Verify_accepts_correct_password()
        {
            Assert.True(PasswordHasher.Verify(Sample, PasswordHasher.Hash(Sample)));
        }

        [Fact]
        public void Verify_rejects_wrong_password()
        {
            Assert.False(PasswordHasher.Verify("wrong", PasswordHasher.Hash(Sample)));
        }

        [Fact]
        public void Same_password_hashes_twice_differ_salts()
        {
            Assert.NotEqual(PasswordHasher.Hash(Sample), PasswordHasher.Hash(Sample));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("cGFzcw==")]
        [InlineData("md5:1024:abc:def")]
        public void Verify_rejects_garbage_or_legacy_stored_values(string stored)
        {
            Assert.False(PasswordHasher.Verify(Sample, stored));
        }

        [Fact]
        public void Verify_roundtrips_manual_stored_string()
        {
            string salt = Convert.ToBase64String(new byte[16]);
            string manual = "pbkdf2-sha256:" + PasswordHasher.Iterations + ":" + salt + ":not-a-real-hash";

            Assert.False(PasswordHasher.Verify(Sample, manual),
                "a malformed-but-parsable stored value must not authenticate");
        }
    }
}
```

- [ ] **Step 2: Run tests, expect failure**

Run: `dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal`
Expected: compile error CS0103/type `PasswordHasher` not found (counts as red).

- [ ] **Step 3: Implement**

Create `BLL\PasswordHasher.cs`:

```csharp
using System;
using System.Security.Cryptography;

namespace BLL
{
    public static class PasswordHasher
    {
        public const int Iterations = 100000;
        private const int SaltSizeBytes = 16;
        private const int KeySizeBytes = 32;
        private const string Prefix = "pbkdf2-sha256";

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);
            return Encode(password, salt, Iterations);
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
                return false;

            string[] parts = stored.Split(':');
            if (parts.Length != 4 || parts[0] != Prefix)
                return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
                return false;

            byte[] salt;
            try { salt = Convert.FromBase64String(parts[2]); }
            catch (FormatException) { return false; }

            string candidate = Encode(password ?? string.Empty, salt, iterations);
            return SlowEquals(candidate, stored);
        }

        private static string Encode(string password, byte[] salt, int iterations)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = derive.GetBytes(KeySizeBytes);
                return Prefix + ":" + iterations + ":"
                     + Convert.ToBase64String(salt) + ":"
                     + Convert.ToBase64String(key);
            }
        }

        private static bool SlowEquals(string left, string right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Length != right.Length) return false;
            int diff = 0;
            for (int i = 0; i < left.Length; i++)
                diff |= left[i] ^ right[i];
            return diff == 0;
        }
    }
}
```

Add to `BLL\BLL.csproj` next to the other Compile entries:

```xml
<Compile Include="PasswordHasher.cs" />
```

- [ ] **Step 4: Run tests, expect all green (old suite included)**

Run: `dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal`
Expected: PASS (new 8 + prior Pricing/StockPolicy/smoke tests).

- [ ] **Step 5: Build solution and commit**

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
git add BLL/PasswordHasher.cs BLL/BLL.csproj CRMPeyvand.Tests/PasswordHasherTests.cs
git commit -m "feat(security): PBKDF2-SHA256 password hasher with versioned storage format"
```

---

### Task 3: Hash passwords in UserBLL; delete reversible encoding

**Files:**
- Modify: `BLL\UserBLL.cs` (Create ~line 57, Update ~line 67, Login ~line 84, remove `Encode` lines 13–28 and `Decode` lines 40–50)
- Modify: `DAL\UserDAL.cs` (Login query, lines ~93–96)

**Interfaces:**
- Consumes: `PasswordHasher.Hash/Verify` from Task 2.
- Produces: external behavior — `UserBLL.Login(userName, password)` returns the User on success else null; `UserBLL.Create(user)` stores a PBKDF2 string; `UserBLL.Update(user, id)` leaves the stored password untouched when `user.Password` is null/whitespace. Signatures of all three public methods stay exactly as today.

- [ ] **Step 1: Rewrite UserBLL**

Delete `Encode` and `Decode` entirely. In `Create`, replace the encode line:

```csharp
user.Password = PasswordHasher.Hash(user.Password);
```

In `Update`, replace the unconditional overwrite with a guard (blank password means "unchanged"):

```csharp
if (!string.IsNullOrWhiteSpace(updated.Password))
    existing.Password = PasswordHasher.Hash(updated.Password);
```

(`updated`/`existing` are the parameter and the freshly loaded entity in whatever local naming the current body uses — preserve the rest of the field-copy logic verbatim.)

Rewrite `Login` in `BLL\UserBLL.cs` to delegate:

```csharp
public static User Login(string UserName, string Password)
{
    return UserDAL.Login(UserName, Password);
}
```

If the current `Login` already delegates, leave it.

- [ ] **Step 2: Rewrite UserDAL.Login**

Replace the ciphertext-equality query (lines ~93–96) with load-then-verify, and stop soft-deleted users from logging in (minor bugfix folded in deliberately):

```csharp
public static User Login(string UserName, string Password)
{
    using (var db = new DB())
    {
        var user = db.Users.Include("UserGroup")
            .FirstOrDefault(i => i.UserName == UserName && !i.DeleteStatus);
        if (user == null || string.IsNullOrEmpty(user.Password))
            return null;
        return BLL.PasswordHasher.Verify(Password, user.Password) ? user : null;
    }
}
```

If `DAL` does not reference `BLL` (it must not — dependency direction is BLL→DAL), put the `Verify` call in `UserBLL.Login` instead: fetch candidate via a new thin `UserDAL.FindByUserName(UserName)` returning the User with `Include("UserGroup")`, then verify in BLL. Use whichever keeps layering intact; the test suite cannot see the difference, the smoke test in Task 9 can.

- [ ] **Step 3: Sweep for stranded callers of Encode/Decode**

Run: `rg -n "\bEncode\b|\bDecode\b" BLL DAL CRMPeyvand --type cs`
Expected: zero hits (the explorer found Decode unused; nothing else calls Encode).

- [ ] **Step 4: Build, test, commit**

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal
git add BLL/UserBLL.cs DAL/UserDAL.cs
git commit -m "feat(security): hash passwords with PBKDF2 in UserBLL, drop reversible Base64"
```

---

### Task 4: Typed access model — additive, legacy still compiles

**Files:**
- Create: `BE\Section.cs`, `BE\Operation.cs`, `BE\AccessGrant.cs`
- Modify: `BE\UserGroup.cs` (add `IsBuiltIn`, add `AccessGrants`; keep `UserAccessRoles` until Task 8)
- Modify: `DAL\DB.cs` (add `DbSet<AccessGrant>`)
- Modify: `BE\BE.csproj` (three Compile entries)

**Interfaces:**
- Produces (exact names/values later tasks depend on):

```csharp
namespace BE
{
    public enum Section
    {
        Customers = 1, CatalogItems = 2, Invoices = 3, Activities = 4, Reminders = 5,
        Users = 6, SmsPanel = 7, Reports = 8, Settings = 9, Discounts = 10
    }

    public enum Operation
    {
        View = 1, Create = 2, Edit = 3, Delete = 4
    }

    public class AccessGrant
    {
        public int id { get; set; }
        public Section Section { get; set; }
        public Operation Operation { get; set; }
        public UserGroup UserGroup { get; set; }
    }
}
```

`UserGroup` gains:

```csharp
public bool IsBuiltIn { get; set; }
public List<AccessGrant> AccessGrants { get; set; } = new List<AccessGrant>();
```

`DB` gains: `public DbSet<AccessGrant> AccessGrants { get; set; }`

EF6 maps enums to `int` columns by convention; the navigation creates shadow FK `UserGroup_id` — identical convention to the existing `User`→`UserGroup` relation, so no fluent config is needed.

- [ ] **Step 1: Write the three BE files** exactly as specified above (one type per file, namespace `BE`).

- [ ] **Step 2: Patch UserGroup and DB** — add the two members to `UserGroup`; add the DbSet line to `DB` beside `UserAccessRoles`. Do NOT remove anything yet.

- [ ] **Step 3: Add Compile entries to `BE\BE.csproj`:**

```xml
<Compile Include="Section.cs" />
<Compile Include="Operation.cs" />
<Compile Include="AccessGrant.cs" />
```

- [ ] **Step 4: Build, test, commit**

Reminder: from this commit onward the app binary is not runnable against an existing DB (expected; Task 9 resolves it). Tests are unaffected.

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal
git add BE/Section.cs BE/Operation.cs BE/AccessGrant.cs BE/UserGroup.cs BE/BE.csproj DAL/DB.cs
git commit -m "feat(access): typed Section/Operation enums and AccessGrant entity"
```

---

### Task 5: AccessDecision (pure) + AccessGuard (typed entry point) with tests

**Files:**
- Create: `BLL\AccessDecision.cs`, `BLL\AccessGuard.cs`
- Test: `CRMPeyvand.Tests\AccessDecisionTests.cs`
- Modify: `BLL\BLL.csproj` (two Compile entries)

**Interfaces:**
- Consumes: Task 4 types.
- Produces:

```csharp
namespace BLL
{
    public static class AccessDecision
    {
        // Pure decision: built-in groups pass everything; otherwise the grant must exist exactly.
        public static bool Evaluate(bool isBuiltIn,
            System.Collections.Generic.ICollection<BE.AccessGrant> grants,
            BE.Section section, BE.Operation operation)
    }

    public static class AccessGuard
    {
        // One DB round trip; missing rows evaluate to False (kills the legacy NRE by construction).
        public static bool Can(BE.User user, BE.Section section, BE.Operation operation)
    }
}
```

- [ ] **Step 1: Write failing tests**

Create `CRMPeyvand.Tests\AccessDecisionTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using BE;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class AccessDecisionTests
    {
        private static List<AccessGrant> Grants(params Section[] sections)
        {
            return sections.Select(s => new AccessGrant { Section = s, Operation = Operation.View })
                           .ToList();
        }

        [Fact]
        public void Built_in_group_passes_even_with_no_grant_rows()
        {
            Assert.True(AccessDecision.Evaluate(true, new List<AccessGrant>(),
                                                Section.Users, Operation.Delete));
        }

        [Fact]
        public void Matching_grant_passes()
        {
            Assert.True(AccessDecision.Evaluate(false, Grants(Section.Customers),
                                                Section.Customers, Operation.View));
        }

        [Fact]
        public void Missing_section_or_operation_fails()
        {
            var grants = new List<AccessGrant>
            {
                new AccessGrant { Section = Section.Customers, Operation = Operation.View },
                new AccessGrant { Section = Section.Invoices,  Operation = Operation.Delete },
            };

            Assert.False(AccessDecision.Evaluate(false, grants, Section.CatalogItems, Operation.View));
            Assert.False(AccessDecision.Evaluate(false, grants, Section.Invoices,     Operation.Edit));
        }

        [Fact]
        public void Null_or_empty_grants_fail()
        {
            Assert.False(AccessDecision.Evaluate(false, null, Section.Reports, Operation.View));
            Assert.False(AccessDecision.Evaluate(false, new List<AccessGrant>(), Section.Reports, Operation.View));
        }
    }
}
```

- [ ] **Step 2: Run tests — expect compile failure** (`AccessDecision` not found).

- [ ] **Step 3: Implement both classes**

`BLL\AccessDecision.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using BE;

namespace BLL
{
    public static class AccessDecision
    {
        public static bool Evaluate(bool isBuiltIn, ICollection<AccessGrant> grants,
                                    Section section, Operation operation)
        {
            if (isBuiltIn)
                return true;
            if (grants == null)
                return false;
            return grants.Any(g => g.Section == section && g.Operation == operation);
        }
    }
}
```

`BLL\AccessGuard.cs`:

```csharp
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    public static class AccessGuard
    {
        public static bool Can(User user, Section section, Operation operation)
        {
            if (user == null)
                return false;

            using (var db = new DB())
            {
                var loaded = db.Users.Include("UserGroup.AccessGrants")
                                     .FirstOrDefault(i => i.id == user.id);
                if (loaded == null || loaded.UserGroup == null)
                    return false;

                return AccessDecision.Evaluate(
                    loaded.UserGroup.IsBuiltIn,
                    loaded.UserGroup.AccessGrants,
                    section,
                    operation);
            }
        }
    }
}
```

Add both to `BLL\BLL.csproj`:

```xml
<Compile Include="AccessDecision.cs" />
<Compile Include="AccessGuard.cs" />
```

- [ ] **Step 4: Run tests, build, commit**

```bash
dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
git add BLL/AccessDecision.cs BLL/AccessGuard.cs BLL/BLL.csproj CRMPeyvand.Tests/AccessDecisionTests.cs
git commit -m "feat(access): centralized typed AccessGuard with pure decision core"
```

---

### Task 6: Convert every enforcement call site; seed built-in group; IsBuiltIn filtering

**Files:**
- Modify: `CRMPeyvand\MainWindow.xaml.cs` (54–201), `CustomerForm.xaml.cs` (44–96), `ProductForm.xaml.cs` (114–132), `InvoiceForm.xaml.cs` (127–137 + new Edit gate), `ActivitiesForm.xaml.cs` (149–167), `ReminderForm.xaml.cs` (39–57), `UsersForm.xaml.cs` (Window_Loaded 279–318 ONLY), `Setting.xaml.cs` (109), `SMSForm.xaml.cs` (52), `CRMPeyvand\OffCodeForm.xaml.cs` (new gates), `RegisterUC.xaml.cs` (`CreateAdminGroup` 47–64), `DAL\UserDAL.cs` (`Read()` line ~63), `DAL\UserGroupDAL.cs` (`Read()` ~31, `ReadTitles()` ~69, rename `AdminUserGruop`→`AdminUserGroup`)

**Interfaces:**
- Consumes: `AccessGuard.Can`, Task 4 enums.
- Produces: zero remaining `Ubll.Access(u, "<string>", <int>)` call sites outside UsersForm's group-editor region (that region dies in Task 7). Built-in group seeded at First Run with all 40 grants. List queries exclude `IsBuiltIn` rows.

Mechanical substitution table (apply everywhere):

| Legacy | Typed |
|---|---|
| `Access(u, "بخش مشتریان", 1)` | `AccessGuard.Can(u, Section.Customers, Operation.View)` |
| `Access(u, "بخش کالاها", n)` | `Section.CatalogItems` |
| `Access(u, "بخش فاکتورها", n)` | `Section.Invoices` |
| `Access(u, "بخش فعالیت ها", n)` | `Section.Activities` |
| `Access(u, "بخش یادآور ها", n)` | `Section.Reminders` |
| `Access(u, "بخش کاربران", n)` | `Section.Users` |
| `Access(u, "پنل پیامکی", n)` | `Section.SmsPanel` |
| `Access(u, "بخش گزارشات", n)` | `Section.Reports` |
| `Access(u, "بخش تنظیمات", n)` | `Section.Settings` |
| magic int `1` / `2` / `3` / `4` | `Operation.View` / `Create` / `Edit` / `Delete` |

Representative before/after (`MainWindow.xaml.cs`):

```csharp
// before
if (!Ubll.Access(loggedInUser, "بخش تنظیمات", 1))
// after
if (!AccessGuard.Can(loggedInUser, Section.Settings, Operation.View))
```

- [ ] **Step 1: Convert MainWindow's nine nav gates** (View ops feeding the `EnterX` booleans; keyboard shortcuts consume those booleans and need no change).

- [ ] **Step 2: Convert the seven forms listed** to their existing operations. Note InvoiceForm currently checks only ops 2 and 4 — convert those, then **close the gap** by adding, beside them:

```csharp
btnEditInvoice.IsEnabled = AccessGuard.Can(u, Section.Invoices, Operation.Edit);
```

(use the form's actual edit button/menu name; mirror the surrounding disabled-style handling).

- [ ] **Step 3: Gate OffCodeForm (Discounts gap closure).** Find every instantiation site: `rg -n "OffCodeForm" CRMPeyvand --type cs`. At each site, wrap opening with `AccessGuard.Can(loggedInUser, Section.Discounts, Operation.View)` following the pattern of sibling nav gates. Inside `OffCodeForm`'s `Window_Loaded`, disable create/edit/delete controls per `AccessGuard.Can(u, Section.Discounts, Operation.Create/Edit/Delete)` mirroring `ProductForm`'s pattern.

- [ ] **Step 4: Rewrite First Run seeding** in `RegisterUC.xaml.cs`:

```csharp
private void CreateAdminGroup()
{
    var grants = Enum.GetValues(typeof(Section)).Cast<Section>()
        .SelectMany(s => Enum.GetValues(typeof(Operation)).Cast<Operation>()
            .Select(o => new AccessGrant { Section = s, Operation = o }))
        .ToList();

    var admin = new UserGroup
    {
        Title = "مدیریت",           // display text only — identity is IsBuiltIn
        IsBuiltIn = true,
        AccessGrants = grants,
    };

    UGbll.Create(admin);
}
```

Adapt `UserGroupBLL.Create`/`UserGroupDAL.Create` so a group with a populated `AccessGrants` collection persists the grants in one `SaveChanges()` (attach group, assign collection, add — EF orders the inserts). Keep the method signature `Create(UserGroup)`.

Add the required usings to `RegisterUC.xaml.cs`: `System.Linq`, `BE`, and `BLL` (already referenced by sibling code-behinds).

- [ ] **Step 5: Replace title-literal filtering with flag filtering.**

In `DAL\UserDAL.Read()` (~line 63), `DAL\UserGroupDAL.Read()` (~31), `ReadTitles()` (~69): remove `Title <> N'مدیریت'` predicates and exclude built-ins instead, e.g.:

```sql
WHERE (ug.IsBuiltIn = 0)
```

(join alias per each query's actual shape). Rename `AdminUserGruop` → `AdminUserGroup` and make it deterministic:

```csharp
public static UserGroup AdminUserGroup()
{
    using (var db = new DB())
        return db.UserGroups.SingleOrDefault(g => g.IsBuiltIn);
}
```

Update callers (`rg -n "AdminUserGruop" --type cs` finds them; expected: `RegisterUC.xaml.cs` binding the first user to the admin group).

- [ ] **Step 6: Prove no stray call sites**

Run: `rg -n '\.Access\(u?, ?"' CRMPeyvand BLL DAL --type cs` and `rg -n 'Ubll\.Access|bll\.Access' CRMPeyvand BLL DAL --type cs`
Expected: only hits inside `UsersForm.xaml.cs` group-editor region (save/edit/load chains) — everything else gone.

- [ ] **Step 7: Build, test, commit**

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal
git commit -am "refactor(access): typed enforcement everywhere, enum seeding, IsBuiltIn filtering"
```

---

### Task 7: Rebuild UsersForm permission area around a real CheckBox matrix

**Files:**
- Modify: `CRMPeyvand\UsersForm.xaml` (replace image-toggle region ~lines 120–405), `CRMPeyvand\UsersForm.xaml.cs` (replace Fill*/ChangeVisibility/toggle-handler regions and group save/load chains; fix password bugs)

**Interfaces:**
- Consumes: `AccessGuard`, `CollectGrants`/`ApplyGrants` below, `UserGroupBLL.Update(int groupId, string title, List<AccessGrant> grants)` introduced here:

`Create(UserGroup)` keeps its Task-6 signature and persists whatever `group.AccessGrants` holds. New here:

```csharp
// UserGroupBLL
public static void Update(int groupId, string title, List<AccessGrant> grants);   // wraps DAL
```

```csharp
// UserGroupDAL.Update — replaces the whole grant set for the group
public static void Update(int groupId, string title, List<AccessGrant> grants)
{
    using (var db = new DB())
    {
        var group = db.UserGroups.Include("AccessGrants")
                                 .SingleOrDefault(g => g.id == groupId);
        if (group == null || group.IsBuiltIn)
            return;

        group.Title = title;
        db.AccessGrants.RemoveRange(group.AccessGrants.ToList());
        foreach (var grant in grants)
            db.AccessGrants.Add(new AccessGrant
            {
                Section = grant.Section,
                Operation = grant.Operation,
                UserGroup = group,
            });
        db.SaveChanges();
    }
}
```

(`Create` analogous: attach group with `AccessGrants = grants`, single SaveChanges.)

- [ ] **Step 1: Replace the toggle-region XAML.** Delete every Rectangle/Border/Image trio and the five header toggles (`cbEnterImage`, `cbAddImage`, `cbEditImage`, `cbDeleteImage`, `cbAllCheckBoxesImage`). Keep the GroupBox, its title TextBox, and the add/edit/delete group buttons under their existing names. Insert:

```xml
<CheckBox x:Name="cbAll" Content="همه" FontWeight="Bold" Margin="0,4,0,4"/>
<Grid x:Name="PermissionGrid">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="90"/>
        <ColumnDefinition Width="90"/>
        <ColumnDefinition Width="90"/>
        <ColumnDefinition Width="90"/>
    </Grid.ColumnDefinitions>
</Grid>
```

- [ ] **Step 2: Matrix code-behind.** Delete `FillAccessRole`, `FillAccessRoleForUpdate`, `FillForUpdate`, `ChangeVisibility`, and ALL `*_MouseLeftButtonDown` toggle handlers (including the mis-wired `cbEnterMessage` handler — bug dies with the images). Add:

```csharp
private static readonly Section[] AllSections = (Section[])Enum.GetValues(typeof(Section));
private static readonly Operation[] AllOperations = (Operation[])Enum.GetValues(typeof(Operation));

private readonly Dictionary<Section, Dictionary<Operation, CheckBox>> _cells =
    new Dictionary<Section, Dictionary<Operation, CheckBox>>();

private static readonly Dictionary<Section, string> SectionCaptions = new Dictionary<Section, string>
{
    { Section.Customers, "بخش مشتریان" },
    { Section.CatalogItems, "بخش کالاها" },
    { Section.Invoices, "بخش فاکتورها" },
    { Section.Activities, "بخش فعالیت ها" },
    { Section.Reminders, "بخش یادآور ها" },
    { Section.Users, "بخش کاربران" },
    { Section.SmsPanel, "پنل پیامکی" },
    { Section.Reports, "بخش گزارشات" },
    { Section.Settings, "بخش تنظیمات" },
    { Section.Discounts, "بخش تخفیف ها" },
};

private void BuildPermissionMatrix()
{
    _cells.Clear();
    PermissionGrid.Children.Clear();
    PermissionGrid.RowDefinitions.Clear();

    PermissionGrid.RowDefinitions.Add(NewRow());
    for (int i = 0; i < AllSections.Length; i++)
        PermissionGrid.RowDefinitions.Add(NewRow());

    Place(new TextBlock { Text = "بخش", FontWeight = FontWeights.Bold }, 0, 0);

    foreach (var op in AllOperations)
    {
        var captured = op;
        var header = new CheckBox
        {
            Content = OperationCaption(captured),
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        header.Click += (s, e) => SetColumn(captured, header.IsChecked == true);
        Place(header, 0, Array.IndexOf(AllOperations, captured) + 1);
    }

    for (int r = 0; r < AllSections.Length; r++)
    {
        var section = AllSections[r];
        Place(new TextBlock { Text = SectionCaptions[section] }, r + 1, 0);

        var cells = new Dictionary<Operation, CheckBox>();
        foreach (var op in AllOperations)
        {
            var cell = new CheckBox { HorizontalAlignment = HorizontalAlignment.Center };
            cells[op] = cell;
            Place(cell, r + 1, Array.IndexOf(AllOperations, op) + 1);
        }
        _cells[section] = cells;
    }

    cbAll.Click += (s, e) =>
    {
        foreach (var op in AllOperations)
            SetColumn(op, cbAll.IsChecked == true);
    };
}

private static RowDefinition NewRow()
{
    return new RowDefinition { Height = new GridLength(28) };
}

private void Place(FrameworkElement element, int row, int column)
{
    Grid.SetRow(element, row);
    Grid.SetColumn(element, column);
    PermissionGrid.Children.Add(element);
}

private void SetColumn(Operation operation, bool value)
{
    foreach (var cells in _cells.Values)
        cells[operation].IsChecked = value;
}

private static string OperationCaption(Operation operation)
{
    switch (operation)
    {
        case Operation.View: return "مشاهده";
        case Operation.Create: return "ایجاد";
        case Operation.Edit: return "ویرایش";
        default: return "حذف";
    }
}

private List<AccessGrant> CollectGrants()
{
    var grants = new List<AccessGrant>();
    foreach (var section in AllSections)
        foreach (var op in AllOperations)
            if (_cells[section][op].IsChecked == true)
                grants.Add(new AccessGrant { Section = section, Operation = op });
    return grants;
}

private void ApplyGrants(IEnumerable<AccessGrant> grants)
{
    var present = new HashSet<Tuple<Section, Operation>>(
        grants.Select(g => Tuple.Create(g.Section, g.Operation)));

    foreach (var section in AllSections)
        foreach (var op in AllOperations)
            _cells[section][op].IsChecked = present.Contains(Tuple.Create(section, op));
}
```

Call `BuildPermissionMatrix()` from `Window_Loaded`. Required usings already present except possibly `System.Collections.Generic`, `System.Linq` — add if missing.

- [ ] **Step 3: Rewire group save/edit/delete/load.** Replace the label-string chains (`btnAddUserGroup_Click` create branch reading `lbl*.Content`, edit branch's nine-way `item.Section == "…"` chain, and `miEditUserGroup_Click`'s mirror chain) with:

```csharp
// create
if (string.IsNullOrWhiteSpace(txtTitle.Text))          // use the form's real title box name
{
    MessageBox.Show("عنوان گروه را وارد کنید");
    return;
}
UGbll.Create(new UserGroup { Title = txtTitle.Text.Trim(), IsBuiltIn = false, AccessGrants = CollectGrants() });

// edit (after fetching ugEdit by the selected row's id as today)
UGbll.Update(selectedGroupId, ugEdit.Title, CollectGrants());

// load-for-edit (fetch with grants, then populate)
var group = /* fetched via db.UserGroups.Include("AccessGrants").SingleOrDefault(g => g.id == selectedGroupId) */;
txtTitle.Text = group.Title;
ApplyGrants(group.AccessGrants);
```

Defensive guard on both edit and delete paths: if the selected group `IsBuiltIn`, show a message and abort (it should never be selectable since Task 6 filtered the lists, but the UI must not trust that).

- [ ] **Step 4: Fix the user-save bugs.** In `btnAdd_Click` (~960–1053): read the primary password box, not the repeat box; validate repeat matches; allow empty password only on edit (meaning "unchanged"); validate 8–32 chars otherwise:

```csharp
string password = txtPass.Password;                 // was: txtReapetPass.Text
string repeat   = txtReapetPass.Password;
bool editing = userEdit != null;

if (!editing || password.Length > 0)
{
    if (password.Length < 8 || password.Length > 32)
    {
        MessageBox.Show("رمز عبور باید بین ۸ تا ۳۲ کاراکتر باشد");
        return;
    }
    if (password != repeat)
    {
        MessageBox.Show("تکرار رمز عبور با رمز عبور مطابقت ندارد");
        return;
    }
}

u.Password = password.Length > 0 ? password : null;   // null ⇒ UserBLL.Update keeps stored hash
```

(Adjust control names to the actual XAML; the essential rules are: source = primary box, empty-on-edit = unchanged.)

- [ ] **Step 5: Build, test, commit**

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal
git commit -am "feat(ui): rebuild UsersForm permissions on CheckBox matrix, fix password bugs"
```

---

### Task 8: Delete the legacy shape

**Files:**
- Delete: `BE\UserAccessRole.cs`
- Modify: `BE\UserGroup.cs` (remove `UserAccessRoles` property), `DAL\DB.cs` (remove `DbSet<UserAccessRole>`), `DAL\UserDAL.cs` (remove `Access(User, string, int)` ~97–118), `BLL\UserBLL.cs` (remove string/int pass-through ~86–89), `BE\BE.csproj` (remove Compile entry)

**Interfaces:**
- Consumes: Task 7's completion (no remaining consumers of the legacy API except UsersForm internals, which were rewritten).

- [ ] **Step 1: Remove the items listed above.** Compiler errors are the completeness oracle — chase every error to its typed replacement, never re-add the legacy member.

- [ ] **Step 2: Literal sweeps (all must come back empty):**

```bash
rg -n "UserAccessRole|CanEnter|CanCreate|CanUpdate|CanDelete" --type cs
rg -n "N'مدیریت'|\"مدیريت\"" DAL BLL --type cs
rg -n "\.Access\(" BLL DAL CRMPeyvand --type cs   # only AccessGuard.cs internal + AccessDecision may reference Access*
```

Persian display captions in `UsersForm.xaml`/`SectionCaptions` may still contain بخش strings — that is correct; the sweep above targets DAL/BLL only.

- [ ] **Step 3: Build, test, commit**

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
dotnet test E:\Developing\Projects\CRMPeyvand\CRMPeyvand.Tests\CRMPeyvand.Tests.csproj -v minimal
git commit -am "chore(access): remove legacy UserAccessRole and string-keyed access API"
```

---

### Task 9: Fresh baseline migration, wipe dev DB, boot + manual smoke

**Files:**
- Delete + Recreate: `DAL\Migrations\202608260000000_InitialCreate.cs/.Designer.cs/.resx` → new scaffolded `InitialCreate` trio reflecting the final model
- Modify: `DAL\DAL.csproj` (mirror however the OLD trio was declared — record its three entries first: 2× Compile, 1× EmbeddedResource — then point them at the new files)
- Throwaway host (repo-external): `%TEMP%\opencode\migrate-host\`

**Interfaces:**
- Consumes: final EF model (Tasks 4–8 complete).
- Produces: runnable app on wiped DB; `__MigrationHistory` contains exactly one row whose model hash matches the assembly.

Why regenerate rather than hand-write an incremental migration: EF6's migrator compares the current model hash against the target model embedded in the latest migration's `.resx`. A hand-written incremental migration embeds no model, so every boot would fail with "pending model changes". Scaffolding a fresh full baseline (Plan-1 R6 route) makes the hash match by construction.

- [ ] **Step 1: Record the old trio's csproj declarations**, then delete the three `202608260000000_InitialCreate.*` files and remove their csproj entries.

- [ ] **Step 2: Build the solution** so `DAL\bin\Debug\DAL.dll`, `BE.dll`, and `EntityFramework.dll` are fresh.

- [ ] **Step 3: Create the throwaway scaffolding host** at `%TEMP%\opencode\migrate-host\` — a console exe targeting net472 referencing those three DLLs by `<Reference><HintPath>`:

`Program.cs`:

```csharp
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using DAL;
using DAL.Migrations;

class Program
{
    static void Main()
    {
        var configuration = new Configuration
        {
            MigrationsAssembly = typeof(DB).Assembly,
            MigrationsNamespace = "DAL.Migrations",
            ContextKey = "DAL.DB",
            AutomaticMigrationsEnabled = false,
        };

        var scaffolder = new MigrationScaffolder(configuration);
        var migration = scaffolder.Scaffold("InitialCreate");

        var target = @"E:\Developing\Projects\CRMPeyvand\DAL\Migrations";
        File.WriteAllText(Path.Combine(target, migration.MigrationId + ".cs"), migration.UserCode);
        File.WriteAllText(Path.Combine(target, migration.MigrationId + ".Designer.cs"), migration.DesignerCode);
        File.WriteAllBytes(Path.Combine(target, migration.MigrationId + ".resx"), migration.Resources);
    }
}
```

Host `App.config`: copy the `<connectionStrings>` block (name `conStr`) from `CRMPeyvand\App.config` verbatim.

- [ ] **Step 4: Run the host** (`dotnet run` or msbuild+cscript the exe). Expect three new `…_InitialCreate.*` files in `DAL\Migrations`. Re-add the three csproj entries copying the metadata style noted in Step 1.

- [ ] **Step 5: Wipe the database:**

```bash
sqlcmd -S . -Q "IF DB_ID(N'CRMPeyvand') IS NOT NULL BEGIN ALTER DATABASE CRMPeyvand SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE CRMPeyvand; END"
```

Fallback if `sqlcmd` is unavailable: append `Database.SetInitializer`-safe deletion inside the same host — `using (var db = new DB()) { if (db.Database.Exists()) db.Database.Delete(); }`.

- [ ] **Step 6: Build, run, manual smoke (~10 min, report results verbatim):**

```bash
dotnet build E:\Developing\Projects\CRMPeyvand\CRMPeyvand.sln -v minimal
# then launch CRMPeyvand\bin\Debug\CRMPeyvand.exe and execute the checklist:
```

1. First Run → RegisterUC appears; create admin user (password 8–32) → LoginUC replaces it; log in OK.
2. UsersForm → «مدیریت» group absent from both grids; create group «فروشنده» with: Customers = all four ✓, Invoices = View+Create ✓, all else ✗; create user «sales» in that group; switch session to sales.
3. As sales: Customer buttons enabled; InvoiceForm: create enabled, **edit disabled**, delete disabled; **OffCodeForm unreachable/disabled**; Settings & SMS nav dimmed.
4. Wrong password → rejection message, no crash.
5. Back as admin: open «فروشنده» for edit → checkboxes reproduce the saved state exactly (round-trip proof); column-header «همه» toggles work.
6. Plan-1 regression: Good «آزمون» 10000/stock 5 + Service 25000; invoice 2×Good + Service; 10% code → 49,500; save; stock = 2; ×99 attempt → insufficient-stock message, no crash; restart → data persists.

- [ ] **Step 7: Delete the throwaway host directory; commit migration artifacts**

```bash
git add DAL/Migrations DAL/DAL.csproj
git commit -m "feat(db): fresh InitialCreate baseline for access-control schema, wipe dev database"
```

---

### Task 10: Final review gate (single human gate — Ruling R1)

**Files:** none (review activity)

- [ ] **Step 1: Prepare review notes**: `git diff master..refactor/access-control --stat`, the ruling recap (R1–R10 equivalents applied here), the two gap-closures (Invoice Edit, Discounts) with file:line, and the smoke-test output from Task 9.

- [ ] **Step 2: Human reviews the full diff.** Fix wave follows if findings appear (fix-all-in-one-wave precedent from Plan 1 applies).

- [ ] **Step 3: On approval** the user merges locally and pushes whenever ready. Plan execution stops here — no merge, no push by executors.
