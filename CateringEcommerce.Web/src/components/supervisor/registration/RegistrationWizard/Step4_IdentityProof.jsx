/**
 * Step 4: Identity Proof (REDESIGNED)
 * ID proof, Address proof, Photo upload
 * Uses FileUploader in file mode for FormData upload
 */

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import PropTypes from 'prop-types';
import { FileText, Shield, ArrowRight, ArrowLeft, X } from 'lucide-react';
import { identityProofSchema } from '../../../../utils/supervisor/validationSchemas';
import FileUploader from '../../../common/FileUploader';

/** Reusable preview for uploaded file (same pattern as owner Step4) */
const FilePreview = ({ file, onRemove }) => {
    if (!file || !file.name) return null;
    const isImage = file.type?.startsWith('image/');
    return (
        <div className="mt-2 border-2 border-green-200 rounded-xl p-2.5 flex items-center justify-between bg-green-50">
            <div className="flex items-center gap-3">
                {isImage ? (
                    <img src={file.previewUrl} alt="preview" className="w-10 h-10 object-cover rounded-lg" />
                ) : (
                    <div className="w-10 h-10 bg-red-100 text-red-600 flex items-center justify-center rounded-lg font-bold text-xs">PDF</div>
                )}
                <span className="text-sm text-neutral-700 truncate max-w-[200px]">{file.name}</span>
            </div>
            <button type="button" onClick={onRemove} className="p-1 text-neutral-400 hover:text-red-600 transition-colors">
                <X className="w-4 h-4" />
            </button>
        </div>
    );
};

const Step4_IdentityProof = ({ data, onNext, onBack }) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
        setValue,
        watch,
    } = useForm({
        resolver: zodResolver(identityProofSchema),
        defaultValues: data,
    });

    const idProofFile = watch('idProofFile');
    const addressProofFile = watch('addressProofFile');
    const photoFile = watch('photoFile');

    const handleFileChange = (fieldName, fileData) => {
        setValue(fieldName, fileData, { shouldValidate: true });
    };

    const onSubmit = (formData) => {
        onNext(formData);
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Section Header */}
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center">
                    <Shield className="w-5 h-5 text-amber-600" />
                </div>
                <div>
                    <h2 className="text-xl font-bold text-neutral-800">Identity Verification</h2>
                    <p className="text-sm text-neutral-500">Upload your documents for verification</p>
                </div>
            </div>

            {/* Info Box */}
            <div className="bg-amber-50 border-l-4 border-amber-400 rounded-lg p-4">
                <p className="text-sm text-amber-800">All documents are securely stored and used only for verification purposes.</p>
            </div>

            <div className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    {/* ID Proof Type */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            ID Proof Type <span className="text-rose-500">*</span>
                        </label>
                        <div className="relative">
                            <FileText className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                            <select
                                {...register('idProofType')}
                                className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 appearance-none bg-white ${
                                    errors.idProofType ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                                } focus:outline-none`}
                            >
                                <option value="">Select ID proof type</option>
                                <option value="AADHAAR">Aadhaar Card</option>
                                <option value="PAN">PAN Card</option>
                                <option value="VOTER_ID">Voter ID</option>
                                <option value="PASSPORT">Passport</option>
                                <option value="DRIVING_LICENSE">Driving License</option>
                            </select>
                        </div>
                        {errors.idProofType && <p className="text-xs text-red-600 mt-1.5">{errors.idProofType.message}</p>}
                    </div>

                    {/* ID Proof Number */}
                    <div>
                        <label className="block text-sm font-semibold text-neutral-800 mb-2">
                            ID Proof Number <span className="text-rose-500">*</span>
                        </label>
                        <input
                            type="text"
                            {...register('idProofNumber')}
                            className={`w-full px-4 py-3 border-2 rounded-xl uppercase transition-all duration-200 ${
                                errors.idProofNumber ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'
                            } focus:outline-none`}
                            placeholder="Enter ID number"
                        />
                        {errors.idProofNumber && <p className="text-xs text-red-600 mt-1.5">{errors.idProofNumber.message}</p>}
                    </div>
                </div>

                {/* ID Proof Upload */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Upload ID Proof <span className="text-rose-500">*</span>
                    </label>
                    <div className="h-16 border-2 border-dashed border-neutral-200 rounded-xl flex items-center justify-center hover:border-rose-300 transition-colors">
                        <FileUploader onFileCropped={(file) => handleFileChange('idProofFile', file)} aspect={1.586} returnType="file" />
                    </div>
                    <FilePreview file={idProofFile} onRemove={() => handleFileChange('idProofFile', {})} />
                    {errors.idProofFile && <p className="text-xs text-red-600 mt-1.5">{errors.idProofFile.message}</p>}
                    <p className="text-xs text-neutral-500 mt-1.5">Upload clear photo of your ID proof (front & back if applicable)</p>
                </div>

                {/* Address Proof Upload */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Upload Address Proof <span className="text-rose-500">*</span>
                    </label>
                    <div className="h-16 border-2 border-dashed border-neutral-200 rounded-xl flex items-center justify-center hover:border-rose-300 transition-colors">
                        <FileUploader onFileCropped={(file) => handleFileChange('addressProofFile', file)} returnType="file" />
                    </div>
                    <FilePreview file={addressProofFile} onRemove={() => handleFileChange('addressProofFile', {})} />
                    {errors.addressProofFile && <p className="text-xs text-red-600 mt-1.5">{errors.addressProofFile.message}</p>}
                    <p className="text-xs text-neutral-500 mt-1.5">Utility bill, bank statement, or rental agreement</p>
                </div>

                {/* Photo Upload */}
                <div>
                    <label className="block text-sm font-semibold text-neutral-800 mb-2">
                        Upload Your Photo <span className="text-rose-500">*</span>
                    </label>
                    <div className="h-16 border-2 border-dashed border-neutral-200 rounded-xl flex items-center justify-center hover:border-rose-300 transition-colors">
                        <FileUploader onFileCropped={(file) => handleFileChange('photoFile', file)} aspect={1} returnType="file" />
                    </div>
                    <FilePreview file={photoFile} onRemove={() => handleFileChange('photoFile', {})} />
                    {errors.photoFile && <p className="text-xs text-red-600 mt-1.5">{errors.photoFile.message}</p>}
                    <p className="text-xs text-neutral-500 mt-1.5">Recent passport-size photo with clear face</p>
                </div>
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

Step4_IdentityProof.propTypes = {
    data: PropTypes.object,
    onNext: PropTypes.func.isRequired,
    onBack: PropTypes.func.isRequired,
};

export default Step4_IdentityProof;
