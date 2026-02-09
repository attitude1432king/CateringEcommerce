/**
 * Step 6: Banking Details
 * Bank account for payment - can be submitted later
 */

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import PropTypes from 'prop-types';
import { Building2, CreditCard } from 'lucide-react';
import { bankingDetailsSchema } from '../../../../utils/supervisor/validationSchemas';
import { DocumentUploader } from '../../common/forms';

const Step6_BankingDetails = ({ data, onNext, onBack }) => {
  const [skipBanking, setSkipBanking] = useState(false);

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: skipBanking ? undefined : zodResolver(bankingDetailsSchema),
    defaultValues: data,
  });

  const onSubmit = (formData) => {
    if (skipBanking) {
      onNext({ skipBanking: true });
    } else {
      onNext(formData);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Banking Details</h2>
        <p className="text-sm text-gray-600 mt-2">
          For receiving payments (can be added later)
        </p>
      </div>

      <div className="space-y-4">
        {/* Skip Banking Option */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                checked={skipBanking}
                onChange={(e) => setSkipBanking(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label className="text-sm font-medium text-blue-900">
                Add banking details later
              </label>
              <p className="text-xs text-blue-700 mt-1">
                You can add this before receiving your first payment
              </p>
            </div>
          </div>
        </div>

        {!skipBanking && (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Account Holder Name */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Account Holder Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  {...register('accountHolderName')}
                  className={`block w-full px-3 py-2 border rounded-lg ${
                    errors.accountHolderName ? 'border-red-300' : 'border-gray-300'
                  }`}
                  placeholder="As per bank records"
                />
                {errors.accountHolderName && (
                  <p className="mt-1 text-xs text-red-600">{errors.accountHolderName.message}</p>
                )}
              </div>

              {/* Bank Name */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Bank Name <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Building2 className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="text"
                    {...register('bankName')}
                    className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                      errors.bankName ? 'border-red-300' : 'border-gray-300'
                    }`}
                    placeholder="e.g., HDFC Bank"
                  />
                </div>
                {errors.bankName && (
                  <p className="mt-1 text-xs text-red-600">{errors.bankName.message}</p>
                )}
              </div>

              {/* Account Number */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Account Number <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  {...register('accountNumber')}
                  className={`block w-full px-3 py-2 border rounded-lg ${
                    errors.accountNumber ? 'border-red-300' : 'border-gray-300'
                  }`}
                  placeholder="Enter account number"
                />
                {errors.accountNumber && (
                  <p className="mt-1 text-xs text-red-600">{errors.accountNumber.message}</p>
                )}
              </div>

              {/* IFSC Code */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  IFSC Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  {...register('ifscCode')}
                  className={`block w-full px-3 py-2 border rounded-lg uppercase ${
                    errors.ifscCode ? 'border-red-300' : 'border-gray-300'
                  }`}
                  placeholder="e.g., HDFC0001234"
                  maxLength={11}
                />
                {errors.ifscCode && (
                  <p className="mt-1 text-xs text-red-600">{errors.ifscCode.message}</p>
                )}
              </div>

              {/* Branch Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Branch Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  {...register('branchName')}
                  className={`block w-full px-3 py-2 border rounded-lg ${
                    errors.branchName ? 'border-red-300' : 'border-gray-300'
                  }`}
                  placeholder="Branch location"
                />
                {errors.branchName && (
                  <p className="mt-1 text-xs text-red-600">{errors.branchName.message}</p>
                )}
              </div>

              {/* Account Type */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Account Type <span className="text-red-500">*</span>
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <CreditCard className="h-5 w-5 text-gray-400" />
                  </div>
                  <select
                    {...register('accountType')}
                    className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                      errors.accountType ? 'border-red-300' : 'border-gray-300'
                    }`}
                  >
                    <option value="">Select type</option>
                    <option value="SAVINGS">Savings</option>
                    <option value="CURRENT">Current</option>
                  </select>
                </div>
                {errors.accountType && (
                  <p className="mt-1 text-xs text-red-600">{errors.accountType.message}</p>
                )}
              </div>
            </div>

            {/* Cancelled Cheque */}
            <Controller
              name="cancelledChequeUrl"
              control={control}
              render={({ field }) => (
                <DocumentUploader
                  label="Upload Cancelled Cheque"
                  category="image"
                  accept="image/*"
                  onUploadComplete={(url) => field.onChange(url)}
                  onRemove={() => field.onChange('')}
                  error={errors.cancelledChequeUrl?.message}
                  required={true}
                  helperText="Photo of cancelled cheque or bank statement with account details"
                />
              )}
            />
          </>
        )}
      </div>

      {/* Actions */}
      <div className="flex gap-3 pt-4">
        <button
          type="button"
          onClick={onBack}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
        >
          Back
        </button>
        <button
          type="submit"
          className="flex-1 px-4 py-2 border border-transparent rounded-lg text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
        >
          Complete Registration
        </button>
      </div>
    </form>
  );
};

Step6_BankingDetails.propTypes = {
  data: PropTypes.object,
  onNext: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired,
};

export default Step6_BankingDetails;
