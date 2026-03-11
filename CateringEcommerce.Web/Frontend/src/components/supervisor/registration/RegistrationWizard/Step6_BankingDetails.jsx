/**
 * Step 6: Banking Details (REDESIGNED)
 * Bank account for payment - can be submitted later
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import PropTypes from 'prop-types';
import { Building2, CreditCard, ArrowLeft, CheckCircle, X } from 'lucide-react';
import { bankingDetailsSchema } from '../../../../utils/supervisor/validationSchemas';
import FileUploader from '../../../common/FileUploader';

/* ------------------------------
   Preview Component (Updated)
-------------------------------- */
const FilePreview = ({ file, onRemove }) => {
    if (!file || !file.name) return null;

    const isImage = file.type?.startsWith('image/');

    return (
        <div className="mt-3 border-2 border-green-200 rounded-xl p-3 flex items-center justify-between bg-green-50">
            <div className="flex items-center gap-3">
                {isImage ? (
                    <img
                        src={file.previewUrl}
                        alt="preview"
                        className="w-12 h-12 object-cover rounded-lg"
                    />
                ) : (
                    <div className="w-12 h-12 bg-red-100 text-red-600 flex items-center justify-center rounded-lg font-bold text-xs">
                        PDF
                    </div>
                )}
                <span className="text-sm text-neutral-700">
                    Uploaded File
                </span>
            </div>

            <button
                type="button"
                onClick={onRemove}
                className="p-1 text-neutral-400 hover:text-red-600 transition-colors"
            >
                <X className="w-4 h-4" />
            </button>
        </div>
    );
};

