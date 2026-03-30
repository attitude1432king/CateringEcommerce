import { useState } from 'react';
import {
    X, ShoppingBag, TrendingUp, Star, DollarSign,
    Phone, Mail, MapPin, Shield, ShieldCheck, CreditCard,
    Building, Image, Video, Calendar, Ban, Play
} from 'lucide-react';
import MediaViewer from '../ui/MediaViewer';

const VIDEO_EXTS = ['mp4', 'webm', 'ogg', 'mov', 'avi', 'mkv'];

const getFileExt = (url) => {
    if (!url) return '';
    return url.split('.').pop().split('?')[0].toLowerCase();
};

const isVideo = (url) => VIDEO_EXTS.includes(getFileExt(url));

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

const fmt = (n) =>
    new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(n ?? 0);

const fmtDate = (d) => {
    if (!d) return 'N/A';
    return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
};

const maskAccount = (num) => {
    if (!num) return 'N/A';
    const s = String(num);
    return s.length > 4 ? `${'*'.repeat(s.length - 4)}${s.slice(-4)}` : s;
};

/**
 * CateringDetailDrawer
 * Props: { catering: AdminCateringDetail, onClose: () => void }
 */
const CateringDetailDrawer = ({ catering, onClose }) => {
    const [mediaViewer, setMediaViewer] = useState({ show: false, items: [], currentIndex: 0 });

    if (!catering) return null;

    const openMedia = (index) => {
        const items = (catering.images || []).map((url, i) => ({
            filePath: url,
            fileName: `Media ${i + 1}`,
            label: isVideo(url) ? `Video ${i + 1}` : `Image ${i + 1}`,
        }));
        setMediaViewer({ show: true, items, currentIndex: index });
    };

    const statusLabel = catering.isBlocked ? 'Blocked' : catering.isActive ? 'Active' : 'Inactive';
    const statusColor = catering.isBlocked
        ? 'bg-orange-100 text-orange-800'
        : catering.isActive
            ? 'bg-green-100 text-green-800'
            : 'bg-gray-100 text-gray-600';

    return (
        <>
            {/* Backdrop */}
            <div className="fixed inset-0 bg-black bg-opacity-50 z-40" onClick={onClose} />

            {/* Drawer */}
            <div className="fixed right-0 top-0 h-full w-full max-w-3xl bg-white shadow-2xl z-50 overflow-hidden flex flex-col animate-slide-in-right">

                {/* Header */}
                <div className="bg-gradient-to-r from-indigo-600 to-purple-600 p-6 text-white flex-shrink-0">
                    <div className="flex items-start justify-between">
                        <div className="flex-1 min-w-0 pr-4">
                            <div className="flex items-center gap-2 flex-wrap">
                                <h2 className="text-xl font-bold truncate">{catering.businessName}</h2>
                                {catering.isVerified && (
                                    <ShieldCheck className="w-5 h-5 text-green-300 flex-shrink-0" title="Verified" />
                                )}
                                <span className={`text-xs font-semibold px-2.5 py-0.5 rounded-full ${statusColor}`}>
                                    {statusLabel}
                                </span>
                            </div>
                            <p className="text-indigo-200 text-sm mt-1">{catering.ownerName}</p>
                            <div className="flex items-center gap-4 mt-2 text-xs text-indigo-300">
                                <span className="flex items-center gap-1">
                                    <Calendar className="w-3.5 h-3.5" />
                                    Registered: {fmtDate(catering.createdDate)}
                                </span>
                                {catering.approvedDate && (
                                    <span className="flex items-center gap-1">
                                        <ShieldCheck className="w-3.5 h-3.5" />
                                        Approved: {fmtDate(catering.approvedDate)}
                                    </span>
                                )}
                            </div>
                        </div>
                        <button
                            onClick={onClose}
                            className="p-2 text-indigo-200 hover:text-white hover:bg-white/10 rounded-lg transition-colors flex-shrink-0"
                        >
                            <X className="w-5 h-5" />
                        </button>
                    </div>
                </div>

                {/* Stats Row */}
                <div className="grid grid-cols-4 divide-x divide-gray-200 border-b border-gray-200 flex-shrink-0">
                    <StatCard icon={ShoppingBag} label="Total Orders" value={catering.totalOrders ?? 0} iconColor="text-blue-500" />
                    <StatCard icon={TrendingUp} label="Total Earnings" value={fmt(catering.totalEarnings)} iconColor="text-green-500" />
                    <StatCard icon={DollarSign} label="Commission" value={fmt(catering.platformCommission)} iconColor="text-purple-500" />
                    <StatCard
                        icon={Star}
                        label="Avg Rating"
                        value={catering.averageRating ? `${Number(catering.averageRating).toFixed(1)} ★` : 'No ratings'}
                        sub={catering.totalReviews ? `${catering.totalReviews} reviews` : null}
                        iconColor="text-yellow-500"
                    />
                </div>

                {/* Scrollable Content */}
                <div className="flex-1 overflow-y-auto p-6 space-y-6">

                    {/* Block Alert */}
                    {catering.isBlocked && (
                        <div className="flex items-start gap-3 p-4 bg-orange-50 border border-orange-200 rounded-lg">
                            <Ban className="w-5 h-5 text-orange-600 flex-shrink-0 mt-0.5" />
                            <div>
                                <p className="text-sm font-semibold text-orange-800">This catering is blocked</p>
                                {catering.blockReason && (
                                    <p className="text-sm text-orange-700 mt-0.5">{catering.blockReason}</p>
                                )}
                            </div>
                        </div>
                    )}

                    {/* Business Info */}
                    <Section title="Business Information" icon={Building}>
                        <InfoRow label="Owner Name" value={catering.ownerName} />
                        <InfoRow label="Phone" value={catering.phone} icon={<Phone className="w-3.5 h-3.5" />} />
                        {catering.alternatePhone && (
                            <InfoRow label="Alt. Phone" value={catering.alternatePhone} icon={<Phone className="w-3.5 h-3.5" />} />
                        )}
                        <InfoRow label="Email" value={catering.email} icon={<Mail className="w-3.5 h-3.5" />} />
                        <InfoRow label="Active" value={catering.isActive ? 'Yes' : 'No'} />
                        <InfoRow label="Verified" value={catering.isVerified ? 'Yes' : 'No'} />
                    </Section>

                    {/* Address */}
                    <Section title="Address" icon={MapPin}>
                        <InfoRow label="Line 1" value={catering.addressLine1} />
                        {catering.addressLine2 && <InfoRow label="Line 2" value={catering.addressLine2} />}
                        <InfoRow label="City" value={catering.city} />
                        <InfoRow label="State" value={catering.state} />
                        <InfoRow label="Pincode" value={catering.pincode} />
                    </Section>

                    {/* Legal */}
                    <Section title="Legal & Compliance" icon={Shield}>
                        <InfoRow label="FSSAI Number" value={catering.fssaiNumber || 'Not provided'} />
                        <InfoRow label="GST Number" value={catering.gstNumber || 'Not Registered'} />
                        <InfoRow label="PAN Number" value={catering.panNumber || 'Not provided'} />
                    </Section>

                    {/* Banking */}
                    <Section title="Banking Details" icon={CreditCard}>
                        <InfoRow label="Account Holder" value={catering.accountHolderName} />
                        <InfoRow label="Bank" value={catering.bankName} />
                        <InfoRow label="Account Number" value={maskAccount(catering.accountNumber)} />
                        <InfoRow label="IFSC Code" value={catering.ifscCode} />
                    </Section>

                    {/* Media (Images & Videos) */}
                    <Section title="Media" icon={Image}>
                        {catering.images && catering.images.length > 0 ? (
                            <div className="grid grid-cols-3 gap-3 p-3">
                                {catering.images.map((url, idx) => {
                                    const fullUrl = `${API_BASE_URL}${url}`;
                                    const video = isVideo(url);
                                    return (
                                        <button
                                            key={idx}
                                            onClick={() => openMedia(idx)}
                                            className="group relative aspect-square bg-gray-100 rounded-lg overflow-hidden border-2 border-gray-200 hover:border-indigo-500 transition-all"
                                        >
                                            {video ? (
                                                <>
                                                    <video
                                                        src={fullUrl}
                                                        className="w-full h-full object-cover"
                                                        muted
                                                        preload="metadata"
                                                    />
                                                    <div className="absolute inset-0 flex items-center justify-center bg-black/30 group-hover:bg-black/50 transition-colors">
                                                        <div className="w-9 h-9 rounded-full bg-white/80 flex items-center justify-center">
                                                            <Play className="w-4 h-4 text-indigo-700 fill-indigo-700 ml-0.5" />
                                                        </div>
                                                    </div>
                                                    <span className="absolute bottom-1 right-1 text-[10px] bg-black/60 text-white px-1 py-0.5 rounded font-medium flex items-center gap-0.5">
                                                        <Video className="w-2.5 h-2.5" /> VIDEO
                                                    </span>
                                                </>
                                            ) : (
                                                <>
                                                    <img
                                                        src={fullUrl}
                                                        alt={`Media ${idx + 1}`}
                                                        className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                                                    />
                                                    <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-20 transition-opacity flex items-center justify-center">
                                                        <Image className="w-5 h-5 text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                                                    </div>
                                                </>
                                            )}
                                        </button>
                                    );
                                })}
                            </div>
                        ) : (
                            <div className="text-center py-6 text-sm text-gray-400">
                                <Image className="w-10 h-10 mx-auto mb-2 opacity-30" />
                                No media uploaded
                            </div>
                        )}
                    </Section>
                </div>
            </div>

            {/* Media Viewer */}
            {mediaViewer.show && (
                <MediaViewer
                    mediaItems={mediaViewer.items}
                    currentIndex={mediaViewer.currentIndex}
                    onClose={() => setMediaViewer({ show: false, items: [], currentIndex: 0 })}
                    onNavigate={(i) => setMediaViewer(prev => ({ ...prev, currentIndex: i }))}
                />
            )}
        </>
    );
};

