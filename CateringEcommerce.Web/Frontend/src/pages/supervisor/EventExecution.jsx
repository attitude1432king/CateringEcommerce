/**
 * EventExecution Page (REDESIGNED)
 * Modern event supervision workflow: Pre-Event -> During-Event -> Post-Event -> Summary
 */

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    ArrowLeft, ClipboardCheck, UtensilsCrossed, FileText, BarChart3, CheckCircle, Calendar, MapPin, Building, Lock
} from 'lucide-react';
import { getAssignmentById } from '../../services/api/supervisor/assignmentApi';
import { PreEventChecklist } from '../../components/supervisor/event';
import { FoodServingMonitor, GuestCountTracker, ExtraQuantityRequest, ClientOTPVerification, LiveIssueReporter } from '../../components/supervisor/event/during-event';
import { PostEventReportSubmit, EventSupervisionSummary } from '../../components/supervisor/event/post-event';
import { SupervisorNavHeader } from './SupervisorDashboard';
import toast from 'react-hot-toast';

const PHASES = [
    { id: 'pre-event', label: 'Pre-Event', shortLabel: 'Pre', icon: ClipboardCheck },
    { id: 'during-event', label: 'During Event', shortLabel: 'During', icon: UtensilsCrossed },
    { id: 'post-event', label: 'Post-Event', shortLabel: 'Post', icon: FileText },
    { id: 'summary', label: 'Summary', shortLabel: 'Summary', icon: BarChart3 },
];

