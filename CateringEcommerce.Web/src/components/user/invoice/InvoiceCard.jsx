import React, { useState } from 'react';
import {
    Download,
    FileText,
    Calendar,
    CreditCard,
    AlertCircle,
    CheckCircle,
    Clock,
    IndianRupee
} from 'lucide-react';
import invoiceApi from '../../../services/invoiceApi';

/**
 * Invoice Card Component
 * Displays invoice summary with download and payment actions
 */
const InvoiceCard = ({ invoice, onPaymentClick, showActions = true }) => {
    const [downloading, setDownloading] = useState(false);

    const getStatusConfig = (status) => {
        const configs = {
            1: { // UNPAID
                label: 'Unpaid',
                color: 'bg-yellow-100 text-yellow-800 border-yellow-200',
                icon: Clock
            },
            2: { // OVERDUE
                label: 'Overdue',
                color: 'bg-red-100 text-red-800 border-red-200',
                icon: AlertCircle
            },
            3: { // PAID
                label: 'Paid',
                color: 'bg-green-100 text-green-800 border-green-200',
                icon: CheckCircle
            },
            4: { // PARTIALLY_PAID
                label: 'Partially Paid',
                color: 'bg-blue-100 text-blue-800 border-blue-200',
                icon: CreditCard
            },
            5: { // CANCELLED
                label: 'Cancelled',
                color: 'bg-gray-100 text-gray-800 border-gray-200',
                icon: AlertCircle
            },
            6: { // REFUNDED
                label: 'Refunded',
                color: 'bg-purple-100 text-purple-800 border-purple-200',
                icon: FileText
            }
        };

        return configs[status] || configs[1];
    };

    const getInvoiceTypeLabel = (type) => {
        const labels = {
            1: { label: 'Booking Invoice', desc: '40% Advance Payment', color: 'text-blue-600' },
            2: { label: 'Pre-Event Invoice', desc: '35% Payment', color: 'text-orange-600' },
            3: { label: 'Final Settlement', desc: '25% + Extras', color: 'text-green-600' }
        };

        return labels[type] || { label: 'Invoice', desc: '', color: 'text-gray-600' };
    };

    const handleDownload = async () => {
        try {
            setDownloading(true);
            await invoiceApi.downloadInvoicePdf(invoice.invoiceId);
        } catch (error) {
            console.error('Download failed:', error);
            alert('Failed to download invoice. Please try again.');
        } finally {
            setDownloading(false);
        }
    };

    const statusConfig = getStatusConfig(invoice.status);
    const typeInfo = getInvoiceTypeLabel(invoice.invoiceType);
    const StatusIcon = statusConfig.icon;

    const isOverdue = invoice.dueDate && new Date(invoice.dueDate) < new Date() && invoice.status === 1;
    const daysUntilDue = invoice.dueDate
        ? Math.ceil((new Date(invoice.dueDate) - new Date()) / (1000 * 60 * 60 * 24))
        : null;

    return (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-shadow">
            {/* Header */}
            <div className="p-4 border-b border-gray-200">
                <div className="flex items-start justify-between">
                    <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                            <FileText className={typeInfo.color} size={20} />
                            <h3 className="font-semibold text-gray-900">
                                {invoice.invoiceNumber}
                            </h3>
                        </div>
                        <p className={`text-sm font-medium ${typeInfo.color}`}>
                            {typeInfo.label}
                        </p>
                        <p className="text-xs text-gray-500 mt-0.5">
                            {typeInfo.desc}
                        </p>
                    </div>

                    <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full border ${statusConfig.color} text-xs font-medium`}>
                        <StatusIcon size={14} />
                        <span>{statusConfig.label}</span>
                    </div>
                </div>

                {invoice.isProforma && (
                    <div className="mt-2 bg-blue-50 border border-blue-200 rounded px-2 py-1 inline-block">
                        <span className="text-xs text-blue-700 font-medium">
                            Proforma Invoice
                        </span>
                    </div>
                )}
            </div>

            {/* Content */}
            <div className="p-4 space-y-3">
                {/* Date info */}
                <div className="flex items-center justify-between text-sm">
                    <div className="flex items-center gap-2 text-gray-600">
                        <Calendar size={16} />
                        <span>Invoice Date:</span>
                    </div>
                    <span className="font-medium text-gray-900">
                        {new Date(invoice.invoiceDate).toLocaleDateString('en-IN', {
                            day: '2-digit',
                            month: 'short',
                            year: 'numeric'
                        })}
                    </span>
                </div>

                {/* Due date */}
                {invoice.dueDate && (
                    <div className="flex items-center justify-between text-sm">
                        <div className="flex items-center gap-2 text-gray-600">
                            <Clock size={16} />
                            <span>Due Date:</span>
                        </div>
                        <span className={`font-medium ${isOverdue ? 'text-red-600' : 'text-gray-900'}`}>
                            {new Date(invoice.dueDate).toLocaleDateString('en-IN', {
                                day: '2-digit',
                                month: 'short',
                                year: 'numeric'
                            })}
                            {daysUntilDue !== null && daysUntilDue >= 0 && (
                                <span className="text-xs text-gray-500 ml-1">
                                    ({daysUntilDue} {daysUntilDue === 1 ? 'day' : 'days'})
                                </span>
                            )}
                        </span>
                    </div>
                )}

                {/* Overdue warning */}
                {isOverdue && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-2">
                        <div className="flex items-center gap-2 text-red-700 text-xs">
                            <AlertCircle size={14} />
                            <span className="font-medium">
                                Overdue by {Math.abs(daysUntilDue)} {Math.abs(daysUntilDue) === 1 ? 'day' : 'days'}
                            </span>
                        </div>
                    </div>
                )}

                {/* Amount breakdown */}
                <div className="bg-gray-50 rounded-lg p-3 space-y-2">
                    <div className="flex items-center justify-between text-sm">
                        <span className="text-gray-600">Subtotal:</span>
                        <span className="font-medium text-gray-900">
                            ₹{invoice.subtotal.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </span>
                    </div>

                    <div className="flex items-center justify-between text-xs text-gray-500">
                        <span>CGST ({invoice.cgstPercent}%):</span>
                        <span>₹{invoice.cgstAmount.toLocaleString('en-IN', { minimumFractionDigits: 2 })}</span>
                    </div>

                    <div className="flex items-center justify-between text-xs text-gray-500">
                        <span>SGST ({invoice.sgstPercent}%):</span>
                        <span>₹{invoice.sgstAmount.toLocaleString('en-IN', { minimumFractionDigits: 2 })}</span>
                    </div>

                    <div className="pt-2 border-t border-gray-200">
                        <div className="flex items-center justify-between">
                            <span className="font-semibold text-gray-900">Total Amount:</span>
                            <span className="text-lg font-bold text-gray-900">
                                ₹{invoice.totalAmount.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                            </span>
                        </div>
                    </div>

                    {/* Payment info */}
                    {invoice.amountPaid > 0 && (
                        <>
                            <div className="flex items-center justify-between text-sm text-green-600">
                                <span>Amount Paid:</span>
                                <span className="font-medium">
                                    ₹{invoice.amountPaid.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                                </span>
                            </div>

                            {invoice.balanceDue > 0 && (
                                <div className="flex items-center justify-between text-sm text-red-600">
                                    <span>Balance Due:</span>
                                    <span className="font-medium">
                                        ₹{invoice.balanceDue.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                                    </span>
                                </div>
                            )}
                        </>
                    )}
                </div>
            </div>

            {/* Actions */}
            {showActions && (
                <div className="p-4 border-t border-gray-200 flex gap-3">
                    <button
                        onClick={handleDownload}
                        disabled={downloading}
                        className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        <Download size={18} />
                        {downloading ? 'Downloading...' : 'Download PDF'}
                    </button>

                    {invoice.status === 1 && onPaymentClick && invoice.balanceDue > 0 && (
                        <button
                            onClick={() => onPaymentClick(invoice)}
                            className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
                        >
                            <CreditCard size={18} />
                            Pay Now
                        </button>
                    )}
                </div>
            )}
        </div>
    );
};

export default InvoiceCard;
