/**
 * RegistrationWizard Component
 * Multi-step registration form for Event Supervisors
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PropTypes from 'prop-types';
import toast from 'react-hot-toast';
import { WorkflowStepper } from '../../common/progress';
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
    'Personal',
    'Address',
    'Experience',
    'Documents',
    'Availability',
    'Banking',
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
      // Prepare registration data
      const registrationData = {
        firstName: completeData.firstName,
        lastName: completeData.lastName,
        email: completeData.email,
        phone: completeData.phone,
        dateOfBirth: completeData.dateOfBirth.toISOString(),
        address: completeData.address,
        preferredZoneId: completeData.preferredZoneId || null,
        hasPriorExperience: completeData.hasPriorExperience || false,
        priorExperienceDetails: completeData.priorExperienceDetails || '',
        idProofType: completeData.idProofType,
        idProofNumber: completeData.idProofNumber,
        idProofUrl: completeData.idProofUrl,
        addressProofUrl: completeData.addressProofUrl,
        photoUrl: completeData.photoUrl,
      };

      // Submit registration
      const response = await registrationApi.submitRegistration(registrationData);

      if (response.success) {
        toast.success('Registration submitted successfully!');

        // Submit banking details if provided
        if (!finalStepData.skipBanking && completeData.accountNumber) {
          try {
            await registrationApi.submitBankingDetails({
              supervisorId: response.data.supervisorId,
              accountHolderName: completeData.accountHolderName,
              bankName: completeData.bankName,
              accountNumber: completeData.accountNumber,
              ifscCode: completeData.ifscCode,
              branchName: completeData.branchName,
              accountType: completeData.accountType,
              cancelledChequeUrl: completeData.cancelledChequeUrl,
            });
          } catch (bankingError) {
            console.error('Banking details error:', bankingError);
            // Don't fail registration if banking fails
            toast.error('Banking details could not be saved. You can add them later.');
          }
        }

        // Store registration ID
        localStorage.setItem('registrationId', response.data.registrationId);
        localStorage.setItem('supervisorId', response.data.supervisorId);

        // Callback
        onComplete?.(response.data);

        // Navigate to progress page
        navigate(`/supervisor/registration/progress/${response.data.registrationId}`);
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
      case 1:
        return <Step1_PersonalDetails {...stepProps} />;
      case 2:
        return <Step2_AddressDetails {...stepProps} />;
      case 3:
        return <Step3_ExperienceDetails {...stepProps} />;
      case 4:
        return <Step4_IdentityProof {...stepProps} />;
      case 5:
        return <Step5_Availability {...stepProps} />;
      case 6:
        return <Step6_BankingDetails {...stepProps} />;
      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            Event Supervisor Registration
          </h1>
          <p className="text-gray-600 mt-2">
            Join our team of professional event supervisors
          </p>
        </div>

        {/* Progress Stepper */}
        <div className="mb-8">
          <WorkflowStepper steps={steps} currentStep={currentStep} />
        </div>

        {/* Form Card */}
        <div className="bg-white rounded-lg shadow-md p-6 md:p-8">
          {submitting ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
              <p className="mt-4 text-gray-600">Submitting your registration...</p>
            </div>
          ) : (
            renderStep()
          )}
        </div>

        {/* Help Text */}
        <div className="mt-6 text-center text-sm text-gray-600">
          <p>Need help? Contact us at support@example.com</p>
        </div>
      </div>
    </div>
  );
};

RegistrationWizard.propTypes = {
  onComplete: PropTypes.func,
};

export default RegistrationWizard;
