/**
 * SupervisorProfile Page (REDESIGNED)
 * Modern profile editing page for Event Supervisors
 */

import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User, Mail, Phone, MapPin, Save, ArrowLeft, AlertTriangle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getSupervisorById, updateSupervisor } from '../../services/api/supervisor/supervisorApi';
import { SupervisorStatusBadge, AuthorityLevelBadge } from '../../components/supervisor/common/badges';
import { SupervisorNavHeader } from './SupervisorDashboard';
import { useSupervisorAuth } from '../../contexts/SupervisorAuthContext'; // P1 FIX: Import context
import toast from 'react-hot-toast';

const profileSchema = z.object({
  firstName: z.string().min(2).max(50),
  lastName: z.string().min(2).max(50),
  email: z.string().email(),
  phone: z.string().regex(/^[0-9]{10}$/),
  address: z.string().min(10).max(500),
  emergencyContactName: z.string().min(2).max(100).optional(),
  emergencyContactPhone: z.string().regex(/^[0-9]{10}$/).optional(),
});

const SupervisorProfile = () => {
  const navigate = useNavigate();
  const { supervisorId } = useSupervisorAuth(); // P1 FIX: Use context instead of localStorage
  const [supervisor, setSupervisor] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const { register, handleSubmit, formState: { errors }, reset } = useForm({ resolver: zodResolver(profileSchema) });

  useEffect(() => { fetchProfile(); }, []);

  const fetchProfile = async () => {
    try {
      // P1 FIX: supervisorId now from context instead of localStorage
      const response = await getSupervisorById(supervisorId);
      if (response.success) { setSupervisor(response.data); reset(response.data); }
    } catch (error) { console.error('Failed to fetch profile:', error); toast.error('Failed to load profile'); }
    finally { setLoading(false); }
  };

  const onSubmit = async (data) => {
    setSaving(true);
    try {
      // P1 FIX: supervisorId now from context instead of localStorage
      const response = await updateSupervisor(supervisorId, data);
      if (response.success) { toast.success('Profile updated successfully'); fetchProfile(); }
      else { toast.error('Failed to update profile'); }
    } catch (error) { console.error('Update error:', error); toast.error('Failed to update profile'); }
    finally { setSaving(false); }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-rose-600 mx-auto mb-4"></div>
          <p className="text-neutral-600 text-sm">Loading profile...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-rose-50/30 via-white to-amber-50/30">
      <SupervisorNavHeader activePath="/supervisor/profile" />

      {/* Page Header */}
      <div className="bg-gradient-to-r from-blue-50 to-cyan-50 border-b-2 border-neutral-100">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <button onClick={() => navigate('/supervisor/dashboard')} className="flex items-center gap-2 text-neutral-600 hover:text-neutral-900 mb-4 text-sm font-medium transition-colors">
            <ArrowLeft className="w-4 h-4" /> Back to Dashboard
          </button>
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
              <User className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <h1 className="text-3xl font-bold text-neutral-800">Profile Settings</h1>
              <p className="text-sm text-neutral-600">Manage your personal information and contact details</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
        {/* Profile Card */}
        <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm overflow-hidden">
          <div className="bg-gradient-to-r from-rose-50 to-amber-50 px-6 py-5 border-b-2 border-neutral-100">
            <div className="flex items-center gap-4">
              {supervisor?.photoUrl ? (
                <img src={supervisor.photoUrl} alt="Profile" className="w-20 h-20 rounded-xl object-cover border-4 border-white shadow-lg" />
              ) : (
                <div className="w-20 h-20 rounded-xl bg-gradient-to-br from-rose-500 to-amber-500 flex items-center justify-center text-white font-bold text-2xl shadow-lg border-4 border-white">
                  {supervisor?.firstName?.[0]}{supervisor?.lastName?.[0]}
                </div>
              )}
              <div>
                <h2 className="text-xl font-bold text-neutral-800">{supervisor?.firstName} {supervisor?.lastName}</h2>
                <div className="flex items-center gap-2 mt-1.5">
                  <SupervisorStatusBadge status={supervisor?.supervisorStatus} />
                  <AuthorityLevelBadge level={supervisor?.authorityLevel} />
                </div>
              </div>
            </div>
          </div>
        </section>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Personal Information */}
          <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 bg-rose-100 rounded-lg flex items-center justify-center"><User className="w-5 h-5 text-rose-600" /></div>
              <div><h3 className="text-xl font-bold text-neutral-800">Personal Information</h3><p className="text-sm text-neutral-500">Your basic profile details</p></div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">First Name <span className="text-rose-500">*</span></label>
                <div className="relative">
                  <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                  <input type="text" {...register('firstName')} className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.firstName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'} focus:outline-none`} />
                </div>
                {errors.firstName && <p className="text-xs text-red-600 mt-1.5">{errors.firstName.message}</p>}
              </div>
              <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">Last Name <span className="text-rose-500">*</span></label>
                <div className="relative">
                  <User className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                  <input type="text" {...register('lastName')} className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.lastName ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'} focus:outline-none`} />
                </div>
                {errors.lastName && <p className="text-xs text-red-600 mt-1.5">{errors.lastName.message}</p>}
              </div>
            </div>
          </section>

          {/* Contact Information */}
          <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center"><Mail className="w-5 h-5 text-blue-600" /></div>
              <div><h3 className="text-xl font-bold text-neutral-800">Contact Information</h3><p className="text-sm text-neutral-500">How we can reach you</p></div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">Email <span className="text-rose-500">*</span></label>
                <div className="relative">
                  <Mail className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                  <input type="email" {...register('email')} className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.email ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'} focus:outline-none`} />
                </div>
                {errors.email && <p className="text-xs text-red-600 mt-1.5">{errors.email.message}</p>}
              </div>
              <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">Phone <span className="text-rose-500">*</span></label>
                <div className="relative">
                  <Phone className="absolute left-3.5 top-1/2 -translate-y-1/2 w-5 h-5 text-neutral-400" />
                  <input type="tel" {...register('phone')} maxLength={10} className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.phone ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'} focus:outline-none`} />
                </div>
                {errors.phone && <p className="text-xs text-red-600 mt-1.5">{errors.phone.message}</p>}
              </div>
            </div>
            <div className="mt-6">
              <label className="block text-sm font-semibold text-neutral-800 mb-2">Address <span className="text-rose-500">*</span></label>
              <div className="relative">
                <MapPin className="absolute left-3.5 top-3 w-5 h-5 text-neutral-400" />
                <textarea {...register('address')} rows={3} className={`w-full pl-11 pr-4 py-3 border-2 rounded-xl transition-all duration-200 ${errors.address ? 'border-red-400 bg-red-50' : 'border-neutral-200 focus:border-rose-400 focus:ring-2 focus:ring-rose-100'} focus:outline-none`} />
              </div>
              {errors.address && <p className="text-xs text-red-600 mt-1.5">{errors.address.message}</p>}
            </div>
          </section>

          {/* Emergency Contact */}
          <section className="bg-white rounded-xl border-2 border-neutral-100 shadow-sm p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 bg-amber-100 rounded-lg flex items-center justify-center"><AlertTriangle className="w-5 h-5 text-amber-600" /></div>
              <div><h3 className="text-xl font-bold text-neutral-800">Emergency Contact</h3><p className="text-sm text-neutral-500">Person to contact in case of emergency</p></div>
            </div>
            <div className="bg-amber-50 border-l-4 border-amber-400 rounded-lg p-4 mb-6">
              <p className="text-sm text-amber-800">Providing an emergency contact is recommended for event supervisors working on-site.</p>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">Contact Name</label>
                <input type="text" {...register('emergencyContactName')} placeholder="Optional" className="w-full px-4 py-3 border-2 border-neutral-200 rounded-xl focus:outline-none focus:border-rose-400 focus:ring-2 focus:ring-rose-100 transition-all duration-200" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-neutral-800 mb-2">Contact Phone</label>
                <input type="tel" {...register('emergencyContactPhone')} placeholder="Optional" maxLength={10} className="w-full px-4 py-3 border-2 border-neutral-200 rounded-xl focus:outline-none focus:border-rose-400 focus:ring-2 focus:ring-rose-100 transition-all duration-200" />
              </div>
            </div>
          </section>

          {/* Actions */}
          <div className="flex gap-4 pt-2">
            <button type="button" onClick={() => navigate('/supervisor/dashboard')} className="flex-1 px-6 py-3.5 bg-white text-neutral-700 rounded-xl font-semibold border-2 border-neutral-200 hover:border-neutral-300 hover:bg-neutral-50 transition-all duration-200">
              Cancel
            </button>
            <button type="submit" disabled={saving} className="flex-1 px-6 py-3.5 bg-gradient-to-r from-rose-600 to-rose-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transform hover:-translate-y-0.5 transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center gap-2">
              <Save className="w-5 h-5" />
              {saving ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default SupervisorProfile;
