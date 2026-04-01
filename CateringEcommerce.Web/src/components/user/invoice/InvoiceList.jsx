import React, { useEffect, useState } from 'react';
import { FileText, Filter, Download, AlertCircle, Loader } from 'lucide-react';
import InvoiceCard from './InvoiceCard';
import invoiceApi from '../../../services/invoiceApi';

/**
 * Invoice List Component
 * Displays list of invoices with filtering and pagination
 */
const InvoiceList = ({ orderId, userId, onPaymentClick }) => {
    const [invoices, setInvoices] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filterStatus, setFilterStatus] = useState('all');
    const [filterType, setFilterType] = useState('all');
    const [downloadingStatement, setDownloadingStatement] = useState(false);

    useEffect(() => {
        fetchInvoices();
    }, [orderId, userId]);

    const fetchInvoices = async () => {
        try {
            setLoading(true);
            setError(null);

            let response;
            if (orderId) {
                response = await invoiceApi.getInvoicesByOrder(orderId);
            } else if (userId) {
                response = await invoiceApi.getUserInvoices(userId, 1, 100);
            }

            if (response?.success) {
                setInvoices(response.data);
            }
        } catch (err) {
            console.error('Error fetching invoices:', err);
            setError('Failed to load invoices');
        } finally {
            setLoading(false);
        }
    };

    const handleDownloadStatement = async () => {
        if (!orderId) return;

        try {
            setDownloadingStatement(true);
            await invoiceApi.downloadConsolidatedStatement(orderId);
        } catch (error) {
            console.error('Failed to download statement:', error);
            alert('Failed to download consolidated statement');
        } finally {
            setDownloadingStatement(false);
        }
    };

    // Filter invoices
    const filteredInvoices = invoices.filter(invoice => {
        const statusMatch = filterStatus === 'all' || invoice.status === parseInt(filterStatus);
        const typeMatch = filterType === 'all' || invoice.invoiceType === parseInt(filterType);
        return statusMatch && typeMatch;
    });

    // Group invoices by type for better organization
    const groupedInvoices = filteredInvoices.reduce((groups, invoice) => {
        const type = invoice.invoiceType;
        if (!groups[type]) {
            groups[type] = [];
        }
        groups[type].push(invoice);
        return groups;
    }, {});

    const getTypeLabel = (type) => {
        const labels = {
            1: 'Booking Invoices',
            2: 'Pre-Event Invoices',
            3: 'Final Settlement Invoices'
        };
        return labels[type] || 'Other Invoices';
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center py-12">
                <Loader className="animate-spin text-blue-600" size={32} />
                <span className="ml-3 text-gray-600">Loading invoices...</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 border border-red-200 rounded-lg p-6">
                <div className="flex items-center gap-3 text-red-700">
                    <AlertCircle size={24} />
                    <div>
                        <h3 className="font-semibold">Error Loading Invoices</h3>
                        <p className="text-sm mt-1">{error}</p>
                    </div>
                </div>
                <button
                    onClick={fetchInvoices}
                    className="mt-4 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
                >
                    Try Again
                </button>
            </div>
        );
    }

    if (!invoices || invoices.length === 0) {
        return (
            <div className="bg-gray-50 border border-gray-200 rounded-lg p-8 text-center">
                <FileText className="mx-auto text-gray-400 mb-3" size={48} />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                    No Invoices Found
                </h3>
                <p className="text-gray-600 text-sm">
                    Invoices will appear here once they are generated for your order.
                </p>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            {/* Header with filters */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
                <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                    <div>
                        <h2 className="text-xl font-bold text-gray-900 flex items-center gap-2">
                            <FileText size={24} />
                            Invoices
                            <span className="text-sm font-normal text-gray-500">
                                ({filteredInvoices.length} {filteredInvoices.length === 1 ? 'invoice' : 'invoices'})
                            </span>
                        </h2>
                    </div>

                    <div className="flex flex-wrap items-center gap-3">
                        {/* Status Filter */}
                        <div className="flex items-center gap-2">
                            <Filter size={16} className="text-gray-400" />
                            <select
                                value={filterStatus}
                                onChange={(e) => setFilterStatus(e.target.value)}
                                className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                            >
                                <option value="all">All Status</option>
                                <option value="1">Unpaid</option>
                                <option value="2">Overdue</option>
                                <option value="3">Paid</option>
                                <option value="4">Partially Paid</option>
                            </select>
                        </div>

                        {/* Type Filter */}
                        <select
                            value={filterType}
                            onChange={(e) => setFilterType(e.target.value)}
                            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        >
                            <option value="all">All Types</option>
                            <option value="1">Booking</option>
                            <option value="2">Pre-Event</option>
                            <option value="3">Final</option>
                        </select>

                        {/* Download Statement */}
                        {orderId && (
                            <button
                                onClick={handleDownloadStatement}
                                disabled={downloadingStatement}
                                className="flex items-center gap-2 px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                <Download size={16} />
                                {downloadingStatement ? 'Downloading...' : 'Consolidated Statement'}
                            </button>
                        )}
                    </div>
                </div>
            </div>

            {/* Invoice groups */}
            {Object.keys(groupedInvoices).length === 0 ? (
                <div className="bg-gray-50 border border-gray-200 rounded-lg p-8 text-center">
                    <AlertCircle className="mx-auto text-gray-400 mb-3" size={48} />
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">
                        No Invoices Match Filters
                    </h3>
                    <p className="text-gray-600 text-sm">
                        Try adjusting your filters to see more invoices.
                    </p>
                </div>
            ) : (
                <div className="space-y-6">
                    {Object.keys(groupedInvoices)
                        .sort((a, b) => parseInt(a) - parseInt(b))
                        .map(type => (
                            <div key={type}>
                                <h3 className="text-lg font-semibold text-gray-900 mb-3">
                                    {getTypeLabel(parseInt(type))}
                                </h3>
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                    {groupedInvoices[type].map(invoice => (
                                        <InvoiceCard
                                            key={invoice.invoiceId}
                                            invoice={invoice}
                                            onPaymentClick={onPaymentClick}
                                        />
                                    ))}
                                </div>
                            </div>
                        ))}
                </div>
            )}

            {/* Summary */}
            <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-lg p-4">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <div>
                        <p className="text-xs text-gray-600 mb-1">Total Invoices</p>
                        <p className="text-2xl font-bold text-gray-900">{invoices.length}</p>
                    </div>
                    <div>
                        <p className="text-xs text-gray-600 mb-1">Total Amount</p>
                        <p className="text-2xl font-bold text-gray-900">
                            ₹{invoices.reduce((sum, inv) => sum + inv.totalAmount, 0).toLocaleString('en-IN')}
                        </p>
                    </div>
                    <div>
                        <p className="text-xs text-gray-600 mb-1">Amount Paid</p>
                        <p className="text-2xl font-bold text-green-600">
                            ₹{invoices.reduce((sum, inv) => sum + inv.amountPaid, 0).toLocaleString('en-IN')}
                        </p>
                    </div>
                    <div>
                        <p className="text-xs text-gray-600 mb-1">Balance Due</p>
                        <p className="text-2xl font-bold text-red-600">
                            ₹{invoices.reduce((sum, inv) => sum + inv.balanceDue, 0).toLocaleString('en-IN')}
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default InvoiceList;
