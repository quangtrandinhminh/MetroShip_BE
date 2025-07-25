namespace MetroShip.Utility.Enums
{
    public enum TrainStatusEnum
    {
        NotScheduled,         // Train has not been scheduled yet
        Scheduled,            // Train is scheduled for operation
        AwaitingDeparture,    // Waiting to depart from the initial station
        Departed,             // Has departed from the starting station
        InTransit,            // Currently moving between stations
        ArrivedAtStation,     // Has arrived at an intermediate station
        WaitingForTransfer,   // Waiting at a transfer station for parcel handoff
        ResumingTransit,      // Resuming journey after parcel transfer
        Delayed,              // Delayed due to issues (e.g., technical, weather)
        Cancelled,            // Train operation has been cancelled
        Completed,            // Journey completed at the final station
        Maintenance,          // Under maintenance, not in service
        OutOfService          // Temporarily out of operation
    }
}