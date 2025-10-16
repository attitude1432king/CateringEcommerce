/*
========================================
File: src/components/owner/RegistrationSuccess.jsx (NEW FILE)
========================================
*/
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function RegistrationSuccess() {
    const navigate = useNavigate();
    const [timeLeft, setTimeLeft] = useState(24 * 60 * 60); // 24 hours in seconds

    useEffect(() => {
        const timer = setInterval(() => {
            setTimeLeft(prevTime => (prevTime > 0 ? prevTime - 1 : 0));
        }, 1000);
        return () => clearInterval(timer);
    }, []);

    const formatTime = (seconds) => {
        const h = Math.floor(seconds / 3600).toString().padStart(2, '0');
        const m = Math.floor((seconds % 3600) / 60).toString().padStart(2, '0');
        const s = (seconds % 60).toString().padStart(2, '0');
        return `${h}:${m}:${s}`;
    };

    return (
        <div className="bg-amber-50 min-h-screen flex items-center justify-center p-4">
            <div className="bg-white p-8 rounded-lg shadow-lg text-center max-w-lg animate-fade-in">
                <div className="mx-auto bg-green-100 rounded-full h-20 w-20 flex items-center justify-center mb-4">
                    <svg className="h-12 w-12 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
                    </svg>
                </div>
                <h1 className="text-2xl font-bold text-neutral-800">Registration Submitted!</h1>
                <p className="text-neutral-600 mt-2">Thank you for joining Feasto. Your application is now under review by our team.</p>
                <div className="my-6 bg-amber-100 border-l-4 border-amber-500 text-amber-700 p-4 rounded-md text-left">
                    <h4 className="font-bold">What's Next?</h4>
                    <p className="text-sm">We will verify your documents and get back to you. This process usually takes up to 24 hours. You will receive an email and SMS once your account is approved.</p>
                </div>
                <div className="text-lg font-semibold">
                    Verification Time Remaining: <span className="text-rose-600 font-bold tracking-wider">{formatTime(timeLeft)}</span>
                </div>
                <button onClick={() => navigate('/')} className="mt-8 bg-rose-600 text-white px-8 py-2 rounded-md font-medium hover:bg-rose-700">
                    Back to Home
                </button>
            </div>
        </div>
    );
}