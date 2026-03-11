import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Heart, Trash2, MapPin, Star, Users, Loader2, Search, Filter } from 'lucide-react';
import { getFavorites, removeFromFavorites } from '../services/favoritesApi';
import { useAuth } from '../contexts/AuthContext';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:44368';

/**
 * WishlistPage - Display user's favorite caterings
 */
const WishlistPage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [removingId, setRemovingId] = useState(null);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 12;

  // Confirmation modal state
  const [removeConfirmation, setRemoveConfirmation] = useState({
    isOpen: false,
    cateringId: null,
    cateringName: ''
  });

  // Fetch favorites on mount and page change
  useEffect(() => {
    if (!user) {
      navigate('/');
      return;
    }
    fetchFavorites();
  }, [currentPage, user, navigate]);

  /**
   * Fetch favorites from API
   */
  const fetchFavorites = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await getFavorites(currentPage, pageSize);

      if (response.result) {
        setFavorites(response.data.favorites || []);
        setTotalPages(response.data.pagination.totalPages);
        setTotalCount(response.data.pagination.totalCount);
      } else {
        setError(response.message || 'Failed to load favorites');
      }
    } catch (err) {
      console.error('Error fetching favorites:', err);
      setError(err.message || 'An error occurred while loading favorites');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Show remove confirmation modal
   */
  const handleRemoveFavorite = (cateringId, cateringName) => {
    setRemoveConfirmation({
      isOpen: true,
      cateringId,
      cateringName
    });
  };

  /**
   * Confirm and remove a catering from favorites
   */
  const confirmRemoveFavorite = async () => {
    const { cateringId } = removeConfirmation;
    setRemovingId(cateringId);
    setRemoveConfirmation({ isOpen: false, cateringId: null, cateringName: '' });

    try {
      const response = await removeFromFavorites(cateringId);

      if (response.result) {
        // Remove from local state
        setFavorites(prev => prev.filter(f => f.cateringId !== cateringId));
        setTotalCount(prev => prev - 1);
      } else {
        alert(response.message || 'Failed to remove from wishlist');
      }
    } catch (err) {
      console.error('Error removing favorite:', err);
      alert(err.message || 'An error occurred while removing from wishlist');
    } finally {
      setRemovingId(null);
    }
  };

  /**
   * Navigate to catering detail page
   */
  const handleViewCatering = (cateringId) => {
    navigate(`/catering/${cateringId}`);
  };

  /**
   * Star rating display component
   */
  const StarRating = ({ rating, count }) => {
    if (!rating || rating === 0) {
      return <span className="text-sm text-gray-400">No reviews yet</span>;
    }

    return (
      <div className="flex items-center gap-1">
        <Star className="w-4 h-4 text-yellow-500 fill-yellow-500" />
        <span className="font-semibold text-gray-900">{rating.toFixed(1)}</span>
        <span className="text-sm text-gray-500">({count || 0})</span>
      </div>
    );
  };

  /**
   * Render loading state
   */
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-12 h-12 text-rose-500 animate-spin mx-auto mb-4" />
          <p className="text-gray-600">Loading your wishlist...</p>
        </div>
      </div>
    );
  }

  /**
   * Render error state
   */
  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 max-w-md text-center">
          <p className="text-red-800 mb-4">{error}</p>
          <button
            onClick={fetchFavorites}
            className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition"
          >
            Try Again
          </button>
        </div>
      </div>
    );
  }

  /**
   * Render empty state
   */
  if (favorites.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="text-center max-w-md">
          <Heart className="w-20 h-20 text-gray-300 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Your Wishlist is Empty</h2>
          <p className="text-gray-600 mb-6">
            Start adding caterings to your wishlist by clicking the heart icon on catering cards.
          </p>
          <button
            onClick={() => navigate('/browse')}
            className="px-6 py-3 bg-rose-500 text-white rounded-lg hover:bg-rose-600 transition"
          >
            Browse Caterings
          </button>
        </div>
      </div>
    );
  }

  /**
   * Main render - Grid of favorites
   */
  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center gap-3 mb-2">
            <Heart className="w-8 h-8 text-rose-500 fill-rose-500" />
            <h1 className="text-3xl font-bold text-gray-900">My Wishlist</h1>
          </div>
          <p className="text-gray-600">
            {totalCount} {totalCount === 1 ? 'catering' : 'caterings'} saved
          </p>
        </div>

        {/* Favorites Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
          {favorites.map((favorite) => (
            <div
              key={favorite.favoriteId}
              className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow"
            >
              {/* Catering Image */}
              <div className="relative h-48 bg-gray-200">
                {favorite.logoUrl ? (
                  <img
                    src={`${API_BASE_URL}${favorite.logoUrl}`}
                    alt={favorite.cateringName}
                    className="w-full h-full object-cover cursor-pointer"
                    onClick={() => handleViewCatering(favorite.cateringId)}
                    onError={(e) => {
                      e.target.src = 'https://via.placeholder.com/400x300?text=No+Image';
                    }}
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-gray-400">
                    <Users className="w-16 h-16" />
                  </div>
                )}

                {/* Remove button overlay */}
                <button
                  onClick={() => handleRemoveFavorite(favorite.cateringId, favorite.cateringName)}
                  disabled={removingId === favorite.cateringId}
                  className="absolute top-2 right-2 p-2 bg-white rounded-full shadow-md hover:bg-red-50 transition disabled:opacity-50"
                  title="Remove from wishlist"
                >
                  {removingId === favorite.cateringId ? (
                    <Loader2 className="w-5 h-5 text-red-500 animate-spin" />
                  ) : (
                    <Trash2 className="w-5 h-5 text-red-500" />
                  )}
                </button>

                {/* Verified Badge */}
                {favorite.isVerified && (
                  <div className="absolute top-2 left-2 px-2 py-1 bg-green-500 text-white text-xs font-semibold rounded">
                    Verified
                  </div>
                )}

                {/* Online Badge */}
                {favorite.isOnline && (
                  <div className="absolute bottom-2 left-2 px-2 py-1 bg-blue-500 text-white text-xs font-semibold rounded">
                    Online
                  </div>
                )}
              </div>

              {/* Catering Info */}
              <div className="p-4">
                <h3
                  className="text-lg font-bold text-gray-900 mb-2 cursor-pointer hover:text-rose-500 transition"
                  onClick={() => handleViewCatering(favorite.cateringId)}
                >
                  {favorite.cateringName}
                </h3>

                {/* Location */}
                {favorite.cityName && (
                  <div className="flex items-center gap-2 text-sm text-gray-600 mb-2">
                    <MapPin className="w-4 h-4" />
                    <span>{favorite.cityName}</span>
                  </div>
                )}

                {/* Rating */}
                <div className="mb-3">
                  <StarRating rating={favorite.averageRating} count={favorite.reviewCount} />
                </div>

                {/* Stats */}
                <div className="flex items-center justify-between text-sm text-gray-600 mb-3">
                  <span>{favorite.completedOrders || 0} orders completed</span>
                  {favorite.minOrderValue && (
                    <span className="font-semibold">Min: ₹{favorite.minOrderValue}</span>
                  )}
                </div>

                {/* Added date */}
                <p className="text-xs text-gray-400 mb-3">
                  Added on {new Date(favorite.addedDate).toLocaleDateString('en-IN', {
                    year: 'numeric',
                    month: 'short',
                    day: 'numeric'
                  })}
                </p>

                {/* View Details Button */}
                <button
                  onClick={() => handleViewCatering(favorite.cateringId)}
                  className="w-full py-2 bg-rose-500 text-white font-semibold rounded-lg hover:bg-rose-600 transition"
                >
                  View Details
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-center gap-2">
            <button
              onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
              disabled={currentPage === 1}
              className="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>

            <div className="flex items-center gap-2">
              {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
                <button
                  key={page}
                  onClick={() => setCurrentPage(page)}
                  className={`px-3 py-2 rounded-lg transition ${
                    currentPage === page
                      ? 'bg-rose-500 text-white'
                      : 'bg-white border border-gray-300 hover:bg-gray-50'
                  }`}
                >
                  {page}
                </button>
              ))}
            </div>

            <button
              onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
              disabled={currentPage === totalPages}
              className="px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
        )}
      </div>

      {/* Remove Confirmation Modal */}
      {removeConfirmation.isOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl max-w-sm w-full p-6 animate-fade-in">
            <div className="text-center mb-6">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-rose-100 mb-4">
                <Heart className="h-6 w-6 text-rose-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Remove from Wishlist?</h3>
              <p className="text-sm text-gray-600">
                Remove "{removeConfirmation.cateringName}" from your wishlist?
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setRemoveConfirmation({ isOpen: false, cateringId: null, cateringName: '' })}
                className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors font-medium"
              >
                Cancel
              </button>
              <button
                onClick={confirmRemoveFavorite}
                className="flex-1 px-4 py-2.5 bg-rose-600 text-white rounded-lg hover:bg-rose-700 transition-colors font-medium"
              >
                Remove
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default WishlistPage;
