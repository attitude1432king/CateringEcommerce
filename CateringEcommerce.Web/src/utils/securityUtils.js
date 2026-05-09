/**
 * Security utility functions for the frontend application
 * Includes redirect validation, input sanitization, and other security helpers
 */

/**
 * Validates if a redirect URL is safe (prevents open redirect attacks)
 * SECURITY: Only allows relative paths within the application
 *
 * @param {string} url - The redirect URL to validate
 * @returns {boolean} True if the URL is safe, false otherwise
 *
 * @example
 * isValidRedirectUrl('/checkout') // true
 * isValidRedirectUrl('http://evil.com') // false
 * isValidRedirectUrl('//evil.com') // false
 * isValidRedirectUrl('/profile?next=http://evil.com') // false
 */
export const isValidRedirectUrl = (url) => {
    if (!url || typeof url !== 'string') {
        return false;
    }

    // Remove leading/trailing whitespace
    const trimmedUrl = url.trim();

    // Empty string is not valid
    if (trimmedUrl.length === 0) {
        return false;
    }

    // SECURITY: Must start with / (relative path) and NOT start with //
    if (!trimmedUrl.startsWith('/') || trimmedUrl.startsWith('//')) {
        return false;
    }

    // SECURITY: Must not contain protocol (http://, https://, javascript:, data:, etc.)
    if (trimmedUrl.match(/^[a-z][a-z0-9+.-]*:/i)) {
        return false;
    }

    // SECURITY: Block common dangerous patterns
    const dangerousPatterns = [
        /javascript:/i,
        /data:/i,
        /vbscript:/i,
        /file:/i,
        /@/,     // Block email-like patterns that could be URLs
        /\\/,    // Block backslashes (path traversal attempts)
        /\.\./   // Block dot-dot path traversal: /profile/../../admin
    ];

    for (const pattern of dangerousPatterns) {
        if (pattern.test(trimmedUrl)) {
            return false;
        }
    }

    // SECURITY: Whitelist of allowed path prefixes
    const allowedPrefixes = [
        '/checkout',
        '/cart',
        '/profile',
        '/orders',
        '/favorites',
        '/wishlist',
        '/catering',
        '/search',
        '/browse',
        '/account',
        '/settings',
        '/notifications',
        '/dashboard',
        '/partner-login',
        '/'  // Root path
    ];

    // Check if URL starts with any allowed prefix
    const isAllowedPath = allowedPrefixes.some(prefix =>
        trimmedUrl === prefix || trimmedUrl.startsWith(prefix + '/')|| trimmedUrl.startsWith(prefix + '?')
    );

    if (!isAllowedPath) {
        console.warn(`[Security] Rejected redirect URL: ${trimmedUrl} (not in whitelist)`);
        return false;
    }

    return true;
};

/**
 * Sanitizes a redirect URL, returning a safe default if invalid
 *
 * @param {string} url - The redirect URL to sanitize
 * @param {string} defaultUrl - Default URL to use if input is invalid (default: '/')
 * @returns {string} A safe redirect URL
 *
 * @example
 * sanitizeRedirectUrl('/checkout') // '/checkout'
 * sanitizeRedirectUrl('http://evil.com') // '/'
 * sanitizeRedirectUrl('//evil.com', '/profile') // '/profile'
 */
export const sanitizeRedirectUrl = (url, defaultUrl = '/') => {
    if (isValidRedirectUrl(url)) {
        return url;
    }

    console.warn(`[Security] Invalid redirect URL sanitized: ${url} → ${defaultUrl}`);
    return defaultUrl;
};

/**
 * Validates if a URL is safe for external links (allows only HTTPS)
 *
 * @param {string} url - The external URL to validate
 * @returns {boolean} True if the URL is safe, false otherwise
 */
export const isValidExternalUrl = (url) => {
    if (!url || typeof url !== 'string') {
        return false;
    }

    try {
        const parsedUrl = new URL(url);

        // Only allow HTTPS for external links (security best practice)
        if (parsedUrl.protocol !== 'https:') {
            return false;
        }

        // Optional: Whitelist allowed external domains
        const allowedDomains = [
            'maps.google.com',
            'www.google.com',
            'accounts.google.com',
            'facebook.com',
            'www.facebook.com'
        ];

        return allowedDomains.some(domain =>
            parsedUrl.hostname === domain || parsedUrl.hostname.endsWith('.' + domain)
        );
    } catch (error) {
        // Invalid URL format
        return false;
    }
};

/**
 * Removes sensitive data from objects before logging (prevents data leakage)
 *
 * @param {object} obj - The object to sanitize
 * @returns {object} Sanitized object with sensitive fields masked
 */
export const sanitizeForLogging = (obj) => {
    if (!obj || typeof obj !== 'object') {
        return obj;
    }

    const sensitiveKeys = [
        'password',
        'token',
        'accessToken',
        'refreshToken',
        'authToken',
        'apiKey',
        'secret',
        'otp',
        'pin',
        'ssn',
        'cardNumber',
        'cvv',
        'accountNumber'
    ];

    const sanitized = { ...obj };

    for (const key in sanitized) {
        const lowerKey = key.toLowerCase();

        // Mask sensitive fields
        if (sensitiveKeys.some(sensitive => lowerKey.includes(sensitive.toLowerCase()))) {
            sanitized[key] = '***REDACTED***';
        }

        // Recursively sanitize nested objects
        if (typeof sanitized[key] === 'object' && sanitized[key] !== null) {
            sanitized[key] = sanitizeForLogging(sanitized[key]);
        }
    }

    return sanitized;
};

/**
 * Validates phone number format (Indian numbers)
 *
 * @param {string} phone - Phone number to validate
 * @returns {boolean} True if valid, false otherwise
 */
export const isValidPhoneNumber = (phone) => {
    if (!phone || typeof phone !== 'string') {
        return false;
    }

    // Indian phone number: +91XXXXXXXXXX or 10 digits
    const phoneRegex = /^(\+91)?[6-9]\d{9}$/;
    return phoneRegex.test(phone.replace(/[\s-]/g, ''));
};

/**
 * Validates email format
 *
 * @param {string} email - Email to validate
 * @returns {boolean} True if valid, false otherwise
 */
export const isValidEmail = (email) => {
    if (!email || typeof email !== 'string') {
        return false;
    }

    // Basic email validation (RFC 5322 simplified)
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    return emailRegex.test(email) && email.length <= 320; // Max length per RFC 5321
};

/**
 * Rate limiting helper (client-side)
 * Tracks function calls and enforces limits
 */
export class RateLimiter {
    constructor() {
        this.calls = new Map();
    }

    /**
     * Checks if an action is rate-limited
     *
     * @param {string} key - Unique identifier for the action
     * @param {number} maxCalls - Maximum allowed calls
     * @param {number} windowMs - Time window in milliseconds
     * @returns {boolean} True if action is allowed, false if rate-limited
     */
    isAllowed(key, maxCalls, windowMs) {
        const now = Date.now();

        if (!this.calls.has(key)) {
            this.calls.set(key, []);
        }

        const timestamps = this.calls.get(key);

        // Remove expired timestamps
        const validTimestamps = timestamps.filter(ts => now - ts < windowMs);

        if (validTimestamps.length >= maxCalls) {
            return false; // Rate limited
        }

        // Add current timestamp
        validTimestamps.push(now);
        this.calls.set(key, validTimestamps);

        return true;
    }

    /**
     * Resets rate limit for a specific key
     *
     * @param {string} key - Unique identifier for the action
     */
    reset(key) {
        this.calls.delete(key);
    }
}

// Export a singleton instance
export const rateLimiter = new RateLimiter();
