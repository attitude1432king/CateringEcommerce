import { useState, useEffect, useRef } from 'react';
import {
  Search, User, ChevronDown, X, Clock,
  Building2, ShoppingBag, Shield, DollarSign, ArrowRight
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAdminAuth } from '../../../contexts/AdminAuthContext';
import { apiCall } from '../../../services/apiUtils';
import { searchApi } from '../../../services/adminApi';
import NotificationBell from '../notifications/NotificationBell';

// ─── helpers ──────────────────────────────────────────────────────────────────

const STATUS_BADGE = {
  green:  'bg-green-100 text-green-800',
  red:    'bg-red-100 text-red-800',
  yellow: 'bg-yellow-100 text-yellow-800',
  blue:   'bg-blue-100 text-blue-800',
  gray:   'bg-gray-100 text-gray-600',
};

const MODULE_ICON = {
  Customer:   User,
  Partner:    Building2,
  Order:      ShoppingBag,
  Supervisor: Shield,
  Earnings:   DollarSign,
};

function highlight(text, query) {
  if (!query || !text) return text;
  const escaped = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const regex = new RegExp(`(${escaped})`, 'gi');
  const parts = text.split(regex);
  return parts.map((part, i) =>
    regex.test(part)
      ? <mark key={i} className="bg-yellow-100 text-yellow-800 font-semibold rounded px-0.5">{part}</mark>
      : part
  );
}

function groupBy(items, key) {
  return items.reduce((acc, item) => {
    const k = item[key];
    if (!acc[k]) acc[k] = [];
    acc[k].push(item);
    return acc;
  }, {});
}

const RECENT_KEY = 'admin_recent_searches';

// ─── component ────────────────────────────────────────────────────────────────

