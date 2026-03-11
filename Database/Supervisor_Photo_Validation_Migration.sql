-- =============================================
-- Supervisor Photo Validation & Upload Migration
-- Date: February 6, 2026
-- Purpose: Add stored procedure for validating and uploading timestamped evidence with GPS
-- =============================================

USE CateringDB;
GO

-- =============================================
-- Stored Procedure: Upload Timestamped Evidence
-- Validates minimum photo requirements and GPS presence
-- =============================================
CREATE OR ALTER PROCEDURE sp_UploadTimestampedEvidence
    @AssignmentId BIGINT,
    @Phase VARCHAR(20), -- PRE_EVENT, DURING_EVENT, POST_EVENT
    @EvidenceData NVARCHAR(MAX) -- JSON array of TimestampedEvidence
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ErrorMessage NVARCHAR(500);
    DECLARE @PhotoCount INT;
    DECLARE @VideoCount INT;
    DECLARE @MinimumPhotos INT = 3;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Parse evidence data to validate
        DECLARE @Evidence TABLE (
            Type VARCHAR(10),
            Url NVARCHAR(500),
            Timestamp DATETIME2,
            GPSLocation NVARCHAR(100),
            Description NVARCHAR(MAX)
        );

        -- Insert parsed JSON into temp table
        INSERT INTO @Evidence (Type, Url, Timestamp, GPSLocation, Description)
        SELECT
            JSON_VALUE(value, '$.Type'),
            JSON_VALUE(value, '$.Url'),
            TRY_CAST(JSON_VALUE(value, '$.Timestamp') AS DATETIME2),
            JSON_VALUE(value, '$.GPSLocation'),
            JSON_VALUE(value, '$.Description')
        FROM OPENJSON(@EvidenceData);

        -- Count photos and videos
        SELECT @PhotoCount = COUNT(*) FROM @Evidence WHERE Type = 'PHOTO';
        SELECT @VideoCount = COUNT(*) FROM @Evidence WHERE Type = 'VIDEO';

        -- Validate minimum photo count
        IF @PhotoCount < @MinimumPhotos
        BEGIN
            SET @ErrorMessage =
                @Phase + ' requires minimum ' + CAST(@MinimumPhotos AS VARCHAR) +
                ' photos. Provided: ' + CAST(@PhotoCount AS VARCHAR);
            THROW 50001, @ErrorMessage, 1;
        END

        -- Validate GPS location is present for all items
        IF EXISTS (SELECT 1 FROM @Evidence WHERE GPSLocation IS NULL OR GPSLocation = '')
        BEGIN
            DECLARE @MissingGPSCount INT;
            SELECT @MissingGPSCount = COUNT(*) FROM @Evidence WHERE GPSLocation IS NULL OR GPSLocation = '';

            SET @ErrorMessage =
                CAST(@MissingGPSCount AS VARCHAR) +
                ' evidence item(s) missing GPS location. GPS is mandatory for all uploads.';
            THROW 50002, @ErrorMessage, 1;
        END

        -- Validate timestamps are reasonable (not too old, not in future)
        DECLARE @Now DATETIME2 = GETUTCDATE();
        DECLARE @MaxFutureMinutes INT = 5;  -- Allow 5 minutes clock skew
        DECLARE @MaxPastDays INT = 7;       -- Allow uploads up to 7 days old

        IF EXISTS (
            SELECT 1 FROM @Evidence
            WHERE Timestamp > DATEADD(MINUTE, @MaxFutureMinutes, @Now)
               OR Timestamp < DATEADD(DAY, -@MaxPastDays, @Now)
        )
        BEGIN
            DECLARE @InvalidTimestampCount INT;
            SELECT @InvalidTimestampCount = COUNT(*)
            FROM @Evidence
            WHERE Timestamp > DATEADD(MINUTE, @MaxFutureMinutes, @Now)
               OR Timestamp < DATEADD(DAY, -@MaxPastDays, @Now);

            SET @ErrorMessage =
                CAST(@InvalidTimestampCount AS VARCHAR) +
                ' evidence item(s) have invalid timestamps. ' +
                'Timestamps must be recent and not in the future.';
            THROW 50003, @ErrorMessage, 1;
        END

        -- All validations passed - store evidence based on phase
        IF @Phase = 'PRE_EVENT'
        BEGIN
            -- Update pre-event verification table
            UPDATE t_sys_pre_event_verification
            SET
                c_checklist_photos = @EvidenceData,
                c_modifieddate = GETUTCDATE()
            WHERE c_assignment_id = @AssignmentId;

            IF @@ROWCOUNT = 0
            BEGIN
                THROW 50004, 'Pre-event verification record not found for this assignment.', 1;
            END
        END
        ELSE IF @Phase = 'DURING_EVENT'
        BEGIN
            -- Check if during-event tracking table exists, if not use assignment table
            IF OBJECT_ID('t_sys_during_event_evidence', 'U') IS NOT NULL
            BEGIN
                -- Insert into dedicated during-event evidence table
                INSERT INTO t_sys_during_event_evidence (
                    c_assignment_id,
                    c_evidence_data,
                    c_uploaded_at
                )
                VALUES (
                    @AssignmentId,
                    @EvidenceData,
                    GETUTCDATE()
                );
            END
            ELSE
            BEGIN
                -- Fallback: Update assignment table with evidence JSON
                UPDATE t_sys_supervisor_assignment
                SET
                    c_during_event_evidence = CASE
                        WHEN c_during_event_evidence IS NULL OR c_during_event_evidence = ''
                        THEN '[' + @EvidenceData + ']'
                        ELSE c_during_event_evidence
                    END,
                    c_modifieddate = GETUTCDATE()
                WHERE c_assignment_id = @AssignmentId;
            END
        END
        ELSE IF @Phase = 'POST_EVENT'
        BEGIN
            -- Update post-event report table
            UPDATE t_sys_post_event_report
            SET
                c_evidence_photos = @EvidenceData,
                c_modifieddate = GETUTCDATE()
            WHERE c_assignment_id = @AssignmentId;

            IF @@ROWCOUNT = 0
            BEGIN
                THROW 50005, 'Post-event report record not found for this assignment.', 1;
            END
        END
        ELSE
        BEGIN
            THROW 50006, 'Invalid phase. Must be PRE_EVENT, DURING_EVENT, or POST_EVENT.', 1;
        END

        -- Log the upload in action log
        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id,
            c_assignment_id,
            c_action_type,
            c_action_description,
            c_action_data,
            c_action_result,
            c_createddate
        )
        SELECT
            sa.c_supervisor_id,
            @AssignmentId,
            'EVIDENCE_UPLOAD',
            @Phase + ' evidence upload: ' + CAST(@PhotoCount AS VARCHAR) + ' photos, ' +
                CAST(@VideoCount AS VARCHAR) + ' videos',
            @EvidenceData,
            'SUCCESS',
            GETUTCDATE()
        FROM t_sys_supervisor_assignment sa
        WHERE sa.c_assignment_id = @AssignmentId;

        COMMIT TRANSACTION;

        -- Return success
        SELECT 1 AS Success, 'Evidence uploaded successfully.' AS Message;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @CatchErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @CatchErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @CatchErrorState INT = ERROR_STATE();

        -- Log the error
        INSERT INTO t_sys_supervisor_action_log (
            c_supervisor_id,
            c_assignment_id,
            c_action_type,
            c_action_description,
            c_action_result,
            c_createddate
        )
        SELECT
            sa.c_supervisor_id,
            @AssignmentId,
            'EVIDENCE_UPLOAD',
            'Failed to upload ' + @Phase + ' evidence',
            'FAILED: ' + @CatchErrorMessage,
            GETUTCDATE()
        FROM t_sys_supervisor_assignment sa
        WHERE sa.c_assignment_id = @AssignmentId;

        -- Return error to caller
        SELECT 0 AS Success, @CatchErrorMessage AS Message;
    END CATCH
