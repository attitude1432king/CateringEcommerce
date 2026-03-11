/**
 * OTPInput Component
 * 6-digit OTP input with auto-focus and paste support
 */

import { useRef, useState, useEffect } from 'react';
import PropTypes from 'prop-types';

const OTPInput = ({
  length = 6,
  value = '',
  onChange,
  onComplete,
  disabled = false,
  error = false,
  className = '',
}) => {
  const [otp, setOtp] = useState(Array(length).fill(''));
  const inputRefs = useRef([]);

  // Initialize input refs
  useEffect(() => {
    inputRefs.current = inputRefs.current.slice(0, length);
  }, [length]);

  // Sync with external value
  useEffect(() => {
    if (value) {
      setOtp(value.split('').slice(0, length).concat(Array(length).fill('')).slice(0, length));
    }
  }, [value, length]);

  const handleChange = (index, digit) => {
    // Only allow numbers
    if (digit && !/^[0-9]$/.test(digit)) return;

    const newOtp = [...otp];
    newOtp[index] = digit;
    setOtp(newOtp);

    // Callback with complete OTP
    const otpString = newOtp.join('');
    onChange?.(otpString);

    // Auto-focus next input
    if (digit && index < length - 1) {
      inputRefs.current[index + 1]?.focus();
    }

    // Call onComplete if all digits filled
    if (newOtp.every(d => d) && onComplete) {
      onComplete(otpString);
    }
  };

  const handleKeyDown = (index, e) => {
    // Handle backspace
    if (e.key === 'Backspace') {
      if (!otp[index] && index > 0) {
        // Move to previous input if current is empty
        inputRefs.current[index - 1]?.focus();
      } else {
        // Clear current input
        const newOtp = [...otp];
        newOtp[index] = '';
        setOtp(newOtp);
        onChange?.(newOtp.join(''));
      }
    }

    // Handle arrow keys
    if (e.key === 'ArrowLeft' && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
    if (e.key === 'ArrowRight' && index < length - 1) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handlePaste = (e) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData('text/plain').trim();
    const digits = pastedData.match(/\d/g);

    if (digits) {
      const newOtp = digits.slice(0, length).concat(Array(length).fill('')).slice(0, length);
      setOtp(newOtp);

      const otpString = newOtp.join('');
      onChange?.(otpString);

      // Focus last filled input or first empty
      const lastFilledIndex = newOtp.findIndex(d => !d);
      const focusIndex = lastFilledIndex === -1 ? length - 1 : lastFilledIndex;
      inputRefs.current[focusIndex]?.focus();

      // Call onComplete if all digits filled
      if (newOtp.every(d => d) && onComplete) {
        onComplete(otpString);
      }
    }
  };

  return (
    <div className={`flex gap-2 justify-center ${className}`}>
      {otp.map((digit, index) => (
        <input
          key={index}
          ref={el => inputRefs.current[index] = el}
          type="text"
          inputMode="numeric"
          maxLength={1}
          value={digit}
          onChange={(e) => handleChange(index, e.target.value)}
          onKeyDown={(e) => handleKeyDown(index, e)}
          onPaste={handlePaste}
          disabled={disabled}
          className={`
            w-12 h-14 text-center text-2xl font-bold rounded-lg border-2 transition-colors
            ${error ? 'border-red-500 bg-red-50' : 'border-gray-300 focus:border-blue-500'}
            ${disabled ? 'bg-gray-100 cursor-not-allowed' : 'bg-white'}
            focus:outline-none focus:ring-2 focus:ring-blue-500/20
          `}
        />
      ))}
    </div>
  );
};

OTPInput.propTypes = {
  length: PropTypes.number,
  value: PropTypes.string,
  onChange: PropTypes.func,
  onComplete: PropTypes.func,
  disabled: PropTypes.bool,
  error: PropTypes.bool,
  className: PropTypes.string,
};

export default OTPInput;
