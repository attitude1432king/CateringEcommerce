import { forwardRef } from "react";

const IconButton = forwardRef(function IconButton(
    {
        "aria-label": ariaLabel,
        dot = false,
        badge,
        size = 38,
        className = "",
        children,
        ...rest
    },
    ref
) {
    return (
        <button
            ref={ref}
            aria-label={ariaLabel}
            className={`icon-btn relative shrink-0 ${className}`}
            style={{ width: size, height: size }}
            {...rest}
        >
            {children}
            {dot && (
                <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-primary border-2 border-white" />
            )}
            {badge != null && badge > 0 && (
                <span className="cart-badge">{badge > 99 ? "99+" : badge}</span>
            )}
        </button>
    );
});

export default IconButton;
