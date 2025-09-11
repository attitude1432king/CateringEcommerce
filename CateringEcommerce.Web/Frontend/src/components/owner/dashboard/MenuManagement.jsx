/*
========================================
File: src/components/owner/dashboard/MenuManagement.jsx (NEW FILE)
========================================
*/

export default function MenuManagement() {
    return (
        <div className="animate-fade-in space-y-6">
            <div className="flex items-center justify-between">
                <h1 className="text-3xl font-bold text-neutral-800">Menu & Packages</h1>
                <div className="flex gap-2">
                    <button className="bg-white text-rose-600 border border-rose-600 px-4 py-2 rounded-md font-medium hover:bg-rose-50">Create Package</button>
                    <button className="bg-rose-600 text-white px-4 py-2 rounded-md font-medium hover:bg-rose-700">Add Menu Item</button>
                </div>
            </div>
            <div className="bg-white p-6 rounded-xl shadow-sm">
                <h3 className="font-semibold text-neutral-800 mb-4">Your Menu Items</h3>
                <div className="text-center py-12 text-neutral-500">
                    <p>Your menu is empty. Click "Add Menu Item" to get started.</p>
                </div>
            </div>
        </div>
    );
}
