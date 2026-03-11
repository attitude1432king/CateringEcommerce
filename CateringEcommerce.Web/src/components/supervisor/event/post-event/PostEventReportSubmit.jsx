/**
 * PostEventReportSubmit Component
 * Submit post-event report with ratings, issues summary, and evidence
 */

import { useState } from 'react';
import PropTypes from 'prop-types';
import { FileText, Star, Camera, CheckCircle2 } from 'lucide-react';
import { eventSupervisionApi } from '../../../../services/api/supervisor';
import { TimestampedEvidenceUpload } from '../../common/forms';
import toast from 'react-hot-toast';

const PostEventReportSubmit = ({ assignmentId, onSubmitted }) => {
  const [submitted, setSubmitted] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [showEvidence, setShowEvidence] = useState(false);
  const [formData, setFormData] = useState({
    overallRating: 0,
    foodQualityRating: 0,
    serviceRating: 0,
    hygieneRating: 0,
    punctualityRating: 0,
    clientFeedback: '',
    issuesSummary: '',
    recommendationsForPartner: '',
    additionalNotes: '',
    cleanupCompleted: false,
    allEquipmentReturned: false,
    evidenceUrls: [],
  });

  const handleRating = (field, value) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleEvidenceUpload = (data) => {
    setFormData((prev) => ({
      ...prev,
      evidenceUrls: [...prev.evidenceUrls, data],
    }));
    setShowEvidence(false);
    toast.success('Evidence added');
  };

  const handleSubmit = async () => {
    if (formData.overallRating === 0) {
      toast.error('Please provide an overall rating');
      return;
    }

    setSubmitting(true);
    try {
      const response = await eventSupervisionApi.submitPostEventReport({
        assignmentId,
        ...formData,
        completedAt: new Date().toISOString(),
      });

      if (response.success) {
        setSubmitted(true);
        toast.success('Post-event report submitted successfully');
        onSubmitted?.();
      } else {
        toast.error(response.message);
      }
    } catch {
      toast.error('Failed to submit report');
    } finally {
      setSubmitting(false);
    }
  };

  const RatingStars = ({ value, onChange, label }) => (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <div className="flex gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <button key={star} onClick={() => onChange(star)} className="focus:outline-none">
            <Star
              className={`w-6 h-6 ${star <= value ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}`}
            />
          </button>
        ))}
      </div>
    </div>
  );

  if (submitted) {
    return (
      <div className="bg-white rounded-lg shadow-md p-8 text-center">
        <CheckCircle2 className="w-16 h-16 text-green-500 mx-auto mb-4" />
        <h2 className="text-xl font-semibold text-green-800">Report Submitted</h2>
        <p className="text-sm text-gray-600 mt-2">
          Your post-event report has been submitted for admin review.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center gap-3 mb-6">
          <FileText className="w-6 h-6 text-green-600" />
          <h2 className="text-xl font-semibold text-gray-900">Post-Event Report</h2>
        </div>

        {/* Ratings Section */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-gray-800 mb-3 uppercase tracking-wide">Ratings</h3>
          <div className="grid grid-cols-2 gap-4">
            <RatingStars value={formData.overallRating} onChange={(v) => handleRating('overallRating', v)} label="Overall Rating *" />
            <RatingStars value={formData.foodQualityRating} onChange={(v) => handleRating('foodQualityRating', v)} label="Food Quality" />
            <RatingStars value={formData.serviceRating} onChange={(v) => handleRating('serviceRating', v)} label="Service Quality" />
            <RatingStars value={formData.hygieneRating} onChange={(v) => handleRating('hygieneRating', v)} label="Hygiene Standards" />
            <RatingStars value={formData.punctualityRating} onChange={(v) => handleRating('punctualityRating', v)} label="Punctuality" />
          </div>
        </div>

        {/* Feedback Section */}
        <div className="space-y-4 mb-6">
          <h3 className="text-sm font-semibold text-gray-800 uppercase tracking-wide">Feedback & Notes</h3>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Client Feedback</label>
            <textarea
              value={formData.clientFeedback}
              onChange={(e) => setFormData((prev) => ({ ...prev, clientFeedback: e.target.value }))}
              rows={3}
              placeholder="Any feedback received from the client..."
              className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Issues Summary</label>
            <textarea
              value={formData.issuesSummary}
              onChange={(e) => setFormData((prev) => ({ ...prev, issuesSummary: e.target.value }))}
              rows={3}
              placeholder="Summary of any issues encountered..."
              className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Recommendations for Partner</label>
            <textarea
              value={formData.recommendationsForPartner}
              onChange={(e) => setFormData((prev) => ({ ...prev, recommendationsForPartner: e.target.value }))}
              rows={2}
              placeholder="Suggestions for the catering partner..."
              className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Additional Notes</label>
            <textarea
              value={formData.additionalNotes}
              onChange={(e) => setFormData((prev) => ({ ...prev, additionalNotes: e.target.value }))}
              rows={2}
              placeholder="Any other observations..."
              className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg"
            />
          </div>
        </div>

        {/* Checklist */}
        <div className="space-y-3 mb-6">
          <h3 className="text-sm font-semibold text-gray-800 uppercase tracking-wide">Completion Checklist</h3>
          <label className="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              checked={formData.cleanupCompleted}
              onChange={(e) => setFormData((prev) => ({ ...prev, cleanupCompleted: e.target.checked }))}
              className="w-4 h-4 text-blue-600 rounded"
            />
            <span className="text-sm text-gray-700">Cleanup completed at venue</span>
          </label>
          <label className="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              checked={formData.allEquipmentReturned}
              onChange={(e) => setFormData((prev) => ({ ...prev, allEquipmentReturned: e.target.checked }))}
              className="w-4 h-4 text-blue-600 rounded"
            />
            <span className="text-sm text-gray-700">All equipment returned / accounted for</span>
          </label>
        </div>

        {/* Evidence */}
        <div className="mb-6">
          <h3 className="text-sm font-semibold text-gray-800 mb-2 uppercase tracking-wide">Evidence</h3>
          <div className="flex items-center gap-3">
            <button
              onClick={() => setShowEvidence(true)}
              className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50"
            >
              <Camera className="w-4 h-4" />
              Add Evidence ({formData.evidenceUrls.length} uploaded)
            </button>
          </div>
        </div>

        {/* Submit */}
        <button
          onClick={handleSubmit}
          disabled={submitting || formData.overallRating === 0}
          className="w-full px-6 py-3 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700 disabled:opacity-50"
        >
          {submitting ? 'Submitting Report...' : 'Submit Post-Event Report'}
        </button>
      </div>

      {/* Evidence Modal */}
      {showEvidence && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-semibold mb-4">Upload Post-Event Evidence</h3>
            <TimestampedEvidenceUpload
              onUploadComplete={handleEvidenceUpload}
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

PostEventReportSubmit.propTypes = {
  assignmentId: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  onSubmitted: PropTypes.func,
};

export default PostEventReportSubmit;
