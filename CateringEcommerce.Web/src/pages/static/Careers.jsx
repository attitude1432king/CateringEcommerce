import React from 'react';
import { Link } from 'react-router-dom';

const OPEN_ROLES = [
    { title: 'Full Stack Developer', team: 'Engineering', location: 'Remote / Hybrid', type: 'Full-time', desc: 'Build and scale our core platform using React, .NET Core, and SQL Server. You\'ll work on features used by thousands of caterers and customers.' },
    { title: 'Growth Marketing Manager', team: 'Marketing', location: 'Mumbai / Remote', type: 'Full-time', desc: 'Drive partner and customer acquisition through data-driven campaigns, SEO strategies, and content marketing initiatives.' },
    { title: 'Operations Associate', team: 'Operations', location: 'Mumbai', type: 'Full-time', desc: 'Manage onboarding of new catering partners, quality assurance processes, and support escalation workflows.' },
];

export default function Careers() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Careers</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-indigo-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">Join Our Team</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        Help us transform how India experiences event catering. We're a small, passionate team building something big.
                    </p>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                {/* Culture section */}
                <div className="max-w-3xl mx-auto text-center mb-16">
                    <h2 className="text-2xl font-bold text-gray-900 mb-4">Why Work at ENYVORA?</h2>
                    <p className="text-gray-600 leading-relaxed">
                        We believe great work happens when talented people have the freedom to do their best work. We offer flexible hours, remote-friendly culture, competitive compensation, and the chance to make a real impact on a fast-growing platform.
                    </p>
                </div>

                {/* Open roles */}
                <h2 className="text-xl font-bold text-gray-900 mb-6">Open Positions</h2>
                <div className="space-y-4 mb-16">
                    {OPEN_ROLES.map((role, i) => (
                        <div key={i} className="border border-gray-100 rounded-2xl p-6 hover:border-indigo-200 hover:shadow-sm transition-all">
                            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-900">{role.title}</h3>
                                    <div className="flex flex-wrap gap-2 mt-2">
                                        <span className="bg-gray-100 text-gray-600 text-xs px-2.5 py-1 rounded-full">{role.team}</span>
                                        <span className="bg-gray-100 text-gray-600 text-xs px-2.5 py-1 rounded-full">{role.location}</span>
                                        <span className="bg-indigo-50 text-indigo-600 text-xs px-2.5 py-1 rounded-full">{role.type}</span>
                                    </div>
                                    <p className="text-gray-500 text-sm mt-3 leading-relaxed">{role.desc}</p>
                                </div>
                                <a href="mailto:careers@enyvora.com?subject=Application: ${role.title}" className="shrink-0 px-5 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors text-center">
                                    Apply Now
                                </a>
                            </div>
                        </div>
                    ))}
                </div>

                {/* CTA */}
                <div className="bg-gray-50 rounded-2xl p-10 text-center">
                    <h3 className="text-xl font-bold text-gray-900 mb-2">Don't see a role that fits?</h3>
                    <p className="text-gray-600 mb-6">We're always looking for exceptional people. Send us your resume and tell us how you can contribute.</p>
                    <a href="mailto:careers@enyvora.com" className="inline-block px-6 py-3 bg-gray-900 text-white font-medium rounded-lg hover:bg-gray-800 transition-colors">
                        careers@enyvora.com
                    </a>
                </div>
            </div>
        </div>
    );
}
