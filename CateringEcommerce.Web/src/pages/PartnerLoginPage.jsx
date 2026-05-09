import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import AuthModal from '../components/user/AuthModal';

const STAT_DEFAULTS = {
    activePartners:  500,
    completedEvents: 5000,
    citiesServed:    20,
};

function useAnimatedCounter(end, duration = 1800) {
    const [count, setCount] = useState(0);
    const [triggered, setTriggered] = useState(false);
    const ref = useRef(null);

    useEffect(() => {
        const node = ref.current;
        if (!node) return;
        const observer = new IntersectionObserver(
            ([entry]) => { if (entry.isIntersecting) setTriggered(true); },
            { threshold: 0.1 }
        );
        observer.observe(node);
        return () => observer.disconnect();
    }, []);

    useEffect(() => {
        if (!triggered || end === 0) return;
        const steps = 60;
        const step = end / steps;
        let current = 0;
        const timer = setInterval(() => {
            current += step;
            if (current >= end) { setCount(end); clearInterval(timer); }
            else setCount(Math.floor(current));
        }, duration / steps);
        return () => clearInterval(timer);
    }, [triggered, end, duration]);

    useEffect(() => { setCount(0); setTriggered(false); }, [end]);

    return [count, ref];
}

const features = [
    {
        emoji: '👥',
        title: 'Reach More Customers',
        desc: 'Get discovered by 50,000+ event planners actively searching for caterers in your city.',
        color: 'bg-sky-50 border-sky-200 text-sky-600',
    },
    {
        emoji: '📋',
        title: 'Manage Everything Easily',
        desc: 'Orders, menu, staff availability — handled from one powerful dashboard.',
        color: 'bg-violet-50 border-violet-200 text-violet-600',
    },
    {
        emoji: '⚡',
        title: 'Instant Order Alerts',
        desc: 'Get notified the moment a booking comes in. Never miss an opportunity.',
        color: 'bg-amber-50 border-amber-200 text-amber-600',
    },
    {
        emoji: '💰',
        title: 'Reliable Weekly Payouts',
        desc: 'Settlements land in your bank every week — on schedule, every time.',
        color: 'bg-emerald-50 border-emerald-200 text-emerald-600',
    },
];

const weekDays = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
const barHeights = [45, 68, 52, 85, 73, 92, 78];

const testimonials = [
    {
        name: 'Rajesh Kumar',
        city: 'Mumbai',
        text: 'Bookings tripled in 3 months. The dashboard is a game-changer.',
        initials: 'RK',
        color: 'bg-rose-100 text-rose-700',
    },
    {
        name: 'Priya Nair',
        city: 'Bangalore',
        text: 'Payouts are always on time. I can plan my business with confidence.',
        initials: 'PN',
        color: 'bg-amber-100 text-amber-700',
    },
    {
        name: 'Amit Shah',
        city: 'Delhi',
        text: 'The instant alerts helped me close 12 new weddings this season.',
        initials: 'AS',
        color: 'bg-sky-100 text-sky-700',
    },
];

