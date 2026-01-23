/*
========================================
File: src/pages/owner/StaffManagementPage.jsx (NEW FILE)
========================================
This is the main page component that will be rendered by the router.
*/
import React from 'react';
import StaffView from '../dashboard/staff/StaffView';

export default function StaffManagement() {
    return (
        <div className="p-4 sm:p-6 lg:p-8">
            <StaffView />
        </div>
    );
}