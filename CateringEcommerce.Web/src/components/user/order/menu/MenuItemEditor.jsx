import React, { useState } from 'react';
import { Plus, Trash2, Edit3, Search } from 'lucide-react';
import { DisabledButton, MonetaryImpactPreview } from '../../../common/safety';

/**
 * MenuItemEditor Component
 *
 * Full menu editing for >7 days before event
 * - Add new items
 * - Remove items
 * - Modify quantities
 * - Requires partner approval
 */

const MenuItemEditor = ({
  currentMenu = [],
  availableItems = [],
  onMenuUpdate,
  isLoading = false
}) => {
  const [editedMenu, setEditedMenu] = useState([...currentMenu]);
  const [searchQuery, setSearchQuery] = useState('');
  const [showAvailableItems, setShowAvailableItems] = useState(false);

  // Calculate price impact
  const calculatePriceImpact = () => {
    const currentTotal = currentMenu.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const newTotal = editedMenu.reduce((sum, item) => sum + (item.price * item.quantity), 0);

    return {
      currentAmount: currentTotal,
      newAmount: newTotal,
      breakdown: editedMenu.map(item => {
        const originalItem = currentMenu.find(orig => orig.itemId === item.itemId);
        if (!originalItem) {
          return { label: `New: ${item.itemName}`, amount: item.price * item.quantity, type: 'add' };
        } else if (originalItem.quantity !== item.quantity) {
          const diff = item.quantity - originalItem.quantity;
          return {
            label: `${item.itemName} (${diff > 0 ? '+' : ''}${diff} qty)`,
            amount: item.price * diff,
            type: diff > 0 ? 'add' : 'deduct'
          };
        }
        return null;
      }).filter(Boolean)
    };
  };

  // Add item to menu
  const handleAddItem = (item) => {
    const exists = editedMenu.find(i => i.itemId === item.itemId);
    if (exists) {
      setEditedMenu(prev =>
        prev.map(i =>
          i.itemId === item.itemId ? { ...i, quantity: i.quantity + 1 } : i
        )
      );
    } else {
      setEditedMenu(prev => [...prev, { ...item, quantity: 1 }]);
    }
  };

  // Remove item from menu
  const handleRemoveItem = (itemId) => {
    setEditedMenu(prev => prev.filter(i => i.itemId !== itemId));
  };

  // Update item quantity
  const handleUpdateQuantity = (itemId, newQuantity) => {
    if (newQuantity <= 0) {
      handleRemoveItem(itemId);
    } else {
      setEditedMenu(prev =>
        prev.map(i =>
          i.itemId === itemId ? { ...i, quantity: newQuantity } : i
        )
      );
    }
  };

  // Filter available items
  const filteredAvailableItems = availableItems.filter(item =>
    item.itemName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    item.itemType.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const hasChanges = JSON.stringify(currentMenu) !== JSON.stringify(editedMenu);
  const priceImpact = hasChanges ? calculatePriceImpact() : null;

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold text-lg">Edit Menu Items</h3>
        <button
          onClick={() => setShowAvailableItems(!showAvailableItems)}
          className="px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition-colors text-sm font-medium"
        >
          {showAvailableItems ? 'Hide' : 'Show'} Available Items
        </button>
      </div>

      {/* Available Items to Add */}
      {showAvailableItems && (
        <div className="border-2 border-blue-200 rounded-lg p-4 bg-blue-50">
          <div className="mb-3">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search available items..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-2 max-h-64 overflow-y-auto">
            {filteredAvailableItems.map((item, index) => (
              <div
                key={index}
                className="flex items-center justify-between p-3 bg-white border border-gray-200 rounded-lg hover:border-blue-400 transition-colors"
              >
                <div className="flex-1">
                  <p className="font-medium text-sm">{item.itemName}</p>
                  <p className="text-xs text-gray-600">{item.itemType}</p>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm font-semibold">₹{item.price}</span>
                  <button
                    onClick={() => handleAddItem(item)}
                    className="w-8 h-8 bg-blue-600 text-white rounded-full hover:bg-blue-700 flex items-center justify-center"
                  >
                    <Plus className="w-4 h-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Edited Menu */}
      <div className="space-y-2">
        <h4 className="font-medium text-gray-700">Current Selection</h4>
        {editedMenu.map((item, index) => (
          <div
            key={index}
            className="flex items-center justify-between p-3 border-2 border-gray-300 rounded-lg bg-gray-50"
          >
            <div className="flex-1">
              <p className="font-medium">{item.itemName}</p>
              <p className="text-sm text-gray-600">{item.itemType}</p>
            </div>

            {/* Quantity Control */}
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2 bg-white rounded-lg border border-gray-300 px-2">
                <button
                  onClick={() => handleUpdateQuantity(item.itemId, item.quantity - 1)}
                  className="w-6 h-6 text-gray-600 hover:text-gray-900"
                >
                  −
                </button>
                <input
                  type="number"
                  value={item.quantity}
                  onChange={(e) => handleUpdateQuantity(item.itemId, parseInt(e.target.value) || 1)}
                  className="w-12 text-center border-none focus:ring-0 py-1"
                  min="1"
                />
                <button
                  onClick={() => handleUpdateQuantity(item.itemId, item.quantity + 1)}
                  className="w-6 h-6 text-gray-600 hover:text-gray-900"
                >
                  +
                </button>
              </div>

              <span className="font-semibold w-20 text-right">
                ₹{(item.price * item.quantity).toFixed(2)}
              </span>

              <button
                onClick={() => handleRemoveItem(item.itemId)}
                className="w-8 h-8 bg-red-100 text-red-600 rounded-full hover:bg-red-200 flex items-center justify-center"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>
        ))}

        {editedMenu.length === 0 && (
          <p className="text-center text-gray-500 py-4">No items in menu. Add items to get started.</p>
        )}
      </div>

      {/* Price Impact */}
      {hasChanges && priceImpact && (
        <MonetaryImpactPreview
          {...priceImpact}
          showPercentage={true}
          warningMessage="Changes require partner approval. Approval typically within 24 hours."
        />
      )}

      {/* Submit Button */}
      <DisabledButton
        onClick={() => onMenuUpdate(editedMenu)}
        disabled={!hasChanges}
        disabledReason="No changes made to menu"
        variant="primary"
        fullWidth
        loading={isLoading}
        icon={Edit3}
      >
        Request Menu Changes (Requires Partner Approval)
      </DisabledButton>
    </div>
  );
};

export default MenuItemEditor;
