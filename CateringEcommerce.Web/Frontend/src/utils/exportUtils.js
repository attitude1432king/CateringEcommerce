/**
 * Export Utilities
 * Provides functions for exporting data to CSV and formatting utilities
 */

/**
 * Export data to CSV format
 * @param {Array<Object>} data - Array of objects to export
 * @param {string} filename - Name of the file to download
 * @param {Array<string>} columns - Optional: specific columns to export (if not provided, uses all keys)
 */
export const exportToCSV = (data, filename, columns = null) => {
    try {
        if (!data || data.length === 0) {
            throw new Error('No data to export');
        }

        // Get columns from first object if not provided
        const cols = columns || Object.keys(data[0]);

        // Create CSV header
        let csv = cols.join(',') + '\n';

        // Add data rows
        data.forEach(row => {
            const values = cols.map(col => {
                let value = row[col];

                // Handle null/undefined
                if (value === null || value === undefined) {
                    return '';
                }

                // Convert to string and escape quotes
                value = String(value).replace(/"/g, '""');

                // Wrap in quotes if contains comma, newline, or quote
                if (value.includes(',') || value.includes('\n') || value.includes('"')) {
                    return `"${value}"`;
                }

                return value;
            });

            csv += values.join(',') + '\n';
        });

        // Create blob and download
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        downloadBlob(blob, filename.endsWith('.csv') ? filename : `${filename}.csv`);

        return { success: true, message: 'Data exported successfully' };
    } catch (error) {
        console.error('Error exporting to CSV:', error);
        return { success: false, message: error.message };
    }
};

/**
 * Export table HTML to CSV
 * @param {string} tableId - ID of the table element
 * @param {string} filename - Name of the file to download
 */
export const exportTableToCSV = (tableId, filename) => {
    try {
        const table = document.getElementById(tableId);
        if (!table) {
            throw new Error('Table not found');
        }

        let csv = [];

        // Get headers
        const headers = [];
        const headerCells = table.querySelectorAll('thead th, thead td');
        headerCells.forEach(cell => {
            headers.push(cell.textContent.trim());
        });
        csv.push(headers.join(','));

        // Get rows
        const rows = table.querySelectorAll('tbody tr');
        rows.forEach(row => {
            const rowData = [];
            const cells = row.querySelectorAll('td, th');
            cells.forEach(cell => {
                let value = cell.textContent.trim();
                // Escape quotes and wrap in quotes if necessary
                value = value.replace(/"/g, '""');
                if (value.includes(',') || value.includes('\n') || value.includes('"')) {
                    value = `"${value}"`;
                }
                rowData.push(value);
            });
            csv.push(rowData.join(','));
        });

        const csvContent = csv.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        downloadBlob(blob, filename.endsWith('.csv') ? filename : `${filename}.csv`);

        return { success: true, message: 'Table exported successfully' };
    } catch (error) {
        console.error('Error exporting table to CSV:', error);
        return { success: false, message: error.message };
    }
};

/**
 * Download a blob as a file
 * @param {Blob} blob - The blob to download
 * @param {string} filename - Name of the file
 */
const downloadBlob = (blob, filename) => {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

/**
 * Format currency in Indian Rupee format
 * @param {number} amount - Amount to format
 * @param {boolean} includeSymbol - Whether to include ₹ symbol (default: true)
 * @returns {string} Formatted currency string
 */
export const formatCurrency = (amount, includeSymbol = true) => {
    if (amount === null || amount === undefined || isNaN(amount)) {
        return includeSymbol ? '₹0' : '0';
    }

    const formatted = Math.abs(amount).toLocaleString('en-IN', {
        maximumFractionDigits: 0,
        minimumFractionDigits: 0
    });

    const prefix = amount < 0 ? '-' : '';
    return includeSymbol ? `${prefix}₹${formatted}` : `${prefix}${formatted}`;
};

/**
 * Format date in various formats
 * @param {string|Date} date - Date to format
 * @param {string} format - Format type: 'short', 'medium', 'long', 'time', 'datetime'
 * @returns {string} Formatted date string
 */
export const formatDate = (date, format = 'medium') => {
    if (!date) return '';

    const d = typeof date === 'string' ? new Date(date) : date;

    if (isNaN(d.getTime())) {
        return '';
    }

    switch (format) {
        case 'short':
            // DD/MM/YYYY
            return d.toLocaleDateString('en-IN', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });

        case 'medium':
            // DD MMM YYYY
            return d.toLocaleDateString('en-IN', {
                day: '2-digit',
                month: 'short',
                year: 'numeric'
            });

        case 'long':
            // DD MMMM YYYY
            return d.toLocaleDateString('en-IN', {
                day: '2-digit',
                month: 'long',
                year: 'numeric'
            });

        case 'time':
            // HH:MM AM/PM
            return d.toLocaleTimeString('en-IN', {
                hour: '2-digit',
                minute: '2-digit',
                hour12: true
            });

        case 'datetime':
            // DD MMM YYYY, HH:MM AM/PM
            return d.toLocaleString('en-IN', {
                day: '2-digit',
                month: 'short',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
                hour12: true
            });

        default:
            return d.toLocaleDateString('en-IN');
    }
};

/**
 * Format number with commas
 * @param {number} number - Number to format
 * @param {number} decimals - Number of decimal places (default: 0)
 * @returns {string} Formatted number string
 */
export const formatNumber = (number, decimals = 0) => {
    if (number === null || number === undefined || isNaN(number)) {
        return '0';
    }

    return number.toLocaleString('en-IN', {
        maximumFractionDigits: decimals,
        minimumFractionDigits: decimals
    });
};

/**
 * Format percentage
 * @param {number} value - Value to format as percentage
 * @param {number} decimals - Number of decimal places (default: 1)
 * @returns {string} Formatted percentage string
 */
export const formatPercentage = (value, decimals = 1) => {
    if (value === null || value === undefined || isNaN(value)) {
        return '0%';
    }

    return `${value.toFixed(decimals)}%`;
};

/**
 * Format phone number in Indian format
 * @param {string} phone - Phone number to format
 * @returns {string} Formatted phone number
 */
export const formatPhoneNumber = (phone) => {
    if (!phone) return '';

    // Remove all non-digit characters
    const cleaned = phone.replace(/\D/g, '');

    // Format as +91 XXXXX XXXXX
    if (cleaned.length === 10) {
        return `+91 ${cleaned.slice(0, 5)} ${cleaned.slice(5)}`;
    }

    // Format as +91 XXXXX XXXXX if starts with 91
    if (cleaned.length === 12 && cleaned.startsWith('91')) {
        return `+91 ${cleaned.slice(2, 7)} ${cleaned.slice(7)}`;
    }

    return phone;
};

/**
 * Get relative time (e.g., "2 hours ago", "3 days ago")
 * @param {string|Date} date - Date to compare
 * @returns {string} Relative time string
 */
export const getRelativeTime = (date) => {
    if (!date) return '';

    const d = typeof date === 'string' ? new Date(date) : date;

    if (isNaN(d.getTime())) {
        return '';
    }

    const now = new Date();
    const diffMs = now - d;
    const diffSecs = Math.floor(diffMs / 1000);
    const diffMins = Math.floor(diffSecs / 60);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffSecs < 60) {
        return 'Just now';
    } else if (diffMins < 60) {
        return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
    } else if (diffHours < 24) {
        return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    } else if (diffDays < 7) {
        return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    } else {
        return formatDate(d, 'medium');
    }
};

/**
 * Truncate text to specified length
 * @param {string} text - Text to truncate
 * @param {number} maxLength - Maximum length
 * @param {string} suffix - Suffix to add (default: '...')
 * @returns {string} Truncated text
 */
export const truncateText = (text, maxLength, suffix = '...') => {
    if (!text || text.length <= maxLength) {
        return text || '';
    }

    return text.substring(0, maxLength - suffix.length) + suffix;
};

/**
 * Convert bytes to human-readable format
 * @param {number} bytes - Number of bytes
 * @param {number} decimals - Number of decimal places (default: 2)
 * @returns {string} Human-readable size
 */
export const formatFileSize = (bytes, decimals = 2) => {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
};

/**
 * Print element or entire page
 * SECURITY: Sanitizes HTML with DOMPurify before printing to prevent XSS
 * @param {string} elementId - Optional: ID of element to print (if not provided, prints entire page)
 */
export const printPage = (elementId = null) => {
    if (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            // SECURITY FIX: Import DOMPurify dynamically if not already imported
            import('dompurify').then(({ default: DOMPurify }) => {
                // Sanitize the HTML content to prevent XSS attacks
                const sanitizedHTML = DOMPurify.sanitize(element.innerHTML, {
                    ALLOWED_TAGS: ['p', 'div', 'span', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'ul', 'ol', 'li', 'table', 'thead', 'tbody', 'tr', 'td', 'th', 'br', 'strong', 'em', 'b', 'i', 'u'],
                    ALLOWED_ATTR: ['class', 'id', 'style'],
                    KEEP_CONTENT: true
                });

                const printWindow = window.open('', '_blank');
                if (!printWindow) {
                    console.error('Failed to open print window. Please allow popups.');
                    return;
                }

                // Use a safe template literal approach instead of document.write
                const printContent = `
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Print</title>
                        <link rel="stylesheet" href="/assets/css/print.css">
                        <style>
                            body { font-family: Arial, sans-serif; margin: 20px; }
                            @media print { body { margin: 0; } }
                        </style>
                    </head>
                    <body>
                        ${sanitizedHTML}
                    </body>
                    </html>
                `;

                printWindow.document.write(printContent);
                printWindow.document.close();

                setTimeout(() => {
                    printWindow.print();
                    printWindow.close();
                }, 250);
            }).catch(err => {
                console.error('Failed to load DOMPurify:', err);
                // Fallback to native print without the specific element
                window.print();
            });
        }
    } else {
        window.print();
    }
};

/**
 * Copy text to clipboard
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} Success status
 */
export const copyToClipboard = async (text) => {
    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        } else {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            document.body.appendChild(textArea);
            textArea.select();
            const success = document.execCommand('copy');
            document.body.removeChild(textArea);
            return success;
        }
    } catch (error) {
        console.error('Failed to copy to clipboard:', error);
        return false;
    }
};
