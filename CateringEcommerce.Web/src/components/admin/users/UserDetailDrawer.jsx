import {
    X, Mail, Phone, MapPin, Calendar, Clock, ShoppingCart,
    Star, ShieldOff, ShieldCheck, Trash2, RotateCcw,
    CheckCircle, XCircle, IndianRupee
} from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '');

const UserDetailDrawer = ({ user, onClose, onBlockUnblock, onDelete, onRestore }) => {
    if (!user) return null;

    const getStatusColor = () => {
        if (user.isDeleted) return 'red';
        if (user.isBlocked) return 'orange';
        if (!user.isActive) return 'gray';
        return 'green';
    };

    const getStatusLabel = () => {
        if (user.isDeleted) return 'Deleted';
        if (user.isBlocked) return 'Blocked';
        if (!user.isActive) return 'Inactive';
        return 'Active';
    };

    const statusColor = getStatusColor();

    return (
        <div className="fixed inset-0 z-50 flex justify-end">
            {/* Backdrop */}
            <div
                className="absolute inset-0 bg-black/40 backdrop-blur-sm"
                onClick={onClose}
            />

            {/* Drawer */}
            <div className="relative w-full max-w-lg bg-white shadow-2xl overflow-y-auto animate-slide-in-right">
                {/* Header */}
                <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 z-10">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-gray-900">User Details</h2>
                        <button
                            onClick={onClose}
                            className="p-1.5 hover:bg-gray-100 rounded-lg transition-colors"
                        >
                            <X className="w-5 h-5 text-gray-500" />
                        </button>
                    </div>
                </div>

                <div className="px-6 py-5 space-y-6">
                    {/* Profile Section */}
                    <div className="flex items-start gap-4">
                        <div className="w-16 h-16 rounded-full bg-indigo-100 flex items-center justify-center flex-shrink-0">
                            {user.profilePhoto ? (
                                <img
                                    src={`${API_BASE_URL}${user.profilePhoto}`}
                                    alt={user.fullName}
                                    className="w-16 h-16 rounded-full object-cover"
                                />
                            ) : (
                                <span className="text-2xl font-bold text-indigo-600">
                                    {user.fullName?.charAt(0)?.toUpperCase() || '?'}
                                </span>
                            )}
                        </div>
                        <div className="flex-1 min-w-0">
                            <h3 className="text-xl font-semibold text-gray-900 truncate">{user.fullName}</h3>
                            <p className="text-sm text-gray-500 mt-0.5">User ID: {user.userId}</p>
                            <div className="mt-2">
                                <span className={`inline-flex px-2.5 py-1 text-xs font-semibold rounded-full bg-${statusColor}-100 text-${statusColor}-800`}>
                                    {getStatusLabel()}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Block Reason */}
                    {user.isBlocked && user.blockReason && (
                        <div className="bg-orange-50 border border-orange-200 rounded-lg p-3">
                            <p className="text-sm text-orange-800">
                                <strong>Block Reason:</strong> {user.blockReason}
                            </p>
                        </div>
                    )}

                    {/* Contact Info */}
                    <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                        <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide">Contact Information</h4>
                        <div className="space-y-2.5">
                            <div className="flex items-center gap-3">
                                <Phone className="w-4 h-4 text-gray-400" />
                                <span className="text-sm text-gray-800">{user.phone}</span>
                                {user.isPhoneVerified && (
                                    <CheckCircle className="w-4 h-4 text-green-500" title="Verified" />
                                )}
                            </div>
                            <div className="flex items-center gap-3">
                                <Mail className="w-4 h-4 text-gray-400" />
                                <span className="text-sm text-gray-800">{user.email || 'Not provided'}</span>
                                {user.email && (
                                    user.isEmailVerified
                                        ? <CheckCircle className="w-4 h-4 text-green-500" title="Verified" />
                                        : <XCircle className="w-4 h-4 text-gray-400" title="Not verified" />
                                )}
                            </div>
                            {(user.cityName || user.stateName) && (
                                <div className="flex items-center gap-3">
                                    <MapPin className="w-4 h-4 text-gray-400" />
                                    <span className="text-sm text-gray-800">
                                        {[user.cityName, user.stateName].filter(Boolean).join(', ')}
                                    </span>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Description */}
                    {user.description && (
                        <div>
                            <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-2">About</h4>
                            <p className="text-sm text-gray-600 leading-relaxed">{user.description}</p>
                        </div>
                    )}

                    {/* Statistics */}
                    <div>
                        <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">Activity Statistics</h4>
                        <div className="grid grid-cols-2 gap-3">
                            <div className="bg-blue-50 rounded-lg p-3 text-center">
                                <div className="flex items-center justify-center gap-1.5 mb-1">
                                    <ShoppingCart className="w-4 h-4 text-blue-600" />
                                </div>
                                <div className="text-xl font-bold text-blue-700">{user.totalOrders}</div>
                                <div className="text-xs text-blue-600">Total Orders</div>
                            </div>
                            <div className="bg-green-50 rounded-lg p-3 text-center">
                                <div className="flex items-center justify-center gap-1.5 mb-1">
                                    <IndianRupee className="w-4 h-4 text-green-600" />
                                </div>
                                <div className="text-xl font-bold text-green-700">
                                    {'\u20B9'}{user.totalSpent?.toLocaleString('en-IN', { minimumFractionDigits: 0 })}
                                </div>
                                <div className="text-xs text-green-600">Total Spent</div>
                            </div>
                            <div className="bg-yellow-50 rounded-lg p-3 text-center">
                                <div className="flex items-center justify-center gap-1.5 mb-1">
                                    <Star className="w-4 h-4 text-yellow-600" />
                                </div>
                                <div className="text-xl font-bold text-yellow-700">{user.totalReviews}</div>
                                <div className="text-xs text-yellow-600">Reviews</div>
                            </div>
                            <div className="bg-purple-50 rounded-lg p-3 text-center">
                                <div className="flex items-center justify-center gap-1.5 mb-1">
                                    <Star className="w-4 h-4 text-purple-600" />
                                </div>
                                <div className="text-xl font-bold text-purple-700">
                                    {user.averageRating > 0 ? user.averageRating.toFixed(1) : '-'}
                                </div>
                                <div className="text-xs text-purple-600">Avg Rating</div>
                            </div>
                        </div>
                    </div>

                    {/* Dates */}
                    <div className="bg-gray-50 rounded-lg p-4 space-y-2.5">
                        <div className="flex items-center gap-3">
                            <Calendar className="w-4 h-4 text-gray-400" />
                            <span className="text-sm text-gray-600">Registered:</span>
                            <span className="text-sm font-medium text-gray-800">
                                {new Date(user.createdDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                            </span>
                        </div>
                        <div className="flex items-center gap-3">
                            <Clock className="w-4 h-4 text-gray-400" />
                            <span className="text-sm text-gray-600">Last Login:</span>
                            <span className="text-sm font-medium text-gray-800">
                                {user.lastLogin
                                    ? formatDistanceToNow(new Date(user.lastLogin), { addSuffix: true })
                                    : 'Never'}
                            </span>
                        </div>
                    </div>

                    {/* Recent Orders */}
                    {user.recentOrders && user.recentOrders.length > 0 && (
                        <div>
                            <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">Recent Orders</h4>
                            <div className="space-y-2">
                                {user.recentOrders.map((order) => (
                                    <div key={order.orderId} className="border border-gray-200 rounded-lg p-3">
                                        <div className="flex items-center justify-between">
                                            <span className="text-sm font-medium text-gray-900">#{order.orderId}</span>
                                            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                                                order.status === 'Completed' ? 'bg-green-100 text-green-700' :
                                                order.status === 'Cancelled' ? 'bg-red-100 text-red-700' :
                                                order.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' :
                                                'bg-blue-100 text-blue-700'
                                            }`}>
                                                {order.status}
                                            </span>
                                        </div>
                                        <div className="text-xs text-gray-500 mt-1">{order.cateringName}</div>
                                        <div className="flex items-center justify-between mt-1.5">
                                            <span className="text-sm font-medium text-gray-800">
                                                {'\u20B9'}{order.totalAmount?.toLocaleString('en-IN')}
                                            </span>
                                            <span className="text-xs text-gray-400">
                                                {new Date(order.eventDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                                            </span>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Recent Reviews */}
                    {user.recentReviews && user.recentReviews.length > 0 && (
                        <div>
                            <h4 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">Recent Reviews</h4>
                            <div className="space-y-2">
                                {user.recentReviews.map((review) => (
                                    <div key={review.reviewId} className="border border-gray-200 rounded-lg p-3">
                                        <div className="flex items-center justify-between">
                                            <span className="text-sm font-medium text-gray-900">{review.cateringName}</span>
                                            <div className="flex items-center gap-0.5">
                                                {[...Array(5)].map((_, i) => (
                                                    <Star
                                                        key={i}
                                                        className={`w-3.5 h-3.5 ${i < review.rating ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}`}
                                                    />
                                                ))}
                                            </div>
                                        </div>
                                        {review.comment && (
                                            <p className="text-xs text-gray-500 mt-1.5 line-clamp-2">{review.comment}</p>
                                        )}
                                        <div className="text-xs text-gray-400 mt-1">
                                            {new Date(review.reviewDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>

                {/* Footer Actions */}
                <div className="sticky bottom-0 bg-white border-t border-gray-200 px-6 py-4">
                    <div className="flex gap-3">
                        {user.isDeleted ? (
                            <button
                                onClick={onRestore}
                                className="flex-1 flex items-center justify-center gap-2 px-4 py-2.5 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors text-sm font-medium"
                            >
                                <RotateCcw className="w-4 h-4" />
                                Restore User
                            </button>
                        ) : (
                            <>
                                <button
                                    onClick={onBlockUnblock}
                                    className={`flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                                        user.isBlocked
                                            ? 'bg-green-600 text-white hover:bg-green-700'
                                            : 'bg-orange-500 text-white hover:bg-orange-600'
                                    }`}
                                >
                                    {user.isBlocked ? (
                                        <><ShieldCheck className="w-4 h-4" /> Unblock</>
                                    ) : (
                                        <><ShieldOff className="w-4 h-4" /> Block</>
                                    )}
                                </button>
                                <button
                                    onClick={onDelete}
                                    className="flex items-center justify-center gap-2 px-4 py-2.5 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors text-sm font-medium"
                                >
                                    <Trash2 className="w-4 h-4" />
                                    Delete
                                </button>
                            </>
                        )}
                    </div>
                </div>
            </div>

            <style>{`
                @keyframes slideInRight {
                    from { transform: translateX(100%); }
                    to { transform: translateX(0); }
                }
                .animate-slide-in-right {
                    animation: slideInRight 0.3s ease-out;
                }
            `}</style>
        </div>
    );
};

export default UserDetailDrawer;
