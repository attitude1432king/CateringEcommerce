import { motion, AnimatePresence } from 'framer-motion';
import { ShoppingBag, ArrowRight } from 'lucide-react';
import { useLocation } from 'react-router-dom';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../contexts/AuthContext';

export default function FloatingCartButton() {
    const { cart, toggleCart, getCartItemCount } = useCart();
    const { isAuthenticated }                    = useAuth();
    const location                               = useLocation();
    const cartCount = getCartItemCount();

    /* Hide on detail page — it has its own bottom CTA bar */
    if (location.pathname.match(/\/caterings\/\d+/)) return null;
    if (!isAuthenticated || !cart || cartCount === 0)  return null;

    return (
        <AnimatePresence>
            <motion.button
                key="floating-cart"
                onClick={toggleCart}
                initial={{ y: 80, opacity: 0 }}
                animate={{ y: 0,  opacity: 1 }}
                exit={{ y: 80, opacity: 0 }}
                whileHover={{ scale: 1.04 }}
                whileTap={{ scale: 0.96 }}
                transition={{ type: 'spring', stiffness: 380, damping: 28 }}
                className="fixed bottom-6 right-6 z-[999] flex items-center gap-3 px-5 py-3.5 rounded-full text-white font-bold shadow-cta"
                style={{ background: 'var(--gradient-catering)' }}
                aria-label={`View cart — ${cartCount} item${cartCount !== 1 ? 's' : ''}`}
            >
                <div className="relative">
                    <ShoppingBag size={20} strokeWidth={2} />
                    <span className="absolute -top-2 -right-2 min-w-[18px] h-[18px] px-1 bg-white text-primary text-[10px] font-extrabold rounded-full flex items-center justify-center border-2 border-white shadow-sm">
                        {cartCount}
                    </span>
                </div>
                <div className="flex flex-col items-start leading-none">
                    <span className="text-[11px] font-medium opacity-90">View Cart</span>
                    <span className="text-sm font-extrabold">₹{cart.totalAmount.toLocaleString()}</span>
                </div>
                <ArrowRight size={16} strokeWidth={2.5} />
            </motion.button>
        </AnimatePresence>
    );
}
