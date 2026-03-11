/**
 * AssignmentDetails Page (REDESIGNED)
 * Modern assignment detail view with actions
 */

import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, CheckCircle, XCircle, LogIn, IndianRupee, Calendar, MapPin, Building, FileText } from 'lucide-react';
import { getAssignmentById, requestPaymentRelease } from '../../services/api/supervisor/assignmentApi';
import { AssignmentStatusBadge, PaymentStatusBadge } from '../../components/supervisor/common/badges';
import { formatCurrency, formatTimestamp } from '../../utils/supervisor/helpers';
import { AssignmentStatus } from '../../utils/supervisor/supervisorEnums';
import AcceptRejectModal from '../../components/supervisor/assignments/AcceptRejectModal';
import CheckInComponent from '../../components/supervisor/assignments/CheckInComponent';
import { SupervisorNavHeader } from './SupervisorDashboard';
import { useSupervisorAuth } from '../../contexts/SupervisorAuthContext'; // P1 FIX: Import context
import toast from 'react-hot-toast';

const DetailRow = ({ icon: Icon, label, value }) => (
    <div className="flex items-start gap-3 p-4 bg-neutral-50 rounded-xl">
        {Icon && <div className="w-9 h-9 bg-white rounded-lg flex items-center justify-center flex-shrink-0 border border-neutral-200">
            <Icon className="w-4 h-4 text-neutral-600" />
        </div>}
        <div>
            <p className="text-xs font-medium text-neutral-500 uppercase tracking-wide">{label}</p>
            <p className="text-sm font-semibold text-neutral-800 mt-0.5">{value || 'N/A'}</p>
        </div>
    </div>
);

