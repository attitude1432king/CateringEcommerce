/**
 * Step 3: Experience Details
 * Prior experience in catering/hospitality
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import PropTypes from 'prop-types';
import { Briefcase } from 'lucide-react';
import { experienceDetailsSchema } from '../../../../utils/supervisor/validationSchemas';


const Step3_ExperienceDetails = ({ data, onNext, onBack }) => {
  const [hasPriorExperience, setHasPriorExperience] = useState(data?.hasPriorExperience || false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(experienceDetailsSchema),
    defaultValues: data,
  });

  return (
    <form onSubmit={handleSubmit(onNext)} className="space-y-6">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Experience Details</h2>
        <p className="text-sm text-gray-600 mt-2">
          Tell us about your background
        </p>
      </div>

      <div className="space-y-4">
        {/* Has Prior Experience */}
        <div className="flex items-start">
          <div className="flex items-center h-5">
            <input
              type="checkbox"
              {...register('hasPriorExperience')}
              onChange={(e) => setHasPriorExperience(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
          </div>
          <div className="ml-3">
            <label className="text-sm font-medium text-gray-700">
              I have prior experience in catering, hospitality, or event management
            </label>
          </div>
        </div>

        {/* Experience Details (conditional) */}
        {hasPriorExperience && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Experience Details <span className="text-red-500">*</span>
            </label>
            <div className="relative">
              <div className="absolute top-3 left-3 pointer-events-none">
                <Briefcase className="h-5 w-5 text-gray-400" />
              </div>
              <textarea
                {...register('priorExperienceDetails')}
                rows={6}
                className={`
                  block w-full pl-10 pr-3 py-2 border rounded-lg
                  focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                  ${errors.priorExperienceDetails ? 'border-red-300 bg-red-50' : 'border-gray-300'}
                `}
                placeholder="Please describe your experience in detail:&#10;- Previous roles and responsibilities&#10;- Years of experience&#10;- Types of events handled&#10;- Skills acquired"
              />
            </div>
            {errors.priorExperienceDetails && (
              <p className="mt-1 text-sm text-red-600">{errors.priorExperienceDetails.message}</p>
            )}
            <p className="mt-1 text-xs text-gray-500">
              Minimum 20 characters required
            </p>
          </div>
        )}

        {!hasPriorExperience && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <p className="text-sm text-blue-800">
              No worries! We provide comprehensive training for new supervisors. You'll learn everything you need to successfully supervise catering events.
            </p>
          </div>
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
          Next Step
        </button>
      </div>
    </form>
  );
};

Step3_ExperienceDetails.propTypes = {
  data: PropTypes.object,
  onNext: PropTypes.func.isRequired,
  onBack: PropTypes.func.isRequired,
};

export default Step3_ExperienceDetails;
