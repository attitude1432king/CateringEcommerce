/**
 * AssignmentsList Page
 * View all supervisor assignments with filters
 */

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Filter, Search } from 'lucide-react';
import { getAssignmentsBySupervisor } from '../../services/api/supervisor/assignmentApi';
import AssignmentCard from '../../components/supervisor/assignments/AssignmentCard';
import { AssignmentStatus } from '../../utils/supervisor/supervisorEnums';
import toast from 'react-hot-toast';

const AssignmentsList = () => {
    const navigate = useNavigate();
    const [assignments, setAssignments] = useState([]);
    const [filteredAssignments, setFilteredAssignments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [statusFilter, setStatusFilter] = useState('ALL');
    const [searchQuery, setSearchQuery] = useState('');

    useEffect(() => {
        fetchAssignments();
    }, []);

    useEffect(() => {
        filterAssignments();
    }, [statusFilter, searchQuery, assignments]);

    const fetchAssignments = async () => {
        try {
            const supervisorId = localStorage.getItem('supervisorId');
            const response = await getAssignmentsBySupervisor(supervisorId);

            if (response.success) {
                setAssignments(response.data);
            } else {
                toast.error('Failed to load assignments');
            }
        } catch (error) {
            console.error('Failed to fetch assignments:', error);
            toast.error('Failed to load assignments');
        } finally {
            setLoading(false);
        }
    };

    const filterAssignments = () => {
        let filtered = [...assignments];

        // Filter by status
        if (statusFilter !== 'ALL') {
            filtered = filtered.filter(a => a.assignmentStatus === statusFilter);
        }

        // Filter by search query
        if (searchQuery) {
            const query = searchQuery.toLowerCase();
            filtered = filtered.filter(
                a =>
                    a.assignmentNumber.toLowerCase().includes(query) ||
                    a.vendorName.toLowerCase().includes(query) ||
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
            <div className="flex justify-center items-center min-h-screen">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="bg-white border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                    <h1 className="text-3xl font-bold text-gray-900">My Assignments</h1>
                    <p className="text-gray-600 mt-1">
                        View and manage your event assignments
                    </p>
                </div>
            </div>

            {/* Filters */}
            <div className="bg-white border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
                    <div className="flex flex-col sm:flex-row gap-4">
                        {/* Search */}
                        <div className="flex-1">
                            <div className="relative">
                                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                    <Search className="h-5 w-5 text-gray-400" />
                                </div>
                                <input
                                    type="text"
                                    placeholder="Search by assignment number, vendor, or event type..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                />
                            </div>
                        </div>

                        {/* Status Filter */}
                        <div className="sm:w-64">
                            <div className="relative">
                                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                    <Filter className="h-5 w-5 text-gray-400" />
                                </div>
                                <select
                                    value={statusFilter}
                                    onChange={(e) => setStatusFilter(e.target.value)}
                                    className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                >
                                    {statusOptions.map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Assignments List */}
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Summary */}
                <div className="mb-6">
                    <p className="text-sm text-gray-600">
                        Showing {filteredAssignments.length} of {assignments.length} assignments
                    </p>
                </div>

                {/* Grid */}
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
                    <div className="bg-white rounded-lg border border-gray-200 p-12 text-center">
                        <p className="text-gray-600">
                            {searchQuery || statusFilter !== 'ALL'
                                ? 'No assignments match your filters'
                                : 'No assignments yet'}
                        </p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default AssignmentsList;