const EventExecution = () => {
    const { assignmentId } = useParams();
    const navigate = useNavigate();
    const [currentPhase, setCurrentPhase] = useState('pre-event');
    const [completedPhases, setCompletedPhases] = useState([]); // P2 FIX: Track completed phases
    const [assignment, setAssignment] = useState(null);
    const [loading, setLoading] = useState(true);
    const [showOTPVerification, setShowOTPVerification] = useState(false);

    useEffect(() => { loadAssignment(); }, [assignmentId]);

    const loadAssignment = async () => {
        try {
            const response = await getAssignmentById(assignmentId);
            if (response.success) {
                setAssignment(response.data?.data || response.data);
                const status = response.data?.data?.status || response.data?.status;
                if (status === 'COMPLETED' || status === 'POST_EVENT_DONE') setCurrentPhase('summary');
                else if (status === 'POST_EVENT') setCurrentPhase('post-event');
                else if (status === 'IN_PROGRESS' || status === 'CHECKED_IN') setCurrentPhase('during-event');
            } else { toast.error('Failed to load assignment'); }
        } catch { toast.error('Failed to load assignment'); }
        finally { setLoading(false); }
    };

    const handlePhaseComplete = (phase) => {
        const idx = PHASES.findIndex((p) => p.id === phase);
        // P2 FIX: Mark phase as completed
        if (!completedPhases.includes(phase)) {
            setCompletedPhases([...completedPhases, phase]);
        }
        if (idx < PHASES.length - 1) {
            setCurrentPhase(PHASES[idx + 1].id);
            toast.success(`${PHASES[idx].label} phase completed`);
        }
    };

    // P2 FIX: Check if a phase can be accessed (sequential enforcement)
    const canAccessPhase = (phaseId) => {
        const phaseIdx = PHASES.findIndex((p) => p.id === phaseId);
        const currentIdx = PHASES.findIndex((p) => p.id === currentPhase);

        // Can always access current phase
        if (phaseId === currentPhase) return true;

        // Can access completed phases
        if (completedPhases.includes(phaseId)) return true;

        // Can access the next phase if all previous phases are completed
        if (phaseIdx === 0) return true; // First phase always accessible

        for (let i = 0; i < phaseIdx; i++) {
            if (!completedPhases.includes(PHASES[i].id)) {
                return false; // Cannot skip incomplete phases
            }
        }

        return true;
    };

    // P2 FIX: Handle phase change with validation
    const handlePhaseChange = (phaseId) => {
        if (canAccessPhase(phaseId)) {
            setCurrentPhase(phaseId);
        } else {
            toast.error('Please complete previous phases first');
        }
    };

    const currentIdx = PHASES.findIndex((p) => p.id === currentPhase);

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4"></div>
                    <p className="text-neutral-600 text-sm">Loading event...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30">
            <SupervisorNavHeader activePath="/supervisor/assignments" />

            {/* Header */}
            <div className="bg-gradient-to-r from-indigo-50 to-purple-50 border-b-2 border-neutral-100">
                <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                    <button onClick={() => navigate(`/supervisor/assignments/${assignmentId}`)} className="flex items-center gap-2 text-neutral-600 hover:text-neutral-900 mb-4 text-sm font-medium transition-colors">
                        <ArrowLeft className="w-4 h-4" /> Back to Assignment
                    </button>
                    <div className="flex items-start justify-between flex-wrap gap-4">
                        <div className="flex items-center gap-4">
                            <div className="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center">
                                <ClipboardCheck className="w-7 h-7 text-indigo-600" />
                            </div>
                            <div>
                                <h1 className="text-3xl font-bold text-neutral-800">Event Supervision</h1>
                                {assignment && (
                                    <div className="flex flex-wrap items-center gap-3 mt-1.5">
                                        <span className="text-sm text-neutral-600">#{assignment.assignmentNumber || assignmentId}</span>
                                        {assignment.eventName && (
                                            <span className="text-sm text-neutral-500 flex items-center gap-1">
                                                <Calendar className="w-3.5 h-3.5" /> {assignment.eventName}
                                            </span>
                                        )}
                                        {(assignment.cateringName || assignment.partnerName) && (
                                            <span className="text-sm text-neutral-500 flex items-center gap-1">
                                                <Building className="w-3.5 h-3.5" /> {assignment.cateringName || assignment.partnerName}
                                            </span>
                                        )}
                                        {assignment.eventLocation && (
                                            <span className="text-sm text-neutral-500 flex items-center gap-1">
                                                <MapPin className="w-3.5 h-3.5" /> {assignment.eventLocation}
                                            </span>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>
                        <div className="flex items-center gap-2 bg-white px-4 py-2 rounded-xl border-2 border-neutral-100 shadow-sm">
                            <span className="text-xs font-medium text-neutral-500 uppercase">Phase</span>
                            <span className="text-sm font-bold text-indigo-700">{currentIdx + 1} / {PHASES.length}</span>
                        </div>
                    </div>
                </div>
            </div>

            {/* Phase Stepper */}
            <div className="bg-white border-b-2 border-neutral-100 shadow-sm">
                <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
                    <div className="flex items-center gap-2 sm:gap-0">
                        {PHASES.map((phase, idx) => {
                            const PhaseIcon = phase.icon;
                            const isActive = currentPhase === phase.id;
                            const isCompleted = completedPhases.includes(phase.id); // P2 FIX: Check completed status
                            const canAccess = canAccessPhase(phase.id); // P2 FIX: Check if accessible
                            const isLocked = !canAccess; // P2 FIX: Phase is locked

                            return (
                                <div key={phase.id} className="flex items-center flex-1">
                                    <button
                                        onClick={() => handlePhaseChange(phase.id)} // P2 FIX: Use validated handler
                                        disabled={isLocked} // P2 FIX: Disable locked phases
                                        className={`flex items-center gap-2.5 px-3 sm:px-4 py-2.5 rounded-xl transition-all duration-200 w-full ${
                                            isActive
                                                ? 'bg-gradient-to-r from-rose-600 to-rose-500 text-white shadow-lg shadow-rose-200'
                                                : isCompleted
                                                ? 'bg-green-50 text-green-700 border-2 border-green-200 hover:bg-green-100'
                                                : isLocked
                                                ? 'bg-neutral-50 text-neutral-300 border-2 border-neutral-100 cursor-not-allowed opacity-60'
                                                : 'bg-neutral-50 text-neutral-500 border-2 border-neutral-100 hover:border-neutral-200'
                                        }`}
                                    >
                                        <div className={`w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 ${
                                            isActive ? 'bg-white/20' : isCompleted ? 'bg-green-100' : isLocked ? 'bg-neutral-100' : 'bg-neutral-100'
                                        }`}>
                                            {isCompleted ? (
                                                <CheckCircle className="w-4 h-4 text-green-600" />
                                            ) : isLocked ? (
                                                <Lock className="w-4 h-4 text-neutral-400" /> // P2 FIX: Lock icon for disabled phases
                                            ) : (
                                                <PhaseIcon className={`w-4 h-4 ${isActive ? 'text-white' : 'text-neutral-500'}`} />
                                            )}
                                        </div>
                                        <div className="hidden sm:block text-left">
                                            <p className={`text-xs font-semibold uppercase tracking-wide ${isActive ? 'text-white/80' : isLocked ? 'text-neutral-400' : ''}`}>Step {idx + 1}</p>
                                            <p className={`text-sm font-bold ${isActive ? 'text-white' : isLocked ? 'text-neutral-400' : ''}`}>{phase.label}</p>
                                        </div>
                                        <span className="sm:hidden text-xs font-bold">{phase.shortLabel}</span>
                                    </button>
                                    {idx < PHASES.length - 1 && (
                                        <div className={`hidden sm:block w-6 h-0.5 flex-shrink-0 mx-1 ${
                                            idx < currentIdx ? 'bg-green-300' : 'bg-neutral-200'
                                        }`} />
                                    )}
                                </div>
                            );
                        })}
                    </div>
                </div>
            </div>

            {/* Phase Content */}
            <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
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
                                if (data?.requiresOTP) setShowOTPVerification(true);
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

                        {/* Complete During-Event */}
                        <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
                            <div className="bg-gradient-to-r from-indigo-50 to-purple-50 rounded-xl p-5 flex flex-col sm:flex-row items-center justify-between gap-4">
                                <div>
                                    <h4 className="text-lg font-bold text-neutral-800">Ready to wrap up?</h4>
                                    <p className="text-sm text-neutral-600 mt-0.5">Complete the during-event phase and proceed to the post-event report.</p>
                                </div>
                                <button
                                    onClick={() => handlePhaseComplete('during-event')}
                                    className="px-6 py-3 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all flex items-center gap-2 whitespace-nowrap"
                                >
                                    <FileText className="w-5 h-5" />
                                    Proceed to Post-Event
                                </button>
                            </div>
                        </section>
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
