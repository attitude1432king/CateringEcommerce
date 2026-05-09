import { useState, useEffect, useRef } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { ShoppingCart, Zap, Menu, X, ChevronDown, LogOut, User, Package } from 'lucide-react';
import { useAuth } from '../../../contexts/AuthContext';
import { useCart } from '../../../contexts/CartContext';
import UserNotifications from '../UserNotifications';
import IconButton from '../../../design-system/components/IconButton';
import Button from '../../../design-system/components/Button';

const generateInitialsAvatar = (name) => {
    if (!name) return 'https://placehold.co/64x64/FF6B35/FFFFFF?text=U';
    const initials = name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64"><circle cx="32" cy="32" r="32" fill="#FF6B35"/><text x="50%" y="50%" dominant-baseline="central" text-anchor="middle" font-family="sans-serif" font-size="28" fill="white">${initials}</text></svg>`;
    return `data:image/svg+xml;base64,${btoa(svg)}`;
};

const navLinks = [
    { label: 'Browse Caterers', to: '/caterings' },
    { label: 'Events',          to: '/events' },
    { label: 'Corporate',       to: '/corporate' },
    { label: 'How it Works',    href: '/#how-it-works' },
];

export default function AppHeader({ onOpenAuthModal }) {
    const { isAuthenticated, user, logout } = useAuth();
    const { getCartItemCount, toggleCart } = useCart();
    const dropdownRef = useRef(null);
    const [isDropdownOpen, setIsDropdownOpen]     = useState(false);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [scrolled, setScrolled]                 = useState(false);
    const navigate  = useNavigate();
    const location  = useLocation();
    const cartCount = getCartItemCount();
    const userAvatar = isAuthenticated ? (user?.profilePhoto || generateInitialsAvatar(user?.name)) : null;

    /* Close dropdown when clicking outside */
    useEffect(() => {
        const handler = (e) => {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
                setIsDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handler);
        return () => document.removeEventListener('mousedown', handler);
    }, []);

    /* Shadow intensifies on scroll */
    useEffect(() => {
        const onScroll = () => setScrolled(window.scrollY > 10);
        window.addEventListener('scroll', onScroll, { passive: true });
        return () => window.removeEventListener('scroll', onScroll);
    }, []);

    /* Close mobile menu on route change */
    useEffect(() => { setIsMobileMenuOpen(false); }, [location.pathname]);

    const handleLogout = () => {
        logout();
        navigate('/');
        setIsDropdownOpen(false);
    };

    return (
        <header
            className={`cust-header transition-shadow duration-200 ${scrolled ? 'shadow-md' : ''}`}
            role="banner"
        >
            <div className="cust-header__inner">
                {/* Logo */}
                <Link to="/" className="cust-logo shrink-0" aria-label="Enyvora home">
                    <img src="/logo.svg" alt="ENYVORA" />
                </Link>

                {/* Desktop nav */}
                <nav className="cust-nav hidden md:flex" aria-label="Main navigation">
                    {navLinks.map(({ label, to, href }) =>
                        to ? (
                            <Link
                                key={label}
                                to={to}
                                className={`cust-nav__a ${location.pathname === to ? 'is-active' : ''}`}
                            >
                                {label}
                            </Link>
                        ) : (
                            <a key={label} href={href} className="cust-nav__a">{label}</a>
                        )
                    )}
                </nav>

                {/* Actions */}
                <div className="cust-header__actions">
                    {/* Notification bell */}
                    {isAuthenticated && <UserNotifications />}

                    {/* Cart */}
                    {isAuthenticated && (
                        <IconButton
                            aria-label={`Shopping cart, ${cartCount} item${cartCount !== 1 ? 's' : ''}`}
                            badge={cartCount}
                            onClick={toggleCart}
                        >
                            <ShoppingCart size={18} strokeWidth={2} />
                        </IconButton>
                    )}

                    {/* Partner link — desktop only when logged out */}
                    {!isAuthenticated && (
                        <Link
                            to="/partner-login"
                            className="hidden lg:inline-flex cust-nav__a px-4 py-2 rounded-lg hover:bg-neutral-50 transition-colors"
                        >
                            Partner With Us
                        </Link>
                    )}

                    {/* Primary CTA */}
                    <button
                        type="button"
                        className="cust-header__cta hidden sm:inline-flex items-center gap-2"
                        onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
                        style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)', color: '#fff' }}
                    >
                        <Zap size={14} strokeWidth={2.5} />
                        <span className="hidden md:inline">Get Quotes</span>
                    </button>

                    {/* User account / Sign in */}
                    {isAuthenticated ? (
                        <div className="relative" ref={dropdownRef}>
                            <button
                                onClick={() => setIsDropdownOpen(v => !v)}
                                aria-haspopup="true"
                                aria-expanded={isDropdownOpen}
                                aria-label="User menu"
                                className="flex items-center gap-2 focus:outline-none group"
                            >
                                <img
                                    src={userAvatar}
                                    alt={user?.name ?? 'User'}
                                    className="h-9 w-9 rounded-full object-cover ring-2 ring-offset-1 ring-primary group-hover:ring-primary-dark transition-all"
                                />
                                <ChevronDown
                                    size={14}
                                    className={`text-neutral-500 hidden md:block transition-transform duration-200 ${isDropdownOpen ? 'rotate-180' : ''}`}
                                />
                            </button>

                            {isDropdownOpen && (
                                <div className="absolute right-0 mt-2 w-52 bg-white rounded-2xl shadow-card-hover py-2 z-50 border border-neutral-100 animate-fadeIn">
                                    <div className="px-4 py-3 border-b border-neutral-100">
                                        <p className="font-semibold text-neutral-900 text-sm truncate">{user?.name || 'User'}</p>
                                        <p className="text-xs text-neutral-400 truncate">{user?.email}</p>
                                    </div>
                                    <Link
                                        to="/profile"
                                        onClick={() => setIsDropdownOpen(false)}
                                        className="flex items-center gap-3 px-4 py-2.5 text-sm text-neutral-700 hover:bg-neutral-50 transition-colors"
                                    >
                                        <User size={15} className="text-neutral-400" /> My Profile
                                    </Link>
                                    <Link
                                        to="/my-orders"
                                        onClick={() => setIsDropdownOpen(false)}
                                        className="flex items-center gap-3 px-4 py-2.5 text-sm text-neutral-700 hover:bg-neutral-50 transition-colors"
                                    >
                                        <Package size={15} className="text-neutral-400" /> My Orders
                                    </Link>
                                    <button
                                        onClick={handleLogout}
                                        className="flex items-center gap-3 w-full px-4 py-2.5 text-sm text-neutral-700 hover:bg-neutral-50 transition-colors border-t border-neutral-100"
                                    >
                                        <LogOut size={15} className="text-neutral-400" /> Logout
                                    </button>
                                </div>
                            )}
                        </div>
                    ) : (
                        <Button
                            variant="luxury"
                            size="sm"
                            onClick={onOpenAuthModal}
                            className="hidden sm:inline-flex"
                        >
                            Sign In
                        </Button>
                    )}

                    {/* Mobile hamburger */}
                    <button
                        type="button"
                        className="md:hidden icon-btn"
                        onClick={() => setIsMobileMenuOpen(v => !v)}
                        aria-label={isMobileMenuOpen ? 'Close menu' : 'Open menu'}
                        aria-expanded={isMobileMenuOpen}
                    >
                        {isMobileMenuOpen
                            ? <X size={18} strokeWidth={2} />
                            : <Menu size={18} strokeWidth={2} />
                        }
                    </button>
                </div>
            </div>

            {/* Mobile slide-down sheet */}
            {isMobileMenuOpen && (
                <div className="md:hidden border-t border-black/5 bg-white/98 animate-fadeInUp">
                    <div className="max-w-screen-xl mx-auto px-6 py-4 space-y-1">
                        {navLinks.map(({ label, to, href }) =>
                            to ? (
                                <Link
                                    key={label}
                                    to={to}
                                    className={`block px-4 py-3 rounded-xl text-sm font-medium transition-colors ${
                                        location.pathname === to
                                            ? 'bg-primary/10 text-primary'
                                            : 'text-neutral-700 hover:bg-neutral-50'
                                    }`}
                                >
                                    {label}
                                </Link>
                            ) : (
                                <a
                                    key={label}
                                    href={href}
                                    className="block px-4 py-3 rounded-xl text-sm font-medium text-neutral-700 hover:bg-neutral-50 transition-colors"
                                >
                                    {label}
                                </a>
                            )
                        )}
                        {!isAuthenticated && (
                            <>
                                <Link to="/partner-login" className="block px-4 py-3 rounded-xl text-sm font-medium text-neutral-700 hover:bg-neutral-50 transition-colors">
                                    Become a Partner
                                </Link>
                                <div className="pt-2 pb-1">
                                    <Button variant="luxury" size="md" className="w-full" onClick={onOpenAuthModal}>
                                        Sign In
                                    </Button>
                                </div>
                            </>
                        )}
                    </div>
                </div>
            )}
        </header>
    );
}
