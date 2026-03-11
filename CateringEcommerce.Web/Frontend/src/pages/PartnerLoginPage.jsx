import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import AuthModal from '../components/user/AuthModal';

// Default fallback values shown while loading or if API fails
const STAT_DEFAULTS = {
    activePartners:  500,
    completedEvents: 5000,
    citiesServed:    20,
};

// Animate a number from 0 to `end` once the element scrolls into view
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

    // Reset when `end` changes (live data arrives after defaults)
    useEffect(() => { setCount(0); setTriggered(false); }, [end]);

    return [count, ref];
}

const features = [
    {
        emoji: '👥',
        title: 'Reach More Customers',
        desc: 'Get discovered by 50,000+ event planners actively searching for caterers in your city.',
        color: 'bg-sky-500/15 border-sky-500/25 text-sky-400',
    },
    {
        emoji: '📋',
        title: 'Manage Everything Easily',
        desc: 'Orders, menu, staff availability — handled from one powerful dashboard.',
        color: 'bg-violet-500/15 border-violet-500/25 text-violet-400',
    },
    {
        emoji: '⚡',
        title: 'Instant Order Alerts',
        desc: 'Get notified the moment a booking comes in. Never miss an opportunity.',
        color: 'bg-amber-500/15 border-amber-500/25 text-amber-400',
    },
    {
        emoji: '💰',
        title: 'Reliable Weekly Payouts',
        desc: 'Settlements land in your bank every week — on schedule, every time.',
        color: 'bg-emerald-500/15 border-emerald-500/25 text-emerald-400',
    },
];

const weekDays = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
const barHeights = [45, 68, 52, 85, 73, 92, 78];