END
GO

-- =============================================
-- Test the stored procedure
-- =============================================
/*
-- Test Case 1: Valid upload with 3 photos
DECLARE @TestEvidence NVARCHAR(MAX) = N'[
    {"Type":"PHOTO","Url":"https://storage.com/photo1.jpg","Timestamp":"2026-02-06T10:00:00Z","GPSLocation":"28.6139,77.2090","Description":"Menu verification"},
    {"Type":"PHOTO","Url":"https://storage.com/photo2.jpg","Timestamp":"2026-02-06T10:05:00Z","GPSLocation":"28.6139,77.2090","Description":"Raw materials check"},
    {"Type":"PHOTO","Url":"https://storage.com/photo3.jpg","Timestamp":"2026-02-06T10:10:00Z","GPSLocation":"28.6139,77.2090","Description":"Setup verification"}
]';

EXEC sp_UploadTimestampedEvidence
    @AssignmentId = 1,
    @Phase = 'PRE_EVENT',
    @EvidenceData = @TestEvidence;

-- Test Case 2: Invalid - Only 2 photos (should fail)
DECLARE @InvalidEvidence NVARCHAR(MAX) = N'[
    {"Type":"PHOTO","Url":"https://storage.com/photo1.jpg","Timestamp":"2026-02-06T10:00:00Z","GPSLocation":"28.6139,77.2090","Description":"Menu"},
    {"Type":"PHOTO","Url":"https://storage.com/photo2.jpg","Timestamp":"2026-02-06T10:05:00Z","GPSLocation":"28.6139,77.2090","Description":"Materials"}
]';

EXEC sp_UploadTimestampedEvidence
    @AssignmentId = 1,
    @Phase = 'PRE_EVENT',
    @EvidenceData = @InvalidEvidence;
-- Expected: Error - "PRE_EVENT requires minimum 3 photos. Provided: 2"

-- Test Case 3: Invalid - Missing GPS (should fail)
DECLARE @NoGPSEvidence NVARCHAR(MAX) = N'[
    {"Type":"PHOTO","Url":"https://storage.com/photo1.jpg","Timestamp":"2026-02-06T10:00:00Z","GPSLocation":"28.6139,77.2090","Description":"Menu"},
    {"Type":"PHOTO","Url":"https://storage.com/photo2.jpg","Timestamp":"2026-02-06T10:05:00Z","GPSLocation":"","Description":"Materials"},
    {"Type":"PHOTO","Url":"https://storage.com/photo3.jpg","Timestamp":"2026-02-06T10:10:00Z","GPSLocation":"28.6139,77.2090","Description":"Setup"}
]';

EXEC sp_UploadTimestampedEvidence
    @AssignmentId = 1,
    @Phase = 'PRE_EVENT',
    @EvidenceData = @NoGPSEvidence;
-- Expected: Error - "1 evidence item(s) missing GPS location. GPS is mandatory for all uploads."
*/

PRINT 'Supervisor Photo Validation Migration Completed Successfully';
PRINT 'Stored Procedure Created: sp_UploadTimestampedEvidence';
PRINT 'Validation Rules Enforced:';
PRINT '  - Minimum 3 photos per phase';
PRINT '  - GPS location mandatory for all uploads';
PRINT '  - Timestamp validation (not future, not too old)';
PRINT '  - Automatic action logging';
GO
