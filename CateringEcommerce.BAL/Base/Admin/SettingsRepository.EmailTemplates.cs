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
                "ModifiedDate" => "t.c_modified_date",
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
                    t.c_category AS Category,
                    t.c_subject AS Subject,
                    t.c_body AS Body,
                    t.c_description AS Description,
                    t.c_version AS Version,
                    t.c_is_active AS IsActive,
                    t.c_created_date AS CreatedDate,
                    t.c_created_by AS CreatedBy,
                    t.c_modified_date AS ModifiedDate,
                    t.c_modified_by AS ModifiedBy,
                    a.c_fullname AS ModifiedByName
                FROM {Table.SysNotificationTemplates} t
                LEFT JOIN t_admin_users a ON t.c_modified_by = a.c_admin_id
                {whereClause}
                ORDER BY {sortColumn} {sortOrder}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var templates = new List<EmailTemplateItem>();

            using (var reader = await _dbHelper.ExecuteReaderAsync(dataQuery, dataParameters.ToArray()))
            {
                while (await reader.ReadAsync())
                {
                    templates.Add(new EmailTemplateItem
                    {
                        TemplateId = reader.GetInt64(reader.GetOrdinal("TemplateId")),
                        TemplateCode = reader.GetString(reader.GetOrdinal("TemplateCode")),
                        TemplateName = reader.GetString(reader.GetOrdinal("TemplateName")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Subject = reader.GetString(reader.GetOrdinal("Subject")),
                        Body = reader.GetString(reader.GetOrdinal("Body")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        Version = reader.GetInt32(reader.GetOrdinal("Version")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt64(reader.GetOrdinal("CreatedBy")),
                        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                        ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt64(reader.GetOrdinal("ModifiedBy")),
                        ModifiedByName = reader.IsDBNull(reader.GetOrdinal("ModifiedByName")) ? null : reader.GetString(reader.GetOrdinal("ModifiedByName"))
                    });
                }
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
                    t.c_category AS Category,
                    t.c_subject AS Subject,
                    t.c_body AS Body,
                    t.c_description AS Description,
                    t.c_version AS Version,
                    t.c_is_active AS IsActive,
                    t.c_created_date AS CreatedDate,
                    t.c_created_by AS CreatedBy,
                    t.c_modified_date AS ModifiedDate,
                    t.c_modified_by AS ModifiedBy,
                    a.c_fullname AS ModifiedByName
                FROM {Table.SysNotificationTemplates} t
                LEFT JOIN t_admin_users a ON t.c_modified_by = a.c_admin_id
                WHERE t.c_template_id = @TemplateId";

            var parameters = new[]
            {
                new SqlParameter("@TemplateId", templateId)
            };

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    return new EmailTemplateItem
                    {
                        TemplateId = reader.GetInt64(reader.GetOrdinal("TemplateId")),
                        TemplateCode = reader.GetString(reader.GetOrdinal("TemplateCode")),
                        TemplateName = reader.GetString(reader.GetOrdinal("TemplateName")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Subject = reader.GetString(reader.GetOrdinal("Subject")),
                        Body = reader.GetString(reader.GetOrdinal("Body")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        Version = reader.GetInt32(reader.GetOrdinal("Version")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt64(reader.GetOrdinal("CreatedBy")),
                        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                        ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt64(reader.GetOrdinal("ModifiedBy")),
                        ModifiedByName = reader.IsDBNull(reader.GetOrdinal("ModifiedByName")) ? null : reader.GetString(reader.GetOrdinal("ModifiedByName"))
                    };
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
                    t.c_category AS Category,
                    t.c_subject AS Subject,
                    t.c_body AS Body,
                    t.c_description AS Description,
                    t.c_version AS Version,
                    t.c_is_active AS IsActive,
                    t.c_created_date AS CreatedDate,
                    t.c_created_by AS CreatedBy,
                    t.c_modified_date AS ModifiedDate,
                    t.c_modified_by AS ModifiedBy,
                    a.c_fullname AS ModifiedByName
                FROM {Table.SysNotificationTemplates} t
                LEFT JOIN t_admin_users a ON t.c_modified_by = a.c_admin_id
                WHERE t.c_template_code = @TemplateCode";

            var parameters = new[]
            {
                new SqlParameter("@TemplateCode", templateCode)
            };

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    return new EmailTemplateItem
                    {
                        TemplateId = reader.GetInt64(reader.GetOrdinal("TemplateId")),
                        TemplateCode = reader.GetString(reader.GetOrdinal("TemplateCode")),
                        TemplateName = reader.GetString(reader.GetOrdinal("TemplateName")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Subject = reader.GetString(reader.GetOrdinal("Subject")),
                        Body = reader.GetString(reader.GetOrdinal("Body")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        Version = reader.GetInt32(reader.GetOrdinal("Version")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt64(reader.GetOrdinal("CreatedBy")),
                        ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                        ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetInt64(reader.GetOrdinal("ModifiedBy")),
                        ModifiedByName = reader.IsDBNull(reader.GetOrdinal("ModifiedByName")) ? null : reader.GetString(reader.GetOrdinal("ModifiedByName"))
                    };
                }
            }

            return null;
        }

        public async Task<bool> UpdateEmailTemplateAsync(UpdateEmailTemplateRequest request, long adminId, string adminName)
        {
            // Get current template for validation and version
            var currentTemplate = await GetEmailTemplateByIdAsync(request.TemplateId);
            if (currentTemplate == null)
            {
                throw new InvalidOperationException("Template not found");
            }

            // Validate template syntax (basic check)
            try
            {
                var subjectTemplate = Template.Parse(request.Subject);
                var bodyTemplate = Template.Parse(request.Body);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Template syntax error: {ex.Message}");
            }

            // Update template (increment version)
            var query = $@"
                UPDATE {Table.SysNotificationTemplates}
                SET
                    c_subject = @Subject,
                    c_body = @Body,
                    c_version = c_version + 1,
                    c_modified_date = GETDATE(),
                    c_modified_by = @ModifiedBy
                WHERE c_template_id = @TemplateId";

            var parameters = new[]
            {
                new SqlParameter("@Subject", request.Subject),
                new SqlParameter("@Body", request.Body),
                new SqlParameter("@ModifiedBy", adminId),
                new SqlParameter("@TemplateId", request.TemplateId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            // Log to audit (using admin activity log)
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

            // If no subject/body provided, get from template
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

            // Prepare sample data
            var sampleData = request.SampleData ?? new Dictionary<string, string>();

            // Provide default values for common variables if not provided
            if (!sampleData.ContainsKey("app_name"))
                sampleData["app_name"] = "Catering Ecommerce Platform";
            if (!sampleData.ContainsKey("customer_name"))
                sampleData["customer_name"] = "John Doe";
            if (!sampleData.ContainsKey("support_email"))
                sampleData["support_email"] = "support@cateringecommerce.com";

            var missingVariables = new List<string>();

            try
            {
                // Render subject
                var subjectTemplate = Template.Parse(subject);
                var renderedSubject = await subjectTemplate.RenderAsync(sampleData);

                // Render body
                var bodyTemplate = Template.Parse(body);
                var renderedBody = await bodyTemplate.RenderAsync(sampleData);

                // Find missing variables (variables in template but not in sample data)
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

            using (var reader = await _dbHelper.ExecuteReaderAsync(query, parameters))
            {
                while (await reader.ReadAsync())
                {
                    variables.Add(new TemplateVariableItem
                    {
                        VariableId = reader.GetInt64(reader.GetOrdinal("VariableId")),
                        TemplateCode = reader.GetString(reader.GetOrdinal("TemplateCode")),
                        VariableName = reader.GetString(reader.GetOrdinal("VariableName")),
                        VariableKey = reader.GetString(reader.GetOrdinal("VariableKey")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        ExampleValue = reader.IsDBNull(reader.GetOrdinal("ExampleValue")) ? null : reader.GetString(reader.GetOrdinal("ExampleValue"))
                    });
                }
            }

            return new TemplateVariablesResponse
            {
                TemplateCode = templateCode,
                Variables = variables
            };
        }

        public async Task<bool> SendTestEmailAsync(long templateId, string toEmail, Dictionary<string, string> sampleData)
        {
            // Get template
            var template = await GetEmailTemplateByIdAsync(templateId);
            if (template == null)
            {
                throw new InvalidOperationException("Template not found");
            }

            // Render template
            var preview = await PreviewTemplateAsync(new TemplatePreviewRequest
            {
                TemplateId = templateId,
                SampleData = sampleData
            });

            // Get email settings
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

            // Send email using SMTP
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
    }
}
