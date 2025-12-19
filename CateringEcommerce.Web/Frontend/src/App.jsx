import React, { useEffect, useRef, useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import AppHeader from './components/user/Header/AppHeader';
import AppFooter from './components/user/Footer/AppFooter';
import AuthModal from './components/user/AuthModal';



export default function App() {
    const [isAuthModalOpen, setIsAuthModalOpen] = useState(false);
    const location = useLocation();


    // Hide header/footer on the full-screen registration page
    const showHeaderFooter = location.pathname !== '/partner-registration';


    // Header measurement
    const headerRef = useRef(null);
    const [headerH, setHeaderH] = useState(0);


    useEffect(() => {
        if (!showHeaderFooter) {
            setHeaderH(0);
            return;
        }


        let raf = null;
        let pollInterval = null;
        let lastDPR = window.devicePixelRatio;


        const updateHeaderHeight = () => {
            if (headerRef.current) {
                const h = headerRef.current.offsetHeight || 0;
                setHeaderH(prev => (prev !== h ? h : prev));
            }
        };


        const debouncedUpdate = () => {
            if (raf) cancelAnimationFrame(raf);
            raf = requestAnimationFrame(() => updateHeaderHeight());
        };


        // Initial measure
        updateHeaderHeight();


        // Common triggers
        window.addEventListener('resize', debouncedUpdate);
        window.addEventListener('pageshow', debouncedUpdate);
        window.addEventListener('focus', debouncedUpdate);


        pollInterval = setInterval(() => {
            if (window.devicePixelRatio !== lastDPR) {
                lastDPR = window.devicePixelRatio;
                debouncedUpdate();
            }
        }, 350);


        let observer = null;
        try {
            observer = new MutationObserver(debouncedUpdate);
            if (headerRef.current) {
                observer.observe(headerRef.current, { attributes: true, childList: true, subtree: true });
            }
        } catch (e) {
            // ignore
        }


        return () => {
            window.removeEventListener('resize', debouncedUpdate);
            window.removeEventListener('pageshow', debouncedUpdate);
            window.removeEventListener('focus', debouncedUpdate);
            if (raf) cancelAnimationFrame(raf); if (raf) cancelAnimationFrame(raf);
            if (pollInterval) clearInterval(pollInterval);
            if (observer) observer.disconnect();
        };
    }, [showHeaderFooter]);


    return (
        <div className="min-h-screen flex flex-col">
            {showHeaderFooter ? (
                <div ref={headerRef}>
                    <AppHeader onOpenAuthModal={() => setIsAuthModalOpen(true)} />
                </div>
            ) : null}


            <main className="flex-1" style={{ paddingTop: headerH }}>
                <Outlet />
            </main>


            {showHeaderFooter ? <AppFooter /> : null}


            <AuthModal isOpen={isAuthModalOpen} onClose={() => setIsAuthModalOpen(false)} />
        </div>
    );
}