export default function PartnerLoginPage() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const navigate = useNavigate();

    // Live stats from DB — starts with defaults, replaced once API responds
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
            .catch(() => { /* keep defaults silently */ });
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

            <div className="min-h-screen bg-slate-950 text-white flex flex-col overflow-hidden">

                {/* ── Header ─────────────────────────────────────────────── */}
                <header className="relative z-20 flex items-center justify-between px-6 md:px-12 py-5 border-b border-white/8">
                    <img src="/logo.svg" alt="ENYVORA Partners" className="h-9 w-auto brightness-0 invert" />
                    <div className="flex items-center gap-3">
                        <span className="text-slate-500 text-sm hidden sm:block">Already a partner?</span>
                        <button
                            onClick={() => setIsAuthModalOpen(true)}
                            className="px-5 py-2.5 text-sm font-semibold rounded-lg bg-white/8 hover:bg-white/15 border border-white/15 transition-all"
                        >
                            Sign In
                        </button>
                    </div>
                </header>

                {/* ── Main ───────────────────────────────────────────────── */}
                <main className="flex-1 flex flex-col lg:flex-row">

                    {/* Left — Copy & CTAs */}
                    <div className="relative flex-1 flex flex-col justify-center px-6 md:px-12 lg:px-16 py-14 lg:py-0">
                        {/* Ambient glow */}
                        <div className="absolute top-1/3 -left-20 w-80 h-80 bg-rose-600/20 rounded-full blur-3xl pointer-events-none" />
                        <div className="absolute bottom-1/4 left-1/3 w-64 h-64 bg-pink-600/10 rounded-full blur-3xl pointer-events-none" />

                        <div className="relative max-w-lg">
                            {/* Label */}
                            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-rose-500/12 border border-rose-500/30 text-rose-400 text-xs font-bold tracking-widest mb-6 uppercase">
                                <span className="w-1.5 h-1.5 rounded-full bg-rose-400 animate-pulse" />
                                Partner Portal
                            </div>

                            {/* Headline */}
                            <h1 className="text-4xl md:text-5xl lg:text-[3.5rem] font-black leading-[1.1] mb-5 tracking-tight">
                                Fill Your Calendar.<br />
                                <span className="bg-gradient-to-r from-rose-400 via-pink-400 to-amber-400 bg-clip-text text-transparent">
                                    Grow Your Business.
                                </span>
                            </h1>

                            <p className="text-slate-400 text-lg leading-relaxed mb-8">
                                Join thousands of catering professionals using ENYVORA to win more bookings, delight customers, and scale their business — effortlessly.
                            </p>

                            {/* Feature grid */}
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mb-9">
                                {features.map(({ emoji, title, desc, color }) => (
                                    <div
                                        key={title}
                                        className={`flex items-start gap-3 rounded-xl p-4 border bg-white/4 hover:bg-white/7 transition-colors border-white/10`}
                                    >
                                        <div className={`w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0 text-lg border ${color}`}>
                                            {emoji}
                                        </div>
                                        <div>
                                            <h3 className="font-semibold text-white text-sm">{title}</h3>
                                            <p className="text-slate-500 text-xs mt-0.5 leading-relaxed">{desc}</p>
                                        </div>
                                    </div>
                                ))}
                            </div>

                            {/* CTA Buttons */}
                            <div className="flex flex-col sm:flex-row gap-3">
                                <button
                                    onClick={() => navigate('/partner-registration')}
                                    className="flex-1 sm:flex-none px-8 py-4 bg-gradient-to-r from-rose-500 to-pink-500 hover:from-rose-400 hover:to-pink-400 text-white font-bold rounded-xl shadow-lg shadow-rose-500/30 transition-all transform hover:-translate-y-0.5 text-sm"
                                >
                                    Register Your Business
                                </button>
                                <button
                                    onClick={() => setIsAuthModalOpen(true)}
                                    className="flex-1 sm:flex-none px-8 py-4 bg-white/8 hover:bg-white/14 text-white font-semibold rounded-xl border border-white/15 transition-all text-sm"
                                >
                                    Login to Dashboard →
                                </button>
                            </div>

                            <p className="text-slate-700 text-xs mt-5">
                                By continuing, you agree to our{' '}
                                <a href="#" className="text-slate-500 hover:text-slate-300 transition-colors">Terms of Service</a>
                                {' '}and{' '}
                                <a href="#" className="text-slate-500 hover:text-slate-300 transition-colors">Privacy Policy</a>.
                            </p>
                        </div>
                    </div>

                    {/* Right — Dashboard Preview */}
                    <div className="relative flex-1 flex items-center justify-center px-6 md:px-12 py-14 lg:py-0 bg-slate-900/60">
                        {/* Grid texture */}
                        <div
                            className="absolute inset-0 opacity-[0.04] pointer-events-none"
                            style={{
                                backgroundImage: 'linear-gradient(to right,#fff 1px,transparent 1px),linear-gradient(to bottom,#fff 1px,transparent 1px)',
                                backgroundSize: '36px 36px',
                            }}
                        />
                        {/* Glow */}
                        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-amber-500/8 rounded-full blur-3xl pointer-events-none" />

                        <div className="relative w-full max-w-sm space-y-3">

                            {/* Stats overview card */}
                            <div className="bg-slate-800/90 backdrop-blur-sm border border-white/10 rounded-2xl p-5 shadow-2xl">
                                <div className="flex items-center justify-between mb-4">
                                    <h3 className="font-semibold text-sm text-white">Platform at a Glance</h3>
                                    <span className="text-xs text-emerald-400 bg-emerald-400/12 border border-emerald-400/25 px-2.5 py-1 rounded-full font-semibold">
                                        ● Live
                                    </span>
                                </div>

                                <div className="grid grid-cols-2 gap-2 mb-5">
                                    <div className="text-center bg-slate-700/50 rounded-xl p-3 border border-white/5">
                                        <p ref={partnersRef} className="text-xl font-black text-white tabular-nums">
                                            {partners >= 1000 ? `${(partners / 1000).toFixed(1)}K` : partners}+
                                        </p>
                                        <p className="text-slate-500 text-xs mt-0.5">Active Partners</p>
                                    </div>
                                    <div className="text-center bg-slate-700/50 rounded-xl p-3 border border-white/5">
                                        <p ref={eventsRef} className="text-xl font-black text-amber-400 tabular-nums">
                                            {events >= 1000 ? `${(events / 1000).toFixed(0)}K` : events}+
                                        </p>
                                        <p className="text-slate-500 text-xs mt-0.5">Events Done</p>
                                    </div>
                                    <div className="text-center bg-slate-700/50 rounded-xl p-3 border border-white/5">
                                        <p ref={citiesRef} className="text-xl font-black text-sky-400 tabular-nums">
                                            {cities}+
                                        </p>
                                        <p className="text-slate-500 text-xs mt-0.5">Cities Served</p>
                                    </div>
                                    {/* Static growth badge — deliberately not a live counter */}
                                    <div className="relative text-center bg-gradient-to-br from-rose-500/25 via-pink-500/15 to-amber-500/20 rounded-xl p-3 border border-rose-500/30 overflow-hidden">
                                        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,rgba(251,113,133,0.15),transparent_70%)] pointer-events-none" />
                                        <div className="relative flex items-center justify-center gap-1 mb-0.5">
                                            <svg className="w-4 h-4 text-amber-400 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                                                <path strokeLinecap="round" strokeLinejoin="round" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
                                            </svg>
                                            <span className="text-xl font-black bg-gradient-to-r from-rose-400 to-amber-400 bg-clip-text text-transparent">
                                                150%
                                            </span>
                                        </div>
                                        <p className="text-slate-400 text-xs mt-0.5">Avg Growth</p>
                                    </div>
                                </div>

                                {/* Mini bar chart */}
                                <p className="text-slate-600 text-xs mb-2">Bookings this week</p>
                                <div className="flex items-end gap-1 h-12">
                                    {barHeights.map((h, i) => (
                                        <div
                                            key={i}
                                            className="flex-1 rounded-t-sm bg-gradient-to-t from-rose-600 to-amber-400 opacity-80"
                                            style={{ height: `${h}%` }}
                                        />
                                    ))}
                                </div>
                                <div className="flex justify-between mt-1.5">
                                    {weekDays.map((d, i) => (
                                        <span key={i} className="flex-1 text-center text-slate-600 text-xs">{d}</span>
                                    ))}
                                </div>
                            </div>

                            {/* Live notification card */}
                            <div className="bg-slate-800/90 backdrop-blur-sm border border-white/10 rounded-xl p-4 flex items-center gap-3">
                                <div className="w-10 h-10 bg-emerald-500/15 border border-emerald-500/25 rounded-full flex items-center justify-center flex-shrink-0">
                                    <svg className="w-5 h-5 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                </div>
                                <div className="flex-1 min-w-0">
                                    <p className="text-white text-sm font-semibold">New Booking Confirmed!</p>
                                    <p className="text-slate-500 text-xs truncate">Wedding reception · 250 guests · ₹1.2L</p>
                                </div>
                                <span className="text-slate-600 text-xs flex-shrink-0">just now</span>
                            </div>

                            {/* Trust / verified badge — commission mentioned here, secondary */}
                            <div className="bg-slate-800/90 backdrop-blur-sm border border-white/10 rounded-xl p-4 flex items-center gap-3">
                                <div className="w-9 h-9 bg-amber-400/12 border border-amber-400/25 rounded-lg flex items-center justify-center flex-shrink-0">
                                    <svg className="w-5 h-5 text-amber-400" fill="currentColor" viewBox="0 0 20 20">
                                        <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                    </svg>
                                </div>
                                <div>
                                    <p className="text-white text-sm font-semibold">Verified Partner Program</p>
                                    <p className="text-slate-500 text-xs">Industry-competitive rates · 100% transparent</p>
                                </div>
                            </div>

                        </div>
                    </div>

                </main>
            </div>
        </>
    );
}