const AdminHeader = () => {
  const { admin, logout } = useAdminAuth();
  const navigate = useNavigate();

  // search state
  const [searchQuery, setSearchQuery]         = useState('');
  const [results, setResults]                 = useState(null);
  const [isSearching, setIsSearching]         = useState(false);
  const [showDropdown, setShowDropdown]       = useState(false);
  const [selectedIndex, setSelectedIndex]     = useState(-1);

  // rbac / placeholder
  const [permissions, setPermissions]         = useState([]);
  const [isSuperAdmin, setIsSuperAdmin]       = useState(false);

  // recent searches
  const [recentSearches, setRecentSearches]   = useState([]);

  // profile menu
  const [showProfileMenu, setShowProfileMenu] = useState(false);

  const debounceRef  = useRef(null);
  const containerRef = useRef(null);

  // ── load permissions once ──────────────────────────────────────────────────
  useEffect(() => {
    apiCall('/admin/auth/permissions')
      .then(res => {
        if (res?.result && res.data) {
          const codes = (res.data.permissions || []).map(p => p.permissionCode);
          setPermissions(codes);
          setIsSuperAdmin(res.data.isSuperAdmin || false);
        }
      })
      .catch(() => {});
  }, []);

  // ── load recent searches ───────────────────────────────────────────────────
  useEffect(() => {
    try {
      const stored = localStorage.getItem(RECENT_KEY);
      if (stored) setRecentSearches(JSON.parse(stored));
    } catch {}
  }, []);

  // ── click-outside ──────────────────────────────────────────────────────────
  useEffect(() => {
    const handler = e => {
      if (containerRef.current && !containerRef.current.contains(e.target)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  // ── helpers ────────────────────────────────────────────────────────────────
  const saveRecent = query => {
    const updated = [query, ...recentSearches.filter(r => r !== query)].slice(0, 5);
    setRecentSearches(updated);
    try { localStorage.setItem(RECENT_KEY, JSON.stringify(updated)); } catch {}
  };

  const closeSearch = () => {
    setShowDropdown(false);
    setSearchQuery('');
    setResults(null);
    setSelectedIndex(-1);
  };

  const doSearch = async query => {
    setIsSearching(true);
    setShowDropdown(true);
    try {
      const res = await searchApi.globalSearch(query);
      if (res?.result) {
        setResults(res.data);
        saveRecent(query);
      }
    } catch {
      setResults({ results: [], totalCount: 0, query });
    } finally {
      setIsSearching(false);
    }
  };

  const navigateTo = item => {
    navigate(item.viewUrl);
    closeSearch();
  };

  // ── placeholder logic ──────────────────────────────────────────────────────
  const buildPlaceholder = () => {
    if (isSuperAdmin) return 'Search users, partners, orders, supervisors...';
    const parts = [];
    if (permissions.includes('USER_VIEW'))       parts.push('users');
    if (permissions.includes('CATERING_VIEW'))   parts.push('partners');
    if (permissions.includes('ORDER_VIEW'))      parts.push('orders');
    if (permissions.includes('SUPERVISOR_VIEW')) parts.push('supervisors');
    if (permissions.includes('EARNINGS_VIEW'))   parts.push('earnings');
    return parts.length ? `Search ${parts.join(', ')}...` : 'Search...';
  };

  // ── event handlers ─────────────────────────────────────────────────────────
  const handleChange = e => {
    const val = e.target.value;
    setSearchQuery(val);
    setSelectedIndex(-1);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (val.trim().length < 2) {
      setResults(null);
      setShowDropdown(val.length > 0 || recentSearches.length > 0);
      return;
    }
    debounceRef.current = setTimeout(() => doSearch(val.trim()), 300);
  };

  const handleFocus = () => {
    if (searchQuery.trim().length >= 2 || recentSearches.length > 0) {
      setShowDropdown(true);
    }
  };

  const handleKeyDown = e => {
    const items = results?.results || [];
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setSelectedIndex(i => Math.min(i + 1, items.length - 1));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setSelectedIndex(i => Math.max(i - 1, -1));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (selectedIndex >= 0 && items[selectedIndex]) {
        navigateTo(items[selectedIndex]);
      } else if (searchQuery.trim().length >= 2) {
        doSearch(searchQuery.trim());
      }
    } else if (e.key === 'Escape') {
      setShowDropdown(false);
      setSelectedIndex(-1);
    }
  };

  // ── render: flat list index for keyboard nav ───────────────────────────────
  const flatItems = results?.results || [];

  // ── render: grouped results ────────────────────────────────────────────────
  const grouped = results ? groupBy(flatItems, 'moduleLabel') : {};

  return (
    <header className="bg-white border-b border-gray-200 h-16 fixed top-0 right-0 left-64 z-40 transition-all duration-300">
      <div className="h-full px-6 flex items-center justify-between">

        {/* ── Search Section ── */}
        <div ref={containerRef} className="flex-1 max-w-2xl relative">

          {/* Input */}
          <div className="relative">
            {isSearching ? (
              <div className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin" />
            ) : (
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            )}

            <input
              type="text"
              value={searchQuery}
              onChange={handleChange}
              onKeyDown={handleKeyDown}
              onFocus={handleFocus}
              placeholder={buildPlaceholder()}
              autoComplete="off"
              className="w-full pl-10 pr-8 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm"
            />

            {searchQuery && (
              <button
                onClick={closeSearch}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                aria-label="Clear search"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>

          {/* ── Dropdown ── */}
          {showDropdown && (
            <div className="absolute top-full left-0 right-0 mt-1 bg-white rounded-lg shadow-xl border border-gray-200 z-50 max-h-[480px] overflow-y-auto">

              {/* Recent searches — shown when query < 2 chars */}
              {searchQuery.trim().length < 2 && recentSearches.length > 0 && (
                <div className="p-3">
                  <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">Recent Searches</p>
                  <div className="flex flex-wrap gap-2">
                    {recentSearches.map(term => (
                      <button
                        key={term}
                        onClick={() => { setSearchQuery(term); doSearch(term); }}
                        className="flex items-center gap-1 px-3 py-1 bg-gray-100 hover:bg-indigo-50 hover:text-indigo-700 rounded-full text-xs text-gray-600 transition-colors"
                      >
                        <Clock className="w-3 h-3" />
                        {term}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {/* Loading skeleton */}
              {isSearching && (
                <div className="p-3 space-y-3">
                  {[1, 2, 3].map(i => (
                    <div key={i} className="flex items-center gap-3 animate-pulse">
                      <div className="w-8 h-8 bg-gray-200 rounded-full flex-shrink-0" />
                      <div className="flex-1 space-y-1.5">
                        <div className="h-3 bg-gray-200 rounded w-1/3" />
                        <div className="h-2.5 bg-gray-100 rounded w-2/3" />
                      </div>
                      <div className="h-5 w-14 bg-gray-100 rounded-full" />
                    </div>
                  ))}
                </div>
              )}

              {/* Results grouped by module */}
              {!isSearching && results && flatItems.length > 0 && (
                <div className="py-1">
                  {Object.entries(grouped).map(([label, items]) => {
                    const Icon = MODULE_ICON[label] || Search;
                    return (
                      <div key={label}>
                        {/* Group header */}
                        <div className="flex items-center gap-2 px-4 py-2 bg-gray-50 border-b border-gray-100">
                          <Icon className="w-3.5 h-3.5 text-gray-400" />
                          <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">{label}</span>
                          <span className="ml-auto text-xs text-gray-400 bg-gray-200 rounded-full px-2 py-0.5">{items.length}</span>
                        </div>

                        {/* Result rows */}
                        {items.map(item => {
                          const globalIdx = flatItems.indexOf(item);
                          const isSelected = globalIdx === selectedIndex;
                          const badgeCls = STATUS_BADGE[item.statusColor] || STATUS_BADGE.gray;

                          return (
                            <button
                              key={`${item.type}-${item.id}`}
                              onClick={() => navigateTo(item)}
                              onMouseEnter={() => setSelectedIndex(globalIdx)}
                              className={`w-full flex items-center gap-3 px-4 py-2.5 text-left transition-colors ${
                                isSelected
                                  ? 'bg-indigo-50 border-l-2 border-indigo-500'
                                  : 'hover:bg-gray-50 border-l-2 border-transparent'
                              }`}
                            >
                              {/* Icon */}
                              <div className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 ${
                                isSelected ? 'bg-indigo-100' : 'bg-gray-100'
                              }`}>
                                <Icon className={`w-4 h-4 ${isSelected ? 'text-indigo-600' : 'text-gray-500'}`} />
                              </div>

                              {/* Text */}
                              <div className="flex-1 min-w-0">
                                <p className="text-sm font-medium text-gray-900 truncate">
                                  {highlight(item.title, searchQuery.trim())}
                                </p>
                                <p className="text-xs text-gray-500 truncate">
                                  {highlight(item.subtitle, searchQuery.trim())}
                                </p>
                              </div>

                              {/* Status badge */}
                              <span className={`flex-shrink-0 text-xs font-medium px-2 py-0.5 rounded-full ${badgeCls}`}>
                                {item.status}
                              </span>

                              {/* View arrow */}
                              <ArrowRight className={`w-4 h-4 flex-shrink-0 ${
                                isSelected ? 'text-indigo-500' : 'text-gray-300'
                              }`} />
                            </button>
                          );
                        })}
                      </div>
                    );
                  })}
                </div>
              )}

              {/* Empty state */}
              {!isSearching && results && flatItems.length === 0 && (
                <div className="flex flex-col items-center justify-center py-10 text-gray-400">
                  <Search className="w-8 h-8 mb-2 opacity-40" />
                  <p className="text-sm font-medium">No results for "{results.query}"</p>
                  <p className="text-xs mt-1">Try a different name, email, or ID</p>
                </div>
              )}
            </div>
          )}
        </div>

        {/* ── Right Section ── */}
        <div className="flex items-center space-x-4 ml-6">

          {/* Notifications */}
          <NotificationBell />

          {/* Admin Profile */}
          <div className="relative">
            <button
              onClick={() => setShowProfileMenu(!showProfileMenu)}
              className="flex items-center space-x-3 p-2 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <div className="w-8 h-8 bg-indigo-600 rounded-full flex items-center justify-center">
                <User className="w-5 h-5 text-white" />
              </div>
              <div className="hidden md:block text-left">
                <p className="text-sm font-medium text-gray-900">
                  {admin?.fullName || 'Admin'}
                </p>
                <p className="text-xs text-gray-500">{admin?.role}</p>
              </div>
              <ChevronDown className="w-4 h-4 text-gray-500" />
            </button>

            {/* Profile Dropdown */}
            {showProfileMenu && (
              <>
                <div
                  className="fixed inset-0 z-10"
                  onClick={() => setShowProfileMenu(false)}
                />
                <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-lg border border-gray-200 py-2 z-20">
                  <div className="px-4 py-3 border-b border-gray-100">
                    <p className="text-sm font-medium text-gray-900">{admin?.fullName}</p>
                    <p className="text-xs text-gray-500 mt-1">{admin?.email}</p>
                  </div>
                  <button
                    onClick={() => setShowProfileMenu(false)}
                    className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
                  >
                    Your Profile
                  </button>
                  <button
                    onClick={() => setShowProfileMenu(false)}
                    className="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
                  >
                    Settings
                  </button>
                  <div className="border-t border-gray-100 mt-2 pt-2">
                    <button
                      onClick={() => { setShowProfileMenu(false); logout(); }}
                      className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition-colors"
                    >
                      Logout
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>

        </div>
      </div>
    </header>
  );
};

export default AdminHeader;
