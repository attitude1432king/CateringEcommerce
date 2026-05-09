import { Star } from "lucide-react";

export default function RatingScore({ rating, reviews, size = "md", className = "" }) {
    const sizes = {
        sm: { pill: "px-2.5 py-1 text-xs gap-1",   star: 11, text: "text-xs" },
        md: { pill: "px-3 py-1.5 text-sm gap-1.5",  star: 13, text: "text-xs" },
        lg: { pill: "px-4 py-2 text-base gap-2",    star: 15, text: "text-sm" },
    };
    const s = sizes[size] ?? sizes.md;

    return (
        <div className={`flex items-center gap-2 ${className}`}>
            <span className={`inline-flex items-center bg-gradient-catering text-white font-bold rounded-full ${s.pill}`}>
                <Star size={s.star} fill="currentColor" strokeWidth={0} />
                {rating}
            </span>
            {reviews != null && (
                <span className={`text-neutral-500 ${s.text}`}>
                    {Number(reviews).toLocaleString()} reviews
                </span>
            )}
        </div>
    );
}
