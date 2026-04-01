/**
 * RegistrationWizard Component (REDESIGNED)
 * Modern multi-step registration form for Event Supervisors
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PropTypes from 'prop-types';
import toast from 'react-hot-toast';
import { CheckCircle, UserPlus } from 'lucide-react';
import { registrationApi } from '../../../../services/api/supervisor';
import Step1_PersonalDetails from './Step1_PersonalDetails';
import Step2_AddressDetails from './Step2_AddressDetails';
import Step3_ExperienceDetails from './Step3_ExperienceDetails';
import Step4_IdentityProof from './Step4_IdentityProof';
import Step5_Availability from './Step5_Availability';
import Step6_BankingDetails from './Step6_BankingDetails';

const RegistrationWizard = ({ onComplete }) => {
    const navigate = useNavigate();
    const [currentStep, setCurrentStep] = useState(1);
    const [formData, setFormData] = useState({});
    const [submitting, setSubmitting] = useState(false);

    const steps = [
        { label: 'Personal', shortLabel: 'Personal' },
        { label: 'Address', shortLabel: 'Address' },
        { label: 'Experience', shortLabel: 'Exp.' },
        { label: 'Documents', shortLabel: 'Docs' },
        { label: 'Availability', shortLabel: 'Avail.' },
        { label: 'Banking', shortLabel: 'Bank' },
    ];

    const handleNext = (stepData) => {
        setFormData({ ...formData, ...stepData });
        setCurrentStep(currentStep + 1);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleBack = () => {
        setCurrentStep(currentStep - 1);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleSubmit = async (finalStepData) => {
        const completeData = { ...formData, ...finalStepData };
        setSubmitting(true);

        try {
            const uploadIfPresent = async (fileData, documentType, supervisorId) => {
                if (!fileData?.file) return '';
                const uploadResponse = await registrationApi.uploadDocument({
                    file: fileData.file,
                    documentType,
                    supervisorId,
                });

                if (!uploadResponse.success) {
                    throw new Error(uploadResponse.message || `Failed to upload ${documentType}`);
                }

                const uploadPayload = uploadResponse.data?.data ?? uploadResponse.data ?? {};
                return uploadPayload.documentUrl || '';
            };

            const registrationData = {
                firstName: completeData.firstName,
                lastName: completeData.lastName,
                email: completeData.email,
                phone: completeData.phone,
                dateOfBirth: completeData.dateOfBirth.toISOString(),
                address: completeData.address,
                pincode: completeData.pincode,
                stateID: completeData.stateID,
                cityID: completeData.cityID,
                preferredZoneId: completeData.preferredZoneId || null,
                hasPriorExperience: completeData.hasPriorExperience || false,
                priorExperienceDetails: completeData.priorExperienceDetails || '',
                idProofType: completeData.idProofType,
                idProofNumber: completeData.idProofNumber,
                idProofUrl: '',
                addressProofUrl: '',
                photoUrl: '',
            };

            const response = await registrationApi.submitRegistration(registrationData);
            const responsePayload = response.data?.data ?? response.data ?? {};

            if (response.success && responsePayload.registrationId) {
                const registrationId = responsePayload.registrationId;
                const supervisorId = responsePayload.supervisorId;

                await uploadIfPresent(completeData.idProofFile, 'IDProof', supervisorId);
                await uploadIfPresent(completeData.addressProofFile, 'AddressProof', supervisorId);
                await uploadIfPresent(completeData.photoFile, 'Photo', supervisorId);
                const cancelledChequeUrl = await uploadIfPresent(completeData.cancelledChequeUrl, 'CancelledCheque', supervisorId);

                toast.success('Registration submitted successfully!');

                if (!finalStepData.skipBanking && completeData.accountNumber) {
                    try {
                        await registrationApi.submitBankingDetails({
                            supervisorId,
                            accountHolderName: completeData.accountHolderName,
                            bankName: completeData.bankName,
                            accountNumber: completeData.accountNumber,
                            ifscCode: completeData.ifscCode,
                            branchName: completeData.branchName,
                            accountType: completeData.accountType,
                            cancelledChequeUrl,
                        });
                    } catch (bankingError) {
                        console.error('Banking details error:', bankingError);
                        toast.error('Banking details could not be saved. You can add them later.');
                    }
                }

                localStorage.setItem('registrationId', String(registrationId));
                if (supervisorId) {
                    localStorage.setItem('supervisorId', String(supervisorId));
                }
                onComplete?.(responsePayload);
                navigate(`/supervisor/registration/progress/${registrationId}`);
            } else {
                toast.error(response.message || 'Registration failed');
            }
        } catch (error) {
            console.error('Registration error:', error);
            toast.error('Failed to submit registration. Please try again.');
        } finally {
            setSubmitting(false);
        }
    };

    const renderStep = () => {
        const stepProps = {
            data: formData,
            onNext: currentStep === 6 ? handleSubmit : handleNext,
            onBack: currentStep > 1 ? handleBack : null,
        };

        switch (currentStep) {
            case 1: return <Step1_PersonalDetails {...stepProps} />;
            case 2: return <Step2_AddressDetails {...stepProps} />;
            case 3: return <Step3_ExperienceDetails {...stepProps} />;
            case 4: return <Step4_IdentityProof {...stepProps} />;
            case 5: return <Step5_Availability {...stepProps} />;
            case 6: return <Step6_BankingDetails {...stepProps} />;
            default: return null;
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 relative overflow-hidden">
            {/* Decorative background blobs */}
            <div className="absolute top-0 right-0 w-96 h-96 bg-rose-100/30 rounded-full blur-3xl -translate-y-1/2 translate-x-1/3 pointer-events-none" />
            <div className="absolute bottom-0 left-0 w-80 h-80 bg-amber-100/30 rounded-full blur-3xl translate-y-1/3 -translate-x-1/4 pointer-events-none" />

            <div className="relative z-10 py-8 px-4 sm:px-6 lg:px-8">
                <div className="max-w-3xl mx-auto">
                    {/* Logo & Header */}
                    <div className="text-center mb-8">
                        <div className="flex justify-center mb-6">
                            <img src="/logo.svg" alt="ENYVORA" className="h-12 w-auto block" />
                        </div>
                        <div className="flex items-center justify-center gap-3 mb-2">
                            <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center">
                                <UserPlus className="w-5 h-5 text-rose-600" />
                            </div>
                            <h1 className="text-3xl font-bold text-neutral-800">
                                Supervisor Registration
                            </h1>
                        </div>
                        <p className="text-neutral-600 text-sm">
                            Join our team of professional event supervisors
                        </p>
                    </div>

                    {/* Modern Step Progress */}
                    <div className="bg-white rounded-2xl border-2 border-neutral-100 shadow-sm p-4 sm:p-6 mb-8">
                        <div className="flex items-center justify-between">
                            {steps.map((step, idx) => {
                                const stepNum = idx + 1;
                                const isActive = currentStep === stepNum;
                                const isCompleted = currentStep > stepNum;

                                return (
                                    <div key={idx} className="flex items-center flex-1">
                                        <div className="flex flex-col items-center flex-shrink-0">
                                            <div className={`w-10 h-10 rounded-xl flex items-center justify-center text-sm font-bold transition-all duration-300 ${
                                                isActive
                                                    ? 'bg-gradient-to-r from-rose-600 to-rose-500 text-white shadow-lg shadow-rose-200'
                                                    : isCompleted
                                                    ? 'bg-green-100 text-green-700 border-2 border-green-200'
                                                    : 'bg-neutral-100 text-neutral-400 border-2 border-neutral-200'
                                            }`}>
                                                {isCompleted ? (
                                                    <CheckCircle className="w-5 h-5 text-green-600" />
                                                ) : (
                                                    stepNum
                                                )}
                                            </div>
                                            <span className={`text-xs font-semibold mt-1.5 ${
                                                isActive ? 'text-rose-600' : isCompleted ? 'text-green-600' : 'text-neutral-400'
                                            }`}>
                                                <span className="hidden sm:inline">{step.label}</span>
                                                <span className="sm:hidden">{step.shortLabel}</span>
                                            </span>
                                        </div>
                                        {idx < steps.length - 1 && (
                                            <div className={`flex-1 h-0.5 mx-2 sm:mx-3 rounded-full transition-colors duration-300 ${
                                                isCompleted ? 'bg-green-300' : 'bg-neutral-200'
                                            }`} />
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                    </div>

                    {/* Form Card */}
                    <div className="bg-white rounded-2xl border-2 border-neutral-100 shadow-xl p-6 md:p-8">
                        {submitting ? (
                            <div className="text-center py-16">
                                <div className="animate-spin rounded-full h-14 w-14 border-t-2 border-b-2 border-rose-600 mx-auto mb-5"></div>
                                <p className="text-lg font-semibold text-neutral-800">Submitting your registration...</p>
                                <p className="text-sm text-neutral-500 mt-2">Please wait while we process your application</p>
                            </div>
                        ) : (
                            renderStep()
                        )}
                    </div>

                    {/* Help Text */}
                    <div className="mt-8 text-center">
                        <div className="bg-blue-50 border-l-4 border-blue-400 rounded-lg p-4 inline-block text-left">
                            <p className="text-sm text-blue-800">
                                Need help? Contact our support team at <span className="font-semibold">support@enyvora.com</span>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

RegistrationWizard.propTypes = {
    onComplete: PropTypes.func,
};

export default RegistrationWizard;
