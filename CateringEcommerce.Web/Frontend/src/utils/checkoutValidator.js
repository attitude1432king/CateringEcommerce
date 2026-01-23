// ===================================
// VALIDATE EVENT DETAILS
// ===================================
export const validateEventDetails = (eventData) => {
  const errors = {};

  // Validate event date
  if (!eventData.eventDate) {
    errors.eventDate = 'Event date is required';
  } else {
    const eventDate = new Date(eventData.eventDate);
    const minDate = new Date();
    minDate.setHours(minDate.getHours() + 24); // 24 hours from now

    if (eventDate < minDate) {
      errors.eventDate = 'Event date must be at least 24 hours in advance';
    }

    const maxDate = new Date();
    maxDate.setDate(maxDate.getDate() + 90); // 90 days from now

    if (eventDate > maxDate) {
      errors.eventDate = 'Event date cannot be more than 90 days in advance';
    }
  }

  // Validate event time
  if (!eventData.eventTime || eventData.eventTime.trim() === '') {
    errors.eventTime = 'Event time is required';
  }

  // Validate event type
  if (!eventData.eventType || eventData.eventType.trim() === '') {
    errors.eventType = 'Event type is required';
  }

  // Validate event location (optional but recommended)
  if (eventData.eventLocation && eventData.eventLocation.length > 500) {
    errors.eventLocation = 'Event location must be less than 500 characters';
  }

  // Validate special instructions (optional)
  if (eventData.specialInstructions && eventData.specialInstructions.length > 1000) {
    errors.specialInstructions = 'Special instructions must be less than 1000 characters';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};

// ===================================
// VALIDATE ADDRESS & CONTACT
// ===================================
export const validateAddressContact = (addressData) => {
  const errors = {};

  // Validate delivery address
  if (!addressData.deliveryAddress || addressData.deliveryAddress.trim() === '') {
    errors.deliveryAddress = 'Delivery address is required';
  } else if (addressData.deliveryAddress.length > 500) {
    errors.deliveryAddress = 'Delivery address must be less than 500 characters';
  }

  // Validate contact person
  if (!addressData.contactPerson || addressData.contactPerson.trim() === '') {
    errors.contactPerson = 'Contact person name is required';
  } else if (addressData.contactPerson.length > 100) {
    errors.contactPerson = 'Contact person name must be less than 100 characters';
  }

  // Validate contact phone
  if (!addressData.contactPhone || addressData.contactPhone.trim() === '') {
    errors.contactPhone = 'Contact phone is required';
  } else {
    // Remove spaces and special characters for validation
    const cleanPhone = addressData.contactPhone.replace(/[\s\-()]/g, '');

    // Check if it's a valid Indian phone number (10 digits)
    const phoneRegex = /^[6-9]\d{9}$/;
    if (!phoneRegex.test(cleanPhone)) {
      errors.contactPhone = 'Please enter a valid 10-digit phone number';
    }
  }

  // Validate contact email
  if (!addressData.contactEmail || addressData.contactEmail.trim() === '') {
    errors.contactEmail = 'Contact email is required';
  } else {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(addressData.contactEmail)) {
      errors.contactEmail = 'Please enter a valid email address';
    } else if (addressData.contactEmail.length > 100) {
      errors.contactEmail = 'Email must be less than 100 characters';
    }
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};

// ===================================
// VALIDATE PAYMENT & REVIEW
// ===================================
export const validatePaymentReview = (paymentData) => {
  const errors = {};

  // Validate payment method
  if (!paymentData.paymentMethod) {
    errors.paymentMethod = 'Payment method is required';
  }

  // Validate payment proof for Bank Transfer
  if (paymentData.paymentMethod === 'BankTransfer') {
    if (!paymentData.paymentProof || !paymentData.paymentProof.base64) {
      errors.paymentProof = 'Payment proof is required for bank transfer';
    }
  }

  // Validate terms acceptance
  if (!paymentData.termsAccepted) {
    errors.termsAccepted = 'You must accept the terms and conditions to proceed';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};

// ===================================
// VALIDATE FILE UPLOAD
// ===================================
export const validateFileUpload = (file) => {
  const errors = {};

  if (!file) {
    errors.file = 'Please select a file';
    return { isValid: false, errors };
  }

  // Check file size (max 5MB)
  const maxSize = 5 * 1024 * 1024; // 5MB in bytes
  if (file.size > maxSize) {
    errors.file = 'File size must be less than 5MB';
  }

  // Check file type (images and PDF only)
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'application/pdf'];
  if (!allowedTypes.includes(file.type)) {
    errors.file = 'Only images (JPEG, PNG, GIF) and PDF files are allowed';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
};

// ===================================
// CONVERT FILE TO BASE64
// ===================================
export const fileToBase64 = (file) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = () => {
      resolve(reader.result);
    };

    reader.onerror = (error) => {
      reject(error);
    };

    reader.readAsDataURL(file);
  });
};

