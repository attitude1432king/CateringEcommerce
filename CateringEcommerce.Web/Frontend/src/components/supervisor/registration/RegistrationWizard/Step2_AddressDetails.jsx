/**
 * Step 2: Address Details
 * Address, Preferred Zone
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import PropTypes from 'prop-types';
import { MapPin } from 'lucide-react';
import { addressDetailsSchema } from '../../../../utils/supervisor/validationSchemas';

const Step2_AddressDetails = ({ data, onNext, onBack }) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(addressDetailsSchema),
    defaultValues: data,
  });

  return (
    <form onSubmit={handleSubmit(onNext)} className="space-y-6">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Address Details</h2>
        <p className="text-sm text-gray-600 mt-2">
          Where are you located?
        </p>
      </div>

      <div className="space-y-4">
        {/* Address */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Complete Address <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute top-3 left-3 pointer-events-none">
              <MapPin className="h-5 w-5 text-gray-400" />
            </div>
            <textarea
              {...register('address')}
              rows={4}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.address ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
              placeholder="Enter your complete address including street, landmark, city, state, pincode"
            />
          </div>
          {errors.address && (
            <p className="mt-1 text-sm text-red-600">{errors.address.message}</p>
          )}
        </div>

        {/* Preferred Zone */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Preferred Work Zone
          </label>
          <select
            {...register('preferredZoneId', { valueAsNumber: true })}
            className={`
              block w-full px-3 py-2 border rounded-lg
              focus:ring-2 focus:ring-blue-500 focus:border-blue-500
              ${errors.preferredZoneId ? 'border-red-300 bg-red-50' : 'border-gray-300'}
            `}
          >
            <option value="">Select a zone (optional)</option>
            <option value={1}>North Bangalore</option>
            <option value={2}>South Bangalore</option>
            <option value={3}>East Bangalore</option>
            <option value={4}>West Bangalore</option>
            <option value={5}>Central Bangalore</option>
          </select>
          {errors.preferredZoneId && (
            <p className="mt-1 text-sm text-red-600">{errors.preferredZoneId.message}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            Select your preferred area for event assignments
          </p>
        </div>
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
          Next Step
        </button>
      </div>
    </form>
  );
};

Step2_AddressDetails.propTypes = {
  data: PropTypes.object,
  onNext: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired,
};

export default Step2_AddressDetails;
