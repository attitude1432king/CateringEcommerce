using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.Data.SqlClient;
using Scriban;
using System.Data;

namespace CateringEcommerce.BAL.Base.Admin
{
    public partial class SettingsRepository
    {
        // =============================================
        // EMAIL TEMPLATE METHODS
        // =============================================

        public async Task<EmailTemplateListResponse> GetEmailTemplatesAsync(EmailTemplateListRequest request)
        {
            var conditions = new List<string>();
            var baseParameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                conditions.Add("t.c_category = @Category");
                baseParameters.Add(new SqlParameter("@Category", request.Category));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                conditions.Add("(t.c_template_code LIKE @SearchTerm OR t.c_template_name LIKE @SearchTerm OR t.c_description LIKE @SearchTerm)");
                baseParameters.Add(new SqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
            }

            if (request.IsActive.HasValue)
            {
                conditions.Add("t.c_is_active = @IsActive");
                baseParameters.Add(new SqlParameter("@IsActive", request.IsActive.Value));
            }

            string whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            // COUNT QUERY
            var countQuery = $@"
                SELECT COUNT(*)
                FROM {Table.SysNotificationTemplates} t
                {whereClause}";

            var totalCount = Convert.ToInt32(
                await _dbHelper.ExecuteScalarAsync(
                    countQuery,
                    baseParameters.Select(CloneParameter).ToArray()
                )
            );

            // DATA QUERY
            var offset = (request.PageNumber - 1) * request.PageSize;

            var sortColumn = request.SortBy switch
            {
                "TemplateCode" => "t.c_template_code",
                "TemplateName" => "t.c_template_name",
                "Category" => "t.c_category",
                "ModifiedDate" => "t.c_modifieddate",
                _ => "t.c_template_name"
            };

            var sortOrder = request.SortOrder?.ToUpper() == "DESC" ? "DESC" : "ASC";

            var dataParameters = baseParameters
                .Select(CloneParameter)
                .ToList();

            dataParameters.Add(new SqlParameter("@Offset", offset));
            dataParameters.Add(new SqlParameter("@PageSize", request.PageSize));

            var dataQuery = $@"
                SELECT
                    t.c_template_id AS TemplateId,
                    t.c_template_code AS TemplateCode,
                    t.c_template_name AS TemplateName,
                    t.c_description AS Description,
                    t.c_language AS Language,
                    t.c_channel AS Channel,
                    t.c_category AS Category,
                    t.c_subject AS Subject,
                    t.c_body AS Body,
                    t.c_version AS Version,
                    t.c_is_active AS IsActive,
                    t.c_usage_count AS UsageCount,
                    t.c_createddate AS CreatedDate,
                    t.c_createdby AS CreatedBy,
                    t.c_modifieddate AS ModifiedDate,
                    t.c_modifiedby AS ModifiedBy,
                    a.c_fullname AS ModifiedByName
                FROM {Table.SysNotificationTemplates} t
                LEFT JOIN {Table.SysAdmin} a ON t.c_modifiedby = a.c_adminid
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var templates = new List<EmailTemplateItem>();

            var templateTable = await _dbHelper.ExecuteAsync(dataQuery, dataParameters.ToArray());
            foreach (DataRow row in templateTable.Rows)
            {
                templates.Add(MapEmailTemplateFromRow(row));
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EmailTemplateListResponse
            {
                Templates = templates,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<EmailTemplateItem> GetEmailTemplateByIdAsync(long templateId)
        {
            var query = $@"
                SELECT
                    t.c_template_id AS TemplateId,
                    t.c_template_code AS TemplateCode,
                    t.c_template_name AS TemplateName,
                    t.c_description AS Description,
                    t.c_language AS Language,
                    t.c_channel AS Channel,
                    t.c_category AS Category,
                    t.c_subject AS Subject,
                    t.c_body AS Body,
                    t.c_version AS Version,
                    t.c_is_active AS IsActive,
                    t.c_usage_count AS UsageCount,
                    t.c_createddate AS CreatedDate,
                    t.c_createdby AS CreatedBy,
                    t.c_modifieddate AS ModifiedDate,
                    t.c_modifiedby AS ModifiedBy,
                    a.c_fullname AS ModifiedByName
                FROM {Table.SysNotificationTemplates} t
                LEFT JOIN {Table.SysAdmin} a ON t.c_modifiedby = a.c_adminid
                WHERE t.c_template_id = @TemplateId";

            var parameters = new[]
            {
                new SqlParameter("@TemplateId", templateId)
            };

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    return MapEmailTemplateFromReader(reader);
                }
            }

            return null;
        }

        public async Task<EmailTemplateItem> GetEmailTemplateByCodeAsync(string templateCode)
        {
            var query = $@"
                SELECT
                    t.c_template_id AS TemplateId,
                    t.c_template_code AS TemplateCode,
                    t.c_template_name AS TemplateName,
                    t.c_description AS Description,
                    t.c_language AS Language,
                    t.c_channel AS Channel,
                    t.c_category AS Category,
                    t.c_subject AS Subject,
                    t.c_body AS Body,
                    t.c_version AS Version,
                    t.c_is_active AS IsActive,
                    t.c_usage_count AS UsageCount,
                    t.c_createddate AS CreatedDate,
                    t.c_createdby AS CreatedBy,
                    t.c_modifieddate AS ModifiedDate,
                    t.c_modifiedby AS ModifiedBy,
                    a.c_fullname AS ModifiedByName
                FROM {Table.SysNotificationTemplates} t
                LEFT JOIN {Table.SysAdmin} a ON t.c_modifiedby = a.c_adminid
                WHERE t.c_template_code = @TemplateCode";

            var parameters = new[]
            {
                new SqlParameter("@TemplateCode", templateCode)
            };

            var templateTable = await _dbHelper.ExecuteAsync(query, parameters);
            if (templateTable.Rows.Count > 0)
            {
                return MapEmailTemplateFromRow(templateTable.Rows[0]);
            }

            return null;
        }

        public async Task<long> CreateEmailTemplateAsync(CreateEmailTemplateRequest request, long adminId)
        {
            var existingTemplate = await GetEmailTemplateByCodeAsync(request.TemplateCode);
            if (existingTemplate != null)
            {
                throw new InvalidOperationException($"Template code '{request.TemplateCode}' already exists.");
            }

            var query = $@"
                INSERT INTO {Table.SysNotificationTemplates}
                (c_template_code, c_template_name, c_description, c_language, c_channel, c_category, c_subject, c_body, c_version, c_is_active, c_usage_count, c_createddate, c_createdby)
                VALUES
                (@TemplateCode, @TemplateName, @Description, @Language, @Channel, @Category, @Subject, @Body, 1, @IsActive, 0, GETDATE(), @CreatedBy);
                SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
                new SqlParameter("@TemplateCode", request.TemplateCode),
                new SqlParameter("@TemplateName", request.TemplateName),
                new SqlParameter("@Description", (object)request.Description ?? DBNull.Value),
                new SqlParameter("@Language", request.Language ?? "en"),
                new SqlParameter("@Channel", request.Channel ?? "EMAIL"),
                new SqlParameter("@Category", request.Category),
                new SqlParameter("@Subject", (object)request.Subject ?? DBNull.Value),
                new SqlParameter("@Body", request.Body),
                new SqlParameter("@IsActive", request.IsActive),
                new SqlParameter("@CreatedBy", adminId)
            };

            var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt64(result);
        }

