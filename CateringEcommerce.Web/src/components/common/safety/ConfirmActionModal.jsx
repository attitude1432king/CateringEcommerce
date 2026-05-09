import React, { useState } from 'react';
import { AlertTriangle, Info, CheckCircle, XCircle } from 'lucide-react';
import MonetaryImpactPreview from './MonetaryImpactPreview';

/**
 * ConfirmActionModal Component
 *
 * Modal for confirming irreversible actions with monetary impact preview
 * Implements UX Safety Rule: "Use confirmations for irreversible actions"
 */

const ConfirmActionModal = ({
  isOpen,
  onClose,
  onConfirm,
  title,
  description,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  type = 'warning', // 'warning', 'danger', 'info', 'success'
  isIrreversible = false,
  monetaryImpact = null, // {currentAmount, newAmount, breakdown, warningMessage}
  requiresConfirmation = false, // Require typing "CONFIRM"
  confirmationText = 'CONFIRM',
  additionalWarnings = [], // Array of warning strings
  isLoading = false
}) => {
  const [confirmationInput, setConfirmationInput] = useState('');

  if (!isOpen) return null;

  // Type styling
  const typeStyles = {
    warning: {
      icon: AlertTriangle,
      iconColor: 'text-amber-600',
      iconBg: 'bg-amber-100',
      buttonBg: 'bg-amber-600 hover:bg-amber-700',
      borderColor: 'border-amber-200'
    },
    danger: {
      icon: XCircle,
      iconColor: 'text-red-600',
      iconBg: 'bg-red-100',
      buttonBg: 'bg-red-600 hover:bg-red-700',
      borderColor: 'border-red-200'
    },
    info: {
      icon: Info,
      iconColor: 'text-blue-600',
      iconBg: 'bg-blue-100',
      buttonBg: 'bg-blue-600 hover:bg-blue-700',
      borderColor: 'border-blue-200'
    },
    success: {
      icon: CheckCircle,
      iconColor: 'text-green-600',
      iconBg: 'bg-green-100',
      buttonBg: 'bg-green-600 hover:bg-green-700',
      borderColor: 'border-green-200'
    }
  };

  const style = typeStyles[type];
  const TypeIcon = style.icon;

  // Check if confirm button should be enabled
  const isConfirmEnabled = !isLoading && (!requiresConfirmation || confirmationInput === confirmationText);

  const handleConfirm = () => {
    if (isConfirmEnabled) {
      onConfirm();
      setConfirmationInput('');
    }
  };

  const handleClose = () => {
    setConfirmationInput('');
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50 backdrop-blur-sm">
      <div className="bg-white rounded-xl max-w-lg w-full shadow-2xl overflow-hidden animate-scale-in">
        {/* Header */}
        <div className={`p-6 border-b-2 ${style.borderColor}`}>
          <div className="flex items-center gap-3">
            <div className={`w-12 h-12 rounded-full ${style.iconBg} flex items-center justify-center`}>
              <TypeIcon className={`w-6 h-6 ${style.iconColor}`} />
            </div>
            <h2 className="text-2xl font-bold text-neutral-900">{title}</h2>
          </div>
        </div>

        {/* Body */}
        <div className="p-6 space-y-4 max-h-[70vh] overflow-y-auto">
          {/* Description */}
          <p className="text-neutral-700 leading-relaxed">{description}</p>

          {/* Irreversible Warning */}
          {isIrreversible && (
            <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded-r-lg">
              <div className="flex items-start gap-2">
                <AlertTriangle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                <div>
                  <p className="font-semibold text-red-900">This action is irreversible</p>
                  <p className="text-sm text-red-800 mt-1">
                    Once confirmed, this action cannot be undone. Please review carefully before proceeding.
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Monetary Impact */}
          {monetaryImpact && (
            <MonetaryImpactPreview
              currentAmount={monetaryImpact.currentAmount}
              newAmount={monetaryImpact.newAmount}
              breakdown={monetaryImpact.breakdown}
              showPercentage={true}
              warningMessage={monetaryImpact.warningMessage}
              infoMessage={monetaryImpact.infoMessage}
            />
          )}

          {/* Additional Warnings */}
          {additionalWarnings && additionalWarnings.length > 0 && (
            <div className="space-y-2">
              {additionalWarnings.map((warning, index) => (
                <div key={index} className="flex items-start gap-2 text-sm text-neutral-700">
                  <AlertTriangle className="w-4 h-4 text-amber-600 flex-shrink-0 mt-0.5" />
                  <p>{warning}</p>
                </div>
              ))}
            </div>
          )}

          {/* Confirmation Input */}
          {requiresConfirmation && (
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-2">
                Type <span className="font-bold">{confirmationText}</span> to confirm this action
              </label>
              <input
                type="text"
                value={confirmationInput}
                onChange={(e) => setConfirmationInput(e.target.value)}
                placeholder={`Type "${confirmationText}"`}
                className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent"
                autoComplete="off"
              />
              {confirmationInput && confirmationInput !== confirmationText && (
                <p className="text-sm text-red-600 mt-1">
                  Text doesn't match. Please type exactly: {confirmationText}
                </p>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className={`p-6 border-t-2 ${style.borderColor} bg-gray-50 flex gap-3`}>
          <button
            onClick={handleClose}
            disabled={isLoading}
            className="flex-1 px-6 py-3 border-2 border-gray-300 text-neutral-700 rounded-lg hover:bg-white transition-colors font-medium disabled:opacity-50"
          >
            {cancelText}
          </button>
          <button
            onClick={handleConfirm}
            disabled={!isConfirmEnabled}
            className={`flex-1 px-6 py-3 ${style.buttonBg} text-white rounded-lg transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2`}
          >
            {isLoading ? (
              <>
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                Processing...
              </>
            ) : (
              confirmText
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfirmActionModal;
