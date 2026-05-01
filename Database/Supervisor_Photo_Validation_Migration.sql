-- =============================================
-- Supervisor Photo Validation & Upload Migration
-- Date: February 6, 2026
-- Purpose: Add stored procedure for validating and uploading timestamped evidence with GPS
-- =============================================

-- =============================================
-- Stored Procedure: Upload Timestamped Evidence
-- Validates minimum photo requirements and GPS presence
-- =============================================
CREATE OR REPLACE FUNCTION sp_UploadTimestampedEvidence(
    p_AssignmentId BIGINT,
    p_Phase VARCHAR,
    p_EvidenceData JSONB
)
RETURNS TABLE (Success INT, Message TEXT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_ErrorMessage TEXT;
    v_PhotoCount INT;
    v_VideoCount INT;
    v_MinimumPhotos INT := 3;
    v_Now TIMESTAMP := NOW();
    v_MaxFutureMinutes INT := 5;
    v_MaxPastDays INT := 7;
BEGIN

    -- Parse JSON
    WITH evidence AS (
        SELECT
            value->>'Type' AS Type,
            value->>'Url' AS Url,
            (value->>'Timestamp')::TIMESTAMP AS Timestamp,
            value->>'GPSLocation' AS GPSLocation,
            value->>'Description' AS Description
        FROM jsonb_array_elements(p_EvidenceData)
    )
    SELECT
        COUNT(*) FILTER (WHERE Type = 'PHOTO'),
        COUNT(*) FILTER (WHERE Type = 'VIDEO')
    INTO v_PhotoCount, v_VideoCount
    FROM evidence;

    -- Validate photo count
    IF v_PhotoCount < v_MinimumPhotos THEN
        v_ErrorMessage := p_Phase || ' requires minimum ' || v_MinimumPhotos ||
                          ' photos. Provided: ' || v_PhotoCount;
        RETURN QUERY SELECT 0, v_ErrorMessage;
        RETURN;
    END IF;

    -- Validate GPS
    IF EXISTS (
        SELECT 1
        FROM jsonb_array_elements(p_EvidenceData) elem
        WHERE elem->>'GPSLocation' IS NULL OR elem->>'GPSLocation' = ''
    ) THEN
        v_ErrorMessage := 'Some evidence items missing GPS location';
        RETURN QUERY SELECT 0, v_ErrorMessage;
        RETURN;
    END IF;

    -- Validate timestamps
    IF EXISTS (
        SELECT 1
        FROM jsonb_array_elements(p_EvidenceData) elem
        WHERE (elem->>'Timestamp')::TIMESTAMP > (v_Now + INTERVAL '5 minutes')
           OR (elem->>'Timestamp')::TIMESTAMP < (v_Now - INTERVAL '7 days')
    ) THEN
        v_ErrorMessage := 'Invalid timestamps found in evidence';
        RETURN QUERY SELECT 0, v_ErrorMessage;
        RETURN;
    END IF;

    -- Phase Handling
    IF p_Phase = 'PRE_EVENT' THEN
        UPDATE t_sys_pre_event_verification
        SET c_checklist_photos = p_EvidenceData,
            c_modifieddate = NOW()
        WHERE c_assignment_id = p_AssignmentId;

        IF NOT FOUND THEN
            RETURN QUERY SELECT 0, 'Pre-event verification record not found';
            RETURN;
        END IF;

    ELSIF p_Phase = 'DURING_EVENT' THEN
        IF EXISTS (
            SELECT 1 FROM information_schema.tables
            WHERE table_name = 't_sys_during_event_evidence'
        ) THEN
            INSERT INTO t_sys_during_event_evidence (
                c_assignment_id,
                c_evidence_data,
                c_uploaded_at
            )
            VALUES (
                p_AssignmentId,
                p_EvidenceData,
                NOW()
            );
        ELSE
            UPDATE t_sys_supervisor_assignment
            SET c_during_event_evidence = p_EvidenceData,
                c_modifieddate = NOW()
            WHERE c_assignment_id = p_AssignmentId;
        END IF;

    ELSIF p_Phase = 'POST_EVENT' THEN
        UPDATE t_sys_post_event_report
        SET c_evidence_photos = p_EvidenceData,
            c_modifieddate = NOW()
        WHERE c_assignment_id = p_AssignmentId;

        IF NOT FOUND THEN
            RETURN QUERY SELECT 0, 'Post-event report not found';
            RETURN;
        END IF;

    ELSE
        RETURN QUERY SELECT 0, 'Invalid phase';
        RETURN;
    END IF;

    -- Log Success
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
        p_AssignmentId,
        'EVIDENCE_UPLOAD',
        p_Phase || ' evidence upload: ' || v_PhotoCount || ' photos, ' || v_VideoCount || ' videos',
        p_EvidenceData::TEXT,
        'SUCCESS',
        NOW()
    FROM t_sys_supervisor_assignment sa
    WHERE sa.c_assignment_id = p_AssignmentId;

    RETURN QUERY SELECT 1, 'Evidence uploaded successfully';

EXCEPTION WHEN OTHERS THEN

    v_ErrorMessage := SQLERRM;

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
        p_AssignmentId,
        'EVIDENCE_UPLOAD',
        'Failed to upload ' || p_Phase || ' evidence',
        'FAILED: ' || v_ErrorMessage,
        NOW()
    FROM t_sys_supervisor_assignment sa
    WHERE sa.c_assignment_id = p_AssignmentId;

    RETURN QUERY SELECT 0, v_ErrorMessage;

END;
$$;
