/** Base shimmer skeleton. Compose or extend for specific shapes. */
export function Skeleton({ className = "", ...rest }) {
    return (
        <div
            className={`animate-pulse bg-neutral-200 rounded-lg ${className}`}
            aria-hidden="true"
            {...rest}
        />
    );
}

/** Card-shaped skeleton that matches CateringCard proportions. */
export function SkeletonCard() {
    return (
        <div className="bg-white rounded-2xl overflow-hidden border border-neutral-100 shadow-card">
            <Skeleton className="h-48 w-full rounded-none" />
            <div className="p-5 space-y-3">
                <Skeleton className="h-5 w-3/4" />
                <Skeleton className="h-4 w-1/2" />
                <Skeleton className="h-4 w-1/3" />
                <div className="flex gap-2 pt-1">
                    <Skeleton className="h-6 w-16 rounded-full" />
                    <Skeleton className="h-6 w-16 rounded-full" />
                </div>
                <Skeleton className="h-10 w-full rounded-xl mt-2" />
            </div>
        </div>
    );
}

/** Row-list skeleton for orders, complaints, etc. */
export function SkeletonList({ rows = 5 }) {
    return (
        <div className="space-y-3">
            {Array.from({ length: rows }).map((_, i) => (
                <div key={i} className="bg-white rounded-xl border border-neutral-100 p-4 flex items-center gap-4">
                    <Skeleton className="h-10 w-10 rounded-full shrink-0" />
                    <div className="flex-1 space-y-2">
                        <Skeleton className="h-4 w-2/5" />
                        <Skeleton className="h-3 w-3/5" />
                    </div>
                    <Skeleton className="h-6 w-20 rounded-full" />
                </div>
            ))}
        </div>
    );
}

/** Hero-section skeleton while video/image loads. */
export function SkeletonHero() {
    return (
        <div className="relative min-h-[560px] bg-neutral-200 animate-pulse flex items-center justify-center">
            <div className="text-center space-y-4 px-4 w-full max-w-2xl">
                <Skeleton className="h-4 w-32 rounded-full mx-auto" />
                <Skeleton className="h-12 w-3/4 mx-auto" />
                <Skeleton className="h-12 w-1/2 mx-auto" />
                <Skeleton className="h-6 w-2/3 mx-auto mt-2" />
                <Skeleton className="h-14 w-full max-w-lg mx-auto rounded-2xl mt-6" />
            </div>
        </div>
    );
}

export default Skeleton;
