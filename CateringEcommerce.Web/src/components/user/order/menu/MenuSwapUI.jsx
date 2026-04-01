import React, { useState } from 'react';
import { Repeat, AlertCircle, CheckCircle } from 'lucide-react';
import { DisabledButton, MonetaryImpactPreview } from '../../../common/safety';

/**
 * MenuSwapUI Component
 *
 * Item swap interface for 3-7 days before event
 * - Swap items of similar value
 * - 10% price cap on increases
 * - Requires partner approval
 */

const MenuSwapUI = ({
  currentMenu = [],
  availableItems = [],
  priceCapPercentage = 10,
  onSwapRequest,
  isLoading = false
}) => {
  const [swaps, setSwaps] = useState([]);

  // Add a swap
  const handleAddSwap = (originalItem, replacementItem) => {
    const priceDiff = replacementItem.price - originalItem.price;
    const percentageIncrease = (priceDiff / originalItem.price) * 100;

    // Check price cap
    if (percentageIncrease > priceCapPercentage) {
      alert(`Price increase exceeds ${priceCapPercentage}% cap. Choose a similar priced item.`);
      return;
    }

    // Remove existing swap for same original item
    const filtered = swaps.filter(s => s.originalItem.itemId !== originalItem.itemId);

    setSwaps([
      ...filtered,
      {
        originalItem,
        replacementItem,
        priceDiff,
        percentageChange: percentageIncrease
      }
    ]);
  };

  // Remove a swap
  const handleRemoveSwap = (originalItemId) => {
    setSwaps(prev => prev.filter(s => s.originalItem.itemId !== originalItemId));
  };

  // Calculate total price impact
  const calculatePriceImpact = () => {
    const currentTotal = currentMenu.reduce((sum, item) => sum + item.price, 0);
    const totalDiff = swaps.reduce((sum, swap) => sum + swap.priceDiff, 0);

    return {
      currentAmount: currentTotal,
      newAmount: currentTotal + totalDiff,
      breakdown: swaps.map(swap => ({
        label: `${swap.originalItem.itemName} → ${swap.replacementItem.itemName}`,
        amount: swap.priceDiff,
        type: swap.priceDiff > 0 ? 'add' : 'deduct'
      })),
      warningMessage: `Price increases limited to ${priceCapPercentage}%. Swaps require partner approval.`
    };
  };

  // Filter available items that can swap with selected item
  const getSimilarPricedItems = (originalItem) => {
    const maxPrice = originalItem.price * (1 + priceCapPercentage / 100);
    return availableItems.filter(
      item =>
        item.itemType === originalItem.itemType &&
        item.itemId !== originalItem.itemId &&
        item.price <= maxPrice
    );
  };

  const hasSwaps = swaps.length > 0;
  const priceImpact = hasSwaps ? calculatePriceImpact() : null;

  return (
    <div className="bg-white rounded-lg p-6 shadow-sm space-y-4">
      <div className="flex items-center gap-2 mb-4">
        <Repeat className="w-5 h-5 text-amber-600" />
        <h3 className="font-semibold text-lg">Item Swap</h3>
      </div>

      <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 mb-4">
        <div className="flex items-start gap-2">
          <AlertCircle className="w-5 h-5 text-amber-700 flex-shrink-0 mt-0.5" />
          <div className="text-sm text-amber-900">
            <p className="font-medium mb-1">Swap Restrictions</p>
            <ul className="list-disc list-inside space-y-1 text-xs">
              <li>Can only swap items of the same type (e.g., Main Course for Main Course)</li>
              <li>Price increase limited to {priceCapPercentage}%</li>
              <li>Cannot add or remove items, only swap</li>
              <li>Requires partner approval</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Current Menu with Swap Options */}
      <div className="space-y-3">
        {currentMenu.map((item, index) => {
          const similarItems = getSimilarPricedItems(item);
          const currentSwap = swaps.find(s => s.originalItem.itemId === item.itemId);

          return (
            <div key={index} className="border-2 border-gray-300 rounded-lg p-4">
              {/* Original Item */}
              <div className="flex items-center justify-between mb-3">
                <div>
                  <p className="font-medium text-gray-900">{item.itemName}</p>
                  <p className="text-sm text-gray-600">{item.itemType}</p>
                </div>
                <span className="font-semibold">₹{item.price.toFixed(2)}</span>
              </div>

              {/* Swap Selection */}
              {!currentSwap ? (
                <>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Swap with:
                  </label>
                  <select
                    onChange={(e) => {
                      const replacementItem = similarItems.find(
                        i => i.itemId === parseInt(e.target.value)
                      );
                      if (replacementItem) {
                        handleAddSwap(item, replacementItem);
                      }
                    }}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent"
                    defaultValue=""
                  >
                    <option value="" disabled>
                      Select replacement item...
                    </option>
                    {similarItems.map((replacement, idx) => {
                      const priceDiff = replacement.price - item.price;
                      const percentChange = ((priceDiff / item.price) * 100).toFixed(1);
                      return (
                        <option key={idx} value={replacement.itemId}>
                          {replacement.itemName} - ₹{replacement.price.toFixed(2)}
                          {priceDiff !== 0 && ` (${priceDiff > 0 ? '+' : ''}${percentChange}%)`}
                        </option>
                      );
                    })}
                  </select>
                  {similarItems.length === 0 && (
                    <p className="text-xs text-gray-500 mt-1">
                      No similar priced items available for swap
                    </p>
                  )}
                </>
              ) : (
                <div className="bg-green-50 border border-green-300 rounded-lg p-3">
                  <div className="flex items-start gap-2 mb-2">
                    <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0" />
                    <div className="flex-1">
                      <p className="text-sm font-medium text-green-900">Swapping to:</p>
                      <p className="text-sm text-green-800">
                        {currentSwap.replacementItem.itemName} - ₹
                        {currentSwap.replacementItem.price.toFixed(2)}
                      </p>
                      {currentSwap.priceDiff !== 0 && (
                        <p className="text-xs text-green-700 mt-1">
                          Price change: {currentSwap.priceDiff > 0 ? '+' : ''}₹
                          {currentSwap.priceDiff.toFixed(2)} (
                          {currentSwap.percentageChange > 0 ? '+' : ''}
                          {currentSwap.percentageChange.toFixed(1)}%)
                        </p>
                      )}
                    </div>
                    <button
                      onClick={() => handleRemoveSwap(item.itemId)}
                      className="text-red-600 hover:text-red-800 text-sm font-medium"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Price Impact */}
      {hasSwaps && priceImpact && (
        <MonetaryImpactPreview
          {...priceImpact}
          showPercentage={true}
        />
      )}

      {/* Submit Button */}
      <DisabledButton
        onClick={() => onSwapRequest(swaps)}
        disabled={!hasSwaps}
        disabledReason="No swaps selected. Choose replacement items to proceed."
        variant="primary"
        fullWidth
        loading={isLoading}
        icon={Repeat}
      >
        Request Item Swaps ({swaps.length} swap{swaps.length !== 1 ? 's' : ''})
      </DisabledButton>
    </div>
  );
};

export default MenuSwapUI;
