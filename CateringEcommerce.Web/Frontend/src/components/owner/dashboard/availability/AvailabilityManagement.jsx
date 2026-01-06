/*
========================================
File: src/components/owner/dashboard/availability/AvailabilityManagement.jsx
========================================
Main container for the Availability Management module.
*/
import React, { useState, useEffect } from 'react';
import GlobalStatusCard from './GlobalStatusCard';
import AvailabilityCalendar from './AvailabilityCalendar';
import Loader from '../../../common/Loader';
import { useToast } from '../../../../contexts/ToastContext';
import { ownerApiService } from '../../../../services/ownerApi';

// Mock Data
const mockAvailabilityData = {
    globalStatus: 'OPEN', // 'OPEN', 'CLOSED'
    specialDates: {
        '2024-10-15': { status: 'FULLY_BOOKED', note: '3 Weddings' },
        '2024-10-25': { status: 'CLOSED', note: 'Family Function' },
        '2024-11-01': { status: 'CLOSED', note: 'Maintenance' }
    }
};

export default function AvailabilityManagement() {
    const [globalStatus, setGlobalStatus] = useState('OPEN');
    const [specialDates, setSpecialDates] = useState({});
    const [isLoading, setIsLoading] = useState(true);
    const { showToast } = useToast();

    const fetchAvailabilityData = async () => {
        setIsLoading(true);
        try {
            const response = await ownerApiService.getAvailability();
            setGlobalStatus(response.data.globalStatus);
            setSpecialDates(response.data.specialDates);
        } catch (error) {
            console.error('Error fetching availability data:', error);
            showToast('Failed to load availability data.', 'error');
        }
        finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        fetchAvailabilityData();
    }, []);

    const handleGlobalStatusChange = (newStatus) => {
        setGlobalStatus(newStatus);
        ownerApiService.updateGlobalAvailability(newStatus);
        showToast(`Catering is now ${newStatus === 'OPEN' ? 'Open' : 'Closed'} globally.`, 'success');
    };

    const handleDateStatusUpdate = (date, status, note) => {
        setIsLoading(true);
        try
        {
            const updatedDates = { ...specialDates };
            if (status === 'OPEN') {
                delete updatedDates[date]; // Remove override if setting back to Open
            } else {
                updatedDates[date] = { status, note };
            }
            setSpecialDates(updatedDates);
            const payload = { date, status, note };
            ownerApiService.updateDateAvailability(payload);
            showToast(`Availability updated for ${date}`, 'success');
        } catch (e) {
            console.error(`Failed to update for ${date}: ${e}`)
            showToast(`Failed to update for ${date}`, 'error')
        }
        finally {
            setIsLoading(false);
        }
    };

    if (isLoading) return <div className="flex justify-center items-center h-96"><Loader /></div>;

    return (
        <div className="p-4 sm:p-6 lg:p-8 space-y-8">
            <div>
                <h1 className="text-3xl font-bold text-neutral-800">Availability Management</h1>
                <p className="text-neutral-500 mt-1">Control when you can accept new catering orders.</p>
            </div>

            {/* Feature 1: Global Status */}
            <GlobalStatusCard
                status={globalStatus}
                onStatusChange={handleGlobalStatusChange}
            />

            {/* Feature 2 & 3: Calendar Management */}
            <div className={`transition-opacity duration-300 ${globalStatus === 'CLOSED' ? 'opacity-50 pointer-events-none' : 'opacity-100'}`}>
                <AvailabilityCalendar
                    specialDates={specialDates}
                    onDateUpdate={handleDateStatusUpdate}
                />
            </div>

            {globalStatus === 'CLOSED' && (
                <div className="bg-amber-50 border-l-4 border-amber-500 p-4 rounded-md">
                    <p className="text-amber-700 text-sm font-medium">
                        Your catering service is currently marked as <strong>Closed Globally</strong>. The calendar settings above will not apply until you open your service.
                    </p>
                </div>
            )}
        </div>
    );
}