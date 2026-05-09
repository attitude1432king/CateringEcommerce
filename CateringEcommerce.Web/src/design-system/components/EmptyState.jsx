import Button from "./Button";

export default function EmptyState({
    illustration,
    title,
    description,
    ctaLabel,
    ctaTo,
    ctaOnClick,
    className = "",
}) {
    return (
        <div className={`flex flex-col items-center justify-center text-center py-16 px-6 ${className}`}>
            {illustration && (
                <div className="mb-6 text-6xl">{illustration}</div>
            )}
            {title && (
                <h3 className="text-xl font-bold text-neutral-800 mb-2">{title}</h3>
            )}
            {description && (
                <p className="text-neutral-500 text-sm max-w-sm leading-relaxed mb-6">{description}</p>
            )}
            {ctaLabel && (ctaTo || ctaOnClick) && (
                <Button variant="primary" size="md" to={ctaTo} onClick={ctaOnClick}>
                    {ctaLabel}
                </Button>
            )}
        </div>
    );
}
