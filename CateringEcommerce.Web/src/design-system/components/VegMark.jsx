/**
 * Standard Indian food veg / non-veg indicator (square border with filled circle).
 * Thin re-export alias kept at src/components/common/VegNonVegIcon.jsx so existing
 * imports continue to work without changes.
 */
export default function VegMark({ isVeg, size = 14, className = "" }) {
    const color = isVeg ? "#16A34A" : "#DC2626";
    return (
        <span
            className={`inline-flex shrink-0 ${className}`}
            style={{
                width: size,
                height: size,
                border: `2px solid ${color}`,
                padding: 2,
                borderRadius: 2,
            }}
            title={isVeg ? "Vegetarian" : "Non-Vegetarian"}
            aria-label={isVeg ? "Vegetarian" : "Non-Vegetarian"}
        >
            <span
                style={{
                    display: "block",
                    width: "100%",
                    height: "100%",
                    borderRadius: "50%",
                    background: color,
                }}
            />
        </span>
    );
}
