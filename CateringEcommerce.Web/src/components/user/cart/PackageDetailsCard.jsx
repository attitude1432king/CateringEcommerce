import React, { useState } from 'react';

/**
 * Package Details Card - Shows comprehensive package breakdown
 * Displays all food categories and items selected in the package
 */
const PackageDetailsCard = ({ packageData, guestCount }) => {
  const [isExpanded, setIsExpanded] = useState(true);

  if (!packageData || !packageData.packageName) return null;

  // Parse package selections if available
  const packageSelections = packageData.packageSelections
    ? (typeof packageData.packageSelections === 'string'
      ? JSON.parse(packageData.packageSelections)
      : packageData.packageSelections)
    : null;

  const packageCategories = packageSelections?.selections
    || (packageSelections?.selectedItems
      ? Object.entries(packageSelections.selectedItems).map(([categoryName, items], index) => ({
          categoryId: index,
          categoryName,
          selectedItems: items
        }))
      : []);

  const sampleTasteSelections = packageSelections?.sampleTasteSelections || [];

  const getCategoryIcon = (categoryName) => {
    const icons = {
      'starters': '🥗',
      'appetizers': '🥙',
      'main course': '🍛',
      'rice & breads': '🍚',
      'desserts': '🍰',
      'beverages': '🥤',
      'salads': '🥗',
      'soups': '🍲',
      'chaat': '🌮',
      'snacks': '🍿',
      'default': '🍽️'
    };

    const key = categoryName.toLowerCase();
    return icons[key] || icons['default'];
  };

  const getCategoryBadgeColor = (index) => {
    const colors = [
      'bg-red-100 text-red-700 border-red-200',
      'bg-orange-100 text-orange-700 border-orange-200',
      'bg-yellow-100 text-yellow-700 border-yellow-200',
      'bg-green-100 text-green-700 border-green-200',
      'bg-blue-100 text-blue-700 border-blue-200',
      'bg-purple-100 text-purple-700 border-purple-200',
    ];
    return colors[index % colors.length];
  };

  const packageTotal = (packageData.packagePrice || 0) * guestCount;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
      {/* Header */}
      <div className="bg-gradient-to-r from-orange-50 to-red-50 border-b border-gray-200 p-6">
        <div className="flex items-start justify-between">
          <div className="flex-1">
            <div className="flex items-center gap-2 mb-2">
              <span className="text-2xl">📦</span>
              <h3 className="text-xl font-bold text-gray-900">{packageData.packageName}</h3>
            </div>
            {packageData.packageCategory && (
              <span className="inline-block bg-orange-100 text-orange-800 text-xs font-semibold px-3 py-1 rounded-full">
                {packageData.packageCategory} Package
              </span>
            )}
            <div className="flex items-center gap-4 mt-3 text-sm text-gray-600">
              <span className="flex items-center gap-1">
                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M13 6a3 3 0 11-6 0 3 3 0 016 0zM18 8a2 2 0 11-4 0 2 2 0 014 0zM14 15a4 4 0 00-8 0v3h8v-3zM6 8a2 2 0 11-4 0 2 2 0 014 0zM16 18v-3a5.972 5.972 0 00-.75-2.906A3.005 3.005 0 0119 15v3h-3zM4.75 12.094A5.973 5.973 0 004 15v3H1v-3a3 3 0 013.75-2.906z" />
                </svg>
                {guestCount} Guests
              </span>
              <span className="flex items-center gap-1">
                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 3a1 1 0 000 2v8a2 2 0 002 2h2.586l-1.293 1.293a1 1 0 101.414 1.414L10 15.414l2.293 2.293a1 1 0 001.414-1.414L12.414 15H15a2 2 0 002-2V5a1 1 0 100-2H3zm11.707 4.707a1 1 0 00-1.414-1.414L10 9.586 8.707 8.293a1 1 0 00-1.414 0l-2 2a1 1 0 101.414 1.414L8 10.414l1.293 1.293a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                ₹{packageData.packagePrice?.toLocaleString('en-IN')} per plate
              </span>
            </div>
          </div>
          <div className="text-right">
            <p className="text-sm text-gray-600 mb-1">Package Total</p>
            <p className="text-2xl font-bold text-orange-600">
              ₹{packageTotal.toLocaleString('en-IN')}
            </p>
          </div>
        </div>

        {/* Expand/Collapse Button */}
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="mt-4 w-full flex items-center justify-center gap-2 py-2 px-4 bg-white hover:bg-gray-50 border border-gray-200 rounded-lg transition-colors text-sm font-medium text-gray-700"
        >
          <span>{isExpanded ? 'Hide' : 'View'} Package Details</span>
          <svg
            className={`w-4 h-4 transition-transform ${isExpanded ? 'rotate-180' : ''}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
          </svg>
        </button>
      </div>

      {/* Package Details (Collapsible) */}
      {isExpanded && (
        <div className="p-6">
          {packageSelections && packageCategories.length > 0 ? (
            <div className="space-y-6">
              <div className="flex items-center gap-2 pb-4 border-b">
                <svg className="w-5 h-5 text-green-600" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                <span className="text-sm font-semibold text-green-700">Package Customized</span>
                <span className="text-xs text-gray-500 ml-auto">
                  {packageCategories.length} Categories Selected
                </span>
              </div>

              {/* Food Categories */}
              {packageCategories.map((category, index) => {
                const items = category.selectedItems || [];
                return (
                <div key={`${category.categoryId}-${category.categoryName}`} className="space-y-3">
                  <div className="flex items-center gap-3">
                    <span className="text-2xl">{getCategoryIcon(category.categoryName)}</span>
                    <div className="flex-1">
                      <h4 className="font-semibold text-gray-900 capitalize">{category.categoryName}</h4>
                      <p className="text-xs text-gray-500">{items.length} items selected</p>
                    </div>
                    <span className={`text-xs font-medium px-3 py-1 rounded-full border ${getCategoryBadgeColor(index)}`}>
                      {items.length} {items.length === 1 ? 'Item' : 'Items'}
                    </span>
                  </div>

                  {/* Food Items in Category */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 pl-11">
                    {items.map((item, itemIndex) => (
                      <div
                        key={itemIndex}
                        className="flex items-center gap-2 p-2 bg-gray-50 rounded-lg"
                      >
                        <div className="w-2 h-2 rounded-full bg-orange-500"></div>
                        <span className="text-sm text-gray-700 flex-1">{item.foodName || item.name || item}</span>
                        {(item.isVeg !== undefined || item.isVegetarian !== undefined) && (
                          <span className={`w-4 h-4 border-2 flex items-center justify-center ${
                            (item.isVeg ?? item.isVegetarian) ? 'border-green-600' : 'border-red-600'
                          }`}>
                            <span className={`w-1.5 h-1.5 rounded-full ${
                              (item.isVeg ?? item.isVegetarian) ? 'bg-green-600' : 'bg-red-600'
                            }`}></span>
                          </span>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )})}

              {sampleTasteSelections.length > 0 && (
                <div className="pt-4 border-t space-y-3">
                  <div className="flex items-center gap-2">
                    <svg className="w-5 h-5 text-teal-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
                    </svg>
                    <h4 className="font-semibold text-gray-900">Sample Taste Selections</h4>
                  </div>
                  {sampleTasteSelections.map((category) => (
                    <div key={`sample-${category.categoryId}-${category.categoryName}`} className="bg-teal-50 border border-teal-100 rounded-xl p-3">
                      <p className="text-sm font-semibold text-teal-900 mb-2">{category.categoryName}</p>
                      <div className="flex flex-wrap gap-2">
                        {(category.selectedItems || []).map((item) => (
                          <span key={item.foodItemId || item.name} className="px-2.5 py-1 bg-white text-teal-700 border border-teal-200 rounded-lg text-xs font-medium">
                            {item.name}
                          </span>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Additional Info */}
              {packageSelections.servingStyle && (
                <div className="pt-4 border-t">
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                      <path d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z" />
                      <path fillRule="evenodd" d="M4 5a2 2 0 012-2 3 3 0 003 3h2a3 3 0 003-3 2 2 0 012 2v11a2 2 0 01-2 2H6a2 2 0 01-2-2V5zm3 4a1 1 0 000 2h.01a1 1 0 100-2H7zm3 0a1 1 0 000 2h3a1 1 0 100-2h-3zm-3 4a1 1 0 100 2h.01a1 1 0 100-2H7zm3 0a1 1 0 100 2h3a1 1 0 100-2h-3z" clipRule="evenodd" />
                    </svg>
                    <span>Serving Style: <strong className="text-gray-900">{packageSelections.servingStyle}</strong></span>
                  </div>
                </div>
              )}
            </div>
          ) : (
            // Default Package View (no customization)
            <div className="text-center py-8">
              <div className="w-16 h-16 bg-orange-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-3xl">🍽️</span>
              </div>
              <h4 className="font-semibold text-gray-900 mb-2">Standard Package</h4>
              <p className="text-sm text-gray-600 mb-4">
                This package includes chef's special selection of dishes
              </p>
              <div className="inline-flex items-center gap-2 bg-blue-50 text-blue-700 text-xs font-medium px-4 py-2 rounded-full">
                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                </svg>
                Contact caterer for detailed menu
              </div>
            </div>
          )}

          {/* Package Features */}
          <div className="mt-6 pt-6 border-t">
            <h4 className="font-semibold text-gray-900 mb-3">Package Includes</h4>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div className="flex items-center gap-2 text-sm text-gray-700">
                <svg className="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                <span>Fresh ingredients</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-700">
                <svg className="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                <span>Professional service</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-700">
                <svg className="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                <span>Hygienic packaging</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-700">
                <svg className="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                </svg>
                <span>On-time delivery</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PackageDetailsCard;