        public async Task<bool> UpdateEmailTemplateAsync(UpdateEmailTemplateRequest request, long adminId, string adminName)
        {
            var currentTemplate = await GetEmailTemplateByIdAsync(request.TemplateId);
            if (currentTemplate == null)
            {
                throw new InvalidOperationException("Template not found");
            }

            var query = $@"
                UPDATE {Table.SysNotificationTemplates}
                SET
                    c_template_name = @TemplateName,
                    c_description = @Description,
                    c_category = @Category,
                    c_subject = @Subject,
                    c_body = @Body,
                    c_is_active = @IsActive,
                    c_version = c_version + 1,
                    c_modifieddate = GETDATE(),
                    c_modifiedby = @ModifiedBy
                WHERE c_template_id = @TemplateId";

            var parameters = new[]
            {
                new SqlParameter("@TemplateName", request.TemplateName),
                new SqlParameter("@Description", (object)request.Description ?? DBNull.Value),
                new SqlParameter("@Category", request.Category),
                new SqlParameter("@Subject", (object)request.Subject ?? DBNull.Value),
                new SqlParameter("@Body", request.Body),
                new SqlParameter("@IsActive", request.IsActive),
                new SqlParameter("@ModifiedBy", adminId),
                new SqlParameter("@TemplateId", request.TemplateId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            if (rowsAffected > 0)
            {
                var auditQuery = $@"
                    INSERT INTO {Table.SysAdminAuditLogs}
                    (c_admin_id, c_action, c_entity_type, c_entity_id, c_old_values, c_new_values, c_change_reason, c_timestamp)
                    VALUES
                    (@AdminId, 'UPDATE', 'EmailTemplate', @EntityId, @OldValues, @NewValues, @ChangeReason, GETDATE())";

                var auditParameters = new[]
                {
                    new SqlParameter("@AdminId", adminId),
                    new SqlParameter("@EntityId", request.TemplateId.ToString()),
                    new SqlParameter("@OldValues", $"Subject: {currentTemplate.Subject}, Version: {currentTemplate.Version}"),
                    new SqlParameter("@NewValues", $"Subject: {request.Subject}, Version: {currentTemplate.Version + 1}"),
                    new SqlParameter("@ChangeReason", request.ChangeReason ?? "Template updated")
                };

                await _dbHelper.ExecuteNonQueryAsync(auditQuery, auditParameters);
            }

            return rowsAffected > 0;
        }

        public async Task<TemplatePreviewResponse> PreviewTemplateAsync(TemplatePreviewRequest request)
        {
            string subject = request.Subject;
            string body = request.Body;

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                EmailTemplateItem template = null;

                if (request.TemplateId.HasValue)
                {
                    template = await GetEmailTemplateByIdAsync(request.TemplateId.Value);
                }
                else if (!string.IsNullOrEmpty(request.TemplateCode))
                {
                    template = await GetEmailTemplateByCodeAsync(request.TemplateCode);
                }

                if (template == null)
                {
                    throw new InvalidOperationException("Template not found");
                }

                subject = template.Subject;
                body = template.Body;
            }

            var sampleData = request.SampleData ?? new Dictionary<string, string>();

            if (!sampleData.ContainsKey("app_name"))
                sampleData["app_name"] = "Catering Ecommerce Platform";
            if (!sampleData.ContainsKey("customer_name"))
                sampleData["customer_name"] = "John Doe";
            if (!sampleData.ContainsKey("support_email"))
                sampleData["support_email"] = "support@cateringecommerce.com";

            var missingVariables = new List<string>();

            try
            {
                var subjectTemplate = Template.Parse(subject);
                var renderedSubject = await subjectTemplate.RenderAsync(sampleData);

                var bodyTemplate = Template.Parse(body);
                var renderedBody = await bodyTemplate.RenderAsync(sampleData);

                var variablePattern = new System.Text.RegularExpressions.Regex(@"{{\s*(\w+)\s*}}");
                var subjectMatches = variablePattern.Matches(subject);
                var bodyMatches = variablePattern.Matches(body);

                var allVariables = new HashSet<string>();
                foreach (System.Text.RegularExpressions.Match match in subjectMatches)
                {
                    allVariables.Add(match.Groups[1].Value);
                }
                foreach (System.Text.RegularExpressions.Match match in bodyMatches)
                {
                    allVariables.Add(match.Groups[1].Value);
                }

                foreach (var variable in allVariables)
                {
                    if (!sampleData.ContainsKey(variable))
                    {
                        missingVariables.Add(variable);
                    }
                }

                return new TemplatePreviewResponse
                {
                    RenderedSubject = renderedSubject,
                    RenderedBody = renderedBody,
                    MissingVariables = missingVariables
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error rendering template: {ex.Message}");
            }
        }

        public async Task<TemplateVariablesResponse> GetTemplateVariablesAsync(string templateCode)
        {
            var query = $@"
                SELECT
                    v.c_variable_id AS VariableId,
                    v.c_template_code AS TemplateCode,
                    v.c_variable_name AS VariableName,
                    v.c_variable_key AS VariableKey,
                    v.c_description AS Description,
                    v.c_example_value AS ExampleValue
                FROM {Table.SysTemplateVariables} v
                WHERE v.c_template_code = @TemplateCode
                ORDER BY v.c_variable_name";

            var parameters = new[]
            {
                new SqlParameter("@TemplateCode", templateCode)
            };

            var variables = new List<TemplateVariableItem>();

            var variableTable = await _dbHelper.ExecuteAsync(query, parameters);
            foreach (DataRow row in variableTable.Rows)
            {
                variables.Add(new TemplateVariableItem
                {
                    VariableId = row.Field<long>("VariableId"),
                    TemplateCode = row.Field<string>("TemplateCode"),
                    VariableName = row.Field<string>("VariableName"),
                    VariableKey = row.Field<string>("VariableKey"),
                    Description = row.IsNull("Description") ? null : row.Field<string>("Description"),
                    ExampleValue = row.IsNull("ExampleValue") ? null : row.Field<string>("ExampleValue")
                });
            }

            return new TemplateVariablesResponse
            {
                TemplateCode = templateCode,
                Variables = variables
            };
        }

        public async Task<bool> SendTestEmailAsync(long templateId, string toEmail, Dictionary<string, string> sampleData)
        {
            var template = await GetEmailTemplateByIdAsync(templateId);
            if (template == null)
            {
                throw new InvalidOperationException("Template not found");
            }

            var preview = await PreviewTemplateAsync(new TemplatePreviewRequest
            {
                TemplateId = templateId,
                SampleData = sampleData
            });

            var emailSettings = await GetSettingsByCategoryAsync("EMAIL");
            var smtpHost = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.SMTP_HOST")?.SettingValue;
            var smtpPort = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.SMTP_PORT")?.SettingValue;
            var smtpUsername = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.SMTP_USERNAME")?.SettingValue;
            var smtpPassword = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.SMTP_PASSWORD")?.SettingValue;
            var enableSsl = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.ENABLE_SSL")?.SettingValue == "true";
            var fromAddress = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.FROM_ADDRESS")?.SettingValue;
            var fromName = emailSettings.FirstOrDefault(s => s.SettingKey == "EMAIL.FROM_NAME")?.SettingValue;

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort))
            {
                throw new InvalidOperationException("SMTP settings not configured");
            }

            using (var smtpClient = new System.Net.Mail.SmtpClient(smtpHost, int.Parse(smtpPort)))
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = enableSsl;

                var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(fromAddress, fromName),
                    Subject = $"[TEST] {preview.RenderedSubject}",
                    Body = preview.RenderedBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to send test email: {ex.Message}");
                }
            }
        }

        // =============================================
        // PRIVATE MAPPING HELPERS
        // =============================================

        private static EmailTemplateItem MapEmailTemplateFromRow(DataRow row)
        {
            return new EmailTemplateItem
            {
                TemplateId = row.Field<long>("TemplateId"),
                TemplateCode = row.Field<string>("TemplateCode"),
                TemplateName = row.Field<string>("TemplateName"),
                Description = row.IsNull("Description") ? null : row.Field<string>("Description"),
                Language = row.Field<string>("Language"),
                Channel = row.Field<string>("Channel"),
                Category = row.Field<string>("Category"),
                Subject = row.IsNull("Subject") ? null : row.Field<string>("Subject"),
                Body = row.Field<string>("Body"),
                Version = row.Field<int>("Version"),
                IsActive = row.Field<bool>("IsActive"),
                UsageCount = row.Field<int>("UsageCount"),
                CreatedDate = row.Field<DateTime>("CreatedDate"),
                CreatedBy = row.IsNull("CreatedBy") ? null : row.Field<long>("CreatedBy"),
                ModifiedDate = row.IsNull("ModifiedDate") ? null : row.Field<DateTime>("ModifiedDate"),
                ModifiedBy = row.IsNull("ModifiedBy") ? null : row.Field<long>("ModifiedBy"),
                ModifiedByName = row.IsNull("ModifiedByName") ? null : row.Field<string>("ModifiedByName")
            };
        }

        private static EmailTemplateItem MapEmailTemplateFromReader(SqlDataReader reader)
        {
            return new EmailTemplateItem
            {
                TemplateId = reader.GetInt64(reader.GetOrdinal("TemplateId")),
                TemplateCode = reader.GetString(reader.GetOrdinal("TemplateCode")),
                TemplateName = reader.GetString(reader.GetOrdinal("TemplateName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Language = reader.GetString(reader.GetOrdinal("Language")),
                Channel = reader.GetString(reader.GetOrdinal("Channel")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Subject = reader.IsDBNull(reader.GetOrdinal("Subject")) ? null : reader.GetString(reader.GetOrdinal("Subject")),
                Body = reader.GetString(reader.GetOrdinal("Body")),
                Version = reader.GetInt32(reader.GetOrdinal("Version")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                UsageCount = reader.GetInt32(reader.GetOrdinal("UsageCount")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt64(reader.GetOrdinal("CreatedBy")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt64(reader.GetOrdinal("ModifiedBy")),
                ModifiedByName = reader.IsDBNull(reader.GetOrdinal("ModifiedByName")) ? null : reader.GetString(reader.GetOrdinal("ModifiedByName"))
            };
        }
    }
}
