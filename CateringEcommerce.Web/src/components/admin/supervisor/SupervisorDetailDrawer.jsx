import { useState, useEffect } from 'react';
import {
    X, User, Mail, Phone, MapPin, Shield, FileText,
    Image as ImageIcon, Home, Briefcase, FileCheck,
    CheckCircle, XCircle, Eye, MessageSquare, Clock,
    CreditCard, Calendar, Star, Activity, Building2,
    Banknote, Award, ChevronRight
} from 'lucide-react';
import { supervisorManagementApi } from '../../../services/adminApi';
import PartnerStatusBadge from '../partner-requests/PartnerStatusBadge';
import MediaViewer from '../ui/MediaViewer';

// SupervisorApprovalStatus enum (must match C# backend: 0-4)
const S = { PENDING: 0, APPROVED: 1, REJECTED: 2, UNDER_REVIEW: 3, INFO_REQUESTED: 4 };

const TABS = [
    { label: 'Personal',     icon: User },
    { label: 'Experience',   icon: Award },
    { label: 'Documents',    icon: FileText },
    { label: 'Availability', icon: Calendar },
    { label: 'Banking',      icon: Banknote },
];

const DAYS_ORDER = ['MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY', 'SUNDAY'];
const DAY_SHORT  = { MONDAY: 'Mon', TUESDAY: 'Tue', WEDNESDAY: 'Wed', THURSDAY: 'Thu', FRIDAY: 'Fri', SATURDAY: 'Sat', SUNDAY: 'Sun' };

// ── Helpers ───────────────────────────────────────────────────────────────────

const parseJson = (raw) => {
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
};

const parseList = (raw) => {
    if (!raw) return [];
    const parsed = parseJson(raw);
    if (Array.isArray(parsed)) return parsed;
    return raw.split(',').map(s => s.trim()).filter(Boolean);
};

const fmtDate = (val) => {
    if (!val) return null;
    try { return new Date(val).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }); }
    catch { return val; }
};

const fmtMoney = (val) => val != null ? `₹${Number(val).toLocaleString('en-IN')}` : null;

const maskAccount = (num) => {
    if (!num) return '—';
    return '•••• •••• ' + num.slice(-4);
};

// ── Small shared components ───────────────────────────────────────────────────

/** Section heading with left accent bar */
const Section = ({ children }) => (
    <div className="flex items-center gap-2 mb-3 mt-6 first:mt-0">
        <div className="w-1 h-4 bg-indigo-500 rounded-full flex-shrink-0" />
        <span className="text-xs font-bold text-indigo-600 uppercase tracking-widest">{children}</span>
    </div>
);

/** Two-column label + value row */
const Field = ({ label, children, value }) => (
    <div className="flex flex-col gap-0.5 py-2.5 border-b border-gray-100 last:border-0">
        <span className="text-xs font-semibold text-gray-400 uppercase tracking-wide">{label}</span>
        <span className="text-sm font-medium text-gray-800 break-words leading-relaxed">
            {children || value || <span className="text-gray-300 italic text-xs">Not provided</span>}
        </span>
    </div>
);

/** Colored pill badge */
const Pill = ({ label, color = 'gray' }) => {
    const colors = {
        blue:   'bg-blue-100 text-blue-700 ring-1 ring-blue-200',
        teal:   'bg-teal-100 text-teal-700 ring-1 ring-teal-200',
        green:  'bg-green-100 text-green-700 ring-1 ring-green-200',
        red:    'bg-red-100 text-red-700 ring-1 ring-red-200',
        orange: 'bg-orange-100 text-orange-700 ring-1 ring-orange-200',
        purple: 'bg-purple-100 text-purple-700 ring-1 ring-purple-200',
        indigo: 'bg-indigo-100 text-indigo-700 ring-1 ring-indigo-200',
        amber:  'bg-amber-100 text-amber-700 ring-1 ring-amber-200',
        gray:   'bg-gray-100 text-gray-600 ring-1 ring-gray-200',
    };
    return (
        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${colors[color] || colors.gray}`}>
            {label}
        </span>
    );
};

/** Workflow step status badge */
const StepBadge = ({ value, trueLabel = 'Completed', falseLabel = 'Pending' }) => {
    const isPositive = value === true || (typeof value === 'string' &&
        ['COMPLETED', 'PASSED', 'APPROVED', 'VERIFIED', 'ACTIVATED'].includes(value.toUpperCase()));
    return (
        <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${
            isPositive
                ? 'bg-green-100 text-green-700 ring-1 ring-green-200'
                : 'bg-gray-100 text-gray-500 ring-1 ring-gray-200'
        }`}>
            {isPositive
                ? <CheckCircle className="w-3.5 h-3.5" />
                : <Clock className="w-3.5 h-3.5 opacity-60" />}
            {typeof value === 'boolean' ? (value ? trueLabel : falseLabel) : (value || falseLabel)}
        </span>
    );
};

