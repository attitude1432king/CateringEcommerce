const variantClasses = {
    default: "bg-white border border-neutral-100 rounded-2xl shadow-card",
    premium: "bg-white rounded-2xl shadow-card hover:shadow-card-hover hover:-translate-y-1 transition-all duration-300",
    luxury:  "bg-white rounded-3xl shadow-lg border border-gray-100 hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 relative overflow-hidden",
    flat:    "bg-white rounded-2xl border border-neutral-100",
};

export default function Card({
    variant = "default",
    padded = true,
    hoverLift = false,
    className = "",
    children,
    ...rest
}) {
    const lift = hoverLift ? "hover:-translate-y-1 transition-transform duration-300" : "";
    return (
        <div
            className={`${variantClasses[variant]} ${padded ? "p-5 md:p-6" : ""} ${lift} ${className}`}
            {...rest}
        >
            {children}
            {variant === "luxury" && (
                <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-gradient-to-r from-transparent via-accent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
            )}
        </div>
    );
}
