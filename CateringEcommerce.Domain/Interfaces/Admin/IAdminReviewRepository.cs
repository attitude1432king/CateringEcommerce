using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminReviewRepository
    {
        AdminReviewListResponse GetAllReviews(AdminReviewListRequest request);
        AdminReviewDetail? GetReviewById(long reviewId);
        bool UpdateReviewVisibility(AdminReviewHideRequest request);
        bool DeleteReview(long reviewId, long deletedBy);
    }
}
