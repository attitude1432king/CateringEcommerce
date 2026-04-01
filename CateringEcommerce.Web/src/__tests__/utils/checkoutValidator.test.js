import { describe, it, expect } from 'vitest';
import {
    validateCheckoutData,
    validateAddressContact,
    validateFileUpload,
    formatEventDate,
    formatEventTime,
    getEventTypeDisplay,
} from '../../utils/checkoutValidator';

// ── Helpers ─────────────────────────────────────────────────────────────────

const futureDate = (daysAhead) => {
    const d = new Date();
    d.setDate(d.getDate() + daysAhead);
    const year = d.getFullYear();
    const month = `${d.getMonth() + 1}`.padStart(2, '0');
    const day = `${d.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const makeFile = (type = 'image/jpeg', sizeMB = 1) =>
    new File(['x'.repeat(sizeMB * 1024 * 1024)], 'test.jpg', { type });

// ── validateCheckoutData — Step 1 (Account) ─────────────────────────────────

describe('validateCheckoutData — step 1 (account)', () => {
    it('authenticated user always passes step 1', () => {
        const { isValid, errors } = validateCheckoutData({ isGuest: false }, null, 1);
        expect(isValid).toBe(true);
        expect(errors).toEqual({});
    });

    it('guest with missing email returns error', () => {
        const { isValid, errors } = validateCheckoutData(
            { isGuest: true, guestEmail: '', guestPhone: '9876543210' },
            null, 1
        );
        expect(isValid).toBe(false);
        expect(errors.guestEmail).toBeTruthy();
    });

    it('guest with invalid email returns error', () => {
        const { isValid, errors } = validateCheckoutData(
            { isGuest: true, guestEmail: 'notanemail', guestPhone: '9876543210' },
            null, 1
        );
        expect(isValid).toBe(false);
        expect(errors.guestEmail).toBeTruthy();
    });

    it('guest with invalid phone (7 digits) returns error', () => {
        const { isValid, errors } = validateCheckoutData(
            { isGuest: true, guestEmail: 'a@b.com', guestPhone: '1234567' },
            null, 1
        );
        expect(isValid).toBe(false);
        expect(errors.guestPhone).toBeTruthy();
    });

    it('valid guest data passes step 1', () => {
        const { isValid } = validateCheckoutData(
            { isGuest: true, guestEmail: 'a@b.com', guestPhone: '9876543210' },
            null, 1
        );
        expect(isValid).toBe(true);
    });
});

// ── validateCheckoutData — Step 2 (Event Details) ───────────────────────────

const validStep2Data = () => ({
    eventType: 'wedding',
    eventDate: futureDate(5),
    eventTime: '18:00',
    guestCount: 50,
    eventAddress: { street: '123 Test St', city: 'Mumbai', state: 'MH', pincode: '400001' },
});

describe('validateCheckoutData — step 2 (event details)', () => {
    it('missing eventType returns error', () => {
        const data = { ...validStep2Data(), eventType: '' };
        const { isValid, errors } = validateCheckoutData(data, null, 2);
        expect(isValid).toBe(false);
        expect(errors.eventType).toBeTruthy();
    });

    it('past event date returns error', () => {
        const data = { ...validStep2Data(), eventDate: futureDate(-2) };
        const { isValid, errors } = validateCheckoutData(data, null, 2);
        expect(isValid).toBe(false);
        expect(errors.eventDate).toBeTruthy();
    });

    it('event date too soon (< 5 days) returns error', () => {
        const data = { ...validStep2Data(), eventDate: futureDate(1) };
        const { isValid, errors } = validateCheckoutData(data, null, 2);
        expect(isValid).toBe(false);
        expect(errors.eventDate).toMatch(/5 days/i);
    });

    it('missing pincode returns error', () => {
        const data = { ...validStep2Data(), eventAddress: { ...validStep2Data().eventAddress, pincode: '' } };
        const { isValid, errors } = validateCheckoutData(data, null, 2);
        expect(isValid).toBe(false);
        expect(errors.eventAddressPincode).toBeTruthy();
    });

    it('invalid pincode (not 6 digits) returns error', () => {
        const data = { ...validStep2Data(), eventAddress: { ...validStep2Data().eventAddress, pincode: '123' } };
        const { isValid, errors } = validateCheckoutData(data, null, 2);
        expect(isValid).toBe(false);
        expect(errors.eventAddressPincode).toBeTruthy();
    });

    it('guest count of 0 returns error', () => {
        const data = { ...validStep2Data(), guestCount: 0 };
        const { isValid, errors } = validateCheckoutData(data, null, 2);
        expect(isValid).toBe(false);
        expect(errors.guestCount).toBeTruthy();
    });

    it('valid data passes step 2', () => {
        const { isValid } = validateCheckoutData(validStep2Data(), null, 2);
        expect(isValid).toBe(true);
    });
});

// ── validateCheckoutData — Step 3 (Delivery Type) ───────────────────────────

describe('validateCheckoutData — step 3 (delivery type)', () => {
    it('missing deliveryType returns error', () => {
        const { isValid, errors } = validateCheckoutData({ deliveryType: '' }, null, 3);
        expect(isValid).toBe(false);
        expect(errors.deliveryType).toBeTruthy();
    });

    it('valid deliveryType passes step 3', () => {
        const { isValid } = validateCheckoutData({ deliveryType: 'event' }, null, 3);
        expect(isValid).toBe(true);
    });
});

// ── validateCheckoutData — Step 4 (Payment) ─────────────────────────────────

describe('validateCheckoutData — step 4 (payment)', () => {
    it('missing paymentMethod returns error', () => {
        const { isValid, errors } = validateCheckoutData(
            { paymentMethod: '', termsAccepted: true }, null, 4
        );
        expect(isValid).toBe(false);
        expect(errors.paymentMethod).toBeTruthy();
    });

    it('terms not accepted returns error', () => {
        const { isValid, errors } = validateCheckoutData(
            { paymentMethod: 'cod', termsAccepted: false }, null, 4
        );
        expect(isValid).toBe(false);
        expect(errors.termsAccepted).toBeTruthy();
    });

    it('valid COD with terms accepted passes step 4', () => {
        const { isValid } = validateCheckoutData(
            { paymentMethod: 'cod', termsAccepted: true }, null, 4
        );
        expect(isValid).toBe(true);
    });
});

// ── validateAddressContact ───────────────────────────────────────────────────

describe('validateAddressContact', () => {
    const validAddress = () => ({
        deliveryAddress: '123 Test Street, Mumbai',
        contactPerson: 'Test User',
        contactPhone: '9876543210',
        contactEmail: 'test@example.com',
    });

    it('missing deliveryAddress returns error', () => {
        const { isValid, errors } = validateAddressContact({ ...validAddress(), deliveryAddress: '' });
        expect(isValid).toBe(false);
        expect(errors.deliveryAddress).toBeTruthy();
    });

    it('invalid phone (7 digits) returns error', () => {
        const { isValid, errors } = validateAddressContact({ ...validAddress(), contactPhone: '1234567' });
        expect(isValid).toBe(false);
        expect(errors.contactPhone).toBeTruthy();
    });

    it('invalid email (no @) returns error', () => {
        const { isValid, errors } = validateAddressContact({ ...validAddress(), contactEmail: 'notanemail' });
        expect(isValid).toBe(false);
        expect(errors.contactEmail).toBeTruthy();
    });

    it('valid data returns isValid=true with empty errors', () => {
        const result = validateAddressContact(validAddress());
        expect(result.isValid).toBe(true);
        expect(result.errors).toEqual({});
    });
});

// ── validateFileUpload ───────────────────────────────────────────────────────

describe('validateFileUpload', () => {
    it('null file returns error', () => {
        const { isValid, errors } = validateFileUpload(null);
        expect(isValid).toBe(false);
        expect(errors.file).toBeTruthy();
    });

    it('file exceeding 5MB returns error', () => {
        const { isValid, errors } = validateFileUpload(makeFile('image/jpeg', 6));
        expect(isValid).toBe(false);
        expect(errors.file).toMatch(/5MB/i);
    });

    it('disallowed MIME type (text/plain) returns error', () => {
        const { isValid, errors } = validateFileUpload(makeFile('text/plain', 1));
        expect(isValid).toBe(false);
        expect(errors.file).toBeTruthy();
    });

    it('valid JPEG under 5MB returns isValid=true', () => {
        const { isValid } = validateFileUpload(makeFile('image/jpeg', 1));
        expect(isValid).toBe(true);
    });

    it('valid PDF under 5MB returns isValid=true', () => {
        const { isValid } = validateFileUpload(makeFile('application/pdf', 2));
        expect(isValid).toBe(true);
    });
});

// ── Display helpers ──────────────────────────────────────────────────────────

describe('formatEventDate', () => {
    it('returns empty string for falsy input', () => {
        expect(formatEventDate('')).toBe('');
        expect(formatEventDate(null)).toBe('');
    });

    it('returns a non-empty formatted string for a valid date', () => {
        const result = formatEventDate('2026-06-15');
        expect(result).toBeTruthy();
        expect(result).toContain('2026');
    });
});

describe('formatEventTime', () => {
    it('returns empty string for falsy input', () => {
        expect(formatEventTime('')).toBe('');
    });

    it('returns a non-empty string for a valid time', () => {
        const result = formatEventTime('14:00');
        expect(result).toBeTruthy();
    });
});

describe('getEventTypeDisplay', () => {
    it('maps known event type codes to labels', () => {
        expect(getEventTypeDisplay('wedding')).toBe('Wedding');
        expect(getEventTypeDisplay('birthday')).toBe('Birthday Party');
        expect(getEventTypeDisplay('corporate')).toBe('Corporate Event');
    });

    it('returns the raw value for unknown event types', () => {
        expect(getEventTypeDisplay('custom_event')).toBe('custom_event');
    });
});
