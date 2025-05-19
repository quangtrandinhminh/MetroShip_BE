using MetroShip.Repository.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetroShip.Service.BusinessModels;

public static class TrackingCodeGenerator
{
    private const string DefaultAppCode = "MS";
    private const string DefaultCountryName = "VN";

    /// <summary>
    /// Generate shipment tracking code
    /// </summary>
    /// <param name="regionCode">The region code of departure station</param>
    /// <param name="shipmentDate">The date that shipment is booked</param>
    /// <param name="departureStationCode">Departure station</param>
    /// <param name="destinationStationCode">Destination station</param>
    /// <param name="maxSeqPerDayAndRgn">Number of shipments per day and region</param>
    /// <returns> E.g: MS-HCMC-L101L1022505190000103VN </returns>
    public static string GenerateShipmentTrackingCode(
        string regionCode,
        DateTimeOffset shipmentDate,
        string departureStationCode,
        string destinationStationCode,
        int maxSeqPerDayAndRgn
    )
    {
        // 1) Compute sequence and check-digit
        int seq = maxSeqPerDayAndRgn + 1;
        int check = seq % 7;

        // 2) Build tracking code via interpolation
        return $"{DefaultAppCode}-" +                           // e.g. "MS"
               $"{regionCode.ToUpperInvariant()}-" +            // e.g. "HCM" :contentReference[oaicite:2]{index=2}
               $"{departureStationCode.ToUpperInvariant()}" +   // e.g. "CL1"
               $"{destinationStationCode.ToUpperInvariant()}" + // e.g. "01L"
               $"{shipmentDate:yyMMdd}" +                       // e.g. "102250519" :contentReference[oaicite:3]{index=3}
               $"{seq:D5}" +                                    // zero-padded 5-digit sequence
               $"{check:D2}" +                                  // zero-padded 2-digit check-digit
               $"{DefaultCountryName}";                         // e.g. "VN"
    }

    /// <summary>
    /// Generate parcel tracking code
    /// </summary>
    /// <param name="shipmentCode"></param>
    /// <param name="seqInShipment"></param>
    /// <returns> E.g: MS-HCMC-L101L1022505190000103VN </returns>
    public static string GenerateParcelCode (string shipmentCode, int seqInShipment)
    {
        // 2) Build tracking code via interpolation
        return $"{shipmentCode}" + "-" +                      // e.g. "MS"
               $"{++seqInShipment:D2}";                       // zero-padded 2-digit sequence
    }
}