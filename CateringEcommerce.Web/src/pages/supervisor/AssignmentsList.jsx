/**
 * AssignmentsList Page (REDESIGNED)
 * Modern assignments list with filters for Event Supervisors
 */

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Filter, Search, ClipboardList } from 'lucide-react';
import { getAssignmentsBySupervisor } from '../../services/api/supervisor/assignmentApi';
import AssignmentCard from '../../components/supervisor/assignments/AssignmentCard';
import { AssignmentStatus } from '../../utils/supervisor/supervisorEnums';
import { SupervisorNavHeader } from './SupervisorDashboard';
import toast from 'react-hot-toast';

const AssignmentsList = () => {
    const navigate = useNavigate();
    const [assignments, setAssignments] = useState([]);
    const [filteredAssignments, setFilteredAssignments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [statusFilter, setStatusFilter] = useState('ALL');
    const [searchQuery, setSearchQuery] = useState('');

    useEffect(() => { fetchAssignments(); }, []);
    useEffect(() => { filterAssignments(); }, [statusFilter, searchQuery, assignments]);

    const fetchAssignments = async () => {
        try {
            const supervisorId = localStorage.getItem('supervisorId');
            const response = await getAssignmentsBySupervisor(supervisorId);
            if (response.success) { setAssignments(response.data); }
            else { toast.error('Failed to load assignments'); }
        } catch (error) { console.error('Failed to fetch assignments:', error); toast.error('Failed to load assignments'); }
        finally { setLoading(false); }
    };

    const filterAssignments = () => {
        let filtered = [...assignments];
        if (statusFilter !== 'ALL') { filtered = filtered.filter(a => a.assignmentStatus === statusFilter); }
        if (searchQuery) {
            const query = searchQuery.toLowerCase();
            filtered = filtered.filter(a =>
                a.assignmentNumber.toLowerCase().includes(query) ||
                (a.cateringName || a.partnerName || '').toLowerCase().includes(query) ||
                a.eventType.toLowerCase().includes(query) ||
                a.orderNumber.toLowerCase().includes(query)
            );
        }
        setFilteredAssignments(filtered);
    };

    const statusOptions = [
        { value: 'ALL', label: 'All Assignments' },
        { value: AssignmentStatus.ASSIGNED, label: 'Assigned' },
        { value: AssignmentStatus.ACCEPTED, label: 'Accepted' },
        { value: AssignmentStatus.IN_PROGRESS, label: 'In Progress' },
        { value: AssignmentStatus.COMPLETED, label: 'Completed' },
        { value: AssignmentStatus.CANCELLED, label: 'Cancelled' },
    ];

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4"></div>
                    <p className="text-neutral-600 text-sm">Loading assignments...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30">
            <SupervisorNavHeader activePath="/supervisor/assignments" />

            {/* Header */}
            <div className="bg-gradient-to-r from-amber-50 to-rose-50 border-b-2 border-neutral-100">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-amber-100 rounded-xl flex items-center justify-center">
                            <ClipboardList className="w-7 h-7 text-amber-600" />
                        </div>
                        <div>
                            <h1 className="text-3xl font-bold text-neutral-800">My Assignments</h1>
                            <p className="text-neutral-600 text-sm mt-1">View and manage your event assignments</p>
                        </div>
                    </div>
                </div>
            </div>

            {/* Filters */}
            <div className="bg-white border-b-2 border-neutral-100 shadow-sm">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
                    <div className="flex flex-col sm:flex-row gap-4">
                        <div className="flex-1">
                            <div className="relative">
                                <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                                <input
                                    type="text"
                                    placeholder="Search by assignment number, vendor, or event type..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    className="w-full pl-11 pr-4 py-3 border-2 border-neutral-200 rounded-xl focus:outline-none focus:border-rose-400 focus:ring-2 focus:ring-rose-100 transition-all duration-200"
                                />
                            </div>
                        </div>
                        <div className="sm:w-64">
                            <div className="relative">
                                <Filter className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                                <select
                                    value={statusFilter}
                                    onChange={(e) => setStatusFilter(e.target.value)}
                                    className="w-full pl-11 pr-4 py-3 border-2 border-neutral-200 rounded-xl bg-white focus:outline-none focus:border-rose-400 focus:ring-2 focus:ring-rose-100 transition-all duration-200 appearance-none"
                                >
                                    {statusOptions.map((option) => (
                                        <option key={option.value} value={option.value}>{option.label}</option>
                                    ))}
                                </select>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <div className="mb-6">
                    <p className="text-sm font-medium text-neutral-500">
                        Showing <span className="text-neutral-800 font-semibold">{filteredAssignments.length}</span> of <span className="text-neutral-800 font-semibold">{assignments.length}</span> assignments
                    </p>
                </div>

                {filteredAssignments.length > 0 ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {filteredAssignments.map((assignment) => (
                            <AssignmentCard
                                key={assignment.assignmentId}
                                assignment={assignment}
                                onClick={() => navigate(`/supervisor/assignments/${assignment.assignmentId}`)}
                            />
                        ))}
                    </div>
                ) : (
                    <div className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-16 text-center">
                        <div className="mx-auto w-20 h-20 bg-neutral-100 rounded-full flex items-center justify-center mb-4">
                            <ClipboardList className="w-10 h-10 text-neutral-400" />
                        </div>
                        <p className="text-lg font-semibold text-neutral-700">
                            {searchQuery || statusFilter !== 'ALL' ? 'No assignments match your filters' : 'No assignments yet'}
                        </p>
                        <p className="text-sm text-neutral-500 mt-2">
                            {searchQuery || statusFilter !== 'ALL' ? 'Try adjusting your search or filter criteria' : 'Your assignments will appear here once assigned'}
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default AssignmentsList;
