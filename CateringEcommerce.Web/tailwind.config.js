/** @type {import('tailwindcss').Config} */

/* Tailwind reads design tokens from src/design-system/tokens.css via CSS variables.
 * Single source of truth — change a value in tokens.css, every Tailwind utility
 * (bg-primary, text-neutral-700, shadow-card, etc.) updates with it.
 */

const cssVar = (name) => `var(${name})`;

/* For colors that need opacity modifier support (bg-primary/10, border-accent/30 …)
 * use the rgb-channel form so Tailwind can inject <alpha-value>. */
const cssVarRgb = (name) => `rgb(var(${name}) / <alpha-value>)`;

export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            fontFamily: {
                sans: ["Inter", "Poppins", "system-ui", "sans-serif"],
                display: ["Inter", "system-ui", "sans-serif"],
                mono: ["ui-monospace", "SFMono-Regular", "SF Mono", "Menlo", "monospace"],
            },
            fontSize: {
                xs:   ["var(--text-xs)",   { lineHeight: "var(--leading-normal)" }],
                sm:   ["var(--text-sm)",   { lineHeight: "var(--leading-normal)" }],
                base: ["var(--text-base)", { lineHeight: "var(--leading-relaxed)" }],
                lg:   ["var(--text-lg)",   { lineHeight: "var(--leading-relaxed)" }],
                xl:   ["var(--text-xl)",   { lineHeight: "var(--leading-snug)" }],
                "2xl": ["var(--text-2xl)", { lineHeight: "var(--leading-snug)" }],
                "3xl": ["var(--text-3xl)", { lineHeight: "var(--leading-snug)" }],
                "4xl": ["var(--text-4xl)", { lineHeight: "var(--leading-tight)" }],
                "5xl": ["var(--text-5xl)", { lineHeight: "var(--leading-tight)" }],
                "6xl": ["var(--text-6xl)", { lineHeight: "var(--leading-tight)" }],
                "7xl": ["var(--text-7xl)", { lineHeight: "var(--leading-tight)" }],
            },
            letterSpacing: {
                tightest: "var(--tracking-tightest)",
                tight:    "var(--tracking-tight)",
                normal:   "var(--tracking-normal)",
                wide:     "var(--tracking-wide)",
                wider:    "var(--tracking-wider)",
                widest:   "var(--tracking-widest)",
            },
            fontWeight: {
                light:     "300",
                normal:    "400",
                medium:    "500",
                semibold:  "600",
                bold:      "700",
                extrabold: "800",
            },
            colors: {
                // Brand — rgb channel form enables /opacity modifiers (bg-primary/10 etc.)
                primary:        cssVarRgb("--color-primary-rgb"),
                "primary-dark": cssVarRgb("--color-primary-dark-rgb"),
                secondary:      cssVarRgb("--color-secondary-rgb"),
                accent:         cssVarRgb("--color-accent-rgb"),
                "deep-red":     cssVar("--color-deep-red"),
                "deeper-red":   cssVar("--color-deeper-red"),

                // Legacy alias namespace — keeps existing class strings working
                catering: {
                    primary:        cssVarRgb("--color-primary-rgb"),
                    "primary-dark": cssVarRgb("--color-primary-dark-rgb"),
                    secondary:      cssVarRgb("--color-secondary-rgb"),
                    accent:         cssVarRgb("--color-accent-rgb"),
                    light:          cssVarRgb("--color-light-rgb"),
                },
                light: cssVarRgb("--color-light-rgb"),

                // Full neutral scale
                neutral: {
                    0:   cssVar("--neutral-0"),
                    50:  cssVar("--neutral-50"),
                    100: cssVar("--neutral-100"),
                    200: cssVar("--neutral-200"),
                    300: cssVar("--neutral-300"),
                    400: cssVar("--neutral-400"),
                    500: cssVar("--neutral-500"),
                    600: cssVar("--neutral-600"),
                    700: cssVar("--neutral-700"),
                    800: cssVar("--neutral-800"),
                    900: cssVar("--neutral-900"),
                    ink: cssVar("--neutral-ink"),
                },

                // Semantic (with /opacity support)
                success:      cssVarRgb("--success-rgb"),
                "success-bg": cssVar("--success-bg"),
                warning:      cssVarRgb("--warning-rgb"),
                "warning-bg": cssVar("--warning-bg"),
                danger:       cssVarRgb("--danger-rgb"),
                "danger-bg":  cssVar("--danger-bg"),
                info:         cssVarRgb("--info-rgb"),
                "info-bg":    cssVar("--info-bg"),

                // Portal accents (dormant in Wave 1; used by Wave 2/3)
                "admin-accent":      cssVarRgb("--admin-accent-rgb"),
                "partner-accent":    cssVarRgb("--partner-accent-rgb"),
                "supervisor-accent": cssVarRgb("--supervisor-accent-rgb"),
            },
            spacing: {
                section: "6rem",
                1:   cssVar("--space-1"),
                2:   cssVar("--space-2"),
                3:   cssVar("--space-3"),
                4:   cssVar("--space-4"),
                5:   cssVar("--space-5"),
                6:   cssVar("--space-6"),
                8:   cssVar("--space-8"),
                10:  cssVar("--space-10"),
                12:  cssVar("--space-12"),
                16:  cssVar("--space-16"),
                20:  cssVar("--space-20"),
                24:  cssVar("--space-24"),
                32:  cssVar("--space-32"),
            },
            borderRadius: {
                sm:   cssVar("--radius-sm"),
                md:   cssVar("--radius-md"),
                lg:   cssVar("--radius-lg"),
                xl:   cssVar("--radius-xl"),
                "2xl": cssVar("--radius-2xl"),
                "3xl": cssVar("--radius-3xl"),
                pill: cssVar("--radius-pill"),
                full: cssVar("--radius-pill"),
            },
            boxShadow: {
                input:        cssVar("--shadow-input"),
                card:         cssVar("--shadow-card"),
                "card-hover": cssVar("--shadow-card-hover"),
                luxury:       cssVar("--shadow-luxury"),
                gold:         cssVar("--shadow-gold"),
                cta:          cssVar("--shadow-cta"),
            },
            backgroundImage: {
                "gradient-catering":      cssVar("--gradient-catering"),
                "gradient-catering-soft": cssVar("--gradient-catering-soft"),
                "gradient-text":          cssVar("--gradient-text"),
                "gradient-gold-shimmer":  cssVar("--gradient-gold-shimmer"),
                "gradient-hero-overlay":  cssVar("--gradient-hero-overlay"),
                "gradient-surface":       cssVar("--gradient-surface"),
                "gradient-luxury-bg":     cssVar("--gradient-luxury-bg"),
            },
            transitionTimingFunction: {
                "out-expo":    cssVar("--ease-out"),
                "in-out-cubic": cssVar("--ease-in-out"),
            },
            transitionDuration: {
                fast:   "150ms",
                base:   "200ms",
                slow:   "300ms",
                slower: "500ms",
                image:  "700ms",
            },
            ringColor: {
                primary: cssVar("--color-primary"),
                accent:  cssVar("--color-accent"),
            },
            ringOffsetColor: {
                primary: cssVar("--color-primary"),
            },
            keyframes: {
                shimmer: {
                    "0%, 100%": { backgroundPosition: "0% 50%" },
                    "50%":      { backgroundPosition: "100% 50%" },
                },
                float: {
                    "0%, 100%": { transform: "translateY(0)" },
                    "50%":      { transform: "translateY(-10px)" },
                },
                fadeInUp: {
                    from: { opacity: "0", transform: "translateY(30px)" },
                    to:   { opacity: "1", transform: "translateY(0)" },
                },
                slideInRight: {
                    from: { transform: "translateX(100%)" },
                    to:   { transform: "translateX(0)" },
                },
            },
            animation: {
                shimmer:       "shimmer 3s ease-in-out infinite",
                float:         "float 3s ease-in-out infinite",
                fadeInUp:      "fadeInUp 0.6s ease-out",
                "slide-in-right": "slideInRight 0.3s ease-out",
            },
        },
    },
    plugins: [],
};
