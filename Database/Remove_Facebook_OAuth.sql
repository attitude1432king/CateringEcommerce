-- Remove Facebook OAuth provider
-- Facebook login has been removed from the platform.
-- This script removes the Facebook provider record so it no longer appears
-- in GetActiveProviders responses.

DELETE FROM t_sys_oauth_provider WHERE c_provider_name = 'FACEBOOK';

PRINT 'Facebook OAuth provider removed.';
