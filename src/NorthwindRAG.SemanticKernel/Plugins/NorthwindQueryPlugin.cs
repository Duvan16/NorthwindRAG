using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.SemanticKernel;

namespace NorthwindRAG.SemanticKernel.Plugins;

public class NorthwindQueryPlugin
{
    private readonly string _connectionString;

    private static readonly string[] ForbiddenKeywords =
    [
        "DROP", "DELETE", "INSERT", "UPDATE", "TRUNCATE",
        "EXEC", "EXECUTE", "xp_", "sp_",
        "ALTER", "CREATE", "GRANT", "REVOKE", "DENY",
        "MERGE", "BULK", "OPENROWSET", "OPENDATASOURCE",
        "INTO",  // SELECT INTO
        "DBCC", "SHUTDOWN", "KILL",
        // Schema introspection
        "INFORMATION_SCHEMA", "SYS.", "SYSOBJECTS", "SYSCOLUMNS",
        "OBJECT_ID", "OBJECT_NAME", "COL_NAME",
        "TABLE_NAME", "COLUMN_NAME", "TABLE_SCHEMA",
        "sp_help", "sp_columns", "sp_tables"
    ];

    public NorthwindQueryPlugin(string connectionString)
    {
        _connectionString = connectionString;
    }

    [KernelFunction("ExecuteSqlQuery")]
    [Description("Executes a read-only SQL SELECT query against the Northwind database to answer numerical or aggregate questions. Only SELECT statements are allowed. Schema introspection queries are not permitted.")]
    public async Task<string> ExecuteSqlQueryAsync(
        [Description("A valid SQL SELECT query for the Northwind database")] string sqlQuery)
    {
        var trimmed = sqlQuery.Trim();

        // Safety: only allow SELECT statements
        if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return "Error: Only SELECT queries are allowed.";

        // Block dangerous and introspection keywords
        var upper = trimmed.ToUpperInvariant();
        foreach (var keyword in ForbiddenKeywords)
        {
            if (upper.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return "Error: Query contains forbidden keywords.";
        }

        // Block multiple statements (semicolon injection)
        if (trimmed.Contains(';'))
            return "Error: Multiple statements are not allowed.";

        // Block comment-based injection
        if (trimmed.Contains("--") || trimmed.Contains("/*"))
            return "Error: SQL comments are not allowed.";

        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(trimmed, conn);
            cmd.CommandTimeout = 10;
            await using var reader = await cmd.ExecuteReaderAsync();

            var results = new List<string>();
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            results.Add(string.Join(" | ", columns));

            int rowCount = 0;
            while (await reader.ReadAsync() && rowCount < 50)
            {
                var row = Enumerable.Range(0, reader.FieldCount)
                    .Select(i => reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "")
                    .ToList();
                results.Add(string.Join(" | ", row));
                rowCount++;
            }

            return string.Join("\n", results);
        }
        catch (Exception ex)
        {
            return $"Query error: {ex.Message}";
        }
    }
}
