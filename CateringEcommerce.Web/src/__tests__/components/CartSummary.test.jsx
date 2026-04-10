import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import CartSummary from '../../components/user/checkout/modern/CartSummary';

// CartSummary is a pure presentational component — no context needed.

const baseCart = {
    cateringName: 'Test Caterers',
    packageName: 'Premium Package',
    packagePrice: 500,
    guestCount: 100,
    taxAmount: 9000,
    totalAmount: 59000,
    decorations: [],
    additionalItems: [],
};

const baseCheckout = { guestCount: 100 };

describe('CartSummary', () => {
    it('renders the catering business name', () => {
        render(<CartSummary cart={baseCart} checkoutData={baseCheckout} />);
        expect(screen.getByText('Test Caterers')).toBeInTheDocument();
    });

    it('recalculates GST and total from the live guest count', () => {
        render(<CartSummary cart={baseCart} checkoutData={{ guestCount: 50 }} />);
        expect(screen.getByText(/₹25,000\.00/)).toBeInTheDocument();
        expect(screen.getByText(/₹4,500\.00/)).toBeInTheDocument();
        expect(screen.getByText(/₹29,500\.00/)).toBeInTheDocument();
    });

    it('avoids NaN for additional items with mixed field shapes', () => {
        const cart = {
            ...baseCart,
            guestCount: 50,
            additionalItems: [
                { name: 'Kachumber Salad', price: '25', quantity: 1 },
                { foodName: 'Lemon Coriander Soup', price: 40, quantity: '2' },
            ],
        };

        render(<CartSummary cart={cart} checkoutData={{ guestCount: 50 }} />);
        expect(screen.getByText(/Kachumber Salad/)).toBeInTheDocument();
        expect(screen.getByText(/Lemon Coriander Soup/)).toBeInTheDocument();
        expect(screen.queryByText(/NaN/)).not.toBeInTheDocument();
    });

    it('shows standalone decorations separately from the package decoration', () => {
        const cart = {
            ...baseCart,
            decorationId: 10,
            decorationName: 'Package Floral Theme',
            decorationPrice: 5000,
            standaloneDecorations: [
                { decorationId: 1, name: 'Floral Theme', price: 5000 },
                { decorationId: 2, name: 'Stage Lighting', price: 2500 },
            ],
        };

        render(<CartSummary cart={cart} checkoutData={baseCheckout} />);
        expect(screen.getByText('Package Floral Theme')).toBeInTheDocument();
        expect(screen.getByText('Additional Decorations')).toBeInTheDocument();
        expect(screen.getByText('Stage Lighting')).toBeInTheDocument();
    });

    it.skip('uses cart.taxAmount for GST row (not hardcoded 18%)', () => {
        // taxAmount = 1234; 18% of subtotal would be 9000 (packagePrice 500 × 100 guests × 0.18)
        const cart = { ...baseCart, taxAmount: 1234, totalAmount: 51234 };
        render(<CartSummary cart={cart} checkoutData={baseCheckout} />);
        const formatted = Number(1234).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        // Use queryAllByText to handle multiple matches
        const matches = screen.queryAllByText(new RegExp(formatted.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')));
        expect(matches.length).toBeGreaterThan(0);
    });

    it.skip('displays cart.totalAmount in total row', () => {
        render(<CartSummary cart={baseCart} checkoutData={baseCheckout} />);
        // ₹59,000.00
        expect(screen.getByText(/59,000/)).toBeInTheDocument();
    });

    it('hides rating badge when catererRating and rating are both null', () => {
        const cart = { ...baseCart, catererRating: null, rating: null };
        render(<CartSummary cart={cart} checkoutData={baseCheckout} />);
        expect(screen.queryByText(/⭐/)).not.toBeInTheDocument();
    });

    it('shows rating badge when catererRating is provided', () => {
        const cart = { ...baseCart, catererRating: 4.7 };
        render(<CartSummary cart={cart} checkoutData={baseCheckout} />);
        expect(screen.getByText(/4\.7/)).toBeInTheDocument();
    });

    it('renders package total as price × guestCount', () => {
        // 500 × 100 = 50,000 — shown in the package row
        render(<CartSummary cart={baseCart} checkoutData={baseCheckout} />);
        const formatted = (500 * 100).toLocaleString('en-IN');
        const matches = screen.queryAllByText(new RegExp(`₹${formatted.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`));
        expect(matches.length).toBeGreaterThan(0);
    });

    it.skip('renders additional item names', () => {
        const cart = {
            ...baseCart,
            additionalItems: [
                { foodName: 'Biryani', price: 150, quantity: 1 },
                { foodName: 'Gulab Jamun', price: 50, quantity: 1 },
            ],
        };
        render(<CartSummary cart={cart} checkoutData={baseCheckout} />);
        expect(screen.getByText(/Biryani/)).toBeInTheDocument();
        expect(screen.getByText(/Gulab Jamun/)).toBeInTheDocument();
    });

    it('shows Place Order button when canPlaceOrder is true', () => {
        render(
            <CartSummary
                cart={baseCart}
                checkoutData={baseCheckout}
                canPlaceOrder={true}
                onPlaceOrder={vi.fn()}
            />
        );
        expect(screen.getByRole('button', { name: /Place Order/i })).toBeInTheDocument();
    });

    it('hides Place Order button when canPlaceOrder is false', () => {
        render(
            <CartSummary
                cart={baseCart}
                checkoutData={baseCheckout}
                canPlaceOrder={false}
            />
        );
        expect(screen.queryByRole('button', { name: /Place Order/i })).not.toBeInTheDocument();
    });

    it('returns null when cart is not provided', () => {
        const { container } = render(
            <CartSummary cart={null} checkoutData={baseCheckout} />
        );
        expect(container.firstChild).toBeNull();
    });
});
