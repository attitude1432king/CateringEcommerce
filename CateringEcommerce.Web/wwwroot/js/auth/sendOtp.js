// wwwroot/js/auth/sendOtp.js

export async function sendOtp(phoneNumber) {
    try {
        const response = await fetch('https://localhost:44386/api/auth/send-otp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ phoneNumber })
        });

        const data = await response.json();
        if (response.ok) {
            return { success: true, message: `OTP sent to ${phoneNumber}`, otp: data.otp };
        } else {
            return { success: false, message: data.message || 'Error sending OTP' };
        }
    } catch (error) {
        console.error('Send OTP failed:', error);
        return { success: false, message: 'Server error' };
    }
}
