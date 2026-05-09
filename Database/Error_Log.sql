CREATE TABLE IF NOT EXISTS t_sys_errorlogs (
    c_id bigserial PRIMARY KEY,

    -- Traceability
    c_error_id uuid NOT NULL UNIQUE,
    c_traceid varchar(100),
    c_correlationid varchar(100),

    -- Core exception info
    c_message text,
    c_exceptiontype varchar(255),
    c_stacktrace text,
    c_innerexception text,
    c_source varchar(255),

    -- Request info
    c_requestpath varchar(500),
    c_requestmethod varchar(10),
    c_queryparams jsonb,
    c_requestbody jsonb,
    c_response_status_code int,

    -- User / context info
    c_userid bigint NULL,
    c_user_role varchar(50) NOT NULL DEFAULT 'Anonymous',
    c_username varchar(255),
    c_ipaddress varchar(50),
    c_useragent text,

    -- Environment info
    c_environment varchar(50),
    c_machinename varchar(100),
    c_applicationname varchar(100),

    -- Severity and performance
    c_loglevel varchar(20) NOT NULL DEFAULT 'Error',
    c_executiontimems int,

    -- Audit
    c_createdate timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_errorlogs_createdate
ON t_sys_errorlogs (c_createdate DESC);

CREATE INDEX IF NOT EXISTS idx_errorlogs_traceid
ON t_sys_errorlogs (c_traceid);

CREATE INDEX IF NOT EXISTS idx_errorlogs_userid
ON t_sys_errorlogs (c_userid);

CREATE INDEX IF NOT EXISTS idx_errorlogs_user_role_createdate
ON t_sys_errorlogs (c_user_role, c_createdate DESC);

CREATE INDEX IF NOT EXISTS idx_errorlogs_requestpath_createdate
ON t_sys_errorlogs (c_requestpath, c_createdate DESC);

CREATE INDEX IF NOT EXISTS idx_errorlogs_status_code
ON t_sys_errorlogs (c_response_status_code);
