import React, { useState, useEffect } from 'react';
import {
  ChefHat,
  Edit3,
  Lock,
  AlertTriangle,
  Info,
  CheckCircle,
  XCircle,
  Clock,
  Repeat
} from 'lucide-react';
import { DisabledButton, ConfirmActionModal } from '../../../common/safety';
import MenuItemEditor from './MenuItemEditor';
import MenuSwapUI from './MenuSwapUI';
import AllergyEmergencyButton from './AllergyEmergencyButton';

/**
 * MenuChangePanel Component
 *
 * Time-based menu modification with progressive restrictions:
 * - >7 days: Full edit with partner approval
 * - 3-7 days: Item swap only, 10% price cap
 * - <3 days: Locked (ingredients procured), dietary/allergy only
 * - At Event: Read-only, emergency allergy button
 */

const MenuChangePanel = ({
  order,
  menuItems = [],
  onMenuChange,
  onAllergyEmergency,
  isLoading = false
}) => {
  const [editMode, setEditMode] = useState(null); // 'full-edit', 'swap-only', 'locked', 'read-only'
  const [pendingChanges, setPendingChanges] = useState([]);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [dietaryNotes, setDietaryNotes] = useState(order.dietaryRestrictions || '');

  const eventDate = new Date(order.eventDate);
  const now = new Date();
  const daysUntilEvent = Math.ceil((eventDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));

  // Determine modification mode based on days until event
  const getModificationMode = () => {
    if (daysUntilEvent < 0) {
      return {
        mode: 'event-passed',
        canEdit: false,
        canSwap: false,
        canAddRemove: false,
        canModifyDietary: false,
        description: 'Event has passed',
        icon: Lock,
        color: 'gray'
      };
    }

    if (daysUntilEvent === 0) {
      return {
        mode: 'event-day',
        canEdit: false,
        canSwap: false,
        canAddRemove: false,
        canModifyDietary: false,
        canAllergyEmergency: true,
        description: 'Event day - Menu is read-only',
        warning: 'Emergency allergy modifications only',
        icon: Lock,
        color: 'red'
      };
    }

    if (daysUntilEvent <= 3) {
      return {
        mode: 'locked',
        canEdit: false,
        canSwap: false,
        canAddRemove: false,
        canModifyDietary: true,
        description: 'Menu locked - Ingredients already procured',
        warning: 'Only dietary restrictions and allergy notes can be updated',
        icon: Lock,
        color: 'red'
      };
    }

    if (daysUntilEvent <= 7) {
      return {
        mode: 'swap-only',
        canEdit: false,
        canSwap: true,
        canAddRemove: false,
        priceCapPercentage: 10,
        requiresPartnerApproval: true,
        description: 'Limited modifications - Item swaps only',
        warning: 'Can swap items of similar value. Price increase limited to 10%.',
        icon: Repeat,
        color: 'amber'
      };
    }

    // > 7 days
    return {
      mode: 'full-edit',
      canEdit: true,
      canSwap: true,
      canAddRemove: true,
      requiresPartnerApproval: true,
      description: 'Full menu editing available',
      warning: 'Changes require catering partner approval',
      icon: Edit3,
      color: 'green'
    };
  };

  const modificationMode = getModificationMode();

  // Get status badge for menu change requests
  const getChangeStatusBadge = (status) => {
    const badges = {
      pending: { label: 'Pending Approval', color: 'bg-amber-100 text-amber-800', icon: Clock },
      approved: { label: 'Approved', color: 'bg-green-100 text-green-800', icon: CheckCircle },
      rejected: { label: 'Rejected', color: 'bg-red-100 text-red-800', icon: XCircle }
    };
    return badges[status] || badges.pending;
  };

  // Existing pending change requests
  const existingChangeRequests = order.menuChangeRequests || [];

  return (
    <div className="space-y-4">
      {/* Mode Banner */}
      <div className={`
        rounded-lg p-4 border-l-4 flex items-start gap-3
        ${modificationMode.color === 'green' ? 'bg-green-50 border-green-500' : ''}
        ${modificationMode.color === 'amber' ? 'bg-amber-50 border-amber-500' : ''}
        ${modificationMode.color === 'red' ? 'bg-red-50 border-red-500' : ''}
        ${modificationMode.color === 'gray' ? 'bg-gray-50 border-gray-400' : ''}
      `}>
        {React.createElement(modificationMode.icon, {
          className: `w-6 h-6 flex-shrink-0 ${
            modificationMode.color === 'green' ? 'text-green-600' : ''
          }${modificationMode.color === 'amber' ? 'text-amber-600' : ''}${
            modificationMode.color === 'red' ? 'text-red-600' : ''
          }${modificationMode.color === 'gray' ? 'text-gray-600' : ''}`
        })}
        <div className="flex-1">
          <h3 className={`font-semibold mb-1 ${
            modificationMode.color === 'green' ? 'text-green-900' : ''
          }${modificationMode.color === 'amber' ? 'text-amber-900' : ''}${
            modificationMode.color === 'red' ? 'text-red-900' : ''
          }${modificationMode.color === 'gray' ? 'text-gray-900' : ''}`}>
            {modificationMode.description}
          </h3>
          {modificationMode.warning && (
            <p className={`text-sm ${
              modificationMode.color === 'green' ? 'text-green-800' : ''
            }${modificationMode.color === 'amber' ? 'text-amber-800' : ''}${
              modificationMode.color === 'red' ? 'text-red-800' : ''
            }${modificationMode.color === 'gray' ? 'text-gray-700' : ''}`}>
              {modificationMode.warning}
            </p>
          )}
          <div className="flex items-center gap-2 mt-2 text-sm">
            <Clock className="w-4 h-4" />
            <span className="font-medium">{daysUntilEvent} days until event</span>
          </div>
        </div>
      </div>

      {/* Current Menu */}
      <div className="bg-white rounded-lg p-6 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-semibold text-lg flex items-center gap-2">
            <ChefHat className="w-5 h-5" />
            Current Menu
          </h3>
          {modificationMode.canAllergyEmergency && (
            <AllergyEmergencyButton onEmergency={onAllergyEmergency} />
          )}
        </div>

        {/* Menu Items List */}
        <div className="space-y-3">
          {menuItems.map((item, index) => (
            <div
              key={index}
              className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              <div className="flex-1">
                <p className="font-medium text-gray-900">{item.itemName}</p>
                <p className="text-sm text-gray-600">{item.itemType}</p>
                {item.dietaryTags && (
                  <div className="flex gap-1 mt-1">
                    {item.dietaryTags.map((tag, i) => (
                      <span
                        key={i}
                        className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full"
                      >
                        {tag}
                      </span>
                    ))}
                  </div>
                )}
              </div>
              <div className="text-right">
                <p className="font-semibold text-gray-900">₹{item.price.toFixed(2)}</p>
                <p className="text-xs text-gray-500">Qty: {item.quantity}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Dietary Restrictions & Allergy Notes */}
      {modificationMode.canModifyDietary && (
        <div className="bg-white rounded-lg p-6 shadow-sm">
          <h3 className="font-semibold text-lg mb-4">Dietary Restrictions & Allergies</h3>
          <textarea
            value={dietaryNotes}
            onChange={(e) => setDietaryNotes(e.target.value)}
            placeholder="E.g., Nut allergy, Lactose intolerant, Gluten-free required..."
            rows={4}
            className="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <p className="text-xs text-gray-600 mt-2">
            <Info className="w-3 h-3 inline mr-1" />
            This information will be highlighted to the chef. Please be specific about severe allergies.
          </p>
          <DisabledButton
            onClick={() => onMenuChange({ dietaryRestrictions: dietaryNotes })}
            disabled={dietaryNotes === order.dietaryRestrictions}
            disabledReason="No changes made"
            variant="primary"
            className="mt-3"
          >
            Update Dietary Notes
          </DisabledButton>
        </div>
      )}

      {/* Menu Swap UI (3-7 days) */}
      {modificationMode.mode === 'swap-only' && (
        <MenuSwapUI
          currentMenu={menuItems}
          availableItems={order.availableSwapItems || []}
          priceCapPercentage={modificationMode.priceCapPercentage}
          onSwapRequest={(swaps) => {
            setPendingChanges(swaps);
            setShowConfirmModal(true);
          }}
          isLoading={isLoading}
        />
      )}

      {/* Full Menu Editor (>7 days) */}
      {modificationMode.mode === 'full-edit' && (
        <MenuItemEditor
          currentMenu={menuItems}
          availableItems={order.availableMenuItems || []}
          onMenuUpdate={(changes) => {
            setPendingChanges(changes);
            setShowConfirmModal(true);
          }}
          isLoading={isLoading}
        />
      )}

      {/* Existing Change Requests */}
      {existingChangeRequests.length > 0 && (
        <div className="bg-white rounded-lg p-6 shadow-sm">
          <h3 className="font-semibold text-lg mb-4">Pending Menu Change Requests</h3>
          <div className="space-y-3">
            {existingChangeRequests.map((request, index) => {
              const statusBadge = getChangeStatusBadge(request.status);
              const StatusIcon = statusBadge.icon;

              return (
                <div
                  key={index}
                  className="border border-gray-200 rounded-lg p-4"
                >
                  <div className="flex items-center justify-between mb-2">
                    <p className="font-medium text-gray-900">{request.changeType}</p>
                    <span className={`text-xs font-medium px-2 py-1 rounded-full ${statusBadge.color} flex items-center gap-1`}>
                      <StatusIcon className="w-3 h-3" />
                      {statusBadge.label}
                    </span>
                  </div>
                  <p className="text-sm text-gray-600 mb-1">{request.description}</p>
                  <p className="text-xs text-gray-500">
                    Requested {new Date(request.requestedDate).toLocaleString('en-IN')}
                  </p>
                  {request.status === 'rejected' && request.rejectionReason && (
                    <div className="mt-2 bg-red-50 border border-red-200 rounded p-2">
                      <p className="text-xs text-red-800">
                        <strong>Reason:</strong> {request.rejectionReason}
                      </p>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Locked State Info */}
      {(modificationMode.mode === 'locked' || modificationMode.mode === 'event-day') && !modificationMode.canModifyDietary && (
        <div className="bg-gray-50 border-2 border-gray-300 rounded-lg p-6 text-center">
          <Lock className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <h3 className="font-semibold text-gray-900 mb-2">Menu is Locked</h3>
          <p className="text-sm text-gray-600 max-w-md mx-auto">
            {modificationMode.mode === 'event-day'
              ? 'Menu modifications are not possible on event day. For emergency allergy issues, use the emergency button above.'
              : 'Ingredients have been procured based on your menu. Changes are no longer possible to ensure quality and timely service.'}
          </p>
        </div>
      )}

      {/* Confirmation Modal */}
      {showConfirmModal && (
        <ConfirmActionModal
          isOpen={showConfirmModal}
          onClose={() => setShowConfirmModal(false)}
          onConfirm={() => {
            onMenuChange(pendingChanges);
            setShowConfirmModal(false);
          }}
          title="Confirm Menu Changes"
          description={`You are requesting ${pendingChanges.length} menu change(s). ${
            modificationMode.requiresPartnerApproval
              ? 'These changes require partner approval and will be processed within 24 hours.'
              : ''
          }`}
          type="info"
          additionalWarnings={
            modificationMode.priceCapPercentage
              ? [`Price increase is limited to ${modificationMode.priceCapPercentage}%`]
              : []
          }
          confirmText="Submit Request"
          isLoading={isLoading}
        />
      )}
    </div>
  );
};

export default MenuChangePanel;
