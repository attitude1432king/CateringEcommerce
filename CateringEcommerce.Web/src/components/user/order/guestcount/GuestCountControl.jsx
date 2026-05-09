import React, { useState, useEffect } from 'react';
import { Users, Plus, Minus, AlertTriangle, Info, Clock, DollarSign } from 'lucide-react';
import { DisabledButton, MonetaryImpactPreview, ConfirmActionModal } from '../../../common/safety';
import GuestCountTimeline from './GuestCountTimeline';

/**
 * GuestCountControl Component
 *
 * Manages guest count with time-based restrictions:
 * - >7 days: Full flexibility
 * - 5-7 days: Increase (1.2x), Decrease (70% refund penalty)
 * - 3-5 days: LOCKED - Decrease disabled, Increase +10% only with 2hr approval
 * - <48h: Emergency increase only, direct partner payment
 * - Event time: Read-only
 */

const GuestCountControl = ({
  order,
  onGuestCountChange,
  isLoading = false
}) => {
  const [newGuestCount, setNewGuestCount] = useState(order.guestCount);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [modalConfig, setModalConfig] = useState(null);
  const [priceImpact, setPriceImpact] = useState(null);

  const eventDate = new Date(order.eventDate);
  const now = new Date();
  const pricePerGuest = order.totalAmount / order.guestCount;

  // Calculate days until event
  const daysUntilEvent = Math.ceil((eventDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

  // Determine current modification rules
  const getModificationRules = () => {
    if (daysUntilEvent < 0) {
      return {
        canIncrease: false,
        canDecrease: false,
        phase: 'event-passed',
        increaseDisabledReason: 'Event has already occurred',
        decreaseDisabledReason: 'Event has already occurred'
      };
    }

    if (daysUntilEvent === 0) {
      return {
        canIncrease: false,
        canDecrease: false,
        phase: 'event-day',
        increaseDisabledReason: 'Guest count is read-only on event day. Emergency changes must be coordinated directly with partner.',
        decreaseDisabledReason: 'Guest count is locked on event day'
      };
    }

    if (daysUntilEvent <= 2) {
      return {
        canIncrease: true,
        canDecrease: false,
        phase: 'emergency-only',
        maxIncrease: order.guestCount * 0.2, // 20% max
        pricingMultiplier: 1.5,
        requiresDirectPayment: true,
        increaseWarning: 'Emergency increase requires direct payment to partner. Platform payment protection does not apply.',
        decreaseDisabledReason: 'Ingredients already procured. Decrease not possible within 48 hours of event.'
      };
    }

    if (daysUntilEvent <= 5 || order.guestCountLocked) {
      return {
        canIncrease: true,
        canDecrease: false,
        phase: 'locked',
        maxIncrease: order.guestCount * 0.1, // 10% max
        pricingMultiplier: 1.3,
        requiresPartnerApproval: true,
        approvalTimeLimit: 2, // hours
        increaseWarning: 'Guest count is locked. Increase limited to 10% and requires partner approval within 2 hours.',
        decreaseDisabledReason: 'Guest count is locked. Procurement has begun. Decrease not allowed.'
      };
    }

    if (daysUntilEvent <= 7) {
      return {
        canIncrease: true,
        canDecrease: true,
        phase: 'restricted',
        pricingMultiplier: 1.2,
        refundPercentage: 0.7, // 70% refund
        requiresPartnerApproval: true,
        increaseWarning: 'Increase allowed with 20% premium pricing due to short notice.',
        decreaseWarning: 'Decrease allowed with 30% penalty (70% refund). Requires partner approval.'
      };
    }

    // > 7 days - full flexibility
    return {
      canIncrease: true,
      canDecrease: true,
      phase: 'flexible',
      pricingMultiplier: 1.0,
      refundPercentage: 1.0 // 100% refund
    };
  };

  const rules = getModificationRules();

  // Calculate price impact when guest count changes
  useEffect(() => {
    if (newGuestCount === order.guestCount) {
      setPriceImpact(null);
      return;
    }

    const difference = newGuestCount - order.guestCount;
    const isIncrease = difference > 0;

    if (isIncrease) {
      // Increase: additional cost
      const additionalGuests = difference;
      const baseAdditionalCost = additionalGuests * pricePerGuest;
      const multiplier = rules.pricingMultiplier || 1.0;
      const additionalCost = baseAdditionalCost * multiplier;
      const newTotalAmount = order.totalAmount + additionalCost;

      setPriceImpact({
        currentAmount: order.totalAmount,
        newAmount: newTotalAmount,
        breakdown: [
          { label: `${additionalGuests} additional guests`, amount: baseAdditionalCost, type: 'add' },
          ...(multiplier > 1.0 ? [
            { label: `${((multiplier - 1) * 100).toFixed(0)}% premium pricing`, amount: baseAdditionalCost * (multiplier - 1), type: 'add' }
          ] : [])
        ],
        warningMessage: rules.increaseWarning,
        infoMessage: rules.requiresPartnerApproval ? 'Requires partner approval' : rules.requiresDirectPayment ? 'Direct partner payment required' : null
      });
    } else {
      // Decrease: refund
      const removedGuests = Math.abs(difference);
      const baseRefund = removedGuests * pricePerGuest;
      const refundMultiplier = rules.refundPercentage || 1.0;
      const actualRefund = baseRefund * refundMultiplier;
      const penalty = baseRefund - actualRefund;
      const newTotalAmount = order.totalAmount - actualRefund;

      setPriceImpact({
        currentAmount: order.totalAmount,
        newAmount: newTotalAmount,
        breakdown: [
          { label: `${removedGuests} guests removed`, amount: -baseRefund, type: 'deduct' },
          ...(refundMultiplier < 1.0 ? [
            { label: `${((1 - refundMultiplier) * 100).toFixed(0)}% cancellation penalty`, amount: penalty, type: 'add' }
          ] : [])
        ],
        warningMessage: rules.decreaseWarning,
        infoMessage: rules.requiresPartnerApproval ? 'Requires partner approval' : null
      });
    }
  }, [newGuestCount, order.guestCount]);

  // Handle increase
  const handleIncrease = () => {
    const maxAllowed = rules.maxIncrease
      ? order.guestCount + Math.floor(rules.maxIncrease)
      : order.guestCount + 100;

    if (newGuestCount < maxAllowed) {
      setNewGuestCount(prev => prev + 1);
    }
  };

  // Handle decrease
  const handleDecrease = () => {
    if (newGuestCount > 1) {
      setNewGuestCount(prev => prev - 1);
    }
  };

  // Handle direct input
  const handleInputChange = (e) => {
    const value = parseInt(e.target.value) || 1;
    const maxAllowed = rules.maxIncrease
      ? order.guestCount + Math.floor(rules.maxIncrease)
      : value;

    setNewGuestCount(Math.max(1, Math.min(value, maxAllowed)));
  };

  // Open confirmation modal
  const handleOpenConfirmation = () => {
    const difference = newGuestCount - order.guestCount;
    const isIncrease = difference > 0;

    setModalConfig({
      title: isIncrease ? 'Confirm Guest Count Increase' : 'Confirm Guest Count Decrease',
      description: isIncrease
        ? `You are increasing the guest count from ${order.guestCount} to ${newGuestCount} (${difference} additional guests).`
        : `You are decreasing the guest count from ${order.guestCount} to ${newGuestCount} (${Math.abs(difference)} guests removed).`,
      type: isIncrease ? 'info' : 'warning',
      isIrreversible: !isIncrease && rules.refundPercentage < 1.0,
      requiresConfirmation: !isIncrease && rules.refundPercentage < 0.8,
      additionalWarnings: [
        ...(rules.requiresPartnerApproval ? ['Partner must approve this change within ' + (rules.approvalTimeLimit || 24) + ' hours'] : []),
        ...(rules.requiresDirectPayment ? ['Additional payment must be made directly to partner'] : []),
        ...(isIncrease && rules.maxIncrease ? [`Maximum increase allowed: ${Math.floor(rules.maxIncrease)} guests`] : [])
      ]
    });
    setShowConfirmModal(true);
  };

  // Handle confirmation
  const handleConfirm = async () => {
    if (onGuestCountChange) {
      await onGuestCountChange(newGuestCount, priceImpact);
      setShowConfirmModal(false);
    }
  };

  const hasChanges = newGuestCount !== order.guestCount;
  const difference = newGuestCount - order.guestCount;
  const isIncrease = difference > 0;

  return (
    <div className="space-y-4">
      {/* Timeline */}
      <GuestCountTimeline
        eventDate={order.eventDate}
        guestCountLocked={order.guestCountLocked}
      />

      {/* Guest Count Control */}
      <div className="bg-white rounded-lg p-6 shadow-sm border-2 border-gray-200">
        <div className="flex items-center gap-2 mb-4">
          <Users className="w-5 h-5 text-neutral-700" />
          <h3 className="font-semibold text-lg">Modify Guest Count</h3>
        </div>

        {/* Current Count Display */}
        <div className="bg-gray-50 rounded-lg p-4 mb-4">
          <p className="text-sm text-neutral-600 mb-1">Current Guest Count</p>
          <p className="text-3xl font-bold text-neutral-900">{order.guestCount}</p>
          {order.guestCountLocked && (
            <span className="inline-block mt-2 text-xs font-medium text-red-700 bg-red-100 px-2 py-1 rounded-full">
              🔒 Locked
            </span>
          )}
        </div>

        {/* Count Adjustment Controls */}
        <div className="flex items-center gap-4 mb-4">
          <DisabledButton
            onClick={handleDecrease}
            disabled={!rules.canDecrease || newGuestCount <= 1}
            disabledReason={!rules.canDecrease ? rules.decreaseDisabledReason : 'Minimum 1 guest required'}
            variant="secondary"
            size="lg"
            icon={Minus}
          />

          <input
            type="number"
            value={newGuestCount}
            onChange={handleInputChange}
            min="1"
            max={rules.maxIncrease ? order.guestCount + Math.floor(rules.maxIncrease) : undefined}
            className="flex-1 text-center text-2xl font-bold border-2 border-gray-300 rounded-lg py-3 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />

          <DisabledButton
            onClick={handleIncrease}
            disabled={!rules.canIncrease}
            disabledReason={rules.increaseDisabledReason}
            variant="secondary"
            size="lg"
            icon={Plus}
          />
        </div>

        {/* Price Impact Preview */}
        {hasChanges && priceImpact && (
          <div className="mb-4">
            <MonetaryImpactPreview
              {...priceImpact}
              showPercentage={true}
            />
          </div>
        )}

        {/* Apply Changes Button */}
        <DisabledButton
          onClick={handleOpenConfirmation}
          disabled={!hasChanges || isLoading}
          disabledReason={!hasChanges ? 'No changes made to guest count' : ''}
          variant="primary"
          fullWidth
          loading={isLoading}
        >
          {isIncrease ? '▲' : '▼'} Apply Changes ({difference > 0 ? '+' : ''}{difference} guests)
        </DisabledButton>

        {/* Phase Info */}
        <div className="mt-4 pt-4 border-t border-gray-200">
          <div className="flex items-start gap-2 text-sm text-neutral-600">
            <Info className="w-4 h-4 flex-shrink-0 mt-0.5" />
            <div>
              <p className="font-medium text-neutral-900 mb-1">Current Modification Phase: {rules.phase}</p>
              <ul className="list-disc list-inside space-y-1 text-xs">
                <li>Increase: {rules.canIncrease ? '✓ Allowed' : '✗ Not allowed'}</li>
                <li>Decrease: {rules.canDecrease ? '✓ Allowed' : '✗ Not allowed'}</li>
                {rules.pricingMultiplier > 1.0 && (
                  <li>Premium pricing: {((rules.pricingMultiplier - 1) * 100).toFixed(0)}%</li>
                )}
                {rules.refundPercentage < 1.0 && (
                  <li>Refund: {(rules.refundPercentage * 100).toFixed(0)}% of original amount</li>
                )}
              </ul>
            </div>
          </div>
        </div>
      </div>

      {/* Confirmation Modal */}
      {showConfirmModal && modalConfig && (
        <ConfirmActionModal
          isOpen={showConfirmModal}
          onClose={() => setShowConfirmModal(false)}
          onConfirm={handleConfirm}
          title={modalConfig.title}
          description={modalConfig.description}
          type={modalConfig.type}
          isIrreversible={modalConfig.isIrreversible}
          monetaryImpact={priceImpact}
          requiresConfirmation={modalConfig.requiresConfirmation}
          additionalWarnings={modalConfig.additionalWarnings}
          isLoading={isLoading}
        />
      )}
    </div>
  );
};

export default GuestCountControl;
