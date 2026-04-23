const GST_RATE = 0.18;

const toNumber = (value, fallback = 0) => {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : fallback;
};

const toPositiveInteger = (value, fallback = 1) => {
    const parsed = Math.trunc(Number(value));
    return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
};

export const normalizeDecorationOrNull = (decoration) => {
    if (!decoration || typeof decoration !== 'object') {
        return null;
    }

    const decorationId = decoration.decorationId ?? decoration.id ?? null;
    const name = decoration.decorationName ?? decoration.name ?? decoration.title ?? null;
    const price = toNumber(decoration.decorationPrice ?? decoration.price);

    if (decorationId == null && !name && price <= 0) {
        return null;
    }

    return {
        decorationId,
        name: name || 'Decoration',
        price,
        totalPrice: price,
    };
};

export const normalizeDecorations = (decorations) => {
    if (!Array.isArray(decorations)) {
        return [];
    }

    return decorations
        .map((decoration) => {
            if (!decoration || typeof decoration !== 'object') {
                return null;
            }

            const decorationId = decoration.decorationId ?? decoration.id ?? null;
            const name = decoration.decorationName ?? decoration.name ?? decoration.title ?? null;
            const price = toNumber(decoration.totalPrice ?? decoration.decorationPrice ?? decoration.price);

            if (decorationId == null && !name && price <= 0) {
                return null;
            }

            return {
                ...decoration,
                decorationId,
                name: name || 'Decoration',
                price,
                totalPrice: price,
            };
        })
        .filter(Boolean);
};

const normalizeAdditionalItems = (additionalItems, guestCount) => {
    if (!Array.isArray(additionalItems)) {
        return [];
    }

    return additionalItems
        .map((item) => {
            if (!item || typeof item !== 'object') {
                return null;
            }

            const price = toNumber(item.price);
            const quantity = toPositiveInteger(item.quantity, 1);
            const totalPrice = price * quantity * guestCount;

            return {
                ...item,
                foodName: item.foodName ?? item.name ?? 'Additional Item',
                quantity,
                price,
                totalPrice,
            };
        })
        .filter(Boolean);
};

export const calculateCartTotals = ({
    packagePrice = 0,
    guestCount = 50,
    additionalItems = [],
    primaryDecoration = null,
    standaloneDecorations = [],
} = {}) => {
    const normalizedGuestCount = Math.max(1, toPositiveInteger(guestCount, 50));
    const normalizedAdditionalItems = normalizeAdditionalItems(additionalItems, normalizedGuestCount);
    const normalizedPrimaryDecoration = normalizeDecorationOrNull(primaryDecoration);
    const normalizedStandaloneDecorations = normalizeDecorations(standaloneDecorations);

    const packageTotal = toNumber(packagePrice) * normalizedGuestCount;
    const additionalItemsTotal = normalizedAdditionalItems.reduce((sum, item) => sum + item.totalPrice, 0);
    const primaryDecorationTotal = normalizedPrimaryDecoration?.totalPrice ?? 0;
    const standaloneDecorationAmount = normalizedStandaloneDecorations.reduce(
        (sum, item) => sum + item.totalPrice,
        0
    );
    const decorationAmount = primaryDecorationTotal + standaloneDecorationAmount;
    const subtotal = packageTotal + additionalItemsTotal + decorationAmount;
    const taxAmount = subtotal * GST_RATE;
    const totalAmount = subtotal + taxAmount;

    return {
        guestCount: normalizedGuestCount,
        additionalItems: normalizedAdditionalItems,
        primaryDecoration: normalizedPrimaryDecoration,
        primaryDecorationTotal,
        standaloneDecorations: normalizedStandaloneDecorations,
        standaloneDecorationAmount,
        packageTotal,
        additionalItemsTotal,
        decorationAmount,
        subtotal,
        taxAmount,
        totalAmount,
    };
};
