/**
 * Step 1: Personal Details
 * Name, Email, Phone, Date of Birth
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import PropTypes from 'prop-types';
import { User, Mail, Phone, Calendar } from 'lucide-react';
import { personalDetailsSchema } from '../../../../utils/supervisor/validationSchemas';

const Step1_PersonalDetails = ({ data, onNext, onBack }) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(personalDetailsSchema),
    defaultValues: data,
  });

  const onSubmit = (formData) => {
    onNext(formData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="text-center mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Personal Details</h2>
        <p className="text-sm text-gray-600 mt-2">
          Let's start with your basic information
        </p>
      </div>

      <div className="space-y-4">
        {/* First Name */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            First Name <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <User className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              {...register('firstName')}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.firstName ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
              placeholder="Enter your first name"
            />
          </div>
          {errors.firstName && (
            <p className="mt-1 text-sm text-red-600">{errors.firstName.message}</p>
          )}
        </div>

        {/* Last Name */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Last Name <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <User className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              {...register('lastName')}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.lastName ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
              placeholder="Enter your last name"
            />
          </div>
          {errors.lastName && (
            <p className="mt-1 text-sm text-red-600">{errors.lastName.message}</p>
          )}
        </div>

        {/* Email */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Email Address <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Mail className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="email"
              {...register('email')}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.email ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
              placeholder="your.email@example.com"
            />
          </div>
          {errors.email && (
            <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
          )}
        </div>

        {/* Phone */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Phone Number <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Phone className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="tel"
              {...register('phone')}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.phone ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
              placeholder="10-digit mobile number"
              maxLength={10}
            />
          </div>
          {errors.phone && (
            <p className="mt-1 text-sm text-red-600">{errors.phone.message}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            Enter 10-digit mobile number without country code
          </p>
        </div>

        {/* Date of Birth */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Date of Birth <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Calendar className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="date"
              {...register('dateOfBirth', {
                valueAsDate: true,
              })}
              className={`
                block w-full pl-10 pr-3 py-2 border rounded-lg
                focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                ${errors.dateOfBirth ? 'border-red-300 bg-red-50' : 'border-gray-300'}
              `}
              max={new Date(new Date().setFullYear(new Date().getFullYear() - 18))
                .toISOString()
                .split('T')[0]}
            />
          </div>
          {errors.dateOfBirth && (
            <p className="mt-1 text-sm text-red-600">{errors.dateOfBirth.message}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            You must be at least 18 years old
          </p>
        </div>
      </div>

      {/* Actions */}
      <div className="flex gap-3 pt-4">
        {onBack && (
          <button
            type="button"
            onClick={onBack}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
          >
            Back
          </button>
        )}
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

Step1_PersonalDetails.propTypes = {
  data: PropTypes.object,
  onNext: PropTypes.func.isRequired,
  onBack: PropTypes.func,
};

export default Step1_PersonalDetails;
