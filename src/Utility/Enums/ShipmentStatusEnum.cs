using System.ComponentModel;

namespace MetroShip.Utility.Enums;

public enum ShipmentStatusEnum
{
    // Initial booking
    AwaitingPayment = 0,             // all parcels accepted

    // Pre-confirmation outcomes
    Rejected = 1,             // all parcels rejected by staff

    // Payment flows
    Unpaid = 2,               // payment timeout
    Cancelled = 3,            // customer-initiated cancellation
    AwaitingRefund = 4,       // cancelled before drop-off
    Refunded = 5,             // refund processed
    NoDropOff = 6,            // paid but never dropped off

    // In-transit flows
    AwaitingDropOff = 7,                 // payment received
    PickedUp = 8,             // parcels scanned at origin station
    InTransit = 9,            // en-route on metro
    AwaitingDelivery = 10,  // all parcels at delivery station
    ApplyingSurcharge = 11,    // parcels overdue → surcharge period
    //UnloadingAtStation,         // Being unloaded at a transit station
    //StorageInWarehouse,
    WaitingForNextTrain = 14,        // Waiting for the next connecting train
    //TransferringToNextTrain,    // Being loaded onto the next train
    Expired = 16,              // long-term storage after surcharge window

    // Return flows
    //ToReturn,            // parcels returned to origin station at delivery station
    //Returning,          // parcels in return transit
    Returned = 19,           // parcels returned to origin station

    // Completion
    //Delivered,     // delivered, waiting for rating
    Completed = 21,             // feedback received, shipment closed

    // Unhappy case
    Delayed = 22,              // parcels delayed at any stage
    Arrived = 23,
    CompletedWithCompensation = 24, // parcels delivered with compensation
    ToCompensate = 25,
    Compensated = 26,          // compensation processed
    DeliveredPartially = 27, // some parcels delivered, some lost
}