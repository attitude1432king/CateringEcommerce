-- Run this query to check if the required tables exist in your database
-- This will help diagnose the 500 errors

SELECT
    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_homepage_stats')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_homepage_stats_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_catering_review')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_catering_review_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_order')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_order_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_catering_owner')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_catering_owner_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_catering_owner_operations')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_catering_owner_operations_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_catering_type_master')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_catering_type_master_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_user')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_user_status,

    CASE
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 't_sys_city')
        THEN 'EXISTS' ELSE 'MISSING'
    END AS t_sys_city_status;

-- If any table shows 'MISSING', you need to run the mastersql.sql script to create them

-- Additional check: See which tables actually exist in your database
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