/** Skeleton loader rows */
const Skeleton = () => (
    <div className="space-y-4 animate-pulse px-6 py-6">
        <div className="flex gap-3">
            <div className="w-14 h-14 bg-gray-200 rounded-full flex-shrink-0" />
            <div className="flex-1 space-y-2 pt-1">
                <div className="h-4 bg-gray-200 rounded w-2/3" />
                <div className="h-3 bg-gray-100 rounded w-1/2" />
            </div>
        </div>
        {[1,2,3,4,5].map(i => (
            <div key={i} className="space-y-1.5">
                <div className="h-2.5 bg-gray-100 rounded w-1/4" />
                <div className="h-3.5 bg-gray-200 rounded w-3/4" />
            </div>
        ))}
    </div>
);

/** Document card with view button */
const DocCard = ({ icon: Icon, label, subtitle, url, onView }) => {
    if (!url) return null;
    return (
        <div className="flex items-center gap-3 p-3.5 bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
            <div className="w-10 h-10 bg-indigo-50 rounded-xl flex items-center justify-center flex-shrink-0 ring-1 ring-indigo-100">
                <Icon className="w-5 h-5 text-indigo-600" />
            </div>
            <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold text-gray-800 truncate">{label}</p>
                {subtitle && <p className="text-xs text-gray-400 truncate mt-0.5">{subtitle}</p>}
            </div>
            <button
                onClick={onView}
                className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-indigo-600 bg-indigo-50 hover:bg-indigo-100 rounded-lg transition-colors flex-shrink-0 ring-1 ring-indigo-100"
            >
                <Eye className="w-3.5 h-3.5" />
                View
            </button>
        </div>
    );
};

// ── Main Drawer ───────────────────────────────────────────────────────────────

