/**
 * Admin Dashboard Design System
 * Color palette, typography, spacing, and theming configuration
 */

export const adminTheme = {
  // Color Palette
  colors: {
    // Primary Colors
    primary: {
      50: '#eef2ff',
      100: '#e0e7ff',
      200: '#c7d2fe',
      300: '#a5b4fc',
      400: '#818cf8',
      500: '#6366f1', // Main primary
      600: '#4f46e5',
      700: '#4338ca',
      800: '#3730a3',
      900: '#312e81',
    },

    // Accent Colors
    accent: {
      orange: {
        50: '#fff7ed',
        100: '#ffedd5',
        500: '#f97316',
        600: '#ea580c',
      },
      green: {
        50: '#f0fdf4',
        100: '#dcfce7',
        500: '#22c55e',
        600: '#16a34a',
      },
    },

    // Status Colors
    status: {
      active: '#22c55e',    // Green
      pending: '#eab308',   // Yellow
      approved: '#3b82f6',  // Blue
      rejected: '#ef4444',  // Red
      blocked: '#dc2626',   // Dark Red
      inactive: '#9ca3af',  // Gray
    },

    // Semantic Colors
    success: '#22c55e',
    warning: '#eab308',
    error: '#ef4444',
    info: '#3b82f6',

    // Neutral Colors
    gray: {
      50: '#f9fafb',
      100: '#f3f4f6',
      200: '#e5e7eb',
      300: '#d1d5db',
      400: '#9ca3af',
      500: '#6b7280',
      600: '#4b5563',
      700: '#374151',
      800: '#1f2937',
      900: '#111827',
    },

    // Background
    background: {
      main: '#f9fafb',
      paper: '#ffffff',
      dark: '#111827',
    },

    // Text
    text: {
      primary: '#111827',
      secondary: '#6b7280',
      disabled: '#9ca3af',
      white: '#ffffff',
    },

    // Border
    border: {
      light: '#e5e7eb',
      main: '#d1d5db',
      dark: '#9ca3af',
    },
  },

  // Typography
  typography: {
    fontFamily: {
      sans: ['Inter', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
      mono: ['Fira Code', 'Consolas', 'Monaco', 'monospace'],
    },
    fontSize: {
      xs: '0.75rem',      // 12px
      sm: '0.875rem',     // 14px
      base: '1rem',       // 16px
      lg: '1.125rem',     // 18px
      xl: '1.25rem',      // 20px
      '2xl': '1.5rem',    // 24px
      '3xl': '1.875rem',  // 30px
      '4xl': '2.25rem',   // 36px
    },
    fontWeight: {
      normal: 400,
      medium: 500,
      semibold: 600,
      bold: 700,
    },
  },

  // Spacing
  spacing: {
    xs: '0.25rem',   // 4px
    sm: '0.5rem',    // 8px
    md: '1rem',      // 16px
    lg: '1.5rem',    // 24px
    xl: '2rem',      // 32px
    '2xl': '3rem',   // 48px
    '3xl': '4rem',   // 64px
  },

  // Border Radius
  borderRadius: {
    none: '0',
    sm: '0.25rem',   // 4px
    md: '0.375rem',  // 6px
    lg: '0.5rem',    // 8px
    xl: '0.75rem',   // 12px
    '2xl': '1rem',   // 16px
    full: '9999px',
  },

  // Shadows
  shadows: {
    sm: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
    md: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
    lg: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
    xl: '0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)',
  },

  // Transitions
  transitions: {
    fast: '150ms cubic-bezier(0.4, 0, 0.2, 1)',
    base: '200ms cubic-bezier(0.4, 0, 0.2, 1)',
    slow: '300ms cubic-bezier(0.4, 0, 0.2, 1)',
  },

  // Breakpoints
  breakpoints: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px',
  },

  // Z-Index
  zIndex: {
    modal: 1000,
    dropdown: 900,
    overlay: 800,
    header: 700,
    sidebar: 600,
  },
};

// Status badge configurations
export const statusConfig = {
  active: {
    color: 'text-green-700',
    bg: 'bg-green-100',
    border: 'border-green-200',
    dot: 'bg-green-500',
  },
  pending: {
    color: 'text-yellow-700',
    bg: 'bg-yellow-100',
    border: 'border-yellow-200',
    dot: 'bg-yellow-500',
  },
  approved: {
    color: 'text-blue-700',
    bg: 'bg-blue-100',
    border: 'border-blue-200',
    dot: 'bg-blue-500',
  },
  rejected: {
    color: 'text-red-700',
    bg: 'bg-red-100',
    border: 'border-red-200',
    dot: 'bg-red-500',
  },
  blocked: {
    color: 'text-red-800',
    bg: 'bg-red-100',
    border: 'border-red-300',
    dot: 'bg-red-600',
  },
  inactive: {
    color: 'text-gray-700',
    bg: 'bg-gray-100',
    border: 'border-gray-200',
    dot: 'bg-gray-500',
  },
  completed: {
    color: 'text-green-700',
    bg: 'bg-green-100',
    border: 'border-green-200',
    dot: 'bg-green-500',
  },
  cancelled: {
    color: 'text-red-700',
    bg: 'bg-red-100',
    border: 'border-red-200',
    dot: 'bg-red-500',
  },
};

export default adminTheme;
