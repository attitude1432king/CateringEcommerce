/**
 * Reusable section header: eyebrow chip + gradient heading + optional sub-copy.
 * Used by every home section, listing header, and detail subsection.
 */
export default function SectionHeader({
    eyebrow,
    title,
    titleGradient,
    subtitle,
    centered = true,
    className = "",
}) {
    return (
        <div className={`${centered ? "text-center" : ""} ${className}`}>
            {eyebrow && (
                <div className="inline-flex items-center gap-2 px-4 py-1.5 bg-gradient-to-r from-accent/10 to-primary/10 rounded-full mb-4">
                    <span className="t-eyebrow">{eyebrow}</span>
                </div>
            )}
            {(title || titleGradient) && (
                <h2 className="t-h2 mb-3">
                    {title && <span>{title} </span>}
                    {titleGradient && (
                        <span className="t-gradient">{titleGradient}</span>
                    )}
                </h2>
            )}
            {subtitle && (
                <p className="t-body-lg max-w-2xl mx-auto">{subtitle}</p>
            )}
        </div>
    );
}