const Step6_BankingDetails = ({ data, onNext, onBack }) => {
    const [skipBanking, setSkipBanking] = useState(false);

    const {
        register,
        handleSubmit,
        formState: { errors },
        setValue,
        watch,
    } = useForm({
        resolver: skipBanking ? undefined : zodResolver(bankingDetailsSchema),
        defaultValues: {
            ...data,
            cancelledChequeUrl: data?.cancelledChequeUrl || {},
        },
    });

    const cancelledChequeFile = watch('cancelledChequeUrl');

    /* ✅ FIXED: Store only base64 string */
    const handleFileChange = (fileData) => {
        setValue('cancelledChequeUrl', fileData || {}, { shouldValidate: true });
    };

    const onSubmit = (formData) => {
        if (skipBanking) {
            onNext({ skipBanking: true });
        } else {
            onNext(formData);
        }
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Section Header */}
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 bg-indigo-100 rounded-lg flex items-center justify-center">
                    <Building2 className="w-5 h-5 text-indigo-600" />
                </div>
                <div>
                    <h2 className="text-xl font-bold text-neutral-800">Banking Details</h2>
                    <p className="text-sm text-neutral-500">For receiving payments (can be added later)</p>
                </div>
            </div>

            <div className="space-y-5">
                {/* Skip Banking Option */}
                <div className="bg-blue-50 border-l-4 border-blue-400 rounded-lg p-4">
                    <label className="flex items-center gap-3 cursor-pointer">
                        <input
                            type="checkbox"
                            checked={skipBanking}
                            onChange={(e) => setSkipBanking(e.target.checked)}
                            className="h-5 w-5 rounded border-2 border-blue-300 text-blue-600 focus:ring-blue-500"
                        />
                        <div>
                            <span className="text-sm font-semibold text-blue-900">Add banking details later</span>
                            <p className="text-xs text-blue-700 mt-0.5">You can add this before receiving your first payment</p>
                        </div>
                    </label>
                </div>

                {!skipBanking && (
                    <>
                        {/* Account Holder Name */}
                        <div>
                            <label className="block text-sm font-semibold text-neutral-800 mb-2">
                                Account Holder Name <span className="text-rose-500">*</span>
                            </label>
                            <input
                                type="text"
                                {...register('accountHolderName')}
                                className={`w-full px-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.accountHolderName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                    } focus:outline-none`}
                                placeholder="As per bank records"
                            />
                            {errors.accountHolderName && <p className="text-xs text-red-600 mt-1.5">{errors.accountHolderName.message}</p>}
                        </div>

                        {/* Bank Name */}
                        <div>
                            <label className="block text-sm font-semibold text-neutral-800 mb-2">
                                Bank Name <span className="text-rose-500">*</span>
                            </label>
                            <div className="relative">
                                <Building2 className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                                <input
                                    type="text"
                                    {...register('bankName')}
                                    className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.bankName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                        } focus:outline-none`}
                                    placeholder="e.g., HDFC Bank"
                                />
                            </div>
                            {errors.bankName && <p className="text-xs text-red-600 mt-1.5">{errors.bankName.message}</p>}
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                            {/* Account Number */}
                            <div>
                                <label className="block text-sm font-semibold text-neutral-800 mb-2">
                                    Account Number <span className="text-rose-500">*</span>
                                </label>
                                <input
                                    type="text"
                                    {...register('accountNumber')}
                                    className={`w-full px-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.accountNumber ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                        } focus:outline-none`}
                                    placeholder="Enter account number"
                                />
                                {errors.accountNumber && <p className="text-xs text-red-600 mt-1.5">{errors.accountNumber.message}</p>}
                            </div>

                            {/* IFSC Code */}
                            <div>
                                <label className="block text-sm font-semibold text-neutral-800 mb-2">
                                    IFSC Code <span className="text-rose-500">*</span>
                                </label>
                                <input
                                    type="text"
                                    {...register('ifscCode')}
                                    className={`w-full px-4 py-3 border-2 rounded-xl uppercase transition-all duration-200 ${errors.ifscCode ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                        } focus:outline-none`}
                                    placeholder="e.g., HDFC0001234"
                                    maxLength={11}
                                />
                                {errors.ifscCode && <p className="text-xs text-red-600 mt-1.5">{errors.ifscCode.message}</p>}
                            </div>

                            {/* Branch Name */}
                            <div>
                                <label className="block text-sm font-semibold text-neutral-800 mb-2">
                                    Branch Name <span className="text-rose-500">*</span>
                                </label>
                                <input
                                    type="text"
                                    {...register('branchName')}
                                    className={`w-full px-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.branchName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                        } focus:outline-none`}
                                    placeholder="Branch location"
                                />
                                {errors.branchName && <p className="text-xs text-red-600 mt-1.5">{errors.branchName.message}</p>}
                            </div>

                            {/* Account Type */}
                            <div>
                                <label className="block text-sm font-semibold text-neutral-800 mb-2">
                                    Account Type <span className="text-rose-500">*</span>
                                </label>
                                <div className="relative">
                                    <CreditCard className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                                    <select
                                        {...register('accountType')}
                                        className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl appearance-none bg-white transition-all duration-200 ${errors.accountType ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                            } focus:outline-none`}
                                    >
                                        <option value="">Select type</option>
                                        <option value="SAVINGS">Savings</option>
                                        <option value="CURRENT">Current</option>
                                    </select>
                                </div>
                                {errors.accountType && <p className="text-xs text-red-600 mt-1.5">{errors.accountType.message}</p>}
                            </div>
                        </div>

                        {/* Upload Cancelled Cheque */}
                        <div>
                            <label className="block text-sm font-semibold mb-2">
                                Upload Cancelled Cheque *
                            </label>

                            <div className="h-52 border-2 border-dashed border-neutral-200 rounded-xl flex items-center justify-center hover:border-rose-300 transition-colors">
                                <FileUploader
                                    onFileCropped={handleFileChange}
                                    aspect={4.5}
                                    returnType="file"
                                />
                            </div>

                            {/* ✅ Preview */}
                            <FilePreview
                                file={cancelledChequeFile}
                                onRemove={() =>
                                    setValue('cancelledChequeUrl', {}, {
                                        shouldValidate: true,
                                    })
                                }
                            />

                            {errors.cancelledChequeUrl && (
                                <p className="text-xs text-red-600 mt-1.5">
                                    {errors.cancelledChequeUrl.message}
                                </p>
                            )}

                            <p className="text-xs text-neutral-500 mt-1.5">
                                Photo of cancelled cheque or bank statement with account details
                            </p>
                        </div>
                    </>
                )}
                </div>

                {/* Actions */}
                <div className="flex gap-4 pt-4">
                    <button
                        type="button"
                        onClick={onBack}
                        className="flex-1 px-6 py-3 bg-white border rounded-xl"
                    >
                        <ArrowLeft className="w-4 h-4 inline mr-2" />
                        Back
                    </button>

                    <button
                        type="submit"
                        className="flex-1 px-6 py-3 bg-green-600 text-white rounded-xl"
                    >
                        <CheckCircle className="w-4 h-4 inline mr-2" />
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
