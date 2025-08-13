/*
========================================
File: src/components/owner/MultiStepProgressBar.jsx (REVISED)
========================================
*/
import React from 'react';

export default function MultiStepProgressBar({ currentStep, steps }) {
    return (
        <div className="w-full px-4 sm:px-0 mb-8">
            <div className="flex">
                {steps.map((stepName, i) => {
                    const step = i + 1;
                    const isCompleted = step < currentStep;
                    const isActive = step === currentStep;
                    return (
                        <React.Fragment key={step}>
                            <div className="flex flex-col items-center w-1/4">
                                <div className={`w-10 h-10 rounded-full flex items-center justify-center text-lg font-bold transition-all duration-300 ${isCompleted ? 'bg-green-500 text-white' :
                                        isActive ? 'bg-rose-600 text-white scale-110' :
                                            'bg-neutral-200 text-neutral-500'
                                    }`}>
                                    {isCompleted ? '?' : step}
                                </div>
                                <p className={`mt-2 text-xs text-center font-semibold transition-colors duration-300 ${isActive ? 'text-rose-600' : 'text-neutral-500'
                                    }`}>{stepName}</p>
                            </div>
                            {step < steps.length && (
                                <div className={`flex-1 h-1 self-start mt-5 transition-colors duration-300 ${isCompleted ? 'bg-green-500' : 'bg-neutral-200'}`}></div>
                            )}
                        </React.Fragment>
                    );
                })}
            </div>
        </div>
    );
}