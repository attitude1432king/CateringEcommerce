/*
========================================
File: src/components/owner/dashboard/ProfileSettings.jsx (REVISED)
========================================
The main container for the multi-section profile settings page for the CATERING OWNER.
*/
import React, { useState, useEffect } from 'react';
import Loader from '../../common/Loader'; // Assuming Loader component exists
import { ownerApiService } from '../../../services/ownerApi'; // For real API calls
import { useToast } from '../../../contexts/ToastContext'; 

// Import the new, dedicated settings components
import BusinessAccountSettings from './settings/BusinessAccountSettings';
import AddressSettings from './settings/AddressSettings';
import ServicesSettings from './settings/ServicesSettings';
import LegalPaymentSettings from './settings/LegalPaymentSettings';

const TABS = {
    BUSINESS: 'Business/Account',
    ADDRESS: 'Address',
    SERVICES: 'Services',
    LEGAL: 'Legal & Payment',
};

const TabButton = ({ label, isActive, onClick }) => (
    <button
        onClick={onClick}
        className={`w-full text-left px-4 py-3 text-sm font-medium rounded-md transition-colors ${isActive
                ? 'bg-rose-100 text-rose-700'
                : 'text-neutral-600 hover:bg-neutral-100'
            }`}
    >
        {label}
    </button>
);

export default function ProfileSettings() {
    const [activeTab, setActiveTab] = useState(TABS.BUSINESS);
    const [profileData, setProfileData] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const { showToast } = useToast();

    const fetchProfileData = async () => {
        setIsLoading(true);
        try {
            const data = await ownerApiService.getOwnerProfile();
            setProfileData(data);
        } catch (error) {
            console.error("Failed to fetch profile data", error);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {    
        fetchProfileData();
    }, []);

    const handleUpdate = async (section, data) => {
        console.log(`Updating ${section} data:`, data);
        try {
            let response;
            setIsLoading(true);
            setIsSaving(true);
            // Call the correct API function based on the section being updated
            switch (section) {
                case 'business':
                    response = await ownerApiService.updateBusinessSettings(data);
                    break;
                case 'address':
                    response = await ownerApiService.updateAddressSettings(data);
                    break;
                case 'services':
                    response = await ownerApiService.updateServicesSettings(data);
                    break;
                case 'legal':
                    response = await ownerApiService.updateLegalPaymentSettings(data);
                    break;
                default:
                    throw new Error("Invalid update section");
            }

            showToast(response.message || 'Details updated successfully!', 'success');

            // Optimistically update the local state
            //setProfileData(prev => ({ ...prev, [section]: data }));
            await fetchProfileData();

        } catch (error) {
            console.error(`Failed to update ${section}`, error);
            showToast(`Error: ${error.message || 'Could not update details.'}`, 'error');
        } finally {
            setIsLoading(false);
            setIsSaving(false);
        }
    };

    const renderContent = () => {
        if (isLoading) {
            return <div className="flex justify-center items-center h-96"><Loader /></div>;
        }

        switch (activeTab) {
            case TABS.BUSINESS:
                return <BusinessAccountSettings initialData={profileData.formData.ownerBusiness} onUpdate={(data) => handleUpdate('business', data)} isSaving={isSaving} />;
            case TABS.ADDRESS:
                return <AddressSettings initialData={profileData.formData.cateringAddress} onUpdate={(data) => handleUpdate('address', data)} isSaving={isSaving} />;
            case TABS.SERVICES:
                return <ServicesSettings initialData={profileData.formData.cateringServices} onUpdate={(data) => handleUpdate('services', data)} isSaving={isSaving} />;
            case TABS.LEGAL:
                return <LegalPaymentSettings initialData={profileData.formData.ownerLegalDocument} onUpdate={(data) => handleUpdate('legal', data)} isSaving={isSaving} />;
            default:
                return null;
        }
    };

    return (
        <div className="animate-fade-in space-y-6">
            <h1 className="text-3xl font-bold text-neutral-800">Profile & Settings</h1>
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
                <aside className="lg:col-span-1">
                    <div className="bg-white p-4 rounded-xl shadow-sm space-y-1 sticky top-8">
                        {Object.values(TABS).map(tab => (
                            <TabButton
                                key={tab}
                                label={tab}
                                isActive={activeTab === tab}
                                onClick={() => setActiveTab(tab)}
                            />
                        ))}
                    </div>
                </aside>
                <main className="lg:col-span-3">
                    {renderContent()}
                </main>
            </div>
        </div>
    );
}
