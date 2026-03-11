/*
========================================
File: src/components/owner/Step5_Agreement.jsx (NEW)
========================================
*/
import React, { useState, useRef, useEffect } from 'react';
import DOMPurify from 'dompurify';

// Signature Pad Component
const SignaturePad = ({ onSignatureChange, signature }) => {
    const canvasRef = useRef(null);
    const [isDrawing, setIsDrawing] = useState(false);
    const [isEmpty, setIsEmpty] = useState(true);

    useEffect(() => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext('2d');

        // Set canvas size
        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width * 2;
        canvas.height = rect.height * 2;
        ctx.scale(2, 2);

        // Set drawing style
        ctx.strokeStyle = '#1F2937';
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';

        // If signature exists, load it
        if (signature) {
            const img = new Image();
            img.onload = () => {
                ctx.drawImage(img, 0, 0, rect.width, rect.height);
                setIsEmpty(false);
            };
            img.src = signature;
        }
    }, [signature]);

    const startDrawing = (e) => {
        const canvas = canvasRef.current;
        const ctx = canvas.getContext('2d');
        const rect = canvas.getBoundingClientRect();

        const x = (e.clientX || e.touches?.[0]?.clientX) - rect.left;
        const y = (e.clientY || e.touches?.[0]?.clientY) - rect.top;

        ctx.beginPath();
        ctx.moveTo(x, y);
        setIsDrawing(true);
        setIsEmpty(false);
    };

    const draw = (e) => {
        if (!isDrawing) return;

        const canvas = canvasRef.current;
        const ctx = canvas.getContext('2d');
        const rect = canvas.getBoundingClientRect();

        const x = (e.clientX || e.touches?.[0]?.clientX) - rect.left;
        const y = (e.clientY || e.touches?.[0]?.clientY) - rect.top;

        ctx.lineTo(x, y);
        ctx.stroke();
    };

    const stopDrawing = () => {
        if (isDrawing) {
            const canvas = canvasRef.current;
            const signatureData = canvas.toDataURL('image/png');
            onSignatureChange(signatureData);
            setIsDrawing(false);
        }
    };

    const clearSignature = () => {
        const canvas = canvasRef.current;
        const ctx = canvas.getContext('2d');
        const rect = canvas.getBoundingClientRect();

        ctx.clearRect(0, 0, rect.width, rect.height);
        setIsEmpty(true);
        onSignatureChange(null);
    };

    return (
        <div className="relative">
            <div className="border-2 border-dashed border-neutral-300 rounded-xl overflow-hidden bg-white relative group hover:border-rose-300 transition-all">
                <canvas
                    ref={canvasRef}
                    className="w-full h-48 touch-none cursor-crosshair"
                    onMouseDown={startDrawing}
                    onMouseMove={draw}
                    onMouseUp={stopDrawing}
                    onMouseLeave={stopDrawing}
                    onTouchStart={startDrawing}
                    onTouchMove={draw}
                    onTouchEnd={stopDrawing}
                />
                {isEmpty && (
                    <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                        <div className="text-center text-neutral-400">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-12 w-12 mx-auto mb-2 opacity-50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                            </svg>
                            <p className="text-sm font-medium">Sign here with your mouse or touch</p>
                        </div>
                    </div>
                )}
            </div>
            <button
                type="button"
                onClick={clearSignature}
                className="mt-3 px-4 py-2 bg-neutral-100 text-neutral-700 rounded-lg text-sm font-semibold hover:bg-neutral-200 transition-all flex items-center gap-2"
            >
                <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M4 2a1 1 0 011 1v2.101a7.002 7.002 0 0111.601 2.566 1 1 0 11-1.885.666A5.002 5.002 0 005.999 7H9a1 1 0 010 2H4a1 1 0 01-1-1V3a1 1 0 011-1zm.008 9.057a1 1 0 011.276.61A5.002 5.002 0 0014.001 13H11a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0v-2.101a7.002 7.002 0 01-11.601-2.566 1 1 0 01.61-1.276z" clipRule="evenodd" />
                </svg>
                Clear Signature
            </button>
        </div>
    );
};

