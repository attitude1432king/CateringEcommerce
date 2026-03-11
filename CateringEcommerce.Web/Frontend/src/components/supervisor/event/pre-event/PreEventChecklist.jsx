/**
 * PreEventChecklist Component
 * Comprehensive checklist for pre-event verification
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { CheckCircle2, Circle, AlertTriangle, Camera } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import { TimestampedEvidenceUpload } from '../../common/forms';
import toast from 'react-hot-toast';

const CHECKLIST_ITEMS = [
    {
        id: 'venue_access',
        category: 'Venue',
        label: 'Venue access confirmed',
        requiresEvidence: true,
        critical: true,
    },
    {
        id: 'setup_area',
        category: 'Venue',
        label: 'Setup area inspected and clean',
        requiresEvidence: true,
        critical: true,
    },
    {
        id: 'power_water',
        category: 'Utilities',
        label: 'Power and water supply verified',
        requiresEvidence: false,
        critical: true,
    },
    {
        id: 'equipment_arrival',
        category: 'Equipment',
        label: 'Catering equipment arrived',
        requiresEvidence: true,
        critical: true,
    },
    {
        id: 'equipment_condition',
        category: 'Equipment',
        label: 'Equipment condition checked',
        requiresEvidence: false,
        critical: true,
    },
    {
        id: 'food_arrival',
        category: 'Food & Supplies',
        label: 'Food items delivered',
        requiresEvidence: true,
        critical: true,
    },
    {
        id: 'food_temperature',
        category: 'Food & Supplies',
        label: 'Food temperature verified',
        requiresEvidence: false,
        critical: true,
    },
    {
        id: 'staff_present',
        category: 'Staffing',
        label: 'All staff members present',
        requiresEvidence: false,
        critical: true,
    },
    {
        id: 'hygiene_standards',
        category: 'Safety',
        label: 'Hygiene standards met',
        requiresEvidence: true,
        critical: true,
    },
    {
        id: 'fire_safety',
        category: 'Safety',
        label: 'Fire safety equipment available',
        requiresEvidence: false,
        critical: false,
    },
    {
        id: 'serving_area',
        category: 'Setup',
        label: 'Serving area setup complete',
        requiresEvidence: true,
        critical: false,
    },
    {
        id: 'guest_capacity',
        category: 'Setup',
        label: 'Guest capacity matches booking',
        requiresEvidence: false,
        critical: false,
    },
];

const PreEventChecklist = ({ assignmentId, onComplete }) => {
    const [checkedItems, setCheckedItems] = useState({});
    const [evidenceUrls, setEvidenceUrls] = useState({});
    const [notes, setNotes] = useState({});
    const [showEvidenceModal, setShowEvidenceModal] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    const handleItemToggle = (itemId) => {
        setCheckedItems((prev) => ({
            ...prev,
            [itemId]: !prev[itemId],
        }));
    };

    const handleEvidenceUpload = (itemId, evidenceData) => {
        setEvidenceUrls((prev) => ({
            ...prev,
            [itemId]: evidenceData,
        }));
        setShowEvidenceModal(null);
        toast.success('Evidence uploaded');
    };

    const handleNoteChange = (itemId, note) => {
        setNotes((prev) => ({
            ...prev,
            [itemId]: note,
        }));
    };

    const getCategoryProgress = (category) => {
        const categoryItems = CHECKLIST_ITEMS.filter((item) => item.category === category);
        const checkedCount = categoryItems.filter((item) => checkedItems[item.id]).length;
        return {
            total: categoryItems.length,
            checked: checkedCount,
            percentage: (checkedCount / categoryItems.length) * 100,
        };
    };

    const getOverallProgress = () => {
        const total = CHECKLIST_ITEMS.length;
        const checked = Object.values(checkedItems).filter(Boolean).length;
        return {
            total,
            checked,
            percentage: (checked / total) * 100,
        };
    };

    const getCriticalIssues = () => {
        return CHECKLIST_ITEMS.filter(
            (item) => item.critical && !checkedItems[item.id]
        );
    };

    const canSubmit = () => {
        const criticalItems = CHECKLIST_ITEMS.filter((item) => item.critical);
        const allCriticalChecked = criticalItems.every((item) => checkedItems[item.id]);
        const requiredEvidenceProvided = CHECKLIST_ITEMS.filter(
            (item) => item.requiresEvidence && checkedItems[item.id]
        ).every((item) => evidenceUrls[item.id]);

        return allCriticalChecked && requiredEvidenceProvided;
    };

    const handleSubmit = async () => {
        if (!canSubmit()) {
            toast.error('Please complete all critical items and provide required evidence');
            return;
        }

        setSubmitting(true);
        try {
            const supervisorId = localStorage.getItem('supervisorId');
            const checklistData = CHECKLIST_ITEMS.map((item) => ({
                itemId: item.id,
                itemLabel: item.label,
                checked: !!checkedItems[item.id],
                evidenceUrl: evidenceUrls[item.id]?.url || null,
                evidenceTimestamp: evidenceUrls[item.id]?.timestamp || null,
                gpsLocation: evidenceUrls[item.id]?.gpsLocation || null,
                notes: notes[item.id] || null,
            }));

            const response = await eventSupervisionApi.submitPreEventChecklist({
                assignmentId,
                supervisorId,
                checklist: checklistData,
                completedAt: new Date().toISOString(),
            });

            if (response.success) {
                toast.success('Pre-event checklist submitted successfully');
                onComplete?.();
            } else {
                toast.error(response.message);
            }
        } catch (error) {
            console.error('Checklist submission error:', error);
            toast.error('Failed to submit checklist');
        } finally {
            setSubmitting(false);
        }
    };

    const categories = [...new Set(CHECKLIST_ITEMS.map((item) => item.category))];
    const progress = getOverallProgress();
    const criticalIssues = getCriticalIssues();

    return (
        <div className="space-y-6">
            {/* Overall Progress */}
            <div className="bg-white rounded-lg shadow-md p-6">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-xl font-semibold text-gray-900">Pre-Event Checklist</h2>
                    <span className="text-sm text-gray-600">
                        {progress.checked} of {progress.total} completed
                    </span>
                </div>

                <div className="w-full bg-gray-200 rounded-full h-3 mb-2">
                    <div
                        className="bg-blue-600 h-3 rounded-full transition-all duration-300"
                        style={{ width: `${progress.percentage}%` }}
                    />
                </div>

                {criticalIssues.length > 0 && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-3 mt-4">
                        <div className="flex items-start gap-2">
                            <AlertTriangle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                            <div>
                                <p className="text-sm font-medium text-red-900">
                                    {criticalIssues.length} Critical Item{criticalIssues.length > 1 ? 's' : ''} Pending
                                </p>
                                <ul className="text-xs text-red-700 mt-1 space-y-1">
                                    {criticalIssues.map((issue) => (
                                        <li key={issue.id}>• {issue.label}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Checklist by Category */}
            {categories.map((category) => {
                const categoryItems = CHECKLIST_ITEMS.filter((item) => item.category === category);
                const categoryProgress = getCategoryProgress(category);

                return (
                    <div key={category} className="bg-white rounded-lg shadow-md p-6">
                        {/* Category Header */}
                        <div className="flex items-center justify-between mb-4">
                            <h3 className="text-lg font-semibold text-gray-900">{category}</h3>
                            <span className="text-sm text-gray-600">
                                {categoryProgress.checked}/{categoryProgress.total}
                            </span>
                        </div>

                        {/* Category Items */}
                        <div className="space-y-3">
                            {categoryItems.map((item) => {
                                const isChecked = checkedItems[item.id];
                                const hasEvidence = evidenceUrls[item.id];
                                const needsEvidence = item.requiresEvidence && isChecked && !hasEvidence;

                                return (
                                    <div key={item.id} className="border border-gray-200 rounded-lg p-4">
                                        {/* Item Header */}
                                        <div className="flex items-start gap-3">
                                            <button
                                                onClick={() => handleItemToggle(item.id)}
                                                className="mt-0.5 flex-shrink-0"
                                            >
                                                {isChecked ? (
                                                    <CheckCircle2 className="w-5 h-5 text-green-600" />
                                                ) : (
                                                    <Circle className="w-5 h-5 text-gray-400" />
                                                )}
                                            </button>

                                            <div className="flex-1">
                                                <div className="flex items-center gap-2">
                                                    <p className={`text-sm font-medium ${isChecked ? 'text-gray-500 line-through' : 'text-gray-900'}`}>
                                                        {item.label}
                                                    </p>
                                                    {item.critical && (
                                                        <span className="text-xs px-2 py-0.5 bg-red-100 text-red-700 rounded">
                                                            Critical
                                                        </span>
                                                    )}
                                                    {item.requiresEvidence && (
                                                        <span className="text-xs px-2 py-0.5 bg-blue-100 text-blue-700 rounded">
                                                            Evidence Required
                                                        </span>
                                                    )}
                                                </div>

                                                {/* Evidence Button */}
                                                {item.requiresEvidence && isChecked && (
                                                    <button
                                                        onClick={() => setShowEvidenceModal(item.id)}
                                                        className={`mt-2 flex items-center gap-2 text-xs ${hasEvidence
                                                                ? 'text-green-600 hover:text-green-700'
                                                                : 'text-blue-600 hover:text-blue-700'
                                                            }`}
                                                    >
                                                        <Camera className="w-4 h-4" />
                                                        {hasEvidence ? 'Evidence uploaded ✓' : 'Upload Evidence'}
                                                    </button>
                                                )}

                                                {needsEvidence && (
                                                    <p className="text-xs text-red-600 mt-1">
                                                        ⚠ Evidence upload required before submission
                                                    </p>
                                                )}

                                                {/* Notes Input */}
                                                {isChecked && (
                                                    <textarea
                                                        value={notes[item.id] || ''}
                                                        onChange={(e) => handleNoteChange(item.id, e.target.value)}
                                                        placeholder="Add notes (optional)"
                                                        rows={2}
                                                        className="mt-2 w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                                    />
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                );
            })}

            {/* Submit Button */}
            <div className="bg-white rounded-lg shadow-md p-6">
                <button
                    onClick={handleSubmit}
                    disabled={!canSubmit() || submitting}
                    className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    {submitting ? 'Submitting...' : 'Submit Pre-Event Checklist'}
                </button>

                {!canSubmit() && (
                    <p className="text-sm text-gray-600 text-center mt-2">
                        Complete all critical items and provide required evidence to submit
                    </p>
                )}
            </div>

            {/* Evidence Upload Modal */}
            {showEvidenceModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-lg max-w-md w-full p-6">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">
                            Upload Evidence
                        </h3>
                        <TimestampedEvidenceUpload
                            onUploadComplete={(evidenceData) => handleEvidenceUpload(showEvidenceModal, evidenceData)}
                            allowedTypes={['photo', 'video']}
                        />
                        <button
                            onClick={() => setShowEvidenceModal(null)}
                            className="mt-4 w-full px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

PreEventChecklist.propTypes = {
    assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
    onComplete: PropTypes.func,
};

export default PreEventChecklist;
