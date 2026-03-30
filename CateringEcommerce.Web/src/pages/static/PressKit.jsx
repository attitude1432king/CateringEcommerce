import React from 'react';
import { Link } from 'react-router-dom';

export default function PressKit() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Press Kit</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-indigo-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">Press Kit</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        Everything journalists and media professionals need to cover ENYVORA.
                    </p>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                {/* Company overview */}
                <div className="max-w-3xl mb-16">
                    <h2 className="text-2xl font-bold text-gray-900 mb-4">Company Overview</h2>
                    <p className="text-gray-600 leading-relaxed mb-4">
                        ENYVORA is India's premium event catering marketplace, connecting customers with certified catering businesses for weddings, corporate events, birthdays, and more. Founded to simplify the catering discovery and booking process, ENYVORA offers a seamless platform with verified caterers, transparent pricing, and reliable service.
                    </p>
                    <p className="text-gray-600 leading-relaxed">
                        With over 1,000 catering partners and 50,000+ events served, ENYVORA is rapidly becoming the go-to platform for event food in India.
                    </p>
                </div>

                {/* Downloads */}
                <h2 className="text-xl font-bold text-gray-900 mb-6">Brand Assets</h2>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-16">
                    {[
                        { name: 'Logo Pack', desc: 'SVG, PNG in light and dark variants', icon: '🎨' },
                        { name: 'Brand Guide', desc: 'Colors, typography, and usage guidelines', icon: '📖' },
                        { name: 'Product Screenshots', desc: 'High-resolution app screenshots', icon: '🖼️' },
                    ].map((asset, i) => (
                        <div key={i} className="border border-gray-100 rounded-2xl p-6 text-center hover:border-indigo-200 hover:shadow-sm transition-all">
                            <div className="text-3xl mb-3">{asset.icon}</div>
                            <h3 className="font-semibold text-gray-900 mb-1">{asset.name}</h3>
                            <p className="text-gray-500 text-sm mb-4">{asset.desc}</p>
                            <button className="w-full py-2 border border-gray-200 text-gray-600 text-sm rounded-lg hover:border-indigo-400 hover:text-indigo-600 transition-colors">
                                Download (Coming Soon)
                            </button>
                        </div>
                    ))}
                </div>

                {/* Press contact */}
                <div className="bg-indigo-50 rounded-2xl p-8 text-center">
                    <h3 className="text-lg font-bold text-gray-900 mb-2">Press Contact</h3>
                    <p className="text-gray-600 mb-4">For media inquiries, interviews, and additional materials, reach out to our communications team.</p>
                    <a href="mailto:press@enyvora.com" className="inline-block px-6 py-3 bg-indigo-600 text-white font-medium rounded-lg hover:bg-indigo-700 transition-colors">
                        press@enyvora.com
                    </a>
                </div>
            </div>
        </div>
    );
}