export default function Step5_Agreement({ formData, setFormData, errors }) {
    const [agreementText, setAgreementText] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        // Fetch agreement letter from backend
        const fetchAgreement = async () => {
            try {
                // Replace this URL with your actual backend endpoint
                const response = await fetch('/api/Owner/GetPartnerAgreement');
                if (response.ok) {
                    const data = await response.json();
                    setAgreementText(data.agreementText || getDefaultAgreement());
                } else {
                    // Fallback to default agreement
                    setAgreementText(getDefaultAgreement());
                }
            } catch (error) {
                console.error('Failed to fetch agreement:', error);
                setAgreementText(getDefaultAgreement());
            } finally {
                setIsLoading(false);
            }
        };

        fetchAgreement();
    }, []);

    const handlePrint = () => {
        // SECURITY FIX: Sanitize all user inputs before printing
        const printWindow = window.open('', '_blank');

        if (!printWindow) {
            console.error('Failed to open print window. Please allow popups.');
            return;
        }

        // Get the current date
        const currentDate = new Date().toLocaleDateString('en-IN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        // SECURITY: Sanitize all user-controlled inputs with DOMPurify
        const sanitizedAgreementText = DOMPurify.sanitize(agreementText, {
            ALLOWED_TAGS: ['br', 'p', 'strong', 'em', 'b', 'i', 'u'],
            ALLOWED_ATTR: [],
            KEEP_CONTENT: true
        });

        const sanitizedCateringName = DOMPurify.sanitize(formData.cateringName || 'Not provided', {
            ALLOWED_TAGS: [],
            ALLOWED_ATTR: []
        });

        const sanitizedOwnerName = DOMPurify.sanitize(formData.ownerName || 'Not provided', {
            ALLOWED_TAGS: [],
            ALLOWED_ATTR: []
        });

        // SECURITY: Validate signature is a valid data URL
        let signatureHTML = '<p style="color: #ef4444; font-style: italic;">No signature provided</p>';
        if (formData.signature) {
            // Validate it's a data URL for image
            if (formData.signature.startsWith('data:image/')) {
                // Additional sanitization: ensure it's a safe data URL
                const sanitizedSignature = DOMPurify.sanitize(formData.signature, {
                    ALLOWED_TAGS: [],
                    ALLOWED_ATTR: []
                });
                signatureHTML = `<img src="${sanitizedSignature}" alt="Partner Signature" class="signature-image" />`;
            } else {
                signatureHTML = '<p style="color: #ef4444; font-style: italic;">Invalid signature format</p>';
            }
        }

        // Build the print content with sanitized data
        const printContent = `
            <!DOCTYPE html>
            <html>
            <head>
                <title>Partner Agreement</title>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        margin: 40px;
                        line-height: 1.6;
                        color: #333;
                    }
                    .header {
                        text-align: center;
                        margin-bottom: 30px;
                        border-bottom: 2px solid #e11d48;
                        padding-bottom: 20px;
                    }
                    .header h1 {
                        color: #e11d48;
                        margin: 0;
                        font-size: 28px;
                    }
                    .header p {
                        margin: 5px 0;
                        color: #666;
                    }
                    .agreement-content {
                        white-space: pre-wrap;
                        font-size: 14px;
                        margin: 30px 0;
                        text-align: justify;
                    }
                    .signature-section {
                        margin-top: 50px;
                        page-break-inside: avoid;
                    }
                    .signature-box {
                        border: 2px solid #e5e7eb;
                        padding: 20px;
                        margin-top: 20px;
                        border-radius: 8px;
                        background-color: #f9fafb;
                    }
                    .signature-label {
                        font-weight: bold;
                        margin-bottom: 10px;
                        color: #1f2937;
                    }
                    .signature-image {
                        max-width: 400px;
                        height: auto;
                        border: 1px solid #d1d5db;
                        padding: 10px;
                        background-color: white;
                        border-radius: 4px;
                    }
                    .date-info {
                        margin-top: 15px;
                        font-size: 13px;
                        color: #666;
                    }
                    .footer {
                        margin-top: 50px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        font-size: 12px;
                        color: #666;
                    }
                    @media print {
                        body {
                            margin: 20px;
                        }
                        .signature-section {
                            page-break-before: auto;
                        }
                    }
                </style>
            </head>
            <body>
                <div class="header">
                    <h1>PARTNER AGREEMENT</h1>
                    <p>ENYVORA Catering Platform</p>
                    <p>Date: ${DOMPurify.sanitize(currentDate)}</p>
                </div>

                <div class="agreement-content">
                    ${sanitizedAgreementText}
                </div>

                <div class="signature-section">
                    <div class="signature-box">
                        <div class="signature-label">Partner's Digital Signature:</div>
                        ${signatureHTML}
                        <div class="date-info">
                            <strong>Signed Date:</strong> ${DOMPurify.sanitize(currentDate)}<br/>
                            <strong>Business Name:</strong> ${sanitizedCateringName}<br/>
                            <strong>Owner Name:</strong> ${sanitizedOwnerName}
                        </div>
                    </div>
                </div>

                <div class="footer">
                    <p>This is a digitally signed agreement. For any queries, please contact ENYVORA support.</p>
                    <p>&copy; ${new Date().getFullYear()} ENYVORA Platform. All rights reserved.</p>
                </div>

                <script>
                    window.onload = function() {
                        window.print();
                    };
                </script>
            </body>
            </html>
        `;

        // SECURITY: Write sanitized content
        printWindow.document.write(printContent);
        printWindow.document.close();
    };

    const getDefaultAgreement = () => {
        return `PARTNER AGREEMENT

This Partner Agreement ("Agreement") is entered into between ENYVORA Platform ("Company") and the undersigned Partner ("Partner").

1. PARTNERSHIP TERMS
By signing this agreement, the Partner agrees to list their catering services on the ENYVORA platform and comply with all terms and conditions set forth herein.

2. SERVICES
The Partner agrees to:
- Provide accurate business information
- Maintain valid FSSAI, GST, and other required licenses
- Fulfill orders placed through the platform in a timely manner
- Maintain quality standards as per platform guidelines
- Update menu and availability regularly

3. PAYMENT TERMS
- Commission structure as per platform policy
- Weekly payout cycle
- All payments subject to order completion and customer satisfaction
- Transparent pricing with no hidden charges

4. QUALITY STANDARDS
The Partner commits to:
- Maintain hygiene and food safety standards
- Use quality ingredients
- Ensure timely delivery
- Provide excellent customer service
- Address customer complaints professionally

5. COMPLIANCE
The Partner shall:
- Comply with all local laws and regulations
- Maintain valid business licenses
- Adhere to food safety standards
- Pay all applicable taxes
- Maintain proper insurance coverage

6. TERMINATION
Either party may terminate this agreement with 30 days written notice. The Company reserves the right to suspend or terminate the Partner's account for violation of terms.

7. DATA PRIVACY
The Company will protect Partner data as per our Privacy Policy. Partner agrees to the collection and use of data for platform operations.

8. INTELLECTUAL PROPERTY
Partner grants the Company right to use their business name, logo, and food images for marketing purposes on the platform.

9. DISPUTE RESOLUTION
Any disputes shall be resolved through mutual discussion. If unresolved, disputes shall be subject to arbitration in accordance with applicable laws.

10. ACCEPTANCE
By signing below, the Partner acknowledges that they have read, understood, and agree to be bound by all terms and conditions of this Agreement.

---

I hereby agree to all the terms and conditions mentioned in this Partner Agreement.`;
    };

    const handleCheckboxChange = (e) => {
        setFormData(prev => ({ ...prev, agreementAccepted: e.target.checked }));
    };

    const handleSignatureChange = (signatureData) => {
        setFormData(prev => ({ ...prev, signature: signatureData }));
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center py-20">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-600 mx-auto mb-4"></div>
                    <p className="text-neutral-600">Loading agreement...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="animate-fade-in space-y-8">
            {/* Header */}
            <div className="bg-gradient-to-r from-rose-50 to-pink-50 p-6 rounded-xl border-l-4 border-rose-500">
                <h3 className="text-3xl font-bold text-neutral-800 mb-2 flex items-center gap-2">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-8 w-8 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    Partner Agreement
                </h3>
                <p className="text-neutral-600 text-sm leading-relaxed">
                    Please read the agreement carefully, accept the terms, and provide your digital signature below.
                </p>
            </div>

            {/* Important Notice */}
            <div className="bg-amber-50 border-l-4 border-amber-400 p-5 rounded-lg">
                <div className="flex items-start gap-3">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-amber-600 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                    <div className="flex-1">
                        <h4 className="font-bold text-amber-900 mb-2">Important Notice</h4>
                        <ul className="text-sm text-amber-800 space-y-1 list-disc list-inside">
                            <li>Read the entire agreement carefully before signing</li>
                            <li>Your signature is legally binding</li>
                            <li>You must accept all terms to proceed with registration</li>
                            <li>Save or print a copy for your records if needed</li>
                        </ul>
                    </div>
                </div>
            </div>

            {/* Agreement Text */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center justify-between mb-6">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-rose-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                            </svg>
                        </div>
                        <div>
                            <h4 className="text-xl font-bold text-neutral-800">Agreement Document</h4>
                            <p className="text-sm text-neutral-500">Please review all sections carefully</p>
                        </div>
                    </div>
                    <button
                        type="button"
                        onClick={handlePrint}
                        className="px-4 py-2 bg-neutral-100 text-neutral-700 rounded-lg text-sm font-semibold hover:bg-neutral-200 transition-all flex items-center gap-2"
                    >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
                        </svg>
                        Print Agreement
                    </button>
                </div>

                <div className="bg-neutral-50 border-2 border-neutral-200 rounded-lg p-6 max-h-96 overflow-y-auto custom-scrollbar">
                    <pre className="whitespace-pre-wrap font-sans text-sm text-neutral-700 leading-relaxed">
                        {agreementText}
                    </pre>
                </div>
            </section>

            {/* Acceptance Checkbox */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-green-600" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                        </svg>
                    </div>
                    <div>
                        <h4 className="text-xl font-bold text-neutral-800">Accept Terms</h4>
                        <p className="text-sm text-neutral-500">Confirm you agree to all terms and conditions</p>
                    </div>
                </div>

                <label className="flex items-start gap-4 p-4 border-2 border-neutral-200 rounded-xl cursor-pointer hover:border-rose-300 hover:bg-rose-50 transition-all group">
                    <input
                        type="checkbox"
                        checked={formData.agreementAccepted || false}
                        onChange={handleCheckboxChange}
                        className="mt-1 h-5 w-5 text-rose-600 border-neutral-300 rounded focus:ring-2 focus:ring-rose-200 cursor-pointer"
                    />
                    <div className="flex-1">
                        <p className="text-sm font-semibold text-neutral-800 group-hover:text-rose-700">
                            I have read and agree to all the terms and conditions of the Partner Agreement
                        </p>
                        <p className="text-xs text-neutral-600 mt-1">
                            By checking this box, you acknowledge that you have read, understood, and agree to be bound by this agreement.
                        </p>
                    </div>
                </label>
                {errors.agreementAccepted && (
                    <p className="text-xs text-red-600 mt-2 flex items-center gap-1">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                        {errors.agreementAccepted}
                    </p>
                )}
            </section>

            {/* Digital Signature */}
            <section className="bg-white p-6 rounded-xl border-2 border-neutral-100 shadow-sm">
                <div className="flex items-center gap-3 mb-6">
                    <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                        </svg>
                    </div>
                    <div>
                        <h4 className="text-xl font-bold text-neutral-800">Digital Signature</h4>
                        <p className="text-sm text-neutral-500">Sign below using your mouse or touch</p>
                    </div>
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
                    <p className="text-sm text-blue-800 flex items-start gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                        </svg>
                        <span><strong>Note:</strong> Your signature will be stored securely and used for verification purposes. Draw your signature in the box below.</span>
                    </p>
                </div>

                <SignaturePad
                    onSignatureChange={handleSignatureChange}
                    signature={formData.signature}
                />

                {errors.signature && (
                    <p className="text-xs text-red-600 mt-2 flex items-center gap-1">
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                        {errors.signature}
                    </p>
                )}
            </section>

            {/* Final Notice */}
            <div className="bg-green-50 border-l-4 border-green-400 p-5 rounded-lg">
                <div className="flex items-start gap-3">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-green-600 flex-shrink-0 mt-0.5" viewBox="0 0 20 20" fill="currentColor">
                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    <div className="flex-1">
                        <h4 className="font-bold text-green-900 mb-2">Ready to Submit?</h4>
                        <p className="text-sm text-green-800">
                            Once you accept the agreement and provide your signature, click "Submit for Verification" to complete your registration. Our team will review your application within 24-48 hours.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}
