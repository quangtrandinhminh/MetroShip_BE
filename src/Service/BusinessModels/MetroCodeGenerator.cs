namespace MetroShip.Service.BusinessModels;

public class MetroCodeGenerator
{
    public static string GenerateStationCode(int order, int lineNumber, string regionCode)
    {
        if (string.IsNullOrWhiteSpace(regionCode))
            throw new ArgumentException("Region code cannot be null or empty.", nameof(regionCode));

        // Generate a station code based on the order, line number, and region code
        return $"{regionCode.ToUpperInvariant()}-L{lineNumber}-{order:D2}";
    }

    public static string GenerateRouteCode(string lineCode, string fromStationCode, string toStationCode)
    {
        if (string.IsNullOrWhiteSpace(fromStationCode) || string.IsNullOrWhiteSpace(toStationCode))
            throw new ArgumentException("Station codes cannot be null or empty.");

        var region = lineCode.Split('-')[0];
        var line = lineCode.Split('-')[1];
        var fromStationCodePart = fromStationCode.Split('-')[2];
        var toStationCodePart = toStationCode.Split('-')[2];

        // Combine the codes of the two stations
        return $"{region}-{line}-{fromStationCodePart}-{toStationCodePart}";
    }

    public static string GenerateMetroLineCode(string regionCode, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(regionCode))
            throw new ArgumentException("Region code cannot be null or empty.", nameof(regionCode));

        // Generate a code based on the region and line number, e.g., "HCM-L1"
        return $"{regionCode.ToUpperInvariant()}-L{lineNumber}";
    }
}