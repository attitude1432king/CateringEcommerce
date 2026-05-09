const variantClasses = {
    gold:    "bg-gradient-to-r from-accent to-secondary text-white",
    green:   "bg-success text-white",
    red:     "bg-danger text-white",
    white:   "bg-white/95 text-neutral-900",
    neutral: "bg-neutral-100 text-neutral-700",
    primary: "bg-gradient-catering text-white",
};

export default function Pill({ variant = "neutral", className = "", children, ...rest }) {
    return (
        <span
            className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-bold tracking-wide ${variantClasses[variant] ?? variantClasses.neutral} ${className}`}
            {...rest}
        >
            {children}
        </span>
    );
}
