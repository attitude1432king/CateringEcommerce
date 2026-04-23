using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models.Delivery;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Common
{
    /// <summary>
    /// Repository for Event Catering Delivery (Status-based, NO GPS)
    /// </summary>
    public class EventDeliveryRepository
    {
        private readonly IDatabaseHelper _dbHelper;
        public EventDeliveryRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ===================================
        // CREATE EVENT DELIVERY
        // ===================================
        public async Task<long> CreateEventDeliveryAsync(InitEventDeliveryRequest request)
        {
            try
            {
                string query = $@"
                    INSERT INTO {Table.SysEventDelivery}
                    (c_orderid, c_ownerid, c_vehicle_number, c_driver_name, c_driver_phone,
                     c_delivery_status, c_scheduled_dispatch_time, c_createddate)
                    VALUES
                    (@OrderId, @OwnerId, @VehicleNumber, @DriverName, @DriverPhone,
                     @DeliveryStatus, @ScheduledDispatchTime, NOW())
                    RETURNING c_event_delivery_id;
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", request.OrderId),
                    new NpgsqlParameter("@OwnerId", request.OwnerId),
                    new NpgsqlParameter("@VehicleNumber", (object?)request.VehicleNumber ?? DBNull.Value),
                    new NpgsqlParameter("@DriverName", (object?)request.DriverName ?? DBNull.Value),
                    new NpgsqlParameter("@DriverPhone", (object?)request.DriverPhone ?? DBNull.Value),
                    new NpgsqlParameter("@DeliveryStatus", (int)EventDeliveryStatus.PreparationStarted),
                    new NpgsqlParameter("@ScheduledDispatchTime", (object?)request.ScheduledDispatchTime ?? DBNull.Value)
                };

                var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating event delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET EVENT DELIVERY BY ID
        // ===================================
        public async Task<EventDeliveryDto?> GetEventDeliveryByIdAsync(long eventDeliveryId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_event_delivery_id, c_orderid, c_ownerid,
                        c_vehicle_number, c_driver_name, c_driver_phone,
                        c_delivery_status, c_scheduled_dispatch_time,
                        c_actual_dispatch_time, c_arrived_time, c_completed_time,
                        c_notes, c_createddate, c_modifieddate
                    FROM {Table.SysEventDelivery}
                    WHERE c_event_delivery_id = @EventDeliveryId
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@EventDeliveryId", eventDeliveryId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapToDto(dt.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving event delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET EVENT DELIVERY BY ORDER ID
        // ===================================
        public async Task<EventDeliveryDto?> GetEventDeliveryByOrderIdAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_event_delivery_id, c_orderid, c_ownerid,
                        c_vehicle_number, c_driver_name, c_driver_phone,
                        c_delivery_status, c_scheduled_dispatch_time,
                        c_actual_dispatch_time, c_arrived_time, c_completed_time,
                        c_notes, c_createddate, c_modifieddate
                    FROM {Table.SysEventDelivery}
                    WHERE c_orderid = @OrderId
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapToDto(dt.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving event delivery by order: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE DELIVERY STATUS
        // ===================================
        public async Task<bool> UpdateDeliveryStatusAsync(long eventDeliveryId, UpdateEventDeliveryStatusRequest request)
        {
            try
            {
                // Build dynamic update based on new status
                string timestampUpdate = request.NewStatus switch
                {
                    EventDeliveryStatus.Dispatched => "c_actual_dispatch_time = NOW(),",
                    EventDeliveryStatus.ArrivedAtVenue => "c_arrived_time = NOW(),",
                    EventDeliveryStatus.EventCompleted => "c_completed_time = NOW(),",
                    _ => ""
                };

                string query = $@"
                    UPDATE {Table.SysEventDelivery}
                    SET
                        c_delivery_status = @NewStatus,
                        {timestampUpdate}
                        c_vehicle_number = COALESCE(@VehicleNumber, c_vehicle_number),
                        c_driver_name = COALESCE(@DriverName, c_driver_name),
                        c_driver_phone = COALESCE(@DriverPhone, c_driver_phone),
                        c_notes = COALESCE(@Notes, c_notes),
                        c_modifieddate = NOW()
                    WHERE c_event_delivery_id = @EventDeliveryId
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@EventDeliveryId", eventDeliveryId),
                    new NpgsqlParameter("@NewStatus", (int)request.NewStatus),
                    new NpgsqlParameter("@VehicleNumber", (object?)request.VehicleNumber ?? DBNull.Value),
                    new NpgsqlParameter("@DriverName", (object?)request.DriverName ?? DBNull.Value),
                    new NpgsqlParameter("@DriverPhone", (object?)request.DriverPhone ?? DBNull.Value),
                    new NpgsqlParameter("@Notes", (object?)request.Notes ?? DBNull.Value)
                };

                int rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating delivery status: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ACTIVE DELIVERIES BY OWNER
        // ===================================
        public async Task<List<EventDeliveryDto>> GetActiveDeliveriesByOwnerAsync(long ownerId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_event_delivery_id, c_orderid, c_ownerid,
                        c_vehicle_number, c_driver_name, c_driver_phone,
                        c_delivery_status, c_scheduled_dispatch_time,
                        c_actual_dispatch_time, c_arrived_time, c_completed_time,
                        c_notes, c_createddate, c_modifieddate
                    FROM {Table.SysEventDelivery}
                    WHERE c_ownerid = @OwnerId
                      AND c_delivery_status < @CompletedStatus
                    ORDER BY c_scheduled_dispatch_time ASC, c_createddate ASC
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OwnerId", ownerId),
                    new NpgsqlParameter("@CompletedStatus", (int)EventDeliveryStatus.EventCompleted)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                List<EventDeliveryDto> deliveries = new List<EventDeliveryDto>();

                foreach (DataRow row in dt.Rows)
                {
                    deliveries.Add(MapToDto(row));
                }

                return deliveries;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active deliveries: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ALL DELIVERIES FOR ADMIN MONITORING
        // ===================================
        public async Task<List<AdminDeliveryMonitorDto>> GetAllDeliveriesForMonitoringAsync()
        {
            try
            {
                string query = $@"
                    SELECT
                        ed.c_orderid,
                        ed.c_ownerid,
                        co.c_business_name as OwnerBusinessName,
                        ed.c_delivery_status,
                        ed.c_scheduled_dispatch_time,
                        ed.c_actual_dispatch_time,
                        ed.c_vehicle_number,
                        ed.c_driver_name,
                        o.c_event_date as EventDate
                    FROM {Table.SysEventDelivery} ed
                    INNER JOIN {Table.SysCateringOwner} co ON ed.c_ownerid = co.c_ownerid
                    LEFT JOIN {Table.SysOrders} o ON ed.c_orderid = o.c_orderid
                    WHERE ed.c_delivery_status < @CompletedStatus
                    ORDER BY ed.c_scheduled_dispatch_time ASC
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@CompletedStatus", (int)EventDeliveryStatus.EventCompleted)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                List<AdminDeliveryMonitorDto> monitors = new List<AdminDeliveryMonitorDto>();

                foreach (DataRow row in dt.Rows)
                {
                    var scheduledTime = row["c_scheduled_dispatch_time"] != DBNull.Value
                        ? Convert.ToDateTime(row["c_scheduled_dispatch_time"])
                        : (DateTime?)null;

                    var actualTime = row["c_actual_dispatch_time"] != DBNull.Value
                        ? Convert.ToDateTime(row["c_actual_dispatch_time"])
                        : (DateTime?)null;

                    var currentStatus = (EventDeliveryStatus)Convert.ToInt32(row["c_delivery_status"]);

                    // Calculate delay
                    bool isDelayed = false;
                    int? delayMinutes = null;

                    if (scheduledTime.HasValue && currentStatus < EventDeliveryStatus.Dispatched && DateTime.Now > scheduledTime.Value)
                    {
                        isDelayed = true;
                        delayMinutes = (int)(DateTime.Now - scheduledTime.Value).TotalMinutes;
                    }

                    monitors.Add(new AdminDeliveryMonitorDto
                    {
                        OrderId = Convert.ToInt64(row["c_orderid"]),
                        OwnerId = Convert.ToInt64(row["c_ownerid"]),
                        OwnerBusinessName = row["OwnerBusinessName"]?.ToString(),
                        CurrentStatus = currentStatus,
                        ScheduledDispatchTime = scheduledTime,
                        ActualDispatchTime = actualTime,
                        IsDelayed = isDelayed,
                        DelayMinutes = delayMinutes,
                        EventDate = row["EventDate"] != DBNull.Value ? Convert.ToDateTime(row["EventDate"]) : DateTime.Now,
                        VehicleNumber = row["c_vehicle_number"]?.ToString(),
                        DriverName = row["c_driver_name"]?.ToString()
                    });
                }

                return monitors;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving admin delivery monitoring: " + ex.Message, ex);
            }
        }

        // ===================================
        // ADD STATUS HISTORY
        // ===================================
        public async Task<long> AddStatusHistoryAsync(EventDeliveryHistoryDto history)
        {
            try
            {
                string query = $@"
                    INSERT INTO {Table.SysEventDeliveryHistory}
                    (c_event_delivery_id, c_orderid, c_previous_status, c_new_status,
                     c_changed_by_userid, c_changed_by_type, c_notes, c_changed_at)
                    VALUES
                    (@EventDeliveryId, @OrderId, @PreviousStatus, @NewStatus,
                     @ChangedByUserId, @ChangedByType, @Notes, NOW())
                    RETURNING c_event_delivery_id;
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@EventDeliveryId", history.EventDeliveryId),
                    new NpgsqlParameter("@OrderId", history.OrderId),
                    new NpgsqlParameter("@PreviousStatus", history.PreviousStatus.HasValue ? (int)history.PreviousStatus.Value : DBNull.Value),
                    new NpgsqlParameter("@NewStatus", (int)history.NewStatus),
                    new NpgsqlParameter("@ChangedByUserId", (object?)history.ChangedByUserId ?? DBNull.Value),
                    new NpgsqlParameter("@ChangedByType", (object?)history.ChangedByType ?? DBNull.Value),
                    new NpgsqlParameter("@Notes", (object?)history.Notes ?? DBNull.Value)
                };

                var result = await _dbHelper.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt64(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding status history: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET STATUS HISTORY
        // ===================================
        public async Task<List<EventDeliveryHistoryDto>> GetStatusHistoryAsync(long eventDeliveryId)
        {
            try
            {
                string query = $@"
                    SELECT
                        c_history_id, c_event_delivery_id, c_orderid,
                        c_previous_status, c_new_status, c_changed_by_userid,
                        c_changed_by_type, c_notes, c_changed_at
                    FROM {Table.SysEventDeliveryHistory}
                    WHERE c_event_delivery_id = @EventDeliveryId
                    ORDER BY c_changed_at ASC
                ";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@EventDeliveryId", eventDeliveryId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
                List<EventDeliveryHistoryDto> history = new List<EventDeliveryHistoryDto>();

                foreach (DataRow row in dt.Rows)
                {
                    history.Add(new EventDeliveryHistoryDto
                    {
                        HistoryId = Convert.ToInt64(row["c_history_id"]),
                        EventDeliveryId = Convert.ToInt64(row["c_event_delivery_id"]),
                        OrderId = Convert.ToInt64(row["c_orderid"]),
                        PreviousStatus = row["c_previous_status"] != DBNull.Value
                            ? (EventDeliveryStatus)Convert.ToInt32(row["c_previous_status"])
                            : null,
                        NewStatus = (EventDeliveryStatus)Convert.ToInt32(row["c_new_status"]),
                        ChangedByUserId = row["c_changed_by_userid"] != DBNull.Value
                            ? Convert.ToInt64(row["c_changed_by_userid"])
                            : null,
                        ChangedByType = row["c_changed_by_type"]?.ToString(),
                        Notes = row["c_notes"]?.ToString(),
                        ChangedAt = Convert.ToDateTime(row["c_changed_at"])
                    });
                }

                return history;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving status history: " + ex.Message, ex);
            }
        }

        // ===================================
        // HELPER: MAP TO DTO
        // ===================================
        private EventDeliveryDto MapToDto(DataRow row)
        {
            return new EventDeliveryDto
            {
                EventDeliveryId = Convert.ToInt64(row["c_event_delivery_id"]),
                OrderId = Convert.ToInt64(row["c_orderid"]),
                OwnerId = Convert.ToInt64(row["c_ownerid"]),
                VehicleNumber = row["c_vehicle_number"] != DBNull.Value ? row["c_vehicle_number"].ToString() : null,
                DriverName = row["c_driver_name"] != DBNull.Value ? row["c_driver_name"].ToString() : null,
                DriverPhone = row["c_driver_phone"] != DBNull.Value ? row["c_driver_phone"].ToString() : null,
                DeliveryStatus = (EventDeliveryStatus)Convert.ToInt32(row["c_delivery_status"]),
                ScheduledDispatchTime = row["c_scheduled_dispatch_time"] != DBNull.Value
                    ? Convert.ToDateTime(row["c_scheduled_dispatch_time"])
                    : null,
                ActualDispatchTime = row["c_actual_dispatch_time"] != DBNull.Value
                    ? Convert.ToDateTime(row["c_actual_dispatch_time"])
                    : null,
                ArrivedTime = row["c_arrived_time"] != DBNull.Value
                    ? Convert.ToDateTime(row["c_arrived_time"])
                    : null,
                CompletedTime = row["c_completed_time"] != DBNull.Value
                    ? Convert.ToDateTime(row["c_completed_time"])
                    : null,
                Notes = row["c_notes"] != DBNull.Value ? row["c_notes"].ToString() : null,
                CreatedAt = Convert.ToDateTime(row["c_createddate"]),
                UpdatedAt = row["c_modifieddate"] != DBNull.Value ? Convert.ToDateTime(row["c_modifieddate"]) : null
            };
        }
    }
}

