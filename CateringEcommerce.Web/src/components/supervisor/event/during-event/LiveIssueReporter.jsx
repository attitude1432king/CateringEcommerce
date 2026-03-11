/**
 * LiveIssueReporter Component
 * Report and track issues during live event
 */

import { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { AlertOctagon, Plus, Clock, CheckCircle2, AlertTriangle, XCircle } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import { TimestampedEvidenceUpload } from '../../common/forms';
import toast from 'react-hot-toast';

const ISSUE_CATEGORIES = [
  { value: 'FOOD_QUALITY', label: 'Food Quality' },
  { value: 'FOOD_QUANTITY', label: 'Food Quantity' },
  { value: 'HYGIENE', label: 'Hygiene' },
  { value: 'STAFF_BEHAVIOR', label: 'Staff Behavior' },
  { value: 'EQUIPMENT', label: 'Equipment Issue' },
  { value: 'SAFETY', label: 'Safety Concern' },
  { value: 'DELAY', label: 'Service Delay' },
  { value: 'OTHER', label: 'Other' },
];

const SEVERITY_LEVELS = [
  { value: 'LOW', label: 'Low', color: 'text-gray-600 bg-gray-100' },
  { value: 'MEDIUM', label: 'Medium', color: 'text-yellow-700 bg-yellow-100' },
  { value: 'HIGH', label: 'High', color: 'text-orange-700 bg-orange-100' },
  { value: 'CRITICAL', label: 'Critical', color: 'text-red-700 bg-red-100' },
];

const LiveIssueReporter = ({ assignmentId, onIssueReported }) => {
  const [showForm, setShowForm] = useState(false);
  const [issues, setIssues] = useState([]);
  const [formData, setFormData] = useState({
    category: '',
    severity: 'MEDIUM',
    description: '',
    immediateAction: '',
    evidenceUrl: null,
  });
  const [submitting, setSubmitting] = useState(false);
  const [showEvidence, setShowEvidence] = useState(false);

  useEffect(() => {
    loadTracking();
  }, [assignmentId]);

  const loadTracking = async () => {
    const response = await eventSupervisionApi.getDuringEventTracking(assignmentId);
    if (response.success && response.data?.data?.issues) {
      setIssues(response.data.data.issues);
    }
  };

  const handleSubmitIssue = async () => {
    if (!formData.category || !formData.description.trim()) {
      toast.error('Please select a category and provide a description');
      return;
    }

    setSubmitting(true);
    try {
      const response = await eventSupervisionApi.recordFoodServingMonitor({
        assignmentId,
        trackingType: 'ISSUE_REPORT',
        issueCategory: formData.category,
        severity: formData.severity,
        description: formData.description,
        immediateAction: formData.immediateAction,
        evidenceUrl: formData.evidenceUrl?.url || null,
        reportedAt: new Date().toISOString(),
      });

      if (response.success) {
        const newIssue = {
          id: Date.now(),
          ...formData,
          reportedAt: new Date().toISOString(),
          status: 'OPEN',
        };
        setIssues((prev) => [newIssue, ...prev]);
        setFormData({ category: '', severity: 'MEDIUM', description: '', immediateAction: '', evidenceUrl: null });
        setShowForm(false);
        toast.success('Issue reported successfully');
        onIssueReported?.(newIssue);
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to report issue');
    } finally {
      setSubmitting(false);
    }
  };

  const getSeverityStyle = (severity) => {
    return SEVERITY_LEVELS.find((s) => s.value === severity)?.color || 'text-gray-600 bg-gray-100';
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-3">
          <AlertOctagon className="w-6 h-6 text-red-600" />
          <h2 className="text-xl font-semibold text-gray-900">Issue Reporter</h2>
          {issues.length > 0 && (
            <span className="text-xs px-2 py-0.5 bg-red-100 text-red-700 rounded-full">
              {issues.filter((i) => i.status === 'OPEN').length} Open
            </span>
          )}
        </div>
        {!showForm && (
          <button
            onClick={() => setShowForm(true)}
            className="flex items-center gap-1 px-3 py-1.5 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700"
          >
            <Plus className="w-4 h-4" /> Report Issue
          </button>
        )}
      </div>

      {/* Report Form */}
      {showForm && (
        <div className="border border-red-200 rounded-lg p-4 mb-4 bg-red-50">
          <h3 className="text-sm font-semibold text-red-900 mb-3">New Issue Report</h3>

          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">Category</label>
                <select
                  value={formData.category}
                  onChange={(e) => setFormData((prev) => ({ ...prev, category: e.target.value }))}
                  className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
                >
                  <option value="">Select category</option>
                  {ISSUE_CATEGORIES.map((cat) => (
                    <option key={cat.value} value={cat.value}>{cat.label}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">Severity</label>
                <div className="flex gap-1">
                  {SEVERITY_LEVELS.map((level) => (
                    <button
                      key={level.value}
                      onClick={() => setFormData((prev) => ({ ...prev, severity: level.value }))}
                      className={`flex-1 px-2 py-2 text-xs rounded-lg border ${
                        formData.severity === level.value ? level.color + ' border-current font-medium' : 'border-gray-200 hover:bg-gray-50'
                      }`}
                    >
                      {level.label}
                    </button>
                  ))}
                </div>
              </div>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">Description</label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData((prev) => ({ ...prev, description: e.target.value }))}
                rows={3}
                placeholder="Describe the issue in detail..."
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">Immediate Action Taken</label>
              <textarea
                value={formData.immediateAction}
                onChange={(e) => setFormData((prev) => ({ ...prev, immediateAction: e.target.value }))}
                rows={2}
                placeholder="What action was taken immediately..."
                className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
              />
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={() => setShowEvidence(true)}
                className="px-3 py-2 text-sm border border-gray-300 rounded-lg hover:bg-white flex items-center gap-1"
              >
                {formData.evidenceUrl ? '+ Evidence Added' : '+ Add Evidence'}
              </button>
              <div className="flex-1" />
              <button
                onClick={() => setShowForm(false)}
                className="px-4 py-2 text-sm border border-gray-300 rounded-lg hover:bg-white"
              >
                Cancel
              </button>
              <button
                onClick={handleSubmitIssue}
                disabled={submitting}
                className="px-4 py-2 text-sm bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
              >
                {submitting ? 'Reporting...' : 'Submit Report'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Issues List */}
      {issues.length === 0 && !showForm ? (
        <div className="text-center py-8 text-gray-500">
          <AlertTriangle className="w-8 h-8 mx-auto mb-2 text-gray-300" />
          <p className="text-sm">No issues reported yet</p>
        </div>
      ) : (
        <div className="space-y-3">
          {issues.map((issue) => (
            <div key={issue.id} className="border border-gray-200 rounded-lg p-3">
              <div className="flex items-start justify-between">
                <div className="flex items-start gap-2">
                  {issue.status === 'OPEN' ? (
                    <AlertTriangle className="w-4 h-4 text-red-500 mt-0.5" />
                  ) : (
                    <CheckCircle2 className="w-4 h-4 text-green-500 mt-0.5" />
                  )}
                  <div>
                    <div className="flex items-center gap-2">
                      <p className="text-sm font-medium text-gray-900">
                        {ISSUE_CATEGORIES.find((c) => c.value === issue.category)?.label || issue.category}
                      </p>
                      <span className={`text-xs px-2 py-0.5 rounded-full ${getSeverityStyle(issue.severity)}`}>
                        {issue.severity}
                      </span>
                    </div>
                    <p className="text-xs text-gray-600 mt-1">{issue.description}</p>
                    {issue.immediateAction && (
                      <p className="text-xs text-blue-600 mt-1">Action: {issue.immediateAction}</p>
                    )}
                  </div>
                </div>
                <span className="text-xs text-gray-400 flex items-center gap-1">
                  <Clock className="w-3 h-3" />
                  {new Date(issue.reportedAt).toLocaleTimeString()}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Evidence Modal */}
      {showEvidence && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-semibold mb-4">Upload Issue Evidence</h3>
            <TimestampedEvidenceUpload
              onUploadComplete={(data) => { setFormData((prev) => ({ ...prev, evidenceUrl: data })); setShowEvidence(false); }}
              allowedTypes={['photo', 'video']}
            />
            <button
              onClick={() => setShowEvidence(false)}
              className="mt-4 w-full px-4 py-2 border border-gray-300 rounded-lg text-sm text-gray-700 hover:bg-gray-50"
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

LiveIssueReporter.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  onIssueReported: PropTypes.func,
};

export default LiveIssueReporter;
