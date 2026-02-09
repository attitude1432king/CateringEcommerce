/**
 * SupervisorProfile Page
 * Edit supervisor profile and settings
 */

import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User, Mail, Phone, MapPin, Save, ArrowLeft } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getSupervisorById, updateSupervisor } from '../../services/api/supervisor/supervisorApi';
import { SupervisorStatusBadge, AuthorityLevelBadge } from '../../components/supervisor/common/badges';
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
  const [supervisor, setSupervisor] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    resolver: zodResolver(profileSchema),
  });

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const supervisorId = localStorage.getItem('supervisorId');
      const response = await getSupervisorById(supervisorId);
      if (response.success) {
        setSupervisor(response.data);
        reset(response.data);
      }
    } catch (error) {
      console.error('Failed to fetch profile:', error);
      toast.error('Failed to load profile');
    } finally {
      setLoading(false);
    }
  };

  const onSubmit = async (data) => {
    setSaving(true);
    try {
      const supervisorId = localStorage.getItem('supervisorId');
      const response = await updateSupervisor(supervisorId, data);

      if (response.success) {
        toast.success('Profile updated successfully');
        fetchProfile();
      } else {
        toast.error('Failed to update profile');
      }
    } catch (error) {
      console.error('Update error:', error);
      toast.error('Failed to update profile');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => navigate('/supervisor/dashboard')}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Dashboard
          </button>
          <h1 className="text-3xl font-bold text-gray-900">Profile Settings</h1>
        </div>

        {/* Profile Card */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="flex items-center gap-4 mb-6">
            {supervisor?.photoUrl ? (
              <img
                src={supervisor.photoUrl}
                alt="Profile"
                className="w-20 h-20 rounded-full object-cover border-2 border-gray-200"
              />
            ) : (
              <div className="w-20 h-20 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold text-2xl">
                {supervisor?.firstName?.[0]}{supervisor?.lastName?.[0]}
              </div>
            )}
            <div>
              <h2 className="text-xl font-semibold text-gray-900">
                {supervisor?.firstName} {supervisor?.lastName}
              </h2>
              <div className="flex items-center gap-2 mt-1">
                <SupervisorStatusBadge status={supervisor?.supervisorStatus} />
                <AuthorityLevelBadge level={supervisor?.authorityLevel} />
              </div>
            </div>
          </div>

          {/* Edit Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* First Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  First Name
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="text"
                    {...register('firstName')}
                    className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                      errors.firstName ? 'border-red-300' : 'border-gray-300'
                    }`}
                  />
                </div>
                {errors.firstName && (
                  <p className="mt-1 text-xs text-red-600">{errors.firstName.message}</p>
                )}
              </div>

              {/* Last Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Last Name
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="text"
                    {...register('lastName')}
                    className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                      errors.lastName ? 'border-red-300' : 'border-gray-300'
                    }`}
                  />
                </div>
                {errors.lastName && (
                  <p className="mt-1 text-xs text-red-600">{errors.lastName.message}</p>
                )}
              </div>

              {/* Email */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Mail className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="email"
                    {...register('email')}
                    className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                      errors.email ? 'border-red-300' : 'border-gray-300'
                    }`}
                  />
                </div>
                {errors.email && (
                  <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>
                )}
              </div>

              {/* Phone */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Phone
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Phone className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    type="tel"
                    {...register('phone')}
                    className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                      errors.phone ? 'border-red-300' : 'border-gray-300'
                    }`}
                    maxLength={10}
                  />
                </div>
                {errors.phone && (
                  <p className="mt-1 text-xs text-red-600">{errors.phone.message}</p>
                )}
              </div>
            </div>

            {/* Address */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Address
              </label>
              <div className="relative">
                <div className="absolute top-3 left-3 pointer-events-none">
                  <MapPin className="h-5 w-5 text-gray-400" />
                </div>
                <textarea
                  {...register('address')}
                  rows={3}
                  className={`block w-full pl-10 pr-3 py-2 border rounded-lg ${
                    errors.address ? 'border-red-300' : 'border-gray-300'
                  }`}
                />
              </div>
              {errors.address && (
                <p className="mt-1 text-xs text-red-600">{errors.address.message}</p>
              )}
            </div>

            {/* Emergency Contact */}
            <div className="border-t border-gray-200 pt-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Emergency Contact
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Contact Name
                  </label>
                  <input
                    type="text"
                    {...register('emergencyContactName')}
                    className="block w-full px-3 py-2 border border-gray-300 rounded-lg"
                    placeholder="Optional"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Contact Phone
                  </label>
                  <input
                    type="tel"
                    {...register('emergencyContactPhone')}
                    className="block w-full px-3 py-2 border border-gray-300 rounded-lg"
                    placeholder="Optional"
                    maxLength={10}
                  />
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={() => navigate('/supervisor/dashboard')}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={saving}
                className="flex-1 px-4 py-2 border border-transparent rounded-lg text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 flex items-center justify-center gap-2"
              >
                <Save className="w-4 h-4" />
                {saving ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default SupervisorProfile;