export default function PartnerLoginPage() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const navigate = useNavigate();

    const [liveStats, setLiveStats] = useState(STAT_DEFAULTS);

    useEffect(() => {
        fetch('/api/app-settings/partner-stats', { credentials: 'include' })
            .then(r => r.ok ? r.json() : null)
            .then(json => {
                if (json?.result && json?.data) {
                    const d = json.data;
                    setLiveStats({
                        activePartners:  d.activePartners  || STAT_DEFAULTS.activePartners,
                        completedEvents: d.completedEvents || STAT_DEFAULTS.completedEvents,
                        citiesServed:    d.citiesServed    || STAT_DEFAULTS.citiesServed,
                    });
                }
            })
            .catch(() => {});
    }, []);

    const [partners, partnersRef] = useAnimatedCounter(liveStats.activePartners,  1600);
    const [events,   eventsRef]   = useAnimatedCounter(liveStats.completedEvents,  2000);
    const [cities,   citiesRef]   = useAnimatedCounter(liveStats.citiesServed,     1400);

    return (
        <>
            <AuthModal
                isOpen={isAuthModalOpen}
                onClose={() => setIsAuthModalOpen(false)}
                isPartnerLogin={true}
            />

            <div className="min-h-screen bg-gradient-to-br from-rose-50 via-white to-amber-50 text-neutral-900 flex flex-col overflow-hidden">

                {/* ── Header ── */}
                <header className="relative z-20 border-b border-neutral-200 bg-white/80 backdrop-blur-sm">
                    <div className="max-w-screen-xl mx-auto flex items-center justify-between px-6 md:px-10 py-4">
                        <img src="/logo.svg" alt="ENYVORA Partners" className="h-9 w-auto" />
                        <div className="flex items-center gap-3">
                            <span className="text-neutral-600 text-sm hidden sm:block">Already a partner?</span>
                            <button
                                onClick={() => setIsAuthModalOpen(true)}
                                className="px-5 py-2.5 text-sm font-semibold rounded-lg bg-white hover:bg-neutral-50 border-2 border-neutral-200 text-neutral-700 transition-all"
                            >
                                Sign In
                            </button>
                        </div>
                    </div>
                </header>

                {/* ── Hero Section ── */}
                <main className="flex-1 flex flex-col">
                    <div className="max-w-screen-xl mx-auto w-full flex flex-col lg:flex-row lg:items-center gap-10 px-6 md:px-10 py-12 lg:py-16">

                        {/* Left — Copy & CTAs */}
                        <div className="relative flex-1 flex flex-col">
                            {/* Decorative blobs */}
                            <div className="absolute top-0 -left-24 w-80 h-80 bg-rose-200 opacity-20 rounded-full blur-3xl mix-blend-multiply pointer-events-none" />
                            <div className="absolute bottom-0 -left-10 w-64 h-64 bg-amber-200 opacity-20 rounded-full blur-3xl mix-blend-multiply pointer-events-none" />

                            <div className="relative">
                                {/* Label */}
                                <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-rose-50 border border-rose-200 text-rose-600 text-xs font-bold tracking-widest mb-5 uppercase">
                                    <span className="w-1.5 h-1.5 rounded-full bg-rose-600 animate-pulse" />
                                    Partner Portal
                                </div>

                                {/* Headline */}
                                <h1 className="text-4xl md:text-5xl xl:text-[3.25rem] font-black leading-[1.1] mb-4 tracking-tight text-neutral-900">
                                    Fill Your Calendar.<br />
                                    <span className="bg-gradient-to-r from-rose-600 via-pink-600 to-amber-500 bg-clip-text text-transparent">
                                        Grow Your Business.
                                    </span>
                                </h1>

                                <p className="text-neutral-600 text-base lg:text-lg leading-relaxed mb-7 max-w-xl">
                                    Join thousands of catering professionals using ENYVORA to win more bookings, delight customers, and scale their business — effortlessly.
                                </p>

                                {/* Feature grid */}
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mb-7">
                                    {features.map(({ emoji, title, desc, color }) => (
                                        <div
                                            key={title}
                                            className="flex items-start gap-3 rounded-xl p-4 border bg-white hover:bg-neutral-50 transition-colors border-neutral-200 shadow-sm"
                                        >
                                            <div className={`w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0 text-lg border ${color}`}>
                                                {emoji}
                                            </div>
                                            <div>
                                                <h3 className="font-semibold text-neutral-800 text-sm">{title}</h3>
                                                <p className="text-neutral-500 text-xs mt-0.5 leading-relaxed">{desc}</p>
                                            </div>
                                        </div>
                                    ))}
                                </div>

                                {/* CTA Buttons */}
                                <div className="flex flex-col sm:flex-row gap-3">
                                    <button
                                        onClick={() => navigate('/partner-registration')}
                                        className="flex-1 sm:flex-none px-8 py-3.5 bg-gradient-to-r from-rose-500 to-rose-600 hover:from-rose-600 hover:to-rose-700 text-white font-bold rounded-xl shadow-lg shadow-rose-500/40 transition-all transform hover:-translate-y-0.5 text-sm"
                                    >
                                        Register Your Business
                                    </button>
                                    <button
                                        onClick={() => setIsAuthModalOpen(true)}
                                        className="flex-1 sm:flex-none px-8 py-3.5 bg-white hover:bg-neutral-50 text-neutral-700 font-semibold rounded-xl border-2 border-neutral-200 transition-all text-sm"
                                    >
                                        Login to Dashboard →
                                    </button>
                                </div>

                                <p className="text-neutral-400 text-xs mt-4">
                                    By continuing, you agree to our{' '}
                                    <a href="#" className="text-neutral-500 hover:text-neutral-800 transition-colors underline underline-offset-2">Terms of Service</a>
                                    {' '}and{' '}
                                    <a href="#" className="text-neutral-500 hover:text-neutral-800 transition-colors underline underline-offset-2">Privacy Policy</a>.
                                </p>
                            </div>
                        </div>

                        {/* Right — Dashboard Preview */}
                        <div className="relative flex-1 flex items-center justify-center lg:justify-end">
                            {/* Decorative glow */}
                            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-amber-100 opacity-40 rounded-full blur-3xl pointer-events-none" />

                            <div className="relative w-full max-w-md space-y-3">

                                {/* Stats overview card */}
                                <div className="bg-white border border-neutral-200 rounded-2xl p-5 shadow-xl">
                                    <div className="flex items-center justify-between mb-4">
                                        <h3 className="font-semibold text-sm text-neutral-900">Platform at a Glance</h3>
                                        <span className="text-xs text-emerald-600 bg-emerald-50 border border-emerald-200 px-2.5 py-1 rounded-full font-semibold">
                                            ● Live
                                        </span>
                                    </div>

                                    <div className="grid grid-cols-4 gap-2 mb-5">
                                        <div className="text-center bg-neutral-50 rounded-xl p-3 border border-neutral-200">
                                            <p ref={partnersRef} className="text-lg font-black text-neutral-900 tabular-nums">
                                                {partners >= 1000 ? `${(partners / 1000).toFixed(1)}K` : partners}+
                                            </p>
                                            <p className="text-neutral-500 text-xs mt-0.5 leading-tight">Active Partners</p>
                                        </div>
                                        <div className="text-center bg-neutral-50 rounded-xl p-3 border border-neutral-200">
                                            <p ref={eventsRef} className="text-lg font-black text-amber-600 tabular-nums">
                                                {events >= 1000 ? `${(events / 1000).toFixed(0)}K` : events}+
                                            </p>
                                            <p className="text-neutral-500 text-xs mt-0.5 leading-tight">Events Done</p>
                                        </div>
                                        <div className="text-center bg-neutral-50 rounded-xl p-3 border border-neutral-200">
                                            <p ref={citiesRef} className="text-lg font-black text-sky-600 tabular-nums">
                                                {cities}+
                                            </p>
                                            <p className="text-neutral-500 text-xs mt-0.5 leading-tight">Cities Served</p>
                                        </div>
                                        <div className="relative text-center bg-gradient-to-br from-rose-50 via-pink-50 to-amber-50 rounded-xl p-3 border border-rose-200 overflow-hidden">
                                            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,rgba(251,113,133,0.1),transparent_70%)] pointer-events-none" />
                                            <div className="relative flex items-center justify-center gap-0.5 mb-0.5">
                                                <svg className="w-3.5 h-3.5 text-amber-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                                                    <path strokeLinecap="round" strokeLinejoin="round" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
                                                </svg>
                                                <span className="text-lg font-black bg-gradient-to-r from-rose-600 to-amber-500 bg-clip-text text-transparent">
                                                    150%
                                                </span>
                                            </div>
                                            <p className="text-neutral-500 text-xs mt-0.5 leading-tight">Avg Growth</p>
                                        </div>
                                    </div>

                                    {/* Mini bar chart */}
                                    <p className="text-neutral-400 text-xs mb-2">Bookings this week</p>
                                    <div className="flex items-end gap-1 h-14">
                                        {barHeights.map((h, i) => (
                                            <div
                                                key={i}
                                                className="flex-1 rounded-t-sm bg-gradient-to-t from-rose-500 to-amber-400 opacity-90"
                                                style={{ height: `${h}%` }}
                                            />
                                        ))}
                                    </div>
                                    <div className="flex justify-between mt-1.5">
                                        {weekDays.map((d, i) => (
                                            <span key={i} className="flex-1 text-center text-neutral-400 text-xs">{d}</span>
                                        ))}
                                    </div>
                                </div>

                                {/* Notification + Trust — side by side */}
                                <div className="grid grid-cols-2 gap-3">
                                    <div className="bg-white border border-neutral-200 rounded-xl p-4 flex items-center gap-3 shadow-md">
                                        <div className="w-9 h-9 bg-emerald-50 border border-emerald-200 rounded-full flex items-center justify-center flex-shrink-0">
                                            <svg className="w-4 h-4 text-emerald-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                        </div>
                                        <div className="min-w-0">
                                            <p className="text-neutral-800 text-xs font-semibold leading-tight">New Booking!</p>
                                            <p className="text-neutral-500 text-xs truncate">Wedding · 250 guests</p>
                                            <p className="text-rose-500 text-xs font-semibold">₹1.2L</p>
                                        </div>
                                    </div>

                                    <div className="bg-white border border-neutral-200 rounded-xl p-4 flex items-center gap-3 shadow-md">
                                        <div className="w-9 h-9 bg-amber-50 border border-amber-200 rounded-lg flex items-center justify-center flex-shrink-0">
                                            <svg className="w-4 h-4 text-amber-600" fill="currentColor" viewBox="0 0 20 20">
                                                <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                            </svg>
                                        </div>
                                        <div>
                                            <p className="text-neutral-800 text-xs font-semibold leading-tight">Verified Partner</p>
                                            <p className="text-neutral-500 text-xs leading-tight">Competitive rates · 100% transparent</p>
                                        </div>
                                    </div>
                                </div>

                                {/* Payout summary card */}
                                <div className="bg-gradient-to-r from-neutral-900 to-neutral-800 border border-neutral-700 rounded-xl p-4 shadow-xl">
                                    <div className="flex items-center justify-between mb-3">
                                        <p className="text-neutral-300 text-xs font-medium">Next Payout</p>
                                        <span className="text-xs text-emerald-400 bg-emerald-900/40 border border-emerald-700 px-2 py-0.5 rounded-full font-semibold">
                                            Scheduled
                                        </span>
                                    </div>
                                    <div className="flex items-end justify-between">
                                        <div>
                                            <p className="text-white text-2xl font-black tabular-nums">₹84,500</p>
                                            <p className="text-neutral-400 text-xs mt-0.5">Across 6 confirmed events</p>
                                        </div>
                                        <div className="text-right">
                                            <p className="text-emerald-400 text-sm font-bold">Friday</p>
                                            <p className="text-neutral-500 text-xs">Direct bank transfer</p>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>

                    {/* ── Testimonials Bar ── */}
                    <div className="border-t border-neutral-200 bg-white/70 backdrop-blur-sm">
                        <div className="max-w-screen-xl mx-auto px-6 md:px-10 py-8">
                            <p className="text-center text-xs font-bold text-neutral-400 uppercase tracking-widest mb-6">
                                Trusted by catering professionals across India
                            </p>
                            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                                {testimonials.map(({ name, city, text, initials, color }) => (
                                    <div key={name} className="flex items-start gap-3 bg-neutral-50 border border-neutral-200 rounded-xl p-4">
                                        <div className={`w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0 text-xs font-bold ${color}`}>
                                            {initials}
                                        </div>
                                        <div>
                                            <p className="text-neutral-700 text-sm leading-relaxed">"{text}"</p>
                                            <p className="text-neutral-400 text-xs mt-1.5 font-medium">{name} · {city}</p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </main>
            </div>
        </>
    );
}
