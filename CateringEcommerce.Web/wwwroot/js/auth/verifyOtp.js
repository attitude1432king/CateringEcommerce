// wwwroot/js/auth/verifyOtp.js

export async function verifyOtp(phoneNumber, otp) {
    try {
        const response = await fetch('https://localhost:44386/api/auth/verify-otp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ phoneNumber, otp })
        });

        const data = await response.json();
        if (response.ok) {
            return { success: true, message: 'OTP verified successfully' };
        } else {
            return { success: false, message: data.message || 'OTP verification failed' };
        }
    } catch (error) {
        console.error('Verify OTP failed:', error);
        return { success: false, message: 'Server error' };
    }
}
