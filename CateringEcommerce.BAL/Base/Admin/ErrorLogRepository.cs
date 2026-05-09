using System.Data;
using System.Data.Common;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Npgsql;
using NpgsqlTypes;

namespace CateringEcommerce.BAL.Base.Admin
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public ErrorLogRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public async Task<long> CreateAsync(ErrorLogEntry entry)
        {
            const string query = @"
                INSERT INTO t_sys_errorlogs
                (
                    c_error_id, c_message, c_exceptiontype, c_stacktrace, c_innerexception,
                    c_source, c_requestpath, c_requestmethod, c_queryparams, c_requestbody,
                    c_response_status_code, c_userid, c_user_role, c_ipaddress, c_useragent,
                    c_traceid, c_correlationid, c_environment, c_machinename, c_applicationname,
                    c_loglevel, c_executiontimems
                )
                VALUES
                (
                    @ErrorId, @Message, @ExceptionType, @StackTrace, @InnerException,
                    @Source, @RequestPath, @RequestMethod, CAST(@QueryParams AS jsonb), CAST(@RequestBody AS jsonb),
                    @ResponseStatusCode, @UserId, @UserRole, @IpAddress, @UserAgent,
                    @TraceId, @CorrelationId, @Environment, @MachineName, @ApplicationName,
                    @LogLevel, @ExecutionTimeMs
                )
                RETURNING c_id;";

            var parameters = new DbParameter[]
            {
                new NpgsqlParameter("@ErrorId", NpgsqlDbType.Uuid) { Value = entry.ErrorId },
                TextParam("@Message", entry.Message),
                TextParam("@ExceptionType", entry.ExceptionType),
                TextParam("@StackTrace", entry.StackTrace),
                TextParam("@InnerException", entry.InnerException),
                TextParam("@Source", entry.Source),
                TextParam("@RequestPath", entry.RequestPath),
                TextParam("@RequestMethod", entry.RequestMethod),
                JsonParam("@QueryParams", entry.QueryParams),
                JsonParam("@RequestBody", entry.RequestBody),
                new NpgsqlParameter("@ResponseStatusCode", NpgsqlDbType.Integer) { Value = entry.ResponseStatusCode },
                new NpgsqlParameter("@UserId", NpgsqlDbType.Bigint) { Value = entry.UserId.HasValue ? entry.UserId.Value : DBNull.Value },
                TextParam("@UserRole", string.IsNullOrWhiteSpace(entry.UserRole) ? "Anonymous" : entry.UserRole),
                TextParam("@IpAddress", entry.IpAddress),
                TextParam("@UserAgent", entry.UserAgent),
                TextParam("@TraceId", entry.TraceId),
                TextParam("@CorrelationId", entry.CorrelationId),
                TextParam("@Environment", entry.Environment),
                TextParam("@MachineName", entry.MachineName),
                TextParam("@ApplicationName", entry.ApplicationName),
                TextParam("@LogLevel", string.IsNullOrWhiteSpace(entry.LogLevel) ? "Error" : entry.LogLevel),
                new NpgsqlParameter("@ExecutionTimeMs", NpgsqlDbType.Integer) { Value = entry.ExecutionTimeMs.HasValue ? entry.ExecutionTimeMs.Value : DBNull.Value }
            };

            return await _dbHelper.ExecuteScalarAsync<long>(query, parameters);
        }

        public async Task<ErrorLogListResponse> GetLogsAsync(ErrorLogListRequest request)
        {
            request.PageNumber = Math.Max(1, request.PageNumber);
            request.PageSize = Math.Clamp(request.PageSize, 1, 100);

            var where = BuildWhereClause(request, out var parameters);
            var orderBy = GetSortColumn(request.SortBy);
            var direction = string.Equals(request.SortOrder, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
            var offset = (request.PageNumber - 1) * request.PageSize;

            var countQuery = $"SELECT COUNT(*) FROM t_sys_errorlogs {where}";
            var totalCount = await _dbHelper.ExecuteScalarAsync<int>(countQuery, CloneParameters(parameters));

            var dataParameters = parameters.ToList();
            dataParameters.Add(new NpgsqlParameter("@PageSize", NpgsqlDbType.Integer) { Value = request.PageSize });
            dataParameters.Add(new NpgsqlParameter("@Offset", NpgsqlDbType.Integer) { Value = offset });

            var dataQuery = $@"
                SELECT
                    c_id, c_error_id, c_message, c_exceptiontype, c_source, c_requestpath,
                    c_requestmethod, c_response_status_code, c_userid, c_user_role,
                    c_environment, c_loglevel, c_createdate
                FROM t_sys_errorlogs
                {where}
                ORDER BY {orderBy} {direction}
                LIMIT @PageSize OFFSET @Offset;";

            var table = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            var logs = table.AsEnumerable().Select(MapListItem).ToList();

            return new ErrorLogListResponse
            {
                Logs = logs,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<ErrorLogDetail?> GetByIdAsync(long id)
        {
            const string query = @"
                SELECT
                    c_id, c_error_id, c_message, c_exceptiontype, c_stacktrace, c_innerexception,
                    c_source, c_requestpath, c_requestmethod, c_queryparams::text AS c_queryparams,
                    c_requestbody::text AS c_requestbody, c_response_status_code, c_userid,
                    c_user_role, c_ipaddress, c_useragent, c_traceid, c_correlationid,
                    c_environment, c_machinename, c_applicationname, c_loglevel,
                    c_executiontimems, c_createdate
                FROM t_sys_errorlogs
                WHERE c_id = @Id;";

            var table = await _dbHelper.ExecuteAsync(query, new DbParameter[]
            {
                new NpgsqlParameter("@Id", NpgsqlDbType.Bigint) { Value = id }
            });

            return table.Rows.Count == 0 ? null : MapDetail(table.Rows[0]);
        }

        public async Task<int> DeleteBeforeAsync(DateTime beforeDate)
        {
            const string query = "DELETE FROM t_sys_errorlogs WHERE c_createdate < @BeforeDate;";
            return await _dbHelper.ExecuteNonQueryAsync(query, new DbParameter[]
            {
                new NpgsqlParameter("@BeforeDate", NpgsqlDbType.TimestampTz) { Value = beforeDate }
            });
        }

        private static string BuildWhereClause(ErrorLogListRequest request, out DbParameter[] parameters)
        {
            var filters = new List<string>();
            var sqlParams = new List<DbParameter>();

            if (request.FromDate.HasValue)
            {
                filters.Add("c_createdate >= @FromDate");
                sqlParams.Add(new NpgsqlParameter("@FromDate", NpgsqlDbType.TimestampTz) { Value = request.FromDate.Value });
            }

            if (request.ToDate.HasValue)
            {
                filters.Add("c_createdate <= @ToDate");
                sqlParams.Add(new NpgsqlParameter("@ToDate", NpgsqlDbType.TimestampTz) { Value = request.ToDate.Value });
            }

            if (request.ErrorId.HasValue)
            {
                filters.Add("c_error_id = @ErrorId");
                sqlParams.Add(new NpgsqlParameter("@ErrorId", NpgsqlDbType.Uuid) { Value = request.ErrorId.Value });
            }

            if (request.UserId.HasValue)
            {
                filters.Add("c_userid = @UserId");
                sqlParams.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Bigint) { Value = request.UserId.Value });
            }

            AddLikeFilter(filters, sqlParams, request.UserRole, "c_user_role", "@UserRole");
            AddLikeFilter(filters, sqlParams, request.RequestPath, "c_requestpath", "@RequestPath");

            if (!string.IsNullOrWhiteSpace(request.HttpMethod))
            {
                filters.Add("UPPER(c_requestmethod) = @HttpMethod");
                sqlParams.Add(new NpgsqlParameter("@HttpMethod", NpgsqlDbType.Text) { Value = request.HttpMethod.Trim().ToUpperInvariant() });
            }

            if (request.StatusCode.HasValue)
            {
                filters.Add("c_response_status_code = @StatusCode");
                sqlParams.Add(new NpgsqlParameter("@StatusCode", NpgsqlDbType.Integer) { Value = request.StatusCode.Value });
            }

            AddLikeFilter(filters, sqlParams, request.Environment, "c_environment", "@Environment");

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                filters.Add("(c_message ILIKE @Keyword OR c_stacktrace ILIKE @Keyword OR c_innerexception ILIKE @Keyword)");
                sqlParams.Add(new NpgsqlParameter("@Keyword", NpgsqlDbType.Text) { Value = $"%{request.Keyword.Trim()}%" });
            }

            parameters = sqlParams.ToArray();
            return filters.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", filters);
        }

        private static void AddLikeFilter(List<string> filters, List<DbParameter> parameters, string? value, string column, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            filters.Add($"{column} ILIKE {parameterName}");
            parameters.Add(new NpgsqlParameter(parameterName, NpgsqlDbType.Text) { Value = $"%{value.Trim()}%" });
        }

        private static string GetSortColumn(string? sortBy)
        {
            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "errorid" => "c_error_id",
                "message" => "c_message",
                "requestpath" => "c_requestpath",
                "httpmethod" or "requestmethod" => "c_requestmethod",
                "statuscode" or "responsestatuscode" => "c_response_status_code",
                "userid" => "c_userid",
                "userrole" => "c_user_role",
                "environment" => "c_environment",
                "loglevel" => "c_loglevel",
                _ => "c_createdate"
            };
        }

        private static ErrorLogListItem MapListItem(DataRow row)
        {
            return new ErrorLogListItem
            {
                Id = Get<long>(row, "c_id"),
                ErrorId = Get<Guid>(row, "c_error_id"),
                Message = GetString(row, "c_message"),
                ExceptionType = GetString(row, "c_exceptiontype"),
                Source = GetString(row, "c_source"),
                RequestPath = GetString(row, "c_requestpath"),
                RequestMethod = GetString(row, "c_requestmethod"),
                ResponseStatusCode = Get<int>(row, "c_response_status_code"),
                UserId = GetNullable<long>(row, "c_userid"),
                UserRole = GetString(row, "c_user_role") ?? "Anonymous",
                Environment = GetString(row, "c_environment"),
                LogLevel = GetString(row, "c_loglevel") ?? "Error",
                CreatedAt = Get<DateTime>(row, "c_createdate")
            };
        }

        private static ErrorLogDetail MapDetail(DataRow row)
        {
            var item = MapListItem(row);
            return new ErrorLogDetail
            {
                Id = item.Id,
                ErrorId = item.ErrorId,
                Message = item.Message,
                ExceptionType = item.ExceptionType,
                Source = item.Source,
                RequestPath = item.RequestPath,
                RequestMethod = item.RequestMethod,
                ResponseStatusCode = item.ResponseStatusCode,
                UserId = item.UserId,
                UserRole = item.UserRole,
                Environment = item.Environment,
                LogLevel = item.LogLevel,
                CreatedAt = item.CreatedAt,
                StackTrace = GetString(row, "c_stacktrace"),
                InnerException = GetString(row, "c_innerexception"),
                QueryParams = GetString(row, "c_queryparams"),
                RequestBody = GetString(row, "c_requestbody"),
                IpAddress = GetString(row, "c_ipaddress"),
                UserAgent = GetString(row, "c_useragent"),
                TraceId = GetString(row, "c_traceid"),
                CorrelationId = GetString(row, "c_correlationid"),
                MachineName = GetString(row, "c_machinename"),
                ApplicationName = GetString(row, "c_applicationname"),
                ExecutionTimeMs = GetNullable<int>(row, "c_executiontimems")
            };
        }

        private static T Get<T>(DataRow row, string column)
        {
            var value = row[column];
            if (value == DBNull.Value)
            {
                return default!;
            }

            if (typeof(T) == typeof(Guid))
            {
                return (T)(object)Guid.Parse(value.ToString()!);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        private static T? GetNullable<T>(DataRow row, string column) where T : struct
        {
            return row[column] == DBNull.Value ? null : Get<T>(row, column);
        }

        private static string? GetString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? null : row[column].ToString();
        }

        private static DbParameter[] CloneParameters(DbParameter[] parameters)
        {
            return parameters.Select(parameter =>
            {
                var original = (NpgsqlParameter)parameter;
                return new NpgsqlParameter(original.ParameterName, original.NpgsqlDbType) { Value = original.Value };
            }).Cast<DbParameter>().ToArray();
        }

        private static NpgsqlParameter TextParam(string name, string? value)
        {
            return new NpgsqlParameter(name, NpgsqlDbType.Text) { Value = string.IsNullOrEmpty(value) ? DBNull.Value : value };
        }

        private static NpgsqlParameter JsonParam(string name, string? value)
        {
            return new NpgsqlParameter(name, NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value };
        }
    }
}
