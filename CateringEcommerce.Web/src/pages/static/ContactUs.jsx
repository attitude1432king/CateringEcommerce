import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { apiService } from '../../services/userApi';

export default function ContactUs() {
    const [form, setForm] = useState({ name: '', email: '', message: '' });
    const [errors, setErrors] = useState({});
    const [status, setStatus] = useState(null); // 'success' | 'error' | null
    const [loading, setLoading] = useState(false);

    const validate = () => {
        const e = {};
        if (!form.name.trim()) e.name = 'Name is required.';
        if (!form.email.trim()) e.email = 'Email is required.';
        else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) e.email = 'Enter a valid email address.';
        if (!form.message.trim()) e.message = 'Message is required.';
        else if (form.message.trim().length < 10) e.message = 'Message must be at least 10 characters.';
        return e;
    };

    const handleChange = (e) => {
        setForm(f => ({ ...f, [e.target.name]: e.target.value }));
        if (errors[e.target.name]) setErrors(err => ({ ...err, [e.target.name]: null }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const errs = validate();
        if (Object.keys(errs).length > 0) { setErrors(errs); return; }

        setLoading(true);
        setStatus(null);
        try {
            const res = await apiService.submitContact(form);
            if (res?.result) {
                setStatus('success');
                setForm({ name: '', email: '', message: '' });
            } else {
                setStatus('error');
            }
        } catch {
            setStatus('error');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Contact Us</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-indigo-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">Contact Us</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        Have a question or feedback? We'd love to hear from you. Fill out the form and we'll get back to you shortly.
                    </p>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-16">
                    {/* Contact info */}
                    <div>
                        <h2 className="text-xl font-bold text-gray-900 mb-6">Get in Touch</h2>
                        <div className="space-y-6">
                            {[
                                { icon: '📧', label: 'General Inquiries', value: 'hello@enyvora.com' },
                                { icon: '🤝', label: 'Partner Support', value: 'partners@enyvora.com' },
                                { icon: '📰', label: 'Press & Media', value: 'press@enyvora.com' },
                                { icon: '💼', label: 'Careers', value: 'careers@enyvora.com' },
                            ].map((c, i) => (
                                <div key={i} className="flex gap-4">
                                    <div className="text-2xl">{c.icon}</div>
                                    <div>
                                        <p className="text-sm font-medium text-gray-700">{c.label}</p>
                                        <a href={`mailto:${c.value}`} className="text-indigo-600 hover:underline text-sm">{c.value}</a>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <div className="mt-10 bg-gray-50 rounded-xl p-5">
                            <p className="text-sm text-gray-500">Support hours: Monday–Saturday, 9 AM – 7 PM IST</p>
                            <p className="text-sm text-gray-500 mt-1">We typically respond within 24 hours.</p>
                        </div>
                    </div>

                    {/* Form */}
                    <div>
                        <h2 className="text-xl font-bold text-gray-900 mb-6">Send a Message</h2>

                        {status === 'success' && (
                            <div className="mb-6 bg-green-50 border border-green-200 text-green-800 text-sm rounded-xl px-5 py-4">
                                Your message has been received! We'll get back to you shortly.
                            </div>
                        )}
                        {status === 'error' && (
                            <div className="mb-6 bg-red-50 border border-red-200 text-red-800 text-sm rounded-xl px-5 py-4">
                                Something went wrong. Please try again or email us directly.
                            </div>
                        )}

                        <form onSubmit={handleSubmit} className="space-y-5" noValidate>
                            <div>
                                <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
                                <input
                                    id="name" name="name" type="text"
                                    value={form.name} onChange={handleChange}
                                    placeholder="Your name"
                                    className={`w-full px-4 py-2.5 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${errors.name ? 'border-red-400' : 'border-gray-200'}`}
                                />
                                {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name}</p>}
                            </div>

                            <div>
                                <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">Email Address</label>
                                <input
                                    id="email" name="email" type="email"
                                    value={form.email} onChange={handleChange}
                                    placeholder="you@example.com"
                                    className={`w-full px-4 py-2.5 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${errors.email ? 'border-red-400' : 'border-gray-200'}`}
                                />
                                {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email}</p>}
                            </div>

                            <div>
                                <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-1">Message</label>
                                <textarea
                                    id="message" name="message" rows={5}
                                    value={form.message} onChange={handleChange}
                                    placeholder="How can we help you?"
                                    className={`w-full px-4 py-2.5 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none ${errors.message ? 'border-red-400' : 'border-gray-200'}`}
                                />
                                {errors.message && <p className="text-red-500 text-xs mt-1">{errors.message}</p>}
                            </div>

                            <button
                                type="submit" disabled={loading}
                                className="w-full py-3 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                            >
                                {loading ? 'Sending...' : 'Send Message'}
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );
}
