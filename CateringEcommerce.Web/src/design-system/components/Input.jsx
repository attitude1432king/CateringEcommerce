import { forwardRef } from "react";

const Input = forwardRef(function Input(
    {
        label,
        error,
        helperText,
        iconLeft,
        iconRight,
        className = "",
        id,
        ...rest
    },
    ref
) {
    const inputId = id ?? (label ? label.toLowerCase().replace(/\s+/g, "-") : undefined);

    return (
        <div className="flex flex-col gap-1 w-full">
            {label && (
                <label htmlFor={inputId} className="text-xs font-semibold text-neutral-700">
                    {label}
                </label>
            )}
            <div className="relative flex items-center">
                {iconLeft && (
                    <span className="absolute left-3.5 text-neutral-400 pointer-events-none">
                        {iconLeft}
                    </span>
                )}
                <input
                    ref={ref}
                    id={inputId}
                    className={`
                        w-full py-3 text-sm bg-white rounded-xl border-2 outline-none
                        transition-all duration-200 font-sans
                        placeholder:text-neutral-400
                        border-neutral-200
                        focus:border-primary focus:ring-4 focus:ring-primary/10
                        disabled:bg-neutral-100 disabled:text-neutral-500 disabled:cursor-not-allowed
                        ${error ? "border-danger focus:border-danger focus:ring-danger/10" : ""}
                        ${iconLeft  ? "pl-10"  : "pl-4"}
                        ${iconRight ? "pr-10"  : "pr-4"}
                        ${className}
                    `}
                    style={{ boxShadow: "var(--shadow-input)" }}
                    {...rest}
                />
                {iconRight && (
                    <span className="absolute right-3.5 text-neutral-400 pointer-events-none">
                        {iconRight}
                    </span>
                )}
            </div>
            {(error || helperText) && (
                <p className={`text-xs mt-0.5 ${error ? "text-danger" : "text-neutral-500"}`}>
                    {error ?? helperText}
                </p>
            )}
        </div>
    );
});

export default Input;
