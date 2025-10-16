
/*
========================================
File: src/components/HowItWorksSection.jsx
========================================
*/
import React from 'react';

export default function HowItWorksSection({ steps }) {
    return (
        <section className="py-12 md:py-16 bg-white">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                <h2 className="text-3xl font-bold text-neutral-800 text-center mb-12">How Feasto Works</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 text-center">
                    {steps.map(step => (
                        <div key={step.id} className="p-6">
                            <div className="text-5xl mb-4 text-rose-500">{step.icon}</div>
                            <h3 className="text-xl font-semibold text-neutral-800 mb-2">{step.title}</h3>
                            <p className="text-neutral-600 text-sm">{step.description}</p>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}