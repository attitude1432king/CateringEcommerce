/*
========================================
File: src/App.jsx (UPDATED)
========================================
This is now a layout component that renders child routes.
*/
import React, { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import AppHeader from './components/user/Header/AppHeader';
import AppFooter from './components/user/Footer/AppFooter';
import AuthModal from './components/user/AuthModal';

export default function App() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const location = useLocation();

    // Hide header/footer on the full-screen registration page
    const showHeaderFooter = location.pathname !== '/partner-registration';

    return (
        <div>
            {showHeaderFooter && <AppHeader onOpenAuthModal={() => setIsAuthModalOpen(true)} />}

            <main>
                <Outlet /> {/* Child routes like HomePage, ProfilePage, etc. will render here */}
            </main>

            {showHeaderFooter && <AppFooter />}
            <AuthModal
                isOpen={isAuthModalOpen}
                onClose={() => setIsAuthModalOpen(false)}
            />
        </div>
    );
}