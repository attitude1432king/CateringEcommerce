import { forwardRef } from "react";
import { ChevronDown } from "lucide-react";

const Select = forwardRef(function Select(
    { label, error, helperText, id, className = "", children, ...rest },
    ref
) {
    const selectId = id ?? (label ? label.toLowerCase().replace(/\s+/g, "-") : undefined);

    return (
        <div className="flex flex-col gap-1 w-full">
            {label && (
                <label htmlFor={selectId} className="text-xs font-semibold text-neutral-700">
                    {label}
                </label>
            )}
            <div className="relative">
                <select
                    ref={ref}
                    id={selectId}
                    className={`
                        w-full pl-4 pr-10 py-3 text-sm bg-white rounded-xl border-2 outline-none
                        appearance-none transition-all duration-200 font-sans text-neutral-900
                        border-neutral-200
                        focus:border-primary focus:ring-4 focus:ring-primary/10
                        disabled:bg-neutral-100 disabled:cursor-not-allowed
                        ${error ? "border-danger focus:border-danger" : ""}
                        ${className}
                    `}
                    style={{ boxShadow: "var(--shadow-input)" }}
                    {...rest}
                >
                    {children}
                </select>
                <ChevronDown
                    size={16}
                    className="absolute right-3.5 top-1/2 -translate-y-1/2 text-neutral-400 pointer-events-none"
                />
            </div>
            {(error || helperText) && (
                <p className={`text-xs mt-0.5 ${error ? "text-danger" : "text-neutral-500"}`}>
                    {error ?? helperText}
                </p>
            )}
        </div>
    );
});

export default Select;
