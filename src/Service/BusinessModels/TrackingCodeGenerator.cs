using MetroShip.Repository.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QRCoder;
using BarcodeStandard;

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
    /// <returns> E.g: MS-HCMC-2505190000103VN </returns>
    public static string GenerateShipmentTrackingCode(
        string regionCode,
        DateTimeOffset shipmentDate
    )
    {
        // 1) Compute sequence and check-digit
        
        int randomDigits = 3;
        string datePart = shipmentDate.ToString("yyyyMMddHHmm");
        string randomPart = GenerateRandomString(randomDigits);

        // 2) Build tracking code via interpolation
        return $"{DefaultAppCode}-" +                           // e.g. "MS"
               $"{regionCode.ToUpperInvariant()}-" +            // e.g. "HCM" :contentReference[oaicite:2]{index=2}
               $"{shipmentDate:yyyyMMddHHmm}" +                     // e.g. "20230519" :contentReference[oaicite:2]{index=2}
               //$"{seq:D3}" +                                    // zero-padded 3-digit sequence
               //$"{check:D2}" +                                  // zero-padded 2-digit check-digit
               $"{randomPart}";                                 // e.g. "103" :contentReference[oaicite:2]{index=2}
               //$"{DefaultCountryName}";                         // e.g. "VN"
    }

    /// <summary>
    /// Generate parcel tracking code
    /// </summary>
    /// <param name="shipmentCode"></param>
    /// <param name="seqInShipment"></param>
    /// <returns> E.g: MS-HCMC-2505190000103VN </returns>
    public static string GenerateParcelCode (string shipmentCode, int seqInShipment)
    {
        // 2) Build tracking code via interpolation
        return $"{shipmentCode}" + "-" +                      // e.g. "MS"
               $"{++seqInShipment:D2}";                       // zero-padded 2-digit sequence
    }

    // Gnerate QR code
    public static string GenerateQRCode(string content)
    {
        // Create QR code generator instance
        QRCodeGenerator qrGenerator = new ();

        // Generate QR code data
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        BitmapByteQRCode qRCode = new (qrCodeData);
        string base64String = Convert.ToBase64String(qRCode.GetGraphic(10));

        // Return the base64 string of the QR code image
        return $"data:image/png;base64,{base64String}";
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}