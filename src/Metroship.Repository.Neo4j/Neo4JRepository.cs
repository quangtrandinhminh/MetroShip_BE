using MetroShip.Repository.Neo4j.Models;
using MetroShip.Utility.Config;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Transactions;
using Newtonsoft.Json.Serialization;

namespace MetroShip.Repository.Neo4j;

public class Neo4JRepository : INeo4JRepository
{
    private readonly GraphClient _client;

    public Neo4JRepository()
    {
        _client = new GraphClient(
            Neo4jSetting.Instance.Uri,
            Neo4jSetting.Instance.Username,
            Neo4jSetting.Instance.Password)
        {
            JsonContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        _client.ConnectAsync();
    }

    public async Task<IList<Route>> GetShortestAndCheapestPath(Guid fromStationId, Guid toStationId)
    {
        var path = await  _client.Cypher
            .Match("(start:Station {StationId: $fromStationId}), (end:Station {StationId: $toStationId})")
            .Call("gds.shortestPath.dijkstra.stream({" +
                               "nodeProjection: 'Station', " +
                               "relationshipProjection: {" +
                                   "Route:" +
                                   "{" +
                                       "type: 'Route'," +
                                       "['LengthKm', 'MetroBasePriceVndPerKm']," +
                                       "'UNDIRECTED'" +
                                   "}" +
                               "}," +
                               "startNode: start," +
                               "endNode: end," +
                               "relationshipWeightProperty: 'LengthKm'})"
                           )
            .Yield("index, totalCost, path")
            .WithParams(new
            {
                fromStationId,
                toStationId
            })
            .Return(path => new
            {
                PathNodes = path.As<List<Route>>(),  // List of routes in the path
                TotalCost = path.As<double>()        // Total cost of the path
            }).ResultsAsync; 

        return path.Select(p => new Route
        {
            FromStationId = p.PathNodes.First().FromStationId,
            ToStationId = p.PathNodes.Last().ToStationId,
            LengthKm = p.PathNodes.Sum(r => r.LengthKm),
            Direction = p.PathNodes.First().Direction,
            IsActive = true // Assuming all routes are active in this context
        }).ToList();
    }

    /*public async Task CreateMetroLineAsync(int id, string name, double totalLength, int totalStations)
    {
        await _client.Cypher
            .Merge("(line:MetroLine { Id: $id })")
            .OnCreate()
            .Set("line = { Id: $id, Name: $name, TotalLength: $totalLength, TotalStations: $totalStations }")
            .WithParams(new { id, name, totalLength, totalStations })
            .ExecuteWithoutResultsAsync();
    }

    public async Task CreateStationAsync(int id, string name, double latitude, double longitude)
    {
        using (var transaction = _client.BeginTransaction())
        {
            await _client.Cypher
                .Merge("(station:Station { Id: $id })")
                .OnCreate()
                .Set("station = { Id: $id, Name: $name, Latitude: $latitude, Longitude: $longitude }")
                .WithParams(new { id, name, latitude, longitude })
                .ExecuteWithoutResultsAsync();

            await transaction.CommitAsync();
        }
    }

    public async Task CreateRouteAsync(int id, int fromStationId, int toStationId, double lengthKm, string direction)
    {
        await _client.Cypher
            .Match("(from:Station { Id: $fromStationId })", "(to:Station { Id: $toStationId })")
            .Merge("(from)-[route:Route { Id: $id }]->(to)")
            .OnCreate()
            .Set("route = { Id: $id, LengthKm: $lengthKm, Direction: $direction }")
            .WithParams(new { id, fromStationId, toStationId, lengthKm, direction })
            .ExecuteWithoutResultsAsync();
    }

    /// <summary>
    /// Get all stations near the user's location within a specified distance.
    /// </summary>
    public async Task<List<Station>> GetAllStationNearUserAsync(double userLatitude, double userLongitude, int maxDistanceInMeters = 1000, int maxCount = 10)
    {
        var query = await _client.Cypher
            .Match("(station:Station)")
            .Where("station.Latitude IS NOT NULL AND station.Longitude IS NOT NULL")
            .With("station, point({latitude: station.Latitude, longitude: station.Longitude}) AS stationPoint, point({latitude: $userLatitude, longitude: $userLongitude}) AS userPoint")
            .Where("distance(userPoint, stationPoint) <= $maxDistanceInMeters")
            .Return(station => station.As<Station>())
            .Limit(maxCount)
            .WithParams(new
            {
                userLatitude,
                userLongitude,
                maxDistanceInMeters
            })
            .Results;

        return query.ToList();
    }*/
}