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
    Stored,
}

/// <summary>
/// All the possible triggers (events) that drive transitions
/// in the Shipment state machine.
/// </summary>
public enum ShipmentTrigger
{
    // Staff actions (pre‐payment)
    StaffConfirmAllParcels,      // staff accepts all parcels within 24 h
    StaffConfirmSomeParcels,     // staff accepts ≥1 parcel, rejects ≥1 parcel
    StaffRejectAllParcels,       // staff rejects every parcel
    ConfirmationTimeout,         // system‐initiated after 24 h without any staff action

    // Customer / payment
    CustomerMakePayment,         // customer completes payment
    PaymentTimeout,              // system‐initiated after payment deadline (no pay in 24 h)
    CustomerCancel,              // customer cancels shipment at any pre‐dropoff stage

    // Refund flow
    RequestRefund,               // enters AwaitingRefund when customer cancels post‐payment but pre‐dropoff
    ProcessRefund,               // staff confirms and issues refund

    // Drop‐off
    CustomerDropOff,             // customer drops off parcels at station A
    DropOffTimeout,              // system‐initiated after drop‐off window closes without drop‐off

    // In‐transit
    ParcelsLoadedOntoMetro,      // staff loads parcels onto the train
    ParcelsArrivedAtDeparture,   // all parcels scanned as “departed”
    ParcelsArrivedAtDestination, // all parcels scanned as “arrived” at destination station

    // Last mile
    MarkOutForDelivery,          // staff hands parcels to warehouse for delivery
    DeliveryTimeout,             // system‐initiated when parcels overdue at delivery station
    CustomerPaySurcharge,        // customer pays overdue surcharge

    // Completion
    ProvideFeedback,             // customer submits rating/feedback post‐delivery
    ExpireShipment               // system‐initiated when parcels move to long‐term storage (> 30 days)
}
