using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace CateringEcommerce.BAL.DatabaseHelper
{
    internal static class SqlQueryTranslator
    {
        private static readonly Regex OffsetFetchRegex = new(
            @"OFFSET\s+(?<offset>[^;\r\n]+?)\s+ROWS\s+FETCH\s+NEXT\s+(?<limit>[^;\r\n]+?)\s+ROWS\s+ONLY",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex BracketIdentifierRegex = new(@"\[(?<name>[^\]]+)\]", RegexOptions.Compiled);
        private static readonly Regex DboRegex = new(@"\[?dbo\]?\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex OutputInsertedRegex = new(
            @"OUTPUT\s+INSERTED\.(?<column>[A-Za-z0-9_]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TopRegex = new(
            @"SELECT\s+TOP\s*\((?<limit>[^\)]+)\)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TopLiteralRegex = new(
            @"SELECT\s+TOP\s+(?<limit>\d+)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static CommandType NormalizeCommandType(CommandType commandType) =>
            commandType == CommandType.StoredProcedure ? CommandType.Text : commandType;

        public static string Normalize(string query, CommandType commandType, DbParameter[]? parameters)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return query;
            }

            if (commandType == CommandType.StoredProcedure)
            {
                return BuildFunctionCall(query, parameters);
            }

            var normalized = query;
            normalized = DboRegex.Replace(normalized, string.Empty);
            normalized = BracketIdentifierRegex.Replace(normalized, "${name}");
            normalized = Regex.Replace(normalized, @"\bGETDATE\s*\(\s*\)", "NOW()", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\bNEWID\s*\(\s*\)", "gen_random_uuid()", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\bISNULL\s*\(", "COALESCE(", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\bLEN\s*\(", "LENGTH(", RegexOptions.IgnoreCase);
            normalized = OutputInsertedRegex.Replace(normalized, string.Empty);
            normalized = OffsetFetchRegex.Replace(normalized, "LIMIT ${limit} OFFSET ${offset}");
            normalized = RewriteTopClause(normalized);
            normalized = normalized.Replace(" WITH (NOLOCK)", string.Empty, StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace(" CAST(SCOPE_IDENTITY() AS BIGINT)", string.Empty, StringComparison.OrdinalIgnoreCase);
            normalized = normalized.Replace(" SCOPE_IDENTITY()", string.Empty, StringComparison.OrdinalIgnoreCase);
            return normalized;
        }

        public static void AddParameters(NpgsqlCommand cmd, DbParameter[]? parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return;
            }

            foreach (var parameter in parameters)
            {
                cmd.Parameters.Add(CloneParameter(parameter));
            }
        }

        private static string BuildFunctionCall(string functionName, DbParameter[]? parameters)
        {
            var inputParams = parameters?
                .Where(p => p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.InputOutput)
                ?? Enumerable.Empty<DbParameter>();

            var argumentList = string.Join(", ", inputParams.Select(p => p.ParameterName));
            return $"SELECT * FROM {functionName}({argumentList})";
        }

        private static string RewriteTopClause(string query)
        {
            string? limit = null;
            var rewritten = TopRegex.Replace(query, match =>
            {
                limit = match.Groups["limit"].Value;
                return "SELECT ";
            }, 1);

            if (limit == null)
            {
                rewritten = TopLiteralRegex.Replace(query, match =>
                {
                    limit = match.Groups["limit"].Value;
                    return "SELECT ";
                }, 1);
            }

            if (limit == null)
            {
                return rewritten;
            }

            return AppendLimit(rewritten, limit);
        }

        private static string AppendLimit(string query, string limit)
        {
            var trimmed = query.TrimEnd();
            if (trimmed.EndsWith(";", StringComparison.Ordinal))
            {
                trimmed = trimmed[..^1];
                return $"{trimmed} LIMIT {limit};";
            }

            return $"{trimmed} LIMIT {limit}";
        }

        private static NpgsqlParameter CloneParameter(DbParameter parameter)
        {
            var npgsqlParameter = new NpgsqlParameter
            {
                ParameterName = parameter.ParameterName,
                Direction = parameter.Direction,
                SourceColumn = parameter.SourceColumn,
                SourceVersion = parameter.SourceVersion,
                SourceColumnNullMapping = parameter.SourceColumnNullMapping,
                Value = parameter.Value ?? DBNull.Value
            };

            if (parameter.Size > 0)
            {
                npgsqlParameter.Size = parameter.Size;
            }

            if (parameter.Precision > 0)
            {
                npgsqlParameter.Precision = parameter.Precision;
            }

            if (parameter.Scale > 0)
            {
                npgsqlParameter.Scale = parameter.Scale;
            }

            return npgsqlParameter;
        }
    }
}
