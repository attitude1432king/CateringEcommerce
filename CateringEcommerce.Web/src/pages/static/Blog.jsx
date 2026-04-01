import React from 'react';
import { Link } from 'react-router-dom';

const SAMPLE_POSTS = [
    { id: 1, title: '10 Tips for Choosing the Perfect Wedding Caterer', date: 'March 10, 2026', category: 'Wedding', excerpt: 'Finding the right caterer for your wedding can feel overwhelming. Here are our top tips to help you make the best choice for your big day.' },
    { id: 2, title: 'Top Food Trends for Corporate Events in 2026', date: 'March 5, 2026', category: 'Corporate', excerpt: 'Corporate events are evolving — discover the cuisine trends that are impressing guests and elevating the experience at business gatherings.' },
    { id: 3, title: 'How to Plan a Memorable Birthday Party Menu', date: 'February 28, 2026', category: 'Birthday', excerpt: 'From dietary preferences to dessert stations, planning your birthday menu with these ideas will ensure every guest leaves satisfied.' },
];

export default function Blog() {
    return (
        <div className="min-h-screen bg-white">
            {/* Breadcrumb */}
            <div className="bg-gray-50 border-b">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3 text-sm text-gray-500">
                    <Link to="/" className="hover:text-gray-700">Home</Link>
                    <span className="mx-2">/</span>
                    <span className="text-gray-700 font-medium">Blog</span>
                </div>
            </div>

            {/* Hero */}
            <div className="bg-gradient-to-br from-indigo-50 to-white py-16">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
                    <h1 className="text-4xl font-bold text-gray-900">ENYVORA Blog</h1>
                    <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto">
                        Catering tips, event planning guides, and inspiration for your next celebration.
                    </p>
                </div>
            </div>

            {/* Coming soon banner */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-10">
                <div className="bg-indigo-50 border border-indigo-100 rounded-xl p-4 text-center text-indigo-700 font-medium text-sm">
                    Full blog launching soon — real articles are on the way!
                </div>
            </div>

            {/* Sample posts */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                    {SAMPLE_POSTS.map(post => (
                        <div key={post.id} className="bg-white border border-gray-100 rounded-2xl overflow-hidden shadow-sm hover:shadow-md transition-shadow">
                            <div className="bg-gradient-to-br from-gray-100 to-gray-200 h-48 flex items-center justify-center">
                                <svg className="w-12 h-12 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                                </svg>
                            </div>
                            <div className="p-6">
                                <span className="inline-block bg-indigo-50 text-indigo-600 text-xs font-semibold px-2.5 py-1 rounded-full mb-3">{post.category}</span>
                                <h3 className="font-bold text-gray-900 mb-2 leading-snug">{post.title}</h3>
                                <p className="text-gray-500 text-sm mb-4 leading-relaxed">{post.excerpt}</p>
                                <div className="flex items-center justify-between">
                                    <span className="text-xs text-gray-400">{post.date}</span>
                                    <button className="text-indigo-600 text-sm font-medium hover:underline">Read more →</button>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