const SupervisorDetailDrawer = ({ supervisorId, onClose, onStatusUpdate }) => {
    const [data,        setData]        = useState(null);
    const [loading,     setLoading]     = useState(true);
    const [activeTab,   setActiveTab]   = useState(0);
    const [mediaViewer, setMediaViewer] = useState({ open: false, items: [], index: 0 });
    const [actionMode,  setActionMode]  = useState(null);  // null | 'reject' | 'info'
    const [actionText,  setActionText]  = useState('');
    const [submitting,  setSubmitting]  = useState(false);

    useEffect(() => {
        setLoading(true);
        supervisorManagementApi.getSupervisorDetails(supervisorId)
            .then(res => { if (res?.result) setData(res.data); })
            .catch(() => {})
            .finally(() => setLoading(false));
    }, [supervisorId]);

    useEffect(() => {
        const handler = e => { if (e.key === 'Escape') onClose(); };
        document.addEventListener('keydown', handler);
        return () => document.removeEventListener('keydown', handler);
    }, [onClose]);

    const openMedia = (url, label) =>
        setMediaViewer({ open: true, items: [{ filePath: url, fileName: label, label }], index: 0 });

    const isPending = data && [S.PENDING, S.UNDER_REVIEW, S.INFO_REQUESTED].includes(data.status);

    const handleStatusAction = async (status, reason = null) => {
        if (!onStatusUpdate) return;
        setSubmitting(true);
        try { await onStatusUpdate(supervisorId, status, reason); }
        finally { setSubmitting(false); }
    };

    // ── Tab 0: Personal Info ──────────────────────────────────────────────────
    const renderPersonal = () => (
        <div>
            <Section>Identity</Section>
            <Field label="Full Name" value={data.fullName} />
            <Field label="Email">
                <a href={`mailto:${data.email}`} className="text-indigo-600 hover:underline inline-flex items-center gap-1.5 font-medium">
                    <Mail className="w-3.5 h-3.5 flex-shrink-0" />{data.email}
                </a>
            </Field>
            <Field label="Phone">
                <span className="inline-flex items-center gap-2">
                    <Phone className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
                    {data.phone}
                    {data.alternatePhone && (
                        <span className="text-gray-400 text-xs">/ {data.alternatePhone}</span>
                    )}
                </span>
            </Field>
            <Field label="Date of Birth" value={data.dateOfBirth} />
            <Field label="Gender" value={data.gender} />

            <Section>Address</Section>
            <Field label="Address" value={data.addressLine1} />
            <Field label="Locality" value={data.locality} />
            <Field label="City / State">
                {[data.city, data.state].filter(Boolean).join(', ') || null}
            </Field>
            <Field label="Pincode" value={data.pincode} />

            <Section>Profile</Section>
            <Field label="Supervisor Type">
                <Pill label={data.supervisorType} color={data.supervisorType === 'CAREER' ? 'blue' : 'teal'} />
            </Field>
            <Field label="Authority Level">
                {data.authorityLevel
                    ? <Pill label={data.authorityLevel} color="indigo" />
                    : null}
            </Field>
            {data.totalEventsSupervised > 0 && (
                <Field label="Events Supervised">
                    <span className="font-semibold text-indigo-700">{data.totalEventsSupervised}</span>
                </Field>
            )}
            {data.averageRating != null && (
                <Field label="Average Rating">
                    <span className="inline-flex items-center gap-1.5 text-amber-600 font-semibold">
                        <Star className="w-4 h-4 fill-amber-400 stroke-amber-400" />
                        {Number(data.averageRating).toFixed(1)}
                        <span className="text-gray-400 font-normal text-xs">/ 5.0</span>
                    </span>
                </Field>
            )}
            <Field label="Registered On" value={fmtDate(data.createdDate)} />
            {data.modifiedDate && (
                <Field label="Last Updated" value={fmtDate(data.modifiedDate)} />
            )}
        </div>
    );

    // ── Tab 1: Experience ─────────────────────────────────────────────────────
    const renderExperience = () => {
        const languages = parseList(data.languagesKnown);
        const hasWorkflow = data.docVerificationStatus || data.interviewResult ||
            data.trainingCompleted || data.certificationPassed || data.activationStatus;

        return (
            <div>
                <Section>Background</Section>
                <Field label="Prior Experience">
                    <span className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold ring-1 ${
                        data.hasPriorExperience
                            ? 'bg-blue-100 text-blue-700 ring-blue-200'
                            : 'bg-gray-100 text-gray-500 ring-gray-200'
                    }`}>
                        {data.hasPriorExperience
                            ? <><CheckCircle className="w-3.5 h-3.5" /> Experienced</>
                            : <><Clock className="w-3.5 h-3.5 opacity-60" /> Fresher</>}
                    </span>
                </Field>
                {data.priorExperienceDetails && (
                    <Field label="Details">
                        <span className="whitespace-pre-wrap leading-relaxed text-gray-700">{data.priorExperienceDetails}</span>
                    </Field>
                )}
                <Field label="Specialization" value={data.specialization} />
                <Field label="Languages Known">
                    {languages.length > 0
                        ? <div className="flex flex-wrap gap-1.5 mt-0.5">
                            {languages.map((l, i) => <Pill key={i} label={l} color="gray" />)}
                          </div>
                        : null}
                </Field>

                {hasWorkflow && (
                    <>
                        <Section>Registration Workflow</Section>
                        <div className="grid grid-cols-2 gap-2.5">
                            {[
                                { label: 'Doc Verification', value: data.docVerificationStatus },
                                { label: 'Interview',        value: data.interviewResult },
                                { label: 'Training',         value: data.trainingCompleted,    trueLabel: 'Completed' },
                                { label: 'Certification',    value: data.certificationPassed,  trueLabel: 'Passed' },
                            ].map(item => (
                                <div key={item.label} className="bg-gray-50 rounded-xl p-3.5 ring-1 ring-gray-100">
                                    <p className="text-xs font-semibold text-gray-400 mb-2 uppercase tracking-wide">{item.label}</p>
                                    <StepBadge value={item.value} trueLabel={item.trueLabel} />
                                </div>
                            ))}
                        </div>
                        {(data.certificationStatus || data.activationStatus) && (
                            <div className="grid grid-cols-2 gap-2.5 mt-2.5">
                                {data.certificationStatus && (
                                    <div className="bg-gray-50 rounded-xl p-3.5 ring-1 ring-gray-100">
                                        <p className="text-xs font-semibold text-gray-400 mb-2 uppercase tracking-wide">Cert. Status</p>
                                        <StepBadge value={data.certificationStatus} trueLabel="Active" />
                                    </div>
                                )}
                                {data.activationStatus && (
                                    <div className="bg-gray-50 rounded-xl p-3.5 ring-1 ring-gray-100">
                                        <p className="text-xs font-semibold text-gray-400 mb-2 uppercase tracking-wide">Activation</p>
                                        <StepBadge value={data.activationStatus} trueLabel="Activated" />
                                    </div>
                                )}
                            </div>
                        )}
                    </>
                )}
            </div>
        );
    };

    // ── Tab 2: Documents ──────────────────────────────────────────────────────
    const renderDocuments = () => {
        const docs = [
            {
                icon: FileText, label: 'Identity Proof',
                subtitle: [data.identityType, data.identityNumber].filter(Boolean).join(' — '),
                url: data.identityProofUrl,
            },
            { icon: ImageIcon, label: 'Photo',             subtitle: null, url: data.photoUrl },
            { icon: Home,      label: 'Address Proof',     subtitle: null, url: data.addressProofUrl },
            ...(data.supervisorType === 'CAREER'
                ? [{ icon: Briefcase, label: 'Resume', subtitle: null, url: data.resumeUrl }]
                : []),
            { icon: FileCheck,  label: 'Agreement',        subtitle: null, url: data.agreementUrl },
            { icon: CreditCard, label: 'Cancelled Cheque', subtitle: null, url: data.cancelledChequeUrl },
        ].filter(d => d.url);

        if (docs.length === 0) {
            return (
                <div className="flex flex-col items-center justify-center py-20 text-gray-300">
                    <div className="w-16 h-16 bg-gray-100 rounded-2xl flex items-center justify-center mb-4">
                        <FileText className="w-8 h-8 text-gray-300" />
                    </div>
                    <p className="text-sm font-semibold text-gray-400">No documents uploaded yet</p>
                    <p className="text-xs text-gray-300 mt-1">Documents will appear here once submitted</p>
                </div>
            );
        }

        return (
            <div className="space-y-3">
                <Section>Uploaded Documents</Section>
                <p className="text-xs text-gray-400 -mt-2 mb-1">{docs.length} document{docs.length !== 1 ? 's' : ''} uploaded</p>
                {docs.map(doc => (
                    <DocCard
                        key={doc.label}
                        icon={doc.icon}
                        label={doc.label}
                        subtitle={doc.subtitle}
                        url={doc.url}
                        onView={() => openMedia(doc.url, doc.label)}
                    />
                ))}
            </div>
        );
    };

    // ── Tab 3: Availability ───────────────────────────────────────────────────
    const renderAvailability = () => {
        const calObj = parseJson(data.availabilityCalendar);
        const eventTypes = parseList(data.preferredEventTypes);
        const availableDays = calObj
            ? DAYS_ORDER.filter(d => calObj[d] === true || calObj[d] === 'true')
            : [];

        return (
            <div>
                <Section>Working Days</Section>
                {calObj ? (
                    <>
                        <div className="flex flex-wrap gap-2 mb-3">
                            {DAYS_ORDER.map(day => {
                                const on = calObj[day] === true || calObj[day] === 'true';
                                return (
                                    <div
                                        key={day}
                                        className={`flex flex-col items-center px-3 py-2.5 rounded-xl text-xs font-bold min-w-[52px] ring-1 transition-colors ${
                                            on
                                                ? 'bg-green-50 text-green-700 ring-green-200'
                                                : 'bg-gray-50 text-gray-300 ring-gray-100'
                                        }`}
                                    >
                                        {DAY_SHORT[day]}
                                        <div className="mt-1.5">
                                            {on
                                                ? <CheckCircle className="w-3.5 h-3.5 text-green-500" />
                                                : <XCircle className="w-3.5 h-3.5 text-gray-200" />}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                        <p className="text-xs text-gray-400">
                            {availableDays.length > 0
                                ? <><span className="font-semibold text-green-600">{availableDays.length}</span> of 7 days available</>
                                : 'No days marked as available'}
                        </p>
                    </>
                ) : (
                    <p className="text-sm text-gray-400 italic mb-4">Not configured</p>
                )}

                <Section>Preferences</Section>
                <Field label="Max Events / Month">
                    {data.maxEventsPerMonth
                        ? <span className="font-semibold text-indigo-700 text-base">{data.maxEventsPerMonth}</span>
                        : null}
                </Field>
                <Field label="Preferred Event Types">
                    {eventTypes.length > 0
                        ? <div className="flex flex-wrap gap-1.5 mt-0.5">
                            {eventTypes.map((t, i) => <Pill key={i} label={t} color="purple" />)}
                          </div>
                        : null}
                </Field>
            </div>
        );
    };

    // ── Tab 4: Banking ────────────────────────────────────────────────────────
    const renderBanking = () => {
        const hasBanking = data.bankAccountHolderName || data.bankName || data.bankAccountNumber;

        if (!hasBanking) {
            return (
                <div className="flex flex-col items-center justify-center py-20 text-gray-300">
                    <div className="w-16 h-16 bg-gray-100 rounded-2xl flex items-center justify-center mb-4">
                        <CreditCard className="w-8 h-8 text-gray-300" />
                    </div>
                    <p className="text-sm font-semibold text-gray-400">Banking details not submitted</p>
                    <p className="text-xs text-gray-300 mt-1">Will be available after account activation</p>
                </div>
            );
        }

        return (
            <div>
                <Section>Account Details</Section>

                {/* Account number card */}
                <div className="bg-gradient-to-br from-indigo-600 to-purple-700 rounded-2xl p-5 mb-4 text-white shadow-lg">
                    <p className="text-xs font-semibold text-indigo-200 uppercase tracking-widest mb-1">Account Number</p>
                    <p className="text-xl font-bold tracking-[0.15em] font-mono mb-4">
                        {maskAccount(data.bankAccountNumber)}
                    </p>
                    <div className="flex items-end justify-between">
                        <div>
                            <p className="text-xs text-indigo-200 mb-0.5">Account Holder</p>
                            <p className="text-sm font-semibold">{data.bankAccountHolderName || '—'}</p>
                        </div>
                        <div className="text-right">
                            <p className="text-xs text-indigo-200 mb-0.5">IFSC</p>
                            <p className="text-sm font-semibold font-mono">{data.bankIfsc || '—'}</p>
                        </div>
                    </div>
                </div>

                <Field label="Bank Name">
                    <span className="inline-flex items-center gap-1.5">
                        <Building2 className="w-4 h-4 text-gray-400" />
                        {data.bankName || '—'}
                    </span>
                </Field>
                {data.bankBranchName && (
                    <Field label="Branch" value={data.bankBranchName} />
                )}
                {data.bankAccountType && (
                    <Field label="Account Type">
                        <Pill label={data.bankAccountType} color="blue" />
                    </Field>
                )}

                {data.compensationType && (
                    <>
                        <Section>Compensation</Section>
                        <Field label="Type">
                            <Pill
                                label={data.compensationType.replace(/_/g, ' ')}
                                color={data.compensationType === 'MONTHLY_SALARY' ? 'green' : data.compensationType === 'HYBRID' ? 'orange' : 'blue'}
                            />
                        </Field>
                        {data.perEventRate != null && (
                            <Field label="Per Event Rate">
                                <span className="font-semibold text-green-700 text-base">{fmtMoney(data.perEventRate)}</span>
                            </Field>
                        )}
                        {data.monthlySalary != null && (
                            <Field label="Monthly Salary">
                                <span className="font-semibold text-green-700 text-base">{fmtMoney(data.monthlySalary)}</span>
                            </Field>
                        )}
                    </>
                )}

                {data.cancelledChequeUrl && (
                    <>
                        <Section>Documents</Section>
                        <DocCard
                            icon={CreditCard}
                            label="Cancelled Cheque"
                            url={data.cancelledChequeUrl}
                            onView={() => openMedia(data.cancelledChequeUrl, 'Cancelled Cheque')}
                        />
                    </>
                )}
            </div>
        );
    };

    const renderTab = () => {
        if (!data) return null;
        switch (activeTab) {
            case 0: return renderPersonal();
            case 1: return renderExperience();
            case 2: return renderDocuments();
            case 3: return renderAvailability();
            case 4: return renderBanking();
            default: return null;
        }
    };

    // ── Action buttons / inline forms ─────────────────────────────────────────
    const renderActions = () => {
        if (!isPending) return null;

        if (actionMode === 'reject' || actionMode === 'info') {
            return (
                <div className="px-5 pb-4 space-y-2.5">
                    <textarea
                        rows={2}
                        value={actionText}
                        onChange={e => setActionText(e.target.value)}
                        placeholder={actionMode === 'reject'
                            ? 'Reason for rejection (required)…'
                            : 'Describe the additional information needed…'}
                        className={`w-full text-sm border rounded-xl px-3.5 py-2.5 focus:outline-none focus:ring-2 resize-none bg-white ${
                            actionMode === 'reject'
                                ? 'border-red-200 focus:ring-red-400 focus:border-red-400'
                                : 'border-purple-200 focus:ring-purple-400 focus:border-purple-400'
                        }`}
                    />
                    <div className="flex gap-2 justify-end">
                        <button
                            onClick={() => { setActionMode(null); setActionText(''); }}
                            className="px-4 py-2 text-xs font-semibold text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            disabled={!actionText.trim() || submitting}
                            onClick={() => handleStatusAction(
                                actionMode === 'reject' ? S.REJECTED : S.INFO_REQUESTED,
                                actionText.trim()
                            )}
                            className={`px-4 py-2 text-xs font-semibold text-white rounded-lg disabled:opacity-50 transition-colors ${
                                actionMode === 'reject'
                                    ? 'bg-red-600 hover:bg-red-700'
                                    : 'bg-purple-600 hover:bg-purple-700'
                            }`}
                        >
                            {submitting ? 'Submitting…' : actionMode === 'reject' ? 'Confirm Reject' : 'Send Request'}
                        </button>
                    </div>
                </div>
            );
        }

        return (
            <div className="px-5 pb-4 flex flex-wrap gap-2">
                <button
                    disabled={submitting}
                    onClick={() => handleStatusAction(S.APPROVED)}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-green-600 hover:bg-green-700 disabled:opacity-50 rounded-xl shadow-sm transition-colors"
                >
                    <CheckCircle className="w-4 h-4" /> Approve
                </button>
                <button
                    onClick={() => { setActionMode('reject'); setActionText(''); }}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl shadow-sm transition-colors"
                >
                    <XCircle className="w-4 h-4" /> Reject
                </button>
                <button
                    disabled={submitting}
                    onClick={() => handleStatusAction(S.UNDER_REVIEW)}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-indigo-700 bg-indigo-50 hover:bg-indigo-100 disabled:opacity-50 rounded-xl ring-1 ring-indigo-200 transition-colors"
                >
                    <Eye className="w-4 h-4" /> Under Review
                </button>
                <button
                    onClick={() => { setActionMode('info'); setActionText(''); }}
                    className="flex items-center gap-1.5 px-4 py-2 text-sm font-semibold text-purple-700 bg-purple-50 hover:bg-purple-100 rounded-xl ring-1 ring-purple-200 transition-colors"
                >
                    <MessageSquare className="w-4 h-4" /> Request Info
                </button>
            </div>
        );
    };

    // ── Avatar initials ───────────────────────────────────────────────────────
    const initials = data?.fullName
        ? data.fullName.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase()
        : '?';

    return (
        <>
            {/* Backdrop */}
            <div
                className="fixed inset-0 z-40 bg-black/40 backdrop-blur-sm transition-opacity"
                onClick={onClose}
            />

            {/* Drawer panel */}
            <div className="fixed right-0 top-0 h-full w-full max-w-2xl z-50 bg-gray-50 shadow-2xl flex flex-col transform transition-transform duration-300 translate-x-0">

                {/* ── Gradient Header ── */}
                <div className="flex-shrink-0 bg-gradient-to-br from-indigo-600 to-purple-700 text-white">
                    {/* Row 1: Avatar + name + close */}
                    <div className="flex items-center gap-4 px-6 pt-5 pb-4">
                        <div className="w-14 h-14 bg-white/20 backdrop-blur-sm rounded-2xl flex items-center justify-center text-white font-bold text-xl flex-shrink-0 ring-2 ring-white/30 shadow-inner">
                            {loading ? <User className="w-6 h-6 text-white/70" /> : initials}
                        </div>
                        <div className="flex-1 min-w-0">
                            <h2 className="text-lg font-bold text-white truncate leading-tight">
                                {loading ? 'Loading…' : (data?.fullName || 'Supervisor Details')}
                            </h2>
                            {!loading && data && (
                                <div className="flex items-center gap-2 mt-1.5 flex-wrap">
                                    <span className={`text-xs font-semibold px-2.5 py-0.5 rounded-full ring-1 ring-white/20 ${
                                        data.supervisorType === 'CAREER'
                                            ? 'bg-blue-400/30 text-white'
                                            : 'bg-teal-400/30 text-white'
                                    }`}>
                                        {data.supervisorType}
                                    </span>
                                    <PartnerStatusBadge statusId={data.status} size="sm" />
                                    {data.city && (
                                        <span className="text-xs text-indigo-200 flex items-center gap-1">
                                            <MapPin className="w-3 h-3" />{data.city}
                                        </span>
                                    )}
                                </div>
                            )}
                        </div>
                        <button
                            onClick={onClose}
                            className="p-2 text-white/70 hover:text-white hover:bg-white/10 rounded-xl transition-colors flex-shrink-0"
                            aria-label="Close drawer"
                        >
                            <X className="w-5 h-5" />
                        </button>
                    </div>

                    {/* Row 2: Action buttons */}
                    {!loading && data && renderActions()}
                </div>

                {/* ── Tab bar ── */}
                <div className="flex-shrink-0 bg-white border-b border-gray-200 px-3 overflow-x-auto">
                    <div className="flex gap-1 py-2">
                        {TABS.map(({ label, icon: Icon }, i) => (
                            <button
                                key={label}
                                onClick={() => setActiveTab(i)}
                                className={`flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold rounded-lg whitespace-nowrap transition-all ${
                                    activeTab === i
                                        ? 'bg-indigo-600 text-white shadow-sm'
                                        : 'text-gray-500 hover:text-gray-700 hover:bg-gray-100'
                                }`}
                            >
                                <Icon className="w-3.5 h-3.5" />
                                {label}
                            </button>
                        ))}
                    </div>
                </div>

                {/* ── Scrollable content ── */}
                <div className="flex-1 overflow-y-auto">
                    {loading ? (
                        <Skeleton />
                    ) : !data ? (
                        <div className="flex flex-col items-center justify-center py-20 text-gray-300">
                            <div className="w-16 h-16 bg-gray-100 rounded-2xl flex items-center justify-center mb-4">
                                <Shield className="w-8 h-8 text-gray-300" />
                            </div>
                            <p className="text-sm font-semibold text-gray-400">Supervisor not found</p>
                        </div>
                    ) : (
                        <div className="p-6">
                            {renderTab()}
                        </div>
                    )}
                </div>
            </div>

            {/* MediaViewer overlay */}
            {mediaViewer.open && (
                <MediaViewer
                    mediaItems={mediaViewer.items}
                    currentIndex={mediaViewer.index}
                    onClose={() => setMediaViewer({ open: false, items: [], index: 0 })}
                    onNavigate={idx => setMediaViewer(prev => ({ ...prev, index: idx }))}
                />
            )}
        </>
    );
};

export default SupervisorDetailDrawer;
