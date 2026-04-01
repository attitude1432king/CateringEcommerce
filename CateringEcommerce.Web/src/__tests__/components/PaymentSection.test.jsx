import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import PaymentSection from '../../components/user/checkout/modern/PaymentSection';

const baseCart = { totalAmount: 10000 };

const renderPayment = (overrides = {}) => {
    const props = {
        stepNumber: 4,
        isActive: true,
        isCompleted: false,
        checkoutData: { paymentMethod: '', termsAccepted: false },
        updateCheckoutData: vi.fn(),
        errors: {},
        cart: baseCart,
        onSubmit: vi.fn(),
        isSubmitting: false,
        ...overrides,
    };
    return render(<PaymentSection {...props} />);
};

describe('PaymentSection', () => {
    it('renders all three payment options', () => {
        renderPayment();
        expect(screen.getByText('Pay Full Amount Online')).toBeInTheDocument();
        expect(screen.getByText(/Split Payment/i)).toBeInTheDocument();
        expect(screen.getByText('Cash on Delivery')).toBeInTheDocument();
    });

    it('shows split breakdown cards when split is selected', () => {
        renderPayment({
            checkoutData: { paymentMethod: 'split', termsAccepted: false },
        });
        expect(screen.getByText(/Pay Now \(40%\)/i)).toBeInTheDocument();
        expect(screen.getByText(/Before Event \(60%\)/i)).toBeInTheDocument();
    });

    it('split pre-booking amount is 40% of total (rounded)', () => {
        // 10000 * 0.40 = 4000 — shown in breakdown card AND hint text
        renderPayment({
            checkoutData: { paymentMethod: 'split', termsAccepted: false },
        });
        const expected = Math.round(10000 * 0.40).toLocaleString('en-IN');
        const matches = screen.queryAllByText(new RegExp(`₹${expected.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`));
        expect(matches.length).toBeGreaterThan(0);
    });

    it('split post-event amount is 60% of total (rounded)', () => {
        // 10000 * 0.60 = 6000 — shown in breakdown card AND hint text
        renderPayment({
            checkoutData: { paymentMethod: 'split', termsAccepted: false },
        });
        const expected = Math.round(10000 * 0.60).toLocaleString('en-IN');
        const matches = screen.queryAllByText(new RegExp(`₹${expected.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`));
        expect(matches.length).toBeGreaterThan(0);
    });

    it('COD button text is "Place Order"', () => {
        renderPayment({
            checkoutData: { paymentMethod: 'cod', termsAccepted: true },
        });
        expect(screen.getByRole('button', { name: /Place Order$/ })).toBeInTheDocument();
    });

    it('online button text is "Place Order & Pay"', () => {
        renderPayment({
            checkoutData: { paymentMethod: 'online', termsAccepted: true },
        });
        expect(screen.getByRole('button', { name: /Place Order & Pay/i })).toBeInTheDocument();
    });

    it('shows termsAccepted error when provided', () => {
        renderPayment({
            errors: { termsAccepted: 'Please accept terms and conditions' },
        });
        expect(screen.getByText(/Please accept terms/i)).toBeInTheDocument();
    });

    it('submit button is disabled when isSubmitting is true', () => {
        renderPayment({
            checkoutData: { paymentMethod: 'cod', termsAccepted: true },
            isSubmitting: true,
        });
        expect(screen.getByRole('button', { name: /Placing Order/i })).toBeDisabled();
    });

    it('selecting a method calls updateCheckoutData', () => {
        const updateCheckoutData = vi.fn();
        renderPayment({ updateCheckoutData });
        const codRadio = screen.getByDisplayValue('cod');
        fireEvent.click(codRadio);
        expect(updateCheckoutData).toHaveBeenCalledWith('paymentMethod', 'cod');
    });

    it('does not render form when not active', () => {
        renderPayment({ isActive: false, isCompleted: false });
        expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });
});
