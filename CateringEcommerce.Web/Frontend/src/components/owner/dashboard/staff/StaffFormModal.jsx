/*
========================================
File: src/components/owner/dashboard/staff/StaffFormModal.jsx (REVISED)
========================================
Implements all the new conditional logic for roles and expertise.
*/
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useToast } from '../../../../contexts/ToastContext';
import SingleFileUploader from '../../../common/SingleFileUploader';
import ToggleSwitch from '../../../common/ToggleSwitch';
import { PREDEFINED_ROLES_DATA, FOOD_RELATED_ROLES_DATA } from '../../../../utils/staticDropDownData';

const RequiredAsterisk = () => <span className="text-red-500 ml-1">*</span>;
const ValidationError = ({ message }) => message ? <p className="text-red-500 text-xs mt-1">{message}</p> : null;

// --- NEW LOGIC ---
const PREDEFINED_ROLES = PREDEFINED_ROLES_DATA;
const FOOD_RELATED_ROLES = FOOD_RELATED_ROLES_DATA;
// --- END NEW LOGIC ---

export default function StaffFormModal({ isOpen, onClose, onSave, editingItem, expertiseCategories }) {

    // The form state 'role' will hold the value from the dropdown (e.g., "Chef" or "Other")
    // 'otherRole' will hold the value from the "Other" text box
    // 'categoryId' will hold the ID (as a string) from the expertise dropdown
    const getInitialState = () => ({
        name: '',
        gender: 'Male',
        contact: '',
        role: '', // Role from dropdown
        otherRole: '', // Text for "Other" role
        categoryId: 0, // ID from expertise dropdown
        experience: 0,
        salaryType: 'Monthly',
        salaryAmount: '',
        availability: true,
        photo: [],
        idProof: [],
        resume: [],
    });
    const [formData, setFormData] = useState(getInitialState());
    const [errors, setErrors] = useState({});
    const [filesToDelete, setFilesToDelete] = useState([]);
    const { showToast } = useToast();

    // --- NEW LOGIC ---
    // Check if the currently selected role is a food-related one
    const isFoodRole = useMemo(() => {
        if (formData.role === 'Other') {
            // Check the "Other" text field for keywords
            const other = formData.otherRole.toLowerCase();
            return other.includes('chef') || other.includes('cook');
        }
        return FOOD_RELATED_ROLES.includes(formData.role);
    }, [formData.role, formData.otherRole]);
    // --- END NEW LOGIC ---


    useEffect(() => {
        if (isOpen) {
            const initialState = getInitialState();
            if (editingItem) {
                // This logic translates the editingItem's simple string fields 
                // into the complex state the form needs (e.g., handling "Other" role)
                const foundRole = PREDEFINED_ROLES.find(r => r === editingItem.role);
                const roleValue = foundRole ? foundRole : 'Other';
                const otherRoleValue = foundRole ? '' : (editingItem.role || '');

                const foundCategory = expertiseCategories.find(c => c.name === editingItem.expertise);
                const categoryIdValue = foundCategory ? foundCategory.categoryId.toString() : 0;

                setFormData({
                    ...initialState,
                    ...editingItem,
                    role: roleValue,
                    otherRole: otherRoleValue,
                    categoryId: categoryIdValue,
                    photo: editingItem.photo || [],
                    idProof: editingItem.idProof || [],
                    resume: editingItem.resume || [],
                });
            } else {
                setFormData(initialState);
            }
            setErrors({});
        }
    }, [editingItem, isOpen, expertiseCategories]); // expertiseCategories is a dependency

    const validate = useCallback(() => {
        const newErrors = {};
        if (!formData.name.trim()) newErrors.name = 'Staff Name is required.';
        if (!formData.contact.trim()) newErrors.contact = 'Contact number is required.';
        if (!/^\d{10}$/.test(formData.contact)) newErrors.contact = 'Please enter a valid 10-digit contact number.';
        if (!formData.role) newErrors.role = 'Role is required.';

        // --- NEW LOGIC ---
        if (formData.role === 'Other' && !formData.otherRole.trim()) {
            newErrors.otherRole = 'Please specify the role.';
        }
        if (isFoodRole && formData.categoryId <= 0) {
            newErrors.categoryId = 'Expertise is required for this role.';
        }
        // --- END NEW LOGIC ---

        if (!formData.salaryAmount || formData.salaryAmount <= 0) newErrors.salaryAmount = 'A valid salary is required.';
        if (formData.photo.length === 0) newErrors.photo = 'A photo is required.';

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    }, [formData, isFoodRole]); // isFoodRole is a dependency

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    // REVISED: This function now tracks deleted paths
    const handleFileChange = (name, media) => {
        // Check if a file is being removed
        if (media.length === 0) {
            const fileToRemove = formData[name]?.[0];
            // If the removed file has a 'path', it's an existing file.
            if (fileToRemove && fileToRemove.path) {
                setFilesToDelete(prev => [...prev, fileToRemove.path]);
            }
        }
        setFormData(prev => ({ ...prev, [name]: media }));
    };

    const handleToggle = (name, value) => setFormData(prev => ({ ...prev, [name]: value }));

    const handleSubmit = (e) => {
        e.preventDefault();
        if (validate()) {
            // Translate form state back into the simple format for the API
            const finalRole = formData.role === 'Other' ? formData.otherRole : formData.role;
            const expertiseCategory = expertiseCategories.find(c => c.categoryId.toString() === formData.categoryId);
            const finalExpertise = isFoodRole && expertiseCategory ? expertiseCategory.name : null;

            const dataToSave = {
                ...formData,
                role: finalRole, // Final, simplified string
                expertise: finalExpertise, // Final, simplified string
            };

            // Clean up temporary form fields
            delete dataToSave.role;
            delete dataToSave.otherRole;
            delete dataToSave.categoryId;

            onSave(formData, filesToDelete);
        } else {
            showToast('Please fill all required fields correctly.', 'error');
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50 p-4">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] flex flex-col">
                <h2 className="p-6 border-b text-2xl font-bold text-neutral-800">{editingItem ? 'Edit Staff Member' : 'Add New Staff Member'}</h2>
                <form onSubmit={handleSubmit} className="overflow-y-auto flex-1 p-6 space-y-6">

                    {/* Section 1: Basic Information */}
                    <fieldset>
                        <legend className="text-lg font-semibold text-rose-700 mb-3">Basic Information</legend>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            <div className="md:col-span-2">
                                <label htmlFor="name" className="block text-sm font-medium text-neutral-700">Full Name <RequiredAsterisk /></label>
                                <input type="text" name="name" id="name" value={formData.name} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.name ? 'border-red-500' : 'border-neutral-300'} rounded-md`} />
                                <ValidationError message={errors.name} />
                            </div>
                            <div>
                                <label htmlFor="gender" className="block text-sm font-medium text-neutral-700">Gender</label>
                                <select name="gender" id="gender" value={formData.gender} onChange={handleChange} autoComplete="off" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md">
                                    <option>Male</option>
                                    <option>Female</option>
                                    <option>Other</option>
                                </select>
                            </div>
                            <div className="md:col-span-3">
                                <label htmlFor="contact" className="block text-sm font-medium text-neutral-700">Contact Number <RequiredAsterisk /></label>
                                <input type="tel" name="contact" id="contact" value={formData.contact} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.contact ? 'border-red-500' : 'border-neutral-300'} rounded-md`} />
                                <ValidationError message={errors.contact} />
                            </div>
                        </div>
                    </fieldset>

                    {/* Section 2: Job Details */}
                    <fieldset>
                        <legend className="text-lg font-semibold text-rose-700 mb-3">Job Details</legend>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label htmlFor="role" className="block text-sm font-medium text-neutral-700">Role / Post <RequiredAsterisk /></label>
                                <select name="role" id="role" value={formData.role} onChange={handleChange} className={`mt-1 block w-full px-3 py-2 border ${errors.role ? 'border-red-500' : 'border-neutral-300'} rounded-md`}>
                                    <option value="" disabled>Select a role</option>
                                    {PREDEFINED_ROLES.map(role => <option key={role} value={role}>{role}</option>)}
                                </select>
                                <ValidationError message={errors.role} />
                            </div>

                            {formData.role === 'Other' && (
                                <div>
                                    <label htmlFor="otherRole" className="block text-sm font-medium text-neutral-700">Please Specify Role <RequiredAsterisk /></label>
                                    <input type="text" name="otherRole" id="otherRole" value={formData.otherRole} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.otherRole ? 'border-red-500' : 'border-neutral-300'} rounded-md`} />
                                    <ValidationError message={errors.otherRole} />
                                </div>
                            )}

                            {isFoodRole && (
                                <div>
                                    <label htmlFor="categoryId" className="block text-sm font-medium text-neutral-700">Expertise <RequiredAsterisk /></label>
                                    <select name="categoryId" id="categoryId" value={formData.categoryId} onChange={handleChange} className={`mt-1 block w-full px-3 py-2 border ${errors.categoryId ? 'border-red-500' : 'border-neutral-300'} rounded-md`}>
                                        <option value="" disabled>Select expertise</option>
                                        {expertiseCategories.map(c => <option key={c.categoryId} value={c.categoryId}>{c.name}</option>)}
                                    </select>
                                    <ValidationError message={errors.categoryId} />
                                </div>
                            )}

                            <div>
                                <label htmlFor="experience" className="block text-sm font-medium text-neutral-700">Experience (Years)</label>
                                <input type="number" name="experience" id="experience" value={formData.experience} onChange={handleChange} autoComplete="off" className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md" />
                            </div>
                        </div>
                    </fieldset>

                    {/* Section 3: Salary Details */}
                    <fieldset>
                        <legend className="text-lg font-semibold text-rose-700 mb-3">Salary Details</legend>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label htmlFor="salaryType" className="block text-sm font-medium text-neutral-700">Salary Type</label>
                                <select name="salaryType" id="salaryType" value={formData.salaryType} onChange={handleChange} className="mt-1 block w-full px-3 py-2 border border-neutral-300 rounded-md">
                                    <option>Monthly</option>
                                    <option>Per Day</option>
                                </select>
                            </div>
                            <div>
                                <label htmlFor="salaryAmount" className="block text-sm font-medium text-neutral-700">Salary Amount (₹) <RequiredAsterisk /></label>
                                <input type="number" name="salaryAmount" id="salaryAmount" value={formData.salaryAmount} onChange={handleChange} autoComplete="off" className={`mt-1 block w-full px-3 py-2 border ${errors.salaryAmount ? 'border-red-500' : 'border-neutral-300'} rounded-md`} />
                                <ValidationError message={errors.salaryAmount} />
                            </div>
                        </div>
                    </fieldset>

                    {/* Section 4: Upload Documents */}
                    <fieldset>
                        <legend className="text-lg font-semibold text-rose-700 mb-3">Upload Documents</legend>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            <SingleFileUploader
                                label={<>Photo <RequiredAsterisk /></>}
                                media={formData.photo}
                                onMediaChange={(media) => handleFileChange('photo', media)}
                                error={errors.photo}
                                accept="image/*"
                            />
                            <SingleFileUploader
                                label="ID Proof (Aadhar, etc.)"
                                media={formData.idProof}
                                onMediaChange={(media) => handleFileChange('idProof', media)}
                                accept="image/*,application/pdf"
                            />
                            <SingleFileUploader
                                label="Resume (Optional)"
                                media={formData.resume}
                                onMediaChange={(media) => handleFileChange('resume', media)}
                                accept=".doc,.docx,application/pdf"
                            />
                        </div>
                    </fieldset>

                    {/* Section 5: Availability */}
                    <fieldset>
                        <legend className="text-lg font-semibold text-rose-700 mb-3">Availability</legend>
                        <ToggleSwitch label="Available for new events" enabled={formData.availability} setEnabled={(value) => handleToggle('availability', value)} />
                        <p className="text-xs text-neutral-500 mt-1">{formData.availability ? 'This staff member is available for assignment.' : 'This staff member is currently unavailable.'}</p>
                    </fieldset>

                </form>
                <div className="p-6 bg-neutral-50 border-t flex justify-end gap-3">
                    <button type="button" onClick={onClose} className="px-5 py-2 rounded-md font-semibold text-neutral-700 bg-neutral-200 hover:bg-neutral-300">Cancel</button>
                    <button type="button" onClick={handleSubmit} className="px-5 py-2 rounded-md font-semibold text-white bg-rose-600 hover:bg-rose-700">
                        {editingItem ? 'Update Staff Member' : 'Save Staff Member'}
                    </button>
                </div>
            </div>
        </div>
    );
}