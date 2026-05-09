const variantClasses = {
    // Status
    pending:   "bg-warning-bg text-yellow-800",
    confirmed: "bg-info-bg text-blue-800",
    completed: "bg-success-bg text-green-800",
    cancelled: "bg-danger-bg text-red-800",
    active:    "bg-success-bg text-green-800",
    // Brand
    premium:   "bg-gradient-catering text-white shadow-[0_4px_12px_rgba(255,107,53,0.3)]",
    luxury:    "bg-gradient-to-r from-accent/10 to-primary/10 border border-accent/30 text-primary",
    gold:      "bg-gradient-to-r from-accent to-yellow-300 text-white",
    // Neutral
    green:     "bg-green-100 text-green-800",
    red:       "bg-red-100 text-red-700",
    white:     "bg-white/95 text-neutral-900 shadow-sm",
    neutral:   "bg-neutral-100 text-neutral-700",
};

const dotColors = {
    pending:   "bg-yellow-500",
    confirmed: "bg-blue-500",
    completed: "bg-green-500",
    cancelled: "bg-red-500",
    active:    "bg-green-500",
};

export default function Badge({
    variant = "neutral",
    dot = false,
    icon,
    className = "",
    children,
    ...rest
}) {
    return (
        <span
            className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold tracking-wide ${variantClasses[variant] ?? variantClasses.neutral} ${className}`}
            {...rest}
        >
            {dot && (
                <span className={`w-1.5 h-1.5 rounded-full ${dotColors[variant] ?? "bg-current"}`} />
            )}
            {icon && <span className="shrink-0">{icon}</span>}
            {children}
        </span>
    );
}