// ===================================
// FORMAT PHONE NUMBER
// ===================================
export const formatPhoneNumber = (phone) => {
  // Remove all non-digit characters
  const cleaned = phone.replace(/\D/g, '');

  // Format as XXX-XXX-XXXX
  if (cleaned.length === 10) {
    return `${cleaned.slice(0, 3)}-${cleaned.slice(3, 6)}-${cleaned.slice(6)}`;
  }

  return phone;
};

// ===================================
// VALIDATE COMPLETE ORDER DATA
// ===================================
export const validateCompleteOrder = (orderData) => {
  const errors = {};

  // Validate all sections
  const eventValidation = validateEventDetails({
    eventDate: orderData.eventDate,
    eventTime: orderData.eventTime,
    eventType: orderData.eventType,
    eventLocation: orderData.eventLocation,
    specialInstructions: orderData.specialInstructions,
  });

  const addressValidation = validateAddressContact({
    deliveryAddress: orderData.deliveryAddress,
    contactPerson: orderData.contactPerson,
    contactPhone: orderData.contactPhone,
    contactEmail: orderData.contactEmail,
  });

  const paymentValidation = validatePaymentReview({
    paymentMethod: orderData.paymentMethod,
    paymentProof: orderData.paymentProof,
    termsAccepted: orderData.termsAccepted,
  });

  // Combine all errors
  return {
    isValid:
      eventValidation.isValid &&
      addressValidation.isValid &&
      paymentValidation.isValid,
    errors: {
      ...eventValidation.errors,
      ...addressValidation.errors,
      ...paymentValidation.errors,
    },
  };
};

