/*
========================================
File: src/pages/PartnerLoginPage.jsx (NEW FILE)
========================================
*/
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import AuthModal from '../components/user/AuthModal'; // We will reuse the AuthModal

export default function PartnerLoginPage() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const navigate = useNavigate();

    return (
        <>
            <AuthModal
                isOpen={isAuthModalOpen}
                onClose={() => setIsAuthModalOpen(false)}
                isPartnerLogin={true} // Prop to tell the modal it's for partners
            />
            <div className="min-h-screen bg-neutral-50 flex flex-col">
                <header className="p-4">
                    <h1 className="text-2xl font-bold text-rose-600">Feasto Partners</h1>
                </header>
                <main className="flex-1 flex flex-col items-center justify-center text-center p-4">
                    <img
                        src="https://img.freepik.com/free-vector/chef-cooking-kitchen-restaurant-cartoon-art-illustration_56104-649.jpg"
                        alt="Chef Cooking"
                        className="w-64 h-64 object-contain mb-6"
                    />
                    <h2 className="text-3xl font-bold text-neutral-800">Feasto Partner Dashboard</h2>
                    <p className="text-neutral-500 mt-2 mb-8">Manage your orders, menu, and earnings all in one place.</p>

                    <div className="space-y-3 w-full max-w-xs">
                        <button
                            onClick={() => setIsAuthModalOpen(true)}
                            className="w-full bg-rose-600 text-white py-3 rounded-md font-semibold hover:bg-rose-700 transition-colors"
                        >
                            Login
                        </button>
                        <button
                            onClick={() => navigate('/partner-registration')}
                            className="w-full bg-white text-rose-600 py-3 rounded-md font-semibold border border-rose-600 hover:bg-rose-50 transition-colors"
                        >
                            Register
                        </button>
                    </div>

                    <p className="text-xs text-neutral-400 mt-12">
                        By continuing, you agree to our Terms of Service and Privacy Policy.
                    </p>
                </main>
            </div>
        </>
    );
}