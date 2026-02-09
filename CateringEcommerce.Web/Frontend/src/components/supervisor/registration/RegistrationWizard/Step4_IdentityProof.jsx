/**
 * Step 4: Identity Proof
 * ID proof, Address proof, Photo upload
 */

import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import PropTypes from 'prop-types';
import { FileText } from 'lucide-react';
import { identityProofSchema } from '../../../../utils/supervisor/validationSchemas';
import { DocumentUploader } from '../../common/forms';

const Step4_IdentityProof = ({ data, onNext, onBack }) => {
  const [uploading, setUploading] = useState(false);

  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm({
    resolver: zodResolver(identityProofSchema),
    defaultValues: data,
  });

  const onSubmit = (formData) => {
    if (uploading) return;
    onNext(formData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Identity Verification</h2>
        <p className="text-sm text-gray-600 mt-2">
          Upload your documents for verification
        </p>
      </div>

      <div className="space-y-6">
        {/* ID Proof Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            ID Proof Type <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <FileText className="h-5 w-5 text-gray-400" />
            </div>
            <select
              {...register('idProofType')}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.idProofType ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
            >
              <option value="">Select ID proof type</option>
              <option value="AADHAAR">Aadhaar Card</option>
              <option value="PAN">PAN Card</option>
              <option value="VOTER_ID">Voter ID</option>
              <option value="PASSPORT">Passport</option>
              <option value="DRIVING_LICENSE">Driving License</option>
            </select>
          </div>
          {errors.idProofType && (
            <p className="mt-1 text-sm text-red-600">{errors.idProofType.message}</p>
          )}
        </div>

        {/* ID Proof Number */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            ID Proof Number <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            {...register('idProofNumber')}
            className={`
              block w-full px-3 py-2 border rounded-lg uppercase
              focus:ring-2 focus:ring-blue-500 focus:border-blue-500
              ${errors.idProofNumber ? 'border-red-300 bg-red-50' : 'border-gray-300'}
            `}
            placeholder="Enter ID number"
          />
          {errors.idProofNumber && (
            <p className="mt-1 text-sm text-red-600">{errors.idProofNumber.message}</p>
          )}
        </div>

        {/* ID Proof Upload */}
        <Controller
          name="idProofUrl"
          control={control}
          render={({ field }) => (
            <DocumentUploader
              label="Upload ID Proof"
              category="image"
              accept="image/*"
              onUploadComplete={(url) => {
                field.onChange(url);
                setUploading(false);
              }}
              onRemove={() => field.onChange('')}
              error={errors.idProofUrl?.message}
              required={true}
              helperText="Upload clear photo of your ID proof (front & back if applicable)"
            />
          )}
        />

        {/* Address Proof Upload */}
        <Controller
          name="addressProofUrl"
          control={control}
          render={({ field }) => (
            <DocumentUploader
              label="Upload Address Proof"
              category="image"
              accept="image/*"
              onUploadComplete={(url) => {
                field.onChange(url);
                setUploading(false);
              }}
              onRemove={() => field.onChange('')}
              error={errors.addressProofUrl?.message}
              required={true}
              helperText="Utility bill, bank statement, or rental agreement"
            />
          )}
        />

        {/* Photo Upload */}
        <Controller
          name="photoUrl"
          control={control}
          render={({ field }) => (
            <DocumentUploader
              label="Upload Your Photo"
              category="image"
              accept="image/*"
              onUploadComplete={(url) => {
                field.onChange(url);
                setUploading(false);
              }}
              onRemove={() => field.onChange('')}
              error={errors.photoUrl?.message}
              required={true}
              helperText="Recent passport-size photo with clear face"
            />
          )}
        />
      </div>

      {/* Actions */}
      <div className="flex gap-3 pt-4">
        <button
          type="button"
          onClick={onBack}
          disabled={uploading}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
        >
          Back
        </button>
        <button
          type="submit"
          disabled={uploading}
          className="flex-1 px-4 py-2 border border-transparent rounded-lg text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50"
        >
          {uploading ? 'Uploading...' : 'Next Step'}
        </button>
      </div>
    </form>
  );
};

Step4_IdentityProof.propTypes = {
  data: PropTypes.object,
  onNext: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired,
};

export default Step4_IdentityProof;