const AssignmentDetails = () => {
    const { assignmentId } = useParams();
    const navigate = useNavigate();
    const { supervisorId } = useSupervisorAuth(); // P1 FIX: Use context instead of localStorage
    const [assignment, setAssignment] = useState(null);
    const [loading, setLoading] = useState(true);
    const [showAcceptModal, setShowAcceptModal] = useState(false);
    const [showRejectModal, setShowRejectModal] = useState(false);
    const [showCheckIn, setShowCheckIn] = useState(false);

    useEffect(() => { fetchAssignment(); }, [assignmentId]);

    const fetchAssignment = async () => {
        try {
            const response = await getAssignmentById(assignmentId);
            if (response.success) { setAssignment(response.data); }
        } catch (error) { console.error('Failed to fetch assignment:', error); toast.error('Failed to load assignment'); }
        finally { setLoading(false); }
    };

    const handleRequestPayment = async () => {
        try {
            // P1 FIX: supervisorId now from context instead of localStorage
            const response = await requestPaymentRelease(assignmentId, supervisorId, assignment.supervisorFee);
            if (response.success) { toast.success('Payment release requested'); fetchAssignment(); }
            else { toast.error(response.message); }
        } catch { toast.error('Failed to request payment'); }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4"></div>
                    <p className="text-neutral-600 text-sm">Loading assignment...</p>
                </div>
            </div>
        );
    }

    if (!assignment) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <p className="text-neutral-600 font-medium">Assignment not found</p>
                    <button onClick={() => navigate('/supervisor/assignments')} className="mt-4 px-5 py-2.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl text-sm font-semibold">
                        Back to Assignments
                    </button>
                </div>
            </div>
        );
    }

    const canAccept = assignment.assignmentStatus === AssignmentStatus.ASSIGNED;
    const canCheckIn = assignment.assignmentStatus === AssignmentStatus.ACCEPTED && !assignment.checkedIn;
    const canRequestPayment = assignment.assignmentStatus === AssignmentStatus.COMPLETED && !assignment.paymentReleaseRequested;

    return (
        <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30">
            <SupervisorNavHeader activePath="/supervisor/assignments" />

            {/* Header */}
            <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-neutral-100">
                <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                    <button onClick={() => navigate('/supervisor/assignments')} className="flex items-center gap-2 text-neutral-600 hover:text-neutral-900 mb-4 text-sm font-medium transition-colors">
                        <ArrowLeft className="w-4 h-4" /> Back to Assignments
                    </button>
                    <div className="flex items-start justify-between flex-wrap gap-4">
                        <div>
                            <h1 className="text-3xl font-bold text-neutral-800">{assignment.assignmentNumber}</h1>
                            <p className="text-sm text-neutral-600 mt-1">Order: {assignment.orderNumber}</p>
                        </div>
                        <div className="flex flex-col items-end gap-2">
                            <AssignmentStatusBadge status={assignment.assignmentStatus} />
                            {assignment.checkedIn && (
                                <span className="text-xs text-green-700 font-semibold flex items-center gap-1 bg-green-100 px-3 py-1 rounded-full">
                                    <CheckCircle className="w-3 h-3" /> Checked In
                                </span>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
                {/* Event Details */}
                <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
                    <div className="flex items-center gap-3 mb-6">
                        <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                            <Calendar className="w-5 h-5 text-blue-600" />
                        </div>
                        <div>
                            <h3 className="text-xl font-bold text-neutral-800">Event Details</h3>
                            <p className="text-sm text-neutral-500">Information about the assigned event</p>
                        </div>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <DetailRow icon={Calendar} label="Event Date" value={formatTimestamp(assignment.eventDate, 'long')} />
                        <DetailRow icon={FileText} label="Event Type" value={assignment.eventType} />
                        <DetailRow icon={MapPin} label="Location" value={assignment.eventLocation} />
                        <DetailRow icon={Building} label="Partner" value={assignment.cateringName || assignment.partnerName} />
                        <DetailRow icon={IndianRupee} label="Supervisor Fee" value={formatCurrency(assignment.supervisorFee)} />
                        <DetailRow icon={Calendar} label="Assigned Date" value={formatTimestamp(assignment.assignedDate, 'short')} />
                    </div>
                </section>

                {/* Notes */}
                {assignment.assignmentNotes && (
                    <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center">
                                <FileText className="w-5 h-5 text-amber-600" />
                            </div>
                            <h3 className="text-xl font-bold text-neutral-800">Notes</h3>
                        </div>
                        <div className="bg-amber-50 border-l-4 border-amber-400 rounded-lg p-4">
                            <p className="text-sm text-neutral-700">{assignment.assignmentNotes}</p>
                        </div>
                    </section>
                )}

                {/* Payment Status */}
                {assignment.assignmentStatus === AssignmentStatus.COMPLETED && (
                    <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                                <IndianRupee className="w-5 h-5 text-green-600" />
                            </div>
                            <h3 className="text-xl font-bold text-neutral-800">Payment Status</h3>
                        </div>
                        <div className="bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-200 rounded-xl p-5 flex items-center justify-between">
                            <div>
                                <p className="text-sm font-semibold text-green-900">Payment</p>
                                <div className="mt-1.5">
                                    <PaymentStatusBadge status={assignment.paymentReleaseApproved ? 'RELEASED' : assignment.paymentReleaseRequested ? 'PENDING' : 'NOT_REQUESTED'} />
                                </div>
                            </div>
                            {canRequestPayment && (
                                <button onClick={handleRequestPayment} className="px-5 py-2.5 bg-gradient-to-r from-green-600 to-green-500 text-white rounded-xl text-sm font-semibold shadow-lg hover:shadow-xl transition-all flex items-center gap-2">
                                    <IndianRupee className="w-4 h-4" /> Request Payment
                                </button>
                            )}
                        </div>
                    </section>
                )}

                {/* Actions */}
                <div className="flex gap-4">
                    {canAccept && (
                        <>
                            <button onClick={() => setShowAcceptModal(true)} className="flex-1 px-6 py-3.5 bg-gradient-to-r from-green-600 to-green-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all flex items-center justify-center gap-2">
                                <CheckCircle className="w-5 h-5" /> Accept Assignment
                            </button>
                            <button onClick={() => setShowRejectModal(true)} className="flex-1 px-6 py-3.5 bg-gradient-to-r from-red-600 to-red-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all flex items-center justify-center gap-2">
                                <XCircle className="w-5 h-5" /> Reject Assignment
                            </button>
                        </>
                    )}
                    {canCheckIn && (
                        <button onClick={() => setShowCheckIn(true)} className="flex-1 px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all flex items-center justify-center gap-2">
                            <LogIn className="w-5 h-5" /> Check In
                        </button>
                    )}
                    {assignment.assignmentStatus === AssignmentStatus.IN_PROGRESS && (
                        <button onClick={() => navigate(`/supervisor/assignments/${assignmentId}/execute`)} className="flex-1 px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all">
                            Continue Event Supervision
                        </button>
                    )}
                </div>
            </div>

            {/* Modals */}
            {showAcceptModal && <AcceptRejectModal assignmentId={assignmentId} action="accept" onClose={() => setShowAcceptModal(false)} onSuccess={() => { setShowAcceptModal(false); fetchAssignment(); }} />}
            {showRejectModal && <AcceptRejectModal assignmentId={assignmentId} action="reject" onClose={() => setShowRejectModal(false)} onSuccess={() => { setShowRejectModal(false); navigate('/supervisor/assignments'); }} />}
            {showCheckIn && <CheckInComponent assignmentId={assignmentId} onClose={() => setShowCheckIn(false)} onSuccess={() => { setShowCheckIn(false); fetchAssignment(); }} />}
        </div>
    );
};

export default AssignmentDetails;