// ── Helpers ────────────────────────────────────────────────────────────────────

const StatCard = ({ icon: Icon, label, value, sub, iconColor }) => (
    <div className="p-4 flex flex-col items-center text-center">
        <Icon className={`w-5 h-5 mb-1 ${iconColor}`} />
        <p className="text-lg font-bold text-gray-900 leading-tight">{value}</p>
        {sub && <p className="text-xs text-gray-400">{sub}</p>}
        <p className="text-xs text-gray-500 mt-0.5">{label}</p>
    </div>
);

const Section = ({ title, icon: Icon, children }) => (
    <section>
        <h3 className="text-sm font-semibold text-indigo-700 uppercase tracking-wider flex items-center gap-2 mb-3">
            <span className="w-1 h-4 bg-indigo-500 rounded-full inline-block" />
            <Icon className="w-4 h-4" />
            {title}
        </h3>
        <div className="bg-gray-50 rounded-lg border border-gray-200 divide-y divide-gray-100">
            {children}
        </div>
    </section>
);

const InfoRow = ({ label, value, icon }) => (
    <div className="flex items-center justify-between px-4 py-2.5">
        <span className="text-xs font-medium text-gray-500 uppercase tracking-wide flex items-center gap-1.5">
            {icon && <span className="text-gray-400">{icon}</span>}
            {label}
        </span>
        <span className="text-sm font-medium text-gray-900 text-right ml-4 max-w-xs truncate">
            {value || 'N/A'}
        </span>
    </div>
);

export default CateringDetailDrawer;
