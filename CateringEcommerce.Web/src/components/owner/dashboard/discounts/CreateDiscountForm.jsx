/*
========================================
File: src/components/owner/dashboard/discounts/CreateDiscountForm.jsx (REVISED)
========================================
The complex form for creating/editing a discount.
Includes "Edit Mode" pre-filling and Delete button.
*/
import React, { useState, useEffect } from 'react';
import { useToast } from '../../../../contexts/ToastContext';
import ToggleSwitch from '../../../common/ToggleSwitch';
import MultiSelectDropdown from '../../../common/MultiSelectDropdown';

// Mock Data for Dropdowns (unchanged)
//const mockFoodItems = [
//    { id: 101, name: "Paneer Tikka" }, { id: 102, name: "Dal Makhani" }, { id: 103, name: "Gulab Jamun" }
//];
//const mockPackages = [
//    { id: 201, name: "Silver Wedding Package" }, { id: 202, name: "Gold Corporate Package" }
//];

export default function CreateDiscountForm({ isOpen, onClose, onSave, editingDiscount, listFoodItems, listPackages }) {
    // Initial State - USING INTEGERS
    const [formData, setFormData] = useState({
        name: '', description: '', type: 1, mode: 1, value: '',
        maxDiscount: 0, minOrderValue: 0, selectedItems: [], startDate: '', endDate: '',
        isActive: true, autoDisable: true, maxUsesPerOrder: 0, maxUsesPerUser: 0, isStackable: false
    });

    // Store initial data for comparison
    const [initialData, setInitialData] = useState(null);
    const [errors, setErrors] = useState({});
    const { showToast } = useToast();

    // Reset form when opening or changing editingDiscount
    useEffect(() => {
        if (isOpen) {
            let data;
            if (editingDiscount) {
                // Populate form for editing
                data = {
                    ...editingDiscount,
                    // Ensure types match what form expects (if api returns strings, convert to int here)
                };
            } else {
                // Reset for create
                data = {
                    name: '', description: '', type: 1, mode: 1, value: '',
                    maxDiscount: '', minOrderValue: '', selectedItems: [], startDate: '', endDate: '',
                    isActive: true, autoDisable: true, maxUsesPerOrder: '', maxUsesPerUser: '', isStackable: false
                };
            }
            setFormData(data);
            setInitialData(data); // Save initial state
            setErrors({});
        }
    }, [isOpen, editingDiscount]);

    // Handlers (unchanged logic)
    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
        if (errors[name]) setErrors(prev => ({ ...prev, [name]: null }));
    };

    const handleTypeChange = (typeVal) => setFormData(prev => ({ ...prev, type: typeVal, selectedItems: [] }));
    const handleModeChange = (modeVal) => setFormData(prev => ({ ...prev, mode: modeVal, value: '', maxDiscount: 0 }));
    const handleToggle = (name, value) => setFormData(prev => ({ ...prev, [name]: value }));
    const handleMultiSelectChange = (ids) => setFormData(prev => ({ ...prev, selectedItems: ids }));

    // Validation Logic (unchanged)
    const validate = () => {
        const newErrors = {};
        if (!formData.name || formData.name.length < 3) newErrors.name = "Name must be at least 3 characters.";
        if (!formData.value || Number(formData.value) <= 0) newErrors.value = "Discount value must be greater than 0.";
        if (formData.mode === 1 && Number(formData.value) > 100) newErrors.value = "Percentage cannot exceed 100%.";
        if (formData.mode === 2 && Number(formData.value) > 50000) newErrors.value = "Flat amount cannot exceed ₹50,000.";
        if ((formData.type === 1 || formData.type === 2) && (!formData.selectedItems || formData.selectedItems.length === 0)) {
            newErrors.selectedItems = "Please select at least one item/package.";
        }
        if (!formData.startDate) newErrors.startDate = "Start date is required.";
        if (!formData.endDate) newErrors.endDate = "End date is required.";
        if (formData.startDate && formData.endDate && new Date(formData.endDate) <= new Date(formData.startDate)) {
            newErrors.endDate = "End date must be after start date.";
        }
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = (e) => {
        e.preventDefault();

        // We compare the JSON stringified versions of current formData and initialData
        if (editingDiscount && JSON.stringify(formData) === JSON.stringify(initialData)) {
            showToast("No changes were made.", "warning");
            return;
        }

        if (validate()) {
            const submissionData = {
                ...formData,
                type: parseInt(formData.type),
                mode: parseInt(formData.mode),
                value: parseFloat(formData.value),
            };

            // Check if specific fields have changed to set the flag
            if (editingDiscount) {
                const hasCriticalChanges =
                    formData.mode !== initialData.mode ||
                    Number(formData.value) !== Number(initialData.value) ||
                    formData.name !== initialData.name;

                submissionData.IsChangeDiscountCode = hasCriticalChanges;
            }

            onSave(submissionData);
        } else {
            showToast("Please fix the errors in the form.", "error");
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50 p-4 overflow-y-auto">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-3xl flex flex-col max-h-[90vh]">

                {/* Header */}
                <div className="p-6 border-b border-neutral-100 flex justify-between items-center">
                    <div>
                        <h2 className="text-2xl font-bold text-neutral-800">{editingDiscount ? 'Edit Discount' : 'Create New Discount'}</h2>
                        <p className="text-sm text-neutral-500 mt-1">Offer discounts to attract more bookings.</p>
                    </div>
                </div>

                {/* Form Content */}
                <form onSubmit={handleSubmit} className="overflow-y-auto flex-1 p-6 space-y-8">

                    {/* SECTION 1: Basic Info */}
                    <section className="space-y-4">
                        <h3 className="text-sm font-bold text-rose-600 uppercase tracking-wide border-b border-rose-100 pb-2">1. Basic Information</h3>
                        <div className="grid grid-cols-1 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Discount Name <span className="text-red-500">*</span></label>
                                <input type="text" name="name" value={formData.name} onChange={handleChange} autoComplete="off" className={`w-full p-2 border rounded-md ${errors.name ? 'border-red-500' : 'border-neutral-300'}`} placeholder="e.g. Summer Wedding Sale" />
                                {errors.name && <p className="text-xs text-red-500 mt-1">{errors.name}</p>}
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Description</label>
                                <textarea name="description" value={formData.description} onChange={handleChange} rows="2" className="w-full p-2 border border-neutral-300 rounded-md" placeholder="Internal note or customer-facing details..."></textarea>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-2">Discount Scope <span className="text-red-500">*</span></label>
                                <div className="flex flex-col sm:flex-row gap-4">
                                    <label className="flex items-center p-3 border rounded-lg cursor-pointer hover:bg-neutral-50 transition-colors">
                                        <input type="radio" name="type" value={1} checked={formData.type === 1} onChange={() => handleTypeChange(1)} className="h-4 w-4 text-rose-600 focus:ring-rose-500" />
                                        <span className="ml-2 text-sm font-medium text-neutral-700">Specific Items</span>
                                    </label>
                                    <label className="flex items-center p-3 border rounded-lg cursor-pointer hover:bg-neutral-50 transition-colors">
                                        <input type="radio" name="type" value={2} checked={formData.type === 2} onChange={() => handleTypeChange(2)} className="h-4 w-4 text-rose-600 focus:ring-rose-500" />
                                        <span className="ml-2 text-sm font-medium text-neutral-700">Specific Packages</span>
                                    </label>
                                    <label className="flex items-center p-3 border rounded-lg cursor-pointer hover:bg-neutral-50 transition-colors">
                                        <input type="radio" name="type" value={3} checked={formData.type === 3} onChange={() => handleTypeChange(3)} className="h-4 w-4 text-rose-600 focus:ring-rose-500" />
                                        <span className="ml-2 text-sm font-medium text-neutral-700">Entire Catering</span>
                                    </label>
                                </div>
                            </div>
                        </div>
                    </section>

                    {/* SECTION 2: Value Config */}
                    <section className="space-y-4">
                        <h3 className="text-sm font-bold text-rose-600 uppercase tracking-wide border-b border-rose-100 pb-2">2. Value Configuration</h3>

                        {/* Mode Selection */}
                        <div className="flex gap-4 mb-4">
                            <label className={`flex-1 flex items-center justify-center p-2 rounded-md cursor-pointer border ${formData.mode === 1 ? 'bg-rose-50 border-rose-500 text-rose-700' : 'border-neutral-200'}`}>
                                <input type="radio" name="mode" value={1} checked={formData.mode === 1} onChange={() => handleModeChange(1)} className="sr-only" />
                                <span className="font-semibold text-sm">% Percentage</span>
                            </label>
                            <label className={`flex-1 flex items-center justify-center p-2 rounded-md cursor-pointer border ${formData.mode === 2 ? 'bg-rose-50 border-rose-500 text-rose-700' : 'border-neutral-200'}`}>
                                <input type="radio" name="mode" value={2} checked={formData.mode === 2} onChange={() => handleModeChange(2)} className="sr-only" />
                                <span className="font-semibold text-sm">₹ Flat Amount</span>
                            </label>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">
                                    {formData.mode === 1 ? 'Percentage Value (%)' : 'Flat Amount (₹)'} <span className="text-red-500">*</span>
                                </label>
                                <input type="number" name="value" value={formData.value} onChange={handleChange} className={`w-full p-2 border no-spinner rounded-md ${errors.value ? 'border-red-500' : 'border-neutral-300'}`} placeholder="0" />
                                {errors.value && <p className="text-xs text-red-500 mt-1">{errors.value}</p>}
                            </div>

                            <div>
                                <label className={`block text-sm font-medium mb-1 ${formData.mode === 2 ? 'text-neutral-400' : 'text-neutral-700'}`}>Max Discount Amount (₹)</label>
                                <input
                                    type="number"
                                    name="maxDiscount"
                                    value={formData.maxDiscount > 0 ? formData.maxDiscount : ''}
                                    onChange={handleChange}
                                    disabled={formData.mode === 2}
                                    className="w-full p-2 border border-neutral-300 rounded-md disabled:bg-neutral-100 disabled:text-neutral-400 no-spinner"
                                    placeholder="Optional limit"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Min Order Value (₹)</label>
                                <input type="number" name="minOrderValue" value={formData.minOrderValue > 0 ? formData.minOrderValue : ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md no-spinner" placeholder="Optional" />
                            </div>
                        </div>
                    </section>

                    {/* SECTION 3: Apply To (Dynamic) */}
                    {formData.type !== 3 && (
                        <section className="space-y-4">
                            <h3 className="text-sm font-bold text-rose-600 uppercase tracking-wide border-b border-rose-100 pb-2">3. Apply Discount To</h3>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">
                                    Select {formData.type === 1 ? 'Food Items' : 'Packages'} <span className="text-red-500">*</span>
                                </label>
                                <MultiSelectDropdown
                                    placeholder={formData.type === 1 ? "Search & Select Items..." : "Search & Select Packages..."}
                                    options={formData.type === 1 ? listFoodItems : listPackages}
                                    selectedIds={formData.selectedItems || []} // Ensure array
                                    onChange={handleMultiSelectChange}
                                />
                                {errors.selectedItems && <p className="text-xs text-red-500 mt-1">{errors.selectedItems}</p>}
                            </div>
                        </section>
                    )}
                    {formData.type === 3 && (
                        <div className="bg-blue-50 border-l-4 border-blue-500 p-4 rounded-r-md">
                            <p className="text-sm text-blue-700"><strong>Note:</strong> This discount will apply automatically to <strong>all</strong> food items and packages in your catering service.</p>
                        </div>
                    )}

                    {/* SECTION 4: Validity */}
                    <section className="space-y-4">
                        <h3 className="text-sm font-bold text-rose-600 uppercase tracking-wide border-b border-rose-100 pb-2">4. Validity & Rules</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Start Date <span className="text-red-500">*</span></label>
                                <input type="date" name="startDate" value={formData.startDate} onChange={handleChange} className={`w-full p-2 border rounded-md ${errors.startDate ? 'border-red-500' : 'border-neutral-300'}`} />
                                {errors.startDate && <p className="text-xs text-red-500 mt-1">{errors.startDate}</p>}
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">End Date <span className="text-red-500">*</span></label>
                                <input type="date" name="endDate" value={formData.endDate} onChange={handleChange} className={`w-full p-2 border rounded-md ${errors.endDate ? 'border-red-500' : 'border-neutral-300'}`} />
                                {errors.endDate && <p className="text-xs text-red-500 mt-1">{errors.endDate}</p>}
                            </div>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Max Uses Per Order</label>
                                <input type="number" name="maxUsesPerOrder" value={formData.maxUsesPerOrder > 0 ? formData.maxUsesPerOrder : ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md no-spinner" placeholder="e.g. 1" />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-neutral-700 mb-1">Max Uses Per User</label>
                                <input type="number" name="maxUsesPerUser" value={formData.maxUsesPerUser > 0 ? formData.maxUsesPerUser : ''} onChange={handleChange} className="w-full p-2 border border-neutral-300 rounded-md no-spinner" placeholder="e.g. 1" />
                            </div>
                        </div>

                        <div className="flex flex-col gap-3 pt-2">
                            <ToggleSwitch label="Active Status" enabled={formData.isActive} setEnabled={(val) => handleToggle('isActive', val)} />
                            <ToggleSwitch label="Auto-disable after expiry" enabled={formData.autoDisable} setEnabled={(val) => handleToggle('autoDisable', val)} />
                            <ToggleSwitch label="Stackable with other discounts" enabled={formData.isStackable} setEnabled={(val) => handleToggle('isStackable', val)} />
                        </div>
                    </section>

                </form>

                {/* Footer Actions */}
                <div className="p-6 bg-neutral-50 border-t flex justify-end gap-3 rounded-b-xl">
                    <button onClick={onClose} className="px-6 py-2.5 rounded-lg text-sm font-semibold text-neutral-700 bg-white border border-neutral-300 hover:bg-neutral-50 transition-colors">
                        Cancel
                    </button>
                    <button onClick={handleSubmit} className="px-6 py-2.5 rounded-lg text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 shadow-md transition-colors">
                        {editingDiscount ? 'Update Discount' : 'Save Discount'}
                    </button>
                </div>
            </div>
        </div>
    );
}