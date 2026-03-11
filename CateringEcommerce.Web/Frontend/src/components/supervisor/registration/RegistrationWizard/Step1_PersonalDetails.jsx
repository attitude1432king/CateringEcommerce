/**
 * Step 1: Personal Details (REDESIGNED)
 * Name, Email, Phone, Date of Birth
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import PropTypes from 'prop-types';
import { User, Mail, Phone, Calendar, ArrowRight } from 'lucide-react';
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

    const onSubmit = (formData) => { onNext(formData); };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Section Header */}
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center">
                    <User className="w-5 h-5 text-rose-600" />
                </div>
                <div>
                    <h2 className="text-xl font-bold text-neutral-800">Personal Details</h2>
                    <p className="text-sm text-neutral-500">Let's start with your basic information</p>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                {/* First Name */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        First Name <span className="text-rose-500">*</span>
                    </label>
                    <div className="relative">
                        <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                        <input
                            type="text"
                            {...register('firstName')}
                            className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                errors.firstName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                            } focus:outline-none`}
                            placeholder="Enter your first name"
                        />
                    </div>
                    {errors.firstName && <p className="text-xs text-red-600 mt-1.5">{errors.firstName.message}</p>}
                </div>

                {/* Last Name */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Last Name <span className="text-rose-500">*</span>
                    </label>
                    <div className="relative">
                        <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                        <input
                            type="text"
                            {...register('lastName')}
                            className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                errors.lastName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                            } focus:outline-none`}
                            placeholder="Enter your last name"
                        />
                    </div>
                    {errors.lastName && <p className="text-xs text-red-600 mt-1.5">{errors.lastName.message}</p>}
                </div>
            </div>

            {/* Email */}
            <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">
                    Email Address <span className="text-rose-500">*</span>
                </label>
                <div className="relative">
                    <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                    <input
                        type="email"
                        {...register('email')}
                        className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                            errors.email ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                        } focus:outline-none`}
                        placeholder="your.email@example.com"
                    />
                </div>
                {errors.email && <p className="text-xs text-red-600 mt-1.5">{errors.email.message}</p>}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                {/* Phone */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Phone Number <span className="text-rose-500">*</span>
                    </label>
                    <div className="relative">
                        <Phone className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                        <input
                            type="tel"
                            {...register('phone')}
                            className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                errors.phone ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                            } focus:outline-none`}
                            placeholder="10-digit mobile number"
                            maxLength={10}
                        />
                    </div>
                    {errors.phone && <p className="text-xs text-red-600 mt-1.5">{errors.phone.message}</p>}
                    <p className="text-xs text-neutral-500 mt-1">Enter 10-digit mobile number without country code</p>
                </div>

                {/* Date of Birth */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Date of Birth <span className="text-rose-500">*</span>
                    </label>
                    <div className="relative">
                        <Calendar className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                        <input
                            type="date"
                            {...register('dateOfBirth', { valueAsDate: true })}
                            className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                errors.dateOfBirth ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                            } focus:outline-none`}
                            max={new Date(new Date().setFullYear(new Date().getFullYear() - 18)).toISOString().split('T')[0]}
                        />
                    </div>
                    {errors.dateOfBirth && <p className="text-xs text-red-600 mt-1.5">{errors.dateOfBirth.message}</p>}
                    <p className="text-xs text-neutral-500 mt-1">You must be at least 18 years old</p>
                </div>
            </div>

            {/* Actions */}
            <div className="flex gap-4 pt-4">
                {onBack && (
                    <button type="button" onClick={onBack} className="flex-1 px-6 py-3.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200">
                        Back
                    </button>
                )}
                <button type="submit" className="flex-1 px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-200 flex items-center justify-center gap-2">
                    Next Step <ArrowRight className="w-4 h-4" />
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
