
/*
========================================
File: src/components/AppFooter.jsx
========================================
*/
import React from 'react';

export default function AppFooter() {
    return (
        <footer className="bg-neutral-800 text-neutral-300 py-12" role="contentinfo" aria-label="Footer">
            <div className="w-full max-w-screen-2xl mx-auto px-4 sm:px-6 lg:px-8 min-w-0">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8 min-w-0">
                    {/* Brand */}
                    <div className="min-w-0">
                        <h5 className="text-lg font-semibold text-white mb-3 flex items-center">
                            <span className="icon-placeholder text-xl mr-1" aria-hidden="true">🍽️</span>
                            <span className="truncate">Feasto</span>
                        </h5>
                        <p className="text-sm leading-relaxed break-words">
                            Your one-stop solution for event catering. Delicious food, delivered.
                        </p>
                    </div>

                    {/* Company */}
                    <nav aria-label="Company" className="min-w-0">
                        <h6 className="text-md font-semibold text-white mb-3">Company</h6>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-rose-400">About Us</a></li>
                            <li><a href="#" className="hover:text-rose-400">Careers</a></li>
                            <li><a href="#" className="hover:text-rose-400">Contact Us</a></li>
                        </ul>
                    </nav>

                    {/* For Caterers */}
                    <nav aria-label="For Caterers" className="min-w-0">
                        <h6 className="text-md font-semibold text-white mb-3">For Caterers</h6>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-rose-400">Partner with Us</a></li>
                            <li><a href="#" className="hover:text-rose-400">Caterer Login</a></li>
                        </ul>
                    </nav>

                    {/* Legal */}
                    <nav aria-label="Legal" className="min-w-0">
                        <h6 className="text-md font-semibold text-white mb-3">Legal</h6>
                        <ul className="space-y-2 text-sm">
                            <li><a href="#" className="hover:text-rose-400">Terms &amp; Conditions</a></li>
                            <li><a href="#" className="hover:text-rose-400">Privacy Policy</a></li>
                            <li><a href="#" className="hover:text-rose-400">FAQ</a></li>
                        </ul>
                    </nav>
                </div>

                <div className="border-t border-neutral-700 pt-8 flex flex-col sm:flex-row justify-between items-center text-sm min-w-0">
                    <p className="truncate">
                        &copy; {new Date().getFullYear()} Feasto. All rights reserved.
                    </p>

                    <div className="flex items-center space-x-4 mt-4 sm:mt-0">
                        {/* Replace these with real links & add rel attributes if target="_blank" */}
                        <a href="#" className="hover:text-rose-400" aria-label="Facebook">Facebook</a>
                        <a href="#" className="hover:text-rose-400" aria-label="Instagram">Instagram</a>
                        <a href="#" className="hover:text-rose-400" aria-label="Twitter">Twitter</a>
                    </div>
                </div>
            </div>
        </footer>

    );
}
