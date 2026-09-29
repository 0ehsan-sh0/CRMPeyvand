using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// <summary>
    /// Builds the DataTable that backs a read-only grid.
    ///
    /// The UI fills grids with PublicMethods.dgvFiller using
    /// AutoGenerateColumns = true, so the column names here ARE the headers the
    /// employee sees. They must therefore match the SQL these queries replaced,
    /// character for character, or the grid silently renders an empty column.
    ///
    /// The row limit (1000 on most screens) is applied by the caller, not here.
    /// </summary>
    public static class GridTable
    {
        /// <summary>How many rows a grid shows. Matches the old "SELECT TOP (1000)".</summary>
        public const int DefaultRowLimit = 1000;

        public static DataTable Build(string[] columnNames, IEnumerable<object[]> rows)
        {
            if (columnNames == null) throw new ArgumentNullException(nameof(columnNames));

            var table = new DataTable();
            foreach (var name in columnNames)
                table.Columns.Add(name, typeof(object));

            if (rows == null) return table;

            foreach (var row in rows)
            {
                if (row == null) continue;
                if (row.Length != columnNames.Length)
                    throw new ArgumentException(
                        "row has " + row.Length + " values but there are " + columnNames.Length + " columns");

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Substring match that behaves the same on both providers.
        ///
        /// string.Contains in a LINQ-to-Entities Where clause is not usable here:
        /// it compiles to CHARINDEX, which SQLite does not have (verified:
        /// "no such function: CHARINDEX"). Filtering after materialisation also
        /// gets Persian text right, because SQLite's LIKE is not Unicode-aware
        /// and the previous N'%...%' pattern was only correct under SQL Server's
        /// collation.
        /// </summary>
        public static bool Matches(string filter, params string[] candidates)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            if (candidates == null) return false;

            foreach (var candidate in candidates)
            {
                if (string.IsNullOrEmpty(candidate)) continue;
                if (candidate.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
