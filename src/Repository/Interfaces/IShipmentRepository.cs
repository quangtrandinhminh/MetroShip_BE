﻿using MetroShip.Repository.Base;
using MetroShip.Repository.Extensions;
using MetroShip.Repository.Models;
using static MetroShip.Repository.Repositories.ShipmentRepository;
using System.Linq.Expressions;
using MetroShip.Utility.Enums;

namespace MetroShip.Repository.Interfaces;

public interface IShipmentRepository : IBaseRepository<Shipment>
{
    int GetQuantityByBookedAtAndRegion(DateTimeOffset bookAtDate, string regionCode);
    Task<PaginatedList<Shipment>> GetPaginatedListForListResponseAsync(int pageNumber,
        int pageSize,
        Expression<Func<Shipment, bool>> predicate = null,
        Expression<Func<Shipment, object>> orderBy = null, bool? isDesc = false);

    Task<Shipment?> GetShipmentByTrackingCodeAsync(string trackingCode);

    /*Task<PaginatedList<ShipmentDto>> GetShipmentsByLineIdAndDateAsync(
        int pageNumber, int pageSize, string lineId, DateTimeOffset date, string? regionId, ShiftEnum? shift);*/

    Task<List<AvailableTimeSlotDto>> FindAvailableTimeSlotsAsync(CheckAvailableTimeSlotsRequest request);
    Task<ShipmentItinerary?> GetItineraryByShipmentIdAsync(string shipmentId);
    Task UpdateShipmentStatusAsync(string shipmentId, ShipmentStatusEnum status);
    Task AddParcelTrackingAsync(string parcelId, string status, string stationId, string updatedBy);
    Task<string> GetStationNameByIdAsync(string stationId);
}