// ===================================
// VALIDATE CHECKOUT DATA (Step-wise)
// ===================================
export const validateCheckoutData = (checkoutData, cart, step) => {
  const errors = {};
  let isValid = true;

  switch (step) {
    case 1: // Account
      if (!checkoutData.isGuest) {
        // User must be authenticated - validated elsewhere
      } else {
        if (!checkoutData.guestEmail) {
          errors.guestEmail = 'Email is required for guest checkout';
          isValid = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(checkoutData.guestEmail)) {
          errors.guestEmail = 'Please enter a valid email address';
          isValid = false;
        }

        if (!checkoutData.guestPhone) {
          errors.guestPhone = 'Phone number is required';
          isValid = false;
        } else if (!/^[0-9]{10}$/.test(checkoutData.guestPhone)) {
          errors.guestPhone = 'Please enter a valid 10-digit phone number';
          isValid = false;
        }
      }
      break;

    case 2: // Event Details
      if (!checkoutData.eventType) {
        errors.eventType = 'Please select event type';
        isValid = false;
      }

      if (!checkoutData.eventDate) {
        errors.eventDate = 'Event date is required';
        isValid = false;
      } else {
        const selectedDate = new Date(checkoutData.eventDate);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (selectedDate < today) {
          errors.eventDate = 'Event date cannot be in the past';
          isValid = false;
        }

        // Require at least 3 days advance booking
        const threeDaysFromNow = new Date();
        threeDaysFromNow.setDate(threeDaysFromNow.getDate() + 3);
        threeDaysFromNow.setHours(0, 0, 0, 0);

        if (selectedDate < threeDaysFromNow) {
          errors.eventDate = 'Catering orders require at least 3 days advance booking';
          isValid = false;
        }
      }

      if (!checkoutData.eventTime) {
        errors.eventTime = 'Event time is required';
        isValid = false;
      }

      if (!checkoutData.guestCount || checkoutData.guestCount < 1) {
        errors.guestCount = 'Guest count must be at least 1';
        isValid = false;
      }

      // Validate address
      if (!checkoutData.eventAddress.street) {
        errors.eventAddressStreet = 'Street address is required';
        isValid = false;
      }

      if (!checkoutData.eventAddress.city) {
        errors.eventAddressCity = 'City is required';
        isValid = false;
      }

      if (!checkoutData.eventAddress.state) {
        errors.eventAddressState = 'State is required';
        isValid = false;
      }

      if (!checkoutData.eventAddress.pincode) {
        errors.eventAddressPincode = 'Pincode is required';
        isValid = false;
      } else if (!/^[0-9]{6}$/.test(checkoutData.eventAddress.pincode)) {
        errors.eventAddressPincode = 'Please enter a valid 6-digit pincode';
        isValid = false;
      }
      break;

    case 3: // Delivery Type
      if (!checkoutData.deliveryType) {
        errors.deliveryType = 'Please select delivery type';
        isValid = false;
      }
      break;

    case 4: // Payment
      if (!checkoutData.paymentMethod) {
        errors.paymentMethod = 'Please select a payment method';
        isValid = false;
      }

      if (checkoutData.paymentMethod === 'partial') {
        if (!checkoutData.advanceAmount || checkoutData.advanceAmount <= 0) {
          errors.advanceAmount = 'Please enter advance payment amount';
          isValid = false;
        } else if (cart && checkoutData.advanceAmount > cart.totalAmount) {
          errors.advanceAmount = 'Advance amount cannot exceed total amount';
          isValid = false;
        } else if (cart && checkoutData.advanceAmount < cart.totalAmount * 0.3) {
          errors.advanceAmount = 'Minimum 30% advance payment required';
          isValid = false;
        }
      }

      if (!checkoutData.termsAccepted) {
        errors.termsAccepted = 'Please accept terms and conditions';
        isValid = false;
      }
      break;

    default:
      break;
  }

  return { isValid, errors };
};

// ===================================
// HELPER FUNCTIONS
// ===================================
export const getEventTypeDisplay = (eventType) => {
  const types = {
    wedding: 'Wedding',
    birthday: 'Birthday Party',
    corporate: 'Corporate Event',
    anniversary: 'Anniversary',
    religious: 'Religious Function',
    social: 'Social Gathering',
    other: 'Other Event'
  };
  return types[eventType] || eventType;
};

export const getDeliveryTypeDisplay = (deliveryType) => {
  const types = {
    sample: 'Sample Delivery (Taste Before You Order)',
    event: 'Event Catering Delivery (Scheduled)'
  };
  return types[deliveryType] || deliveryType;
};

export const getPaymentMethodDisplay = (paymentMethod) => {
  const methods = {
    online: 'Pay Full Amount Online',
    partial: 'Pay Advance (30% minimum)',
    cod: 'Cash on Delivery'
  };
  return methods[paymentMethod] || paymentMethod;
};

export const formatEventDate = (date) => {
  if (!date) return '';
  const d = new Date(date);
  return d.toLocaleDateString('en-IN', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

export const formatEventTime = (time) => {
  if (!time) return '';
  return new Date(`2000-01-01T${time}`).toLocaleTimeString('en-IN', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
  });
};
