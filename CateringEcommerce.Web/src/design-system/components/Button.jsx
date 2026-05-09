import { forwardRef } from "react";
import { Link } from "react-router-dom";

const variantClasses = {
    primary:   "bg-gradient-catering text-white shadow-card hover:shadow-card-hover hover:scale-[1.02] focus:ring-2 focus:ring-primary/40",
    secondary: "bg-white border-2 border-primary text-primary shadow-card hover:shadow-card-hover hover:bg-light focus:ring-2 focus:ring-primary/30",
    tertiary:  "bg-transparent text-neutral-700 hover:bg-light focus:ring-2 focus:ring-primary/20",
    luxury:    "bg-gradient-catering text-white shadow-cta hover:shadow-gold hover:scale-[1.03] focus:ring-2 focus:ring-accent/40 btn-luxury-shine",
    ghost:     "bg-transparent text-neutral-600 hover:bg-neutral-100 focus:ring-2 focus:ring-neutral-300",
    rose:      "bg-gradient-to-r from-[#E11D48] to-[#F43F5E] text-white shadow-[0_4px_12px_rgba(225,29,72,0.3)] hover:shadow-[0_6px_20px_rgba(225,29,72,0.4)] focus:ring-2 focus:ring-[#E11D48]/40",
    indigo:    "bg-gradient-to-r from-[#4F46E5] to-[#9333EA] text-white shadow-[0_4px_12px_rgba(79,70,229,0.3)] hover:shadow-[0_6px_20px_rgba(79,70,229,0.4)] focus:ring-2 focus:ring-[#4F46E5]/40",
};

const sizeClasses = {
    sm: "px-4 py-2 text-sm rounded-lg gap-1.5",
    md: "px-6 py-2.5 text-sm rounded-xl gap-2",
    lg: "px-8 py-3.5 text-base rounded-xl gap-2",
};

const Button = forwardRef(function Button(
    {
        variant = "primary",
        size = "md",
        loading = false,
        disabled = false,
        iconLeft,
        iconRight,
        as: Tag,
        to,
        href,
        className = "",
        children,
        ...rest
    },
    ref
) {
    const base =
        "inline-flex items-center justify-center font-semibold transition-all duration-200 focus:outline-none active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none overflow-hidden relative";
    const classes = `${base} ${variantClasses[variant] ?? variantClasses.primary} ${sizeClasses[size]} ${className}`;
    const isDisabled = disabled || loading;

    const content = (
        <>
            {/* Shine overlay on luxury variant */}
            {variant === "luxury" && (
                <span className="absolute inset-0 -translate-x-full hover:translate-x-full transition-transform duration-700 bg-gradient-to-r from-transparent via-white/25 to-transparent pointer-events-none" />
            )}
            {loading ? (
                <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
                </svg>
            ) : (
                iconLeft && <span className="shrink-0">{iconLeft}</span>
            )}
            {children}
            {!loading && iconRight && <span className="shrink-0">{iconRight}</span>}
        </>
    );

    if (to) {
        return <Link ref={ref} to={to} className={classes} {...rest}>{content}</Link>;
    }
    if (href) {
        return <a ref={ref} href={href} className={classes} {...rest}>{content}</a>;
    }
    if (Tag) {
        return <Tag ref={ref} className={classes} disabled={isDisabled} {...rest}>{content}</Tag>;
    }
    return (
        <button ref={ref} className={classes} disabled={isDisabled} {...rest}>
            {content}
        </button>
    );
});

export default Button;
