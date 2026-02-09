/*
========================================
File: src/App.jsx (REVISED)
========================================
The main layout component. Now dynamically adjusts main content padding based on header height.
*/
import React, { useState, useRef, useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import AppHeader from './components/user/Header/AppHeader';
import AppFooter from './components/user/Footer/AppFooter';
import AuthModal from './components/user/AuthModal';
import EnhancedCartDrawer from './components/user/EnhancedCartDrawer';
import FloatingCartButton from './components/user/FloatingCartButton';



export default function App() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const location = useLocation();

    // Determine if header/footer should be shown
    const showHeaderFooter = location.pathname !== '/partner-registration' && location.pathname !== '/partner-login';

    // Header measurement logic
    const headerRef = useRef(null);
    const [headerHeight, setHeaderHeight] = useState(0);

    useEffect(() => {
        if (!showHeaderFooter) {
            setHeaderHeight(0);
            return;
        }

        const updateHeaderHeight = () => {
            if (headerRef.current) {
                setHeaderHeight(headerRef.current.offsetHeight);
            }
        };

        // Measure initially
        updateHeaderHeight();

        // Measure on resize
        window.addEventListener('resize', updateHeaderHeight);

        // Use a MutationObserver to detect changes in the header's DOM (e.g. content loading)
        const observer = new MutationObserver(updateHeaderHeight);
        if (headerRef.current) {
            observer.observe(headerRef.current, { childList: true, subtree: true, attributes: true });
        }

        return () => {
            window.removeEventListener('resize', updateHeaderHeight);
            observer.disconnect();
        };
    }, [showHeaderFooter]);

    return (
        /* 1. FLEX CONTAINER: 
           min-h-screen ensures the app is at least 100vh tall.
           flex-col stacks children vertically.
        */
        <div className="flex flex-col min-h-screen bg-gray-50 font-sans text-gray-900">

            {/* HEADER: Fixed at the top, removed from flow */}
            {showHeaderFooter && (
                <div ref={headerRef} className="fixed top-0 left-0 right-0 z-50">
                    <AppHeader onOpenAuthModal={() => setIsAuthModalOpen(true)} />
                </div>
            )}

            {showHeaderFooter}

            {/* 2. MAIN CONTENT: 
               flex-grow: Forces this element to expand and fill all available vertical space.
               This is what pushes the footer to the bottom when content is short.
               paddingTop: Prevents the fixed header from covering content.
            */}
            <main
                className="flex-grow w-full relative flex flex-col"
                style={{ paddingTop: showHeaderFooter ? `${headerHeight}px` : 0 }}
            >
                {/* Outlet renders the page content */}
                <Outlet />
            </main>

            {/* 3. FOOTER: 
               flex-shrink-0: Ensures the footer never shrinks if space is tight.
               Placed naturally in the flow after <main>.
               NO position:fixed or absolute here.
            */}
            {showHeaderFooter && (
                <div className="flex-shrink-0 w-full z-10 relative">
                    <AppFooter />
                </div>
            )}

            <AuthModal
                isOpen={isAuthModalOpen}
                onClose={() => setIsAuthModalOpen(false)}
            />

            <EnhancedCartDrawer />

            {/* Floating Cart Button - Only visible when cart has items */}
            <FloatingCartButton />
        </div>
    );
}