using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.BAL.Base.User
{
    /// <summary>
    /// Service for user-side home page and catering browsing operations.
    /// Handles business logic for searching, browsing, and viewing catering businesses.
    /// </summary>
    public class HomeService : IHomeService
    {
    
        private readonly SqlDatabaseManager _db;


        public HomeService(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }


        /// <summary>
        /// Gets verified catering businesses by city ID.
        /// If cityId is less than or equal to 0, returns all verified catering businesses.
        /// </summary>
        /// <param name="cityId">The city ID to filter by. Pass 0 or negative value to get all.</param>
        /// <returns>List of verified catering businesses</returns>
        public async Task<List<CateringBusinessListDto>> GetVerifiedCateringListAsync(string cityName)
        {
            try
            {
                List<CateringBusinessListDto> cateringList;
                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                Locations locations = new Locations(_db.GetConnectionString());

                int cityId = await locations.GetCityID(cityName);

                if (cityId > 0)
                {
                    cateringList = await cateringRepository.GetVerifiedCateringsForBrowseInternalAsync(cityId);
                }
                else
                {
                    cateringList = await cateringRepository.GetVerifiedCateringsForBrowseInternalAsync();
                }

                return cateringList ?? new List<CateringBusinessListDto>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching catering businesses.", ex);
            }
        }

        /// <summary>
        /// Gets detailed catering profile for user browsing/viewing.
        /// Returns complete information including contact details, address, services, and ratings.
        /// </summary>
        /// <param name="cateringId">The catering/owner ID</param>
        /// <returns>Detailed catering profile</returns>
        public async Task<CateringDetailDto> GetCateringDetailForBrowsingAsync(long cateringId)
        {
            try
            {

                if (cateringId <= 0)
                {
                    throw new ArgumentException("Invalid catering ID.", nameof(cateringId));
                }

                CateringBrowseRepository cateringRepository = new CateringBrowseRepository(_db.GetConnectionString());
                var cateringDetail = await cateringRepository.GetCateringDetailForUserBrowseAsync(cateringId);

                if (cateringDetail == null)
                {
                    throw new KeyNotFoundException($"Catering with ID {cateringId} not found or not verified.");
                }

                return cateringDetail;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching catering details.", ex);
            }
        }
    }
}
