using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminCateringRepository
    {
        AdminCateringListResponse GetAllCaterings(AdminCateringListRequest request);
        AdminCateringDetail? GetCateringById(long cateringId);
        bool UpdateCateringStatus(AdminCateringStatusUpdate request);
        bool DeleteCatering(long cateringId, long deletedBy);
    }
}
