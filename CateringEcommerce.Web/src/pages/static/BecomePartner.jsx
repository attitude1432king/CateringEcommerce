import React from 'react';
import { Link } from 'react-router-dom';

export default function BecomePartner() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-neutral-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-neutral-500">
                    <Link to="/" className="hover:text-neutral-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-neutral-700 font-medium">Become a Partner</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-orange-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-neutral-900">Grow Your Catering Business</h1>
                    <p className="mt-4 text-lg text-neutral-600 max-w-2xl mx-auto">
                        Join 1,000+ catering partners on ENYVORA and reach thousands of customers looking to book for their events.
                    </p>
                    <Link to="/partner-registration" className="mt-8 inline-block px-8 py-4 bg-primary text-white font-semibold rounded-xl hover:bg-primary transition-colors text-lg">
                        Register as a Partner
                    </Link>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                {/* Benefits */}
                <h2 className="text-2xl font-bold text-neutral-900 text-center mb-10">Why Partner with ENYVORA?</h2>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-16">
                    {[
                        { icon: '💰', title: 'Earn More', desc: 'Access a large customer base actively searching for catering services. More bookings, more revenue — with transparent commission tiers.' },
                        { icon: '🚀', title: 'Grow Faster', desc: 'Our platform handles discovery, marketing, and booking management so you can focus on what you do best — cooking great food.' },
                        { icon: '🤝', title: 'Expert Support', desc: 'Dedicated partner support team to help you set up, grow, and resolve any issues quickly. You\'re never on your own.' },
                    ].map((b, i) => (
                        <div key={i} className="bg-white border border-gray-100 rounded-2xl p-8 text-center shadow-sm">
                            <div className="text-4xl mb-4">{b.icon}</div>
                            <h3 className="text-lg font-semibold text-neutral-900 mb-2">{b.title}</h3>
                            <p className="text-neutral-500 text-sm leading-relaxed">{b.desc}</p>
                        </div>
                    ))}
                </div>

                {/* How it works */}
                <h2 className="text-2xl font-bold text-neutral-900 text-center mb-10">How It Works</h2>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-16">
                    {[
                        { step: '01', title: 'Register & Verify', desc: 'Complete our simple 5-step onboarding. Upload your documents, menu, and photos for approval.' },
                        { step: '02', title: 'Get Discovered', desc: 'Your profile goes live and appears in search results for customers in your area looking for caterers.' },
                        { step: '03', title: 'Accept Bookings', desc: 'Receive booking requests, manage orders through your dashboard, and get paid securely.' },
                    ].map((s, i) => (
                        <div key={i} className="flex gap-4">
                            <div className="text-3xl font-bold text-orange-200 shrink-0">{s.step}</div>
                            <div>
                                <h3 className="font-semibold text-neutral-900 mb-1">{s.title}</h3>
                                <p className="text-neutral-500 text-sm leading-relaxed">{s.desc}</p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* CTA */}
                <div className="bg-orange-50 rounded-2xl p-10 text-center">
                    <h3 className="text-xl font-bold text-neutral-900 mb-2">Ready to get started?</h3>
                    <p className="text-neutral-600 mb-6">Registration is free. Start receiving bookings within days of approval.</p>
                    <Link to="/partner-registration" className="inline-block px-8 py-3 bg-primary text-white font-semibold rounded-xl hover:bg-primary transition-colors">
                        Register Now — It's Free
                    </Link>
                </div>
            </div>
        </div>
    );
}
