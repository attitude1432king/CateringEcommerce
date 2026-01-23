using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Models.Delivery;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Common
{
    /// <summary>
    /// Repository for Sample Delivery (Third-party tracking)
    /// </summary>
    public class SampleDeliveryRepository
    {
        private readonly SqlDatabaseManager _db;

        public SampleDeliveryRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        // ===================================
        // CREATE SAMPLE DELIVERY
        // ===================================
        public async Task<long> CreateSampleDeliveryAsync(CreateSampleDeliveryRequest request)
        {
            try
            {
                string query = $@"
                    INSERT INTO {Table.SysSampleDelivery}
                    (c_orderid, c_userid, c_ownerid, c_provider, c_delivery_status, c_created_at)
                    VALUES
                    (@OrderId, @UserId, @OwnerId, @Provider, @DeliveryStatus, GETDATE());

                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", request.OrderId),
                    new SqlParameter("@UserId", request.UserId),
                    new SqlParameter("@OwnerId", request.OwnerId),
                    new SqlParameter("@Provider", request.Provider),
                    new SqlParameter("@DeliveryStatus", (int)SampleDeliveryStatus.Requested)
                };

                var result = await _db.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating sample delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET SAMPLE DELIVERY BY ID
        // ===================================
        public async Task<SampleDeliveryDto?> GetSampleDeliveryByIdAsync(long sampleDeliveryId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_sample_delivery_id, c_orderid, c_userid, c_ownerid,
                        c_provider, c_tracking_url, c_tracking_id, c_delivery_status,
                        c_created_at, c_updated_at
                    FROM {Table.SysSampleDelivery}
                    WHERE c_sample_delivery_id = @SampleDeliveryId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@SampleDeliveryId", sampleDeliveryId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapToDto(dt.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving sample delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET SAMPLE DELIVERY BY ORDER ID
        // ===================================
        public async Task<SampleDeliveryDto?> GetSampleDeliveryByOrderIdAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_sample_delivery_id, c_orderid, c_userid, c_ownerid,
                        c_provider, c_tracking_url, c_tracking_id, c_delivery_status,
                        c_created_at, c_updated_at
                    FROM {Table.SysSampleDelivery}
                    WHERE c_orderid = @OrderId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapToDto(dt.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving sample delivery by order: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE TRACKING INFO
        // ===================================
        public async Task<bool> UpdateTrackingInfoAsync(long sampleDeliveryId, string trackingUrl, string trackingId)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysSampleDelivery}
                    SET
                        c_tracking_url = @TrackingUrl,
                        c_tracking_id = @TrackingId,
                        c_updated_at = GETDATE()
                    WHERE c_sample_delivery_id = @SampleDeliveryId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@SampleDeliveryId", sampleDeliveryId),
                    new SqlParameter("@TrackingUrl", trackingUrl),
                    new SqlParameter("@TrackingId", trackingId)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating tracking info: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE DELIVERY STATUS
        // ===================================
        public async Task<bool> UpdateDeliveryStatusAsync(long sampleDeliveryId, SampleDeliveryStatus newStatus)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysSampleDelivery}
                    SET
                        c_delivery_status = @NewStatus,
                        c_updated_at = GETDATE()
                    WHERE c_sample_delivery_id = @SampleDeliveryId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@SampleDeliveryId", sampleDeliveryId),
                    new SqlParameter("@NewStatus", (int)newStatus)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating delivery status: " + ex.Message, ex);
            }
        }

        // ===================================
        // HELPER: MAP TO DTO
        // ===================================
        private SampleDeliveryDto MapToDto(DataRow row)
        {
            return new SampleDeliveryDto
            {
                SampleDeliveryId = Convert.ToInt64(row["c_sample_delivery_id"]),
                OrderId = Convert.ToInt64(row["c_orderid"]),
                UserId = Convert.ToInt64(row["c_userid"]),
                OwnerId = Convert.ToInt64(row["c_ownerid"]),
                Provider = row["c_provider"] != DBNull.Value ? row["c_provider"].ToString() : null,
                TrackingUrl = row["c_tracking_url"] != DBNull.Value ? row["c_tracking_url"].ToString() : null,
                TrackingId = row["c_tracking_id"] != DBNull.Value ? row["c_tracking_id"].ToString() : null,
                DeliveryStatus = (SampleDeliveryStatus)Convert.ToInt32(row["c_delivery_status"]),
                CreatedAt = Convert.ToDateTime(row["c_created_at"]),
                UpdatedAt = row["c_updated_at"] != DBNull.Value ? Convert.ToDateTime(row["c_updated_at"]) : null
            };
        }
    }
}
