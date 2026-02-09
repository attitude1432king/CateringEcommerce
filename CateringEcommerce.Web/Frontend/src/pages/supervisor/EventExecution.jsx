/**
 * EventExecution Page
 * Full event supervision workflow: Pre-Event -> During-Event -> Post-Event -> Summary
 */

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  ClipboardCheck,
  UtensilsCrossed,
  FileText,
  BarChart3,
} from 'lucide-react';
import { getAssignmentById } from '../../services/api/supervisor/assignmentApi';
import { PreEventChecklist } from '../../components/supervisor/event';
import { FoodServingMonitor, GuestCountTracker, ExtraQuantityRequest, ClientOTPVerification, LiveIssueReporter } from '../../components/supervisor/event/during-event';
import { PostEventReportSubmit, EventSupervisionSummary } from '../../components/supervisor/event/post-event';
import toast from 'react-hot-toast';

const PHASES = [
  { id: 'pre-event', label: 'Pre-Event', icon: ClipboardCheck },
  { id: 'during-event', label: 'During Event', icon: UtensilsCrossed },
  { id: 'post-event', label: 'Post-Event', icon: FileText },
  { id: 'summary', label: 'Summary', icon: BarChart3 },
];

const EventExecution = () => {
  const { assignmentId } = useParams();
  const navigate = useNavigate();
  const [currentPhase, setCurrentPhase] = useState('pre-event');
  const [assignment, setAssignment] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showOTPVerification, setShowOTPVerification] = useState(false);

  useEffect(() => {
    loadAssignment();
  }, [assignmentId]);

  const loadAssignment = async () => {
    try {
      const response = await getAssignmentById(assignmentId);
      if (response.success) {
        setAssignment(response.data?.data || response.data);
        // Auto-detect phase based on assignment status
        const status = response.data?.data?.status || response.data?.status;
        if (status === 'COMPLETED' || status === 'POST_EVENT_DONE') {
          setCurrentPhase('summary');
        } else if (status === 'POST_EVENT') {
          setCurrentPhase('post-event');
        } else if (status === 'IN_PROGRESS' || status === 'CHECKED_IN') {
          setCurrentPhase('during-event');
        }
      } else {
        toast.error('Failed to load assignment');
      }
    } catch {
      toast.error('Failed to load assignment');
    } finally {
      setLoading(false);
    }
  };

  const handlePhaseComplete = (phase) => {
    const idx = PHASES.findIndex((p) => p.id === phase);
    if (idx < PHASES.length - 1) {
      setCurrentPhase(PHASES[idx + 1].id);
      toast.success(`${PHASES[idx].label} phase completed`);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-600" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 py-4">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate(-1)}
              className="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg"
            >
              <ArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <h1 className="text-xl font-semibold text-gray-900">Event Supervision</h1>
              {assignment && (
                <p className="text-sm text-gray-500">
                  Assignment #{assignmentId}
                  {assignment.eventName && ` - ${assignment.eventName}`}
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Phase Navigation */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4">
          <div className="flex">
            {PHASES.map((phase, idx) => {
              const PhaseIcon = phase.icon;
              const isActive = currentPhase === phase.id;
              const currentIdx = PHASES.findIndex((p) => p.id === currentPhase);
              const isCompleted = idx < currentIdx;

              return (
                <button
                  key={phase.id}
                  onClick={() => setCurrentPhase(phase.id)}
                  className={`flex-1 flex items-center justify-center gap-2 py-3 text-sm font-medium border-b-2 transition-colors ${
                    isActive
                      ? 'border-blue-600 text-blue-600'
                      : isCompleted
                      ? 'border-green-500 text-green-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <PhaseIcon className="w-4 h-4" />
                  <span className="hidden sm:inline">{phase.label}</span>
                </button>
              );
            })}
          </div>
        </div>
      </div>

      {/* Phase Content */}
      <div className="max-w-4xl mx-auto px-4 py-6">
        {currentPhase === 'pre-event' && (
          <PreEventChecklist
            assignmentId={assignmentId}
            onComplete={() => handlePhaseComplete('pre-event')}
          />
        )}

        {currentPhase === 'during-event' && (
          <div className="space-y-6">
            <FoodServingMonitor assignmentId={assignmentId} onUpdate={loadAssignment} />
            <GuestCountTracker
              assignmentId={assignmentId}
              expectedGuests={assignment?.expectedGuestCount || 0}
              onUpdate={loadAssignment}
            />
            <ExtraQuantityRequest
              assignmentId={assignmentId}
              onRequestSent={(data) => {
                if (data?.requiresOTP) {
                  setShowOTPVerification(true);
                }
              }}
            />
            {showOTPVerification && (
              <ClientOTPVerification
                assignmentId={assignmentId}
                purpose="EXTRA_QUANTITY"
                onVerified={() => {
                  setShowOTPVerification(false);
                  toast.success('Extra quantity approved by client');
                }}
              />
            )}
            <LiveIssueReporter assignmentId={assignmentId} />

            {/* Complete During-Event Button */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <button
                onClick={() => handlePhaseComplete('during-event')}
                className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700"
              >
                Proceed to Post-Event Report
              </button>
            </div>
          </div>
        )}

        {currentPhase === 'post-event' && (
          <PostEventReportSubmit
            assignmentId={assignmentId}
            onSubmitted={() => handlePhaseComplete('post-event')}
          />
        )}

        {currentPhase === 'summary' && (
          <EventSupervisionSummary assignmentId={assignmentId} />
        )}
      </div>
    </div>
  );
};

export default EventExecution;
