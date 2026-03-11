/**
 * Step 3: Experience Details (REDESIGNED)
 * Prior experience in catering/hospitality
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import PropTypes from 'prop-types';
import { Briefcase, ArrowRight, ArrowLeft, CheckCircle } from 'lucide-react';
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
            {/* Section Header */}
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                    <Briefcase className="w-5 h-5 text-purple-600" />
                </div>
                <div>
                    <h2 className="text-xl font-bold text-neutral-800">Experience Details</h2>
                    <p className="text-sm text-neutral-500">Tell us about your background</p>
                </div>
            </div>

            <div className="space-y-5">
                {/* Has Prior Experience */}
                <div className="bg-neutral-50 rounded-xl p-4 border-2 border-neutral-100">
                    <label className="flex items-center gap-3 cursor-pointer">
                        <input
                            type="checkbox"
                            {...register('hasPriorExperience')}
                            onChange={(e) => setHasPriorExperience(e.target.checked)}
                            className="h-5 w-5 rounded border-2 border-neutral-300 text-rose-600 focus:ring-rose-500"
                        />
                        <span className="text-sm font-semibold text-neutral-800">
                            I have prior experience in catering, hospitality, or event management
                        </span>
                    </label>
                </div>

                {/* Experience Details (conditional) */}
                {hasPriorExperience && (
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            Experience Details <span className="text-rose-500">*</span>
                        </label>
                        <div className="relative">
                            <Briefcase className="absolute left-3.5 top-3 w-5 h-5 text-neutral-400" />
                            <textarea
                                {...register('priorExperienceDetails')}
                                rows={6}
                                className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${
                                    errors.priorExperienceDetails ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                } focus:outline-none`}
                                placeholder={`Please describe your experience in detail:\n- Previous roles and responsibilities\n- Years of experience\n- Types of events handled\n- Skills acquired`}
                            />
                        </div>
                        {errors.priorExperienceDetails && <p className="text-xs text-red-600 mt-1.5">{errors.priorExperienceDetails.message}</p>}
                        <p className="text-xs text-neutral-500 mt-1.5">Minimum 20 characters required</p>
                    </div>
                )}

                {!hasPriorExperience && (
                    <div className="bg-green-50 border-l-4 border-green-400 rounded-lg p-5">
                        <div className="flex items-start gap-3">
                            <CheckCircle className="w-5 h-5 text-green-600 mt-0.5 flex-shrink-0" />
                            <div>
                                <p className="text-sm font-semibold text-green-900">No worries!</p>
                                <p className="text-sm text-green-800 mt-1">
                                    We provide comprehensive training for new supervisors. You'll learn everything you need to successfully supervise catering events.
                                </p>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Actions */}
            <div className="flex gap-4 pt-4">
                <button type="button" onClick={onBack} className="flex-1 px-6 py-3.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200 flex items-center justify-center gap-2">
                    <ArrowLeft className="w-4 h-4" /> Back
                </button>
                <button type="submit" className="flex-1 px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all duration-200 flex items-center justify-center gap-2">
                    Next Step <ArrowRight className="w-4 h-4" />
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
