using System.ComponentModel;

namespace MetroShip.Utility.Enums;

public enum ShipmentStatusEnum
{
    // Initial booking
    AwaitingPayment,             // all parcels accepted

    // Pre-confirmation outcomes
    Rejected,             // all parcels rejected by staff

    // Payment flows
    Unpaid,               // payment timeout
    Cancelled,            // customer-initiated cancellation
    AwaitingRefund,       // cancelled before drop-off
    Refunded,             // refund processed
    NoDropOff,            // paid but never dropped off

    // In-transit flows
    AwaitingDropOff,                 // payment received
    PickedUp,             // parcels scanned at origin station
    LoadOnMetro,
    InTransit,            // en-route on metro
    AwaitingDelivery,  // all parcels at delivery station
    ApplyingSurcharge,    // parcels overdue → surcharge period
    UnloadingAtStation,         // Being unloaded at a transit station
    StorageInWarehouse,
    WaitingForNextTrain,        // Waiting for the next connecting train
    TransferringToNextTrain,    // Being loaded onto the next train
    Expired,              // long-term storage after surcharge window

    // Return flows
    ToReturn,            // parcels returned to origin station at delivery station
    Returning,          // parcels in return transit
    Returned,           // parcels returned to origin station

    // Completion
    Delivered,     // delivered, waiting for rating
    Completed,             // feedback received, shipment closed

    // Unhappy case
    Delayed,              // parcels delayed at any stage
    Arrived,
    CompletedWithCompensation, // parcels delivered with compensation
    ToCompensate,
    Compensated,          // compensation processed
}