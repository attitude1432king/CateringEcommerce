/**
 * EnhancedCartDrawer — Enyvora design upgrade.
 * All business logic (auth guard, scroll-lock, close-on-escape, checkout nav) is preserved.
 * CartDrawer.jsx (legacy) is no longer used; this is the only cart drawer.
 */
import { useEffect, useState } from 'react';
import { useNavigate }         from 'react-router-dom';
import { X, ShoppingBag, CheckCircle, Trash2, ArrowRight, Info } from 'lucide-react';
import { useCart }       from '../../contexts/CartContext';
import { useAuthGuard }  from '../../hooks/useAuthGuard';
import AuthModal         from './AuthModal';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

export default function EnhancedCartDrawer() {
    const { cart, clearCart, isCartOpen, setIsCartOpen } = useCart();
    const { requireAuth, showAuthModal, handleAuthClose, isAuthenticated } = useAuthGuard();
    const navigate = useNavigate();
    const [isClosing,        setIsClosing]        = useState(false);
    const [showClearConfirm, setShowClearConfirm] = useState(false);

    /* Animated close */
    const handleClose = () => {
        setIsClosing(true);
        setTimeout(() => { setIsCartOpen(false); setIsClosing(false); }, 300);
    };

    const handleClearCartConfirm = () => {
        clearCart();
        setShowClearConfirm(false);
        handleClose();
    };

    const handleProceedToCheckout = () => {
        handleClose();
        requireAuth(() => navigate('/checkout'), '/checkout');
    };

    /* Close on Escape */
    useEffect(() => {
        const onKey = (e) => { if (e.key === 'Escape' && isCartOpen) handleClose(); };
        window.addEventListener('keydown', onKey);
        return () => window.removeEventListener('keydown', onKey);
    }, [isCartOpen]);

    /* Scroll-lock */
    useEffect(() => {
        if (!isCartOpen && !showClearConfirm) return;
        const scrollY = window.scrollY;
        const prev = {
            bodyOverflow: document.body.style.overflow,
            bodyPosition: document.body.style.position,
            bodyTop:      document.body.style.top,
            bodyWidth:    document.body.style.width,
            htmlOverflow: document.documentElement.style.overflow,
        };
        document.body.style.overflow  = 'hidden';
        document.body.style.position  = 'fixed';
        document.body.style.top       = `-${scrollY}px`;
        document.body.style.width     = '100%';
        document.documentElement.style.overflow = 'hidden';
        return () => {
            document.body.style.overflow  = prev.bodyOverflow;
            document.body.style.position  = prev.bodyPosition;
            document.body.style.top       = prev.bodyTop;
            document.body.style.width     = prev.bodyWidth;
            document.documentElement.style.overflow = prev.htmlOverflow;
            window.scrollTo(0, scrollY);
        };
    }, [isCartOpen, showClearConfirm]);

    if (!isCartOpen || !cart) return null;

    const {
        cateringName,
        cateringLogo,
        packageName,
        guestCount     = 50,
        additionalItems = [],
        baseAmount     = 0,
        taxAmount      = 0,
        totalAmount    = 0,
    } = cart;

    const slideClass = isClosing ? 'translate-x-full' : 'translate-x-0';

    return (
        <>
            {/* Backdrop */}
            <div
                className={`fixed inset-0 bg-black/60 backdrop-blur-sm z-[9998] transition-opacity duration-300 ${isClosing ? 'opacity-0' : 'opacity-100'}`}
                onClick={handleClose}
            />

            {/* Drawer */}
            <div
                role="dialog"
                aria-modal="true"
                aria-label="Your Cart"
                className={`fixed right-0 top-0 h-full w-full sm:w-[460px] bg-white shadow-2xl z-[9999] flex flex-col transition-transform duration-300 ease-out ${slideClass}`}
            >
                {/* ── Header ── */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-neutral-100">
                    <div className="flex items-center gap-3">
                        <div className="w-9 h-9 rounded-xl flex items-center justify-center" style={{ background: 'var(--gradient-catering)' }}>
                            <ShoppingBag size={17} className="text-white" strokeWidth={2} />
                        </div>
                        <div>
                            <h2 className="font-bold text-neutral-900 text-lg leading-none">Your Cart</h2>
                            <p className="text-xs text-neutral-400 mt-0.5">
                                {isAuthenticated ? 'Review your selection' : 'Sign in to checkout'}
                            </p>
                        </div>
                    </div>
                    <button
                        onClick={handleClose}
                        className="icon-btn"
                        aria-label="Close cart"
                    >
                        <X size={18} strokeWidth={2} />
                    </button>
                </div>

                {/* ── Scrollable body ── */}
                <div className="flex-1 overflow-y-auto px-6 py-5 space-y-5">
                    {/* Caterer info */}
                    <div className="flex items-center gap-4 p-4 rounded-2xl bg-neutral-50 border border-neutral-100">
                        <div className="w-14 h-14 rounded-xl overflow-hidden bg-white shadow-input shrink-0">
                            {cateringLogo ? (
                                <img src={`${API_BASE_URL}${cateringLogo}`} alt={cateringName} className="w-full h-full object-cover" />
                            ) : (
                                <div className="w-full h-full flex items-center justify-center text-white text-xl font-bold" style={{ background: 'var(--gradient-catering)' }}>
                                    {cateringName?.charAt(0)}
                                </div>
                            )}
                        </div>
                        <div>
                            <p className="font-bold text-neutral-900">{cateringName}</p>
                            <span className="inline-flex items-center gap-1 text-xs text-success font-semibold mt-1">
                                <CheckCircle size={12} strokeWidth={2.5} /> Verified Caterer
                            </span>
                        </div>
                    </div>

                    {/* Package */}
                    <section>
                        <h4 className="text-xs font-bold text-neutral-500 uppercase tracking-wider mb-2 flex items-center gap-2">
                            <span className="w-1.5 h-4 rounded-full" style={{ background: 'var(--color-primary)' }} />
                            Package Details
                        </h4>
                        <div className="cart-row">
                            <span />
                            <div>
                                <p className="cart-row__t">{packageName}</p>
                                <p className="cart-row__d">for {guestCount} guests</p>
                            </div>
                            <span className="cart-row__p">₹{baseAmount.toLocaleString()}</span>
                            <span />
                        </div>
                    </section>

                    {/* Add-ons */}
                    {additionalItems.length > 0 && (
                        <section>
                            <h4 className="text-xs font-bold text-neutral-500 uppercase tracking-wider mb-2 flex items-center gap-2">
                                <span className="w-1.5 h-4 rounded-full bg-warning" />
                                Add-ons
                            </h4>
                            {additionalItems.map((item, i) => (
                                <div key={i} className="cart-row">
                                    <span />
                                    <div>
                                        <p className="cart-row__t">{item.name}</p>
                                        <p className="cart-row__d">Qty {item.quantity} × {guestCount} guests</p>
                                    </div>
                                    <p className="cart-row__p">₹{(item.price * item.quantity * guestCount).toLocaleString()}</p>
                                    <span />
                                </div>
                            ))}
                        </section>
                    )}

                    {/* Bill summary */}
                    <div className="quote__est">
                        <div className="row"><span>Subtotal</span><span>₹{(totalAmount - taxAmount).toLocaleString()}</span></div>
                        <div className="row">
                            <span className="flex items-center gap-1">GST (18%) <Info size={12} className="text-neutral-400" /></span>
                            <span>₹{taxAmount.toLocaleString()}</span>
                        </div>
                        <div className="row total"><span>Total Amount</span><span>₹{totalAmount.toLocaleString()}</span></div>
                    </div>

                    {/* Auth notice */}
                    {!isAuthenticated && (
                        <div className="flex items-start gap-3 p-4 rounded-xl bg-info-bg border border-info/20 text-sm">
                            <Info size={16} className="text-info mt-0.5 shrink-0" />
                            <div>
                                <p className="font-semibold text-blue-900">Sign in Required</p>
                                <p className="text-blue-700 text-xs mt-0.5">You'll be asked to sign in before checkout</p>
                            </div>
                        </div>
                    )}
                </div>

                {/* ── Footer ── */}
                <div className="border-t border-neutral-100 bg-white px-6 py-4 space-y-3 shadow-[0_-8px_20px_rgba(0,0,0,0.06)]">
                    <div className="flex items-end justify-between">
                        <p className="text-xs text-neutral-500">Total Amount</p>
                        <p className="text-2xl font-extrabold text-neutral-900">₹{totalAmount.toLocaleString()}</p>
                    </div>
                    <button
                        onClick={handleProceedToCheckout}
                        className="w-full flex items-center justify-center gap-2 py-3.5 rounded-xl text-white font-bold text-base transition-all duration-200 hover:scale-[1.02] active:scale-[0.98]"
                        style={{ background: 'var(--gradient-catering)', boxShadow: 'var(--shadow-cta)' }}
                    >
                        Proceed to Checkout <ArrowRight size={18} strokeWidth={2.5} />
                    </button>
                    <button
                        onClick={() => setShowClearConfirm(true)}
                        className="w-full flex items-center justify-center gap-2 py-2 text-sm font-medium text-danger hover:text-red-700 transition-colors"
                    >
                        <Trash2 size={14} /> Clear Cart
                    </button>
                </div>
            </div>

            {/* Auth modal */}
            {showAuthModal && (
                <AuthModal isOpen={showAuthModal} onClose={handleAuthClose} isPartnerLogin={false} />
            )}

            {/* Clear-cart confirm */}
            {showClearConfirm && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[10001] flex items-center justify-center p-4">
                    <div className="bg-white rounded-2xl shadow-2xl max-w-sm w-full p-6 animate-fadeIn" onClick={e => e.stopPropagation()}>
                        <div className="text-center mb-5">
                            <div className="mx-auto w-12 h-12 rounded-full bg-danger-bg flex items-center justify-center mb-3">
                                <Trash2 size={22} className="text-danger" />
                            </div>
                            <h3 className="font-bold text-neutral-900 text-lg mb-1">Clear Cart?</h3>
                            <p className="text-sm text-neutral-500">All items will be removed. This cannot be undone.</p>
                        </div>
                        <div className="flex gap-3">
                            <button onClick={() => setShowClearConfirm(false)} className="flex-1 py-2.5 rounded-xl bg-neutral-100 text-neutral-700 font-semibold hover:bg-neutral-200 transition-colors text-sm">
                                Cancel
                            </button>
                            <button onClick={handleClearCartConfirm} className="flex-1 py-2.5 rounded-xl bg-danger text-white font-semibold hover:bg-red-700 transition-colors text-sm">
                                Clear Cart
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}
