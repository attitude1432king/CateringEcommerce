// Static dropdown data for catering e-commerce application

// Staff Management post roles predefined options
    // --- NEW LOGIC ---
    export const PREDEFINED_ROLES_DATA = [
        'Chef', 'Cook', 'Head Chef', 'Specialist Cook',
        'Server', 'Waiter', 'Manager', 'Decorator',
        'Helper', 'Cleaner', 'Driver', 'Other'
    ];

    export const FOOD_RELATED_ROLES_DATA = ['Chef', 'Cook', 'Head Chef', 'Specialist Cook'];
// --- END NEW LOGIC ---

// Discount type options for filtering
 export const discountTypeOptions = [
    { id: 0, name: 'All Types' }, // 0 for All
    { id: 1, name: 'Individual Food Items' },
    { id: 2, name: 'Food Packages' },
    { id: 3, name: 'Entire Catering' },
];

// Enum Mappings Discount type for Display
 export const DISCOUNT_TYPE_LABELS = {
    1: 'Individual Food Items',
    2: 'Food Packages',
    3: 'Entire Catering'
};

// Days and Months for availability calendar
export const DAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
export const MONTHS = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];