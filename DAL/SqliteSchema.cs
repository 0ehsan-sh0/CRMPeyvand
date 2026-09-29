using System;
using System.Data;
using System.Data.Common;

namespace DAL
{
    /// <summary>
    /// The SQLite schema, written by hand.
    ///
    /// EF6's SQLite provider cannot generate tables: Database.CreateIfNotExists()
    /// returns without throwing and creates nothing (verified). EF6 Code First
    /// Migrations cannot be used either, because DAL/Migrations hardcodes
    /// "dbo." table names and its SqlGenerator is SQL Server only. So the DDL
    /// lives here and is applied on first run.
    ///
    /// Names must match the SQL Server schema exactly, including the lowercase
    /// "id" on 12 tables versus uppercase "Id" on CatalogItems and InvoiceLines,
    /// and including "RememberMes" (EF6's pluraliser produced that spelling and
    /// existing SQL Server data is keyed to it).
    /// </summary>
    public static class SqliteSchema
    {
        public const string Invariant = "System.Data.SQLite";

        public const string Ddl = @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS UserGroups (
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Title       TEXT NULL,
    IsBuiltIn   INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS AccessGrants (
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Section     INTEGER NOT NULL,
    Operation   INTEGER NOT NULL,
    UserGroup_id INTEGER NULL REFERENCES UserGroups (id)
);

CREATE TABLE IF NOT EXISTS Users (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name         TEXT NULL,
    UserName     TEXT NULL,
    Password     TEXT NULL,
    Picture      TEXT NULL,
    RegDate      DATETIME NOT NULL,
    DeleteStatus INTEGER NOT NULL,
    UserGroup_id INTEGER NULL REFERENCES UserGroups (id)
);

CREATE TABLE IF NOT EXISTS Customers (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name         TEXT NULL,
    Phone        TEXT NULL,
    RegDate      DATETIME NOT NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS ActivityCategories (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    CategoryName TEXT NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Activities (
    id                   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Title                TEXT NULL,
    Info                 TEXT NULL,
    RegDate              DATETIME NOT NULL,
    DeleteStatus         INTEGER NOT NULL,
    ActivityCategory_id  INTEGER NULL REFERENCES ActivityCategories (id),
    Customer_id          INTEGER NULL REFERENCES Customers (id),
    User_id              INTEGER NULL REFERENCES Users (id)
);

CREATE TABLE IF NOT EXISTS OffCodes (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Code         TEXT NULL,
    IsPrice      INTEGER NOT NULL,
    Price        DECIMAL(18,2) NULL,
    Percent      INTEGER NULL,
    RegDate      DATETIME NOT NULL,
    ExpireDate   DATETIME NULL,
    LimitCount   INTEGER NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Invoices (
    id              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    RegDate         DATETIME NOT NULL,
    IsCheckedout    INTEGER NOT NULL,
    CheckoutDate    DATETIME NULL,
    DeleteStatus    INTEGER NOT NULL,
    OffCode         TEXT NULL,
    DiscountAmount  DECIMAL(18,2) NOT NULL,
    Customer_id     INTEGER NULL REFERENCES Customers (id),
    User_id         INTEGER NULL REFERENCES Users (id)
);

CREATE TABLE IF NOT EXISTS CatalogItems (
    Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name       TEXT NULL,
    Kind       INTEGER NOT NULL,
    SalePrice  DECIMAL(18,2) NOT NULL,
    Stock      INTEGER NOT NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS InvoiceLines (
    Id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    InvoiceId     INTEGER NOT NULL,
    CatalogItemId INTEGER NOT NULL,
    Quantity      INTEGER NOT NULL,
    UnitPrice     DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceId)     REFERENCES Invoices (id)     ON DELETE CASCADE,
    FOREIGN KEY (CatalogItemId) REFERENCES CatalogItems (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Reminders (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Title        TEXT NULL,
    Info         TEXT NULL,
    RegDate      DATETIME NOT NULL,
    RemindDate   DATETIME NOT NULL,
    DeleteStatus INTEGER NOT NULL,
    IsReminded   INTEGER NOT NULL,
    User_id      INTEGER NULL REFERENCES Users (id)
);

CREATE TABLE IF NOT EXISTS MessagePanels (
    id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    APIToken   TEXT NULL,
    LineNumber TEXT NULL,
    RegDate    DATETIME NOT NULL,
    EditDate   DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Messages (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Content      TEXT NULL,
    DeleteStatus INTEGER NOT NULL,
    RegDate      DATETIME NOT NULL
);

-- EF6 pluralised DbSet<RememberMe> to ""RememberMes"" (no trailing s on the Me).
-- The name is load-bearing: existing SQL Server rows live in a table of this name.
CREATE TABLE IF NOT EXISTS RememberMes (
    id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    UserName      TEXT NULL,
    IsRemembered  INTEGER NOT NULL,
    LastLoginTime DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_AccessGrants_UserGroup_id ON AccessGrants (UserGroup_id);
CREATE INDEX IF NOT EXISTS IX_Users_UserGroup_id          ON Users (UserGroup_id);
CREATE INDEX IF NOT EXISTS IX_Activities_Category_id     ON Activities (ActivityCategory_id);
CREATE INDEX IF NOT EXISTS IX_Activities_Customer_id      ON Activities (Customer_id);
CREATE INDEX IF NOT EXISTS IX_Activities_User_id          ON Activities (User_id);
CREATE INDEX IF NOT EXISTS IX_Invoices_Customer_id        ON Invoices (Customer_id);
CREATE INDEX IF NOT EXISTS IX_Invoices_User_id            ON Invoices (User_id);
CREATE INDEX IF NOT EXISTS IX_InvoiceLines_InvoiceId      ON InvoiceLines (InvoiceId);
CREATE INDEX IF NOT EXISTS IX_InvoiceLines_CatalogItemId  ON InvoiceLines (CatalogItemId);
CREATE INDEX IF NOT EXISTS IX_Reminders_User_id           ON Reminders (User_id);
";

        public static void Ensure(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (connection.State != ConnectionState.Open) connection.Open();

            using (var command = connection.CreateCommand())
            {
                // PRAGMA foreign_keys is a no-op inside a transaction, and the
                // batch below is not one, so this applies.
                command.CommandText = Ddl;
                command.ExecuteNonQuery();
            }
        }
    }
}
