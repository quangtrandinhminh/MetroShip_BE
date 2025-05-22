using MetroShip.Repository.Models;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.BusinessModels;

/// <summary>
/// Lớp tiện ích chứa thuật toán tạo graph và tìm đường đi trong mạng lưới metro
/// </summary>
public class MetroGraph
{
    // Cấu trúc dữ liệu cho đồ thị
    private Dictionary<string, List<string>> _adjacencyList;
    private Dictionary<(string from, string to), Route> _routesMap;
    private Dictionary<string, Station> _stationsMap;
    private Dictionary<string, MetroLine> _linesMap;

    /// <summary>
    /// Khởi tạo một đồ thị metro từ dữ liệu routes, stations và metroLines
    /// </summary>
    public MetroGraph(List<Route> routes, List<Station> stations, List<MetroLine> metroLines)
    {
        // Khởi tạo cấu trúc dữ liệu
        _adjacencyList = new Dictionary<string, List<string>>();
        _routesMap = new Dictionary<(string from, string to), Route>();
        _stationsMap = stations.ToDictionary(s => s.Id, s => s);
        _linesMap = metroLines.ToDictionary(l => l.Id, l => l);

        // Xây dựng adjacency list và routes mapping
        foreach (var route in routes)
        {
            if (!_adjacencyList.ContainsKey(route.FromStationId))
                _adjacencyList[route.FromStationId] = new List<string>();

            _adjacencyList[route.FromStationId].Add(route.ToStationId);
            _routesMap[(route.FromStationId, route.ToStationId)] = route;
        }
    }

    /// <summary>
    /// Constructor riêng tư cho việc tạo MetroGraph từ một path
    /// </summary>
    /*private MetroGraph(
        Dictionary<string, List<string>> adjacencyList,
        Dictionary<(string from, string to), Route> routesMap,
        Dictionary<string, Station> stationsMap,
        Dictionary<string, MetroLine> linesMap)
    {
        _adjacencyList = adjacencyList;
        _routesMap = routesMap;
        _stationsMap = stationsMap;
        _linesMap = linesMap;
    }*/

    /// <summary>
    /// Tìm đường đi ngắn nhất giữa hai ga theo số lượng ga
    /// </summary>
    /// <param name="fromStationId">ID ga xuất phát</param>
    /// <param name="toStationId">ID ga đích</param>
    /// <returns>List station Id on path, or null if not found</returns>
    public List<string> FindShortestPath(string fromStationId, string toStationId)
    {
        // Nếu ga xuất phát hoặc ga đích không tồn tại
        if (!_stationsMap.ContainsKey(fromStationId) || !_stationsMap.ContainsKey(toStationId))
            throw new AppException(
                               ErrorCode.NotFound,
                                              ResponseMessageStation.STATION_NOT_FOUND,
                                                             StatusCodes.Status404NotFound
                                                                        );

        // Khởi tạo queue và dictionaries cho BFS
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        var parent = new Dictionary<string, string>(); // Lưu trữ parent của mỗi node để tái tạo đường đi
        bool found = false;

        // Bắt đầu BFS
        queue.Enqueue(fromStationId);
        visited.Add(fromStationId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == toStationId)
            {
                found = true;
                break;
            }

            // Nếu node hiện tại không có trong adjacency list, bỏ qua
            if (!_adjacencyList.ContainsKey(current))
                continue;

            foreach (var neighbor in _adjacencyList[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Nếu không tìm thấy đường đi
        if (!found)
            return null;

        // Tái tạo đường đi từ end về start, sau đó đảo ngược lại
        var path = new List<string>();
        string current_node = toStationId;

        while (current_node != fromStationId)
        {
            path.Add(current_node);
            current_node = parent[current_node];
        }

        path.Add(fromStationId);
        path.Reverse();

        return path;
    }

    /// <summary>
    /// Tạo một MetroGraph mới chỉ chứa các ga và tuyến đường trong path
    /// </summary>
    /// <param name="path">Đường đi dưới dạng danh sách ID ga</param>
    /// <returns>MetroGraph mới chứa các ga và tuyến đường trong path</returns>
    public BestPathGraphResponse CreateResponseFromPath(List<string> path, IMapperlyMapper _mapper)
    {
        if (path == null || path.Count < 2)
            return null;

        // Tạo cấu trúc dữ liệu mới
        var newAdjacencyList = new Dictionary<string, List<string>>();
        var newRoutesMap = new Dictionary<(string from, string to), Route>();
        var newStationsMap = new Dictionary<string, Station>();
        var newLinesMap = new Dictionary<string, MetroLine>();

        // Thêm các ga trên đường đi vào map mới
        foreach (var stationId in path)
        {
            if (_stationsMap.TryGetValue(stationId, out var station))
            {
                newStationsMap[stationId] = station;
            }
        }

        // Thêm các tuyến đường trên đường đi
        for (int i = 0; i < path.Count - 1; i++)
        {
            var fromId = path[i];
            var toId = path[i + 1];

            // Thêm vào adjacency list mới
            if (!newAdjacencyList.ContainsKey(fromId))
                newAdjacencyList[fromId] = new List<string>();

            newAdjacencyList[fromId].Add(toId);

            // Thêm route vào map mới
            if (_routesMap.TryGetValue((fromId, toId), out var route))
            {
                newRoutesMap[(fromId, toId)] = route;

                // Thêm line vào map mới nếu chưa có
                if (!newLinesMap.ContainsKey(route.LineId) && _linesMap.TryGetValue(route.LineId, out var line))
                {
                    newLinesMap[route.LineId] = line;
                }
            }
        }

        var response = new BestPathGraphResponse
        {
            Routes = newRoutesMap.Values.Select(r => _mapper.MapToRouteResponse(r)).ToList(),
            Stations = newStationsMap.Values.Select(s => _mapper.MapToStationResponse(s)).ToList(),
            MetroLines = newLinesMap.Values.Select(l => _mapper.MapToMetroLineResponse(l)).ToList()
        };

        foreach (var route in response.Routes)
        {
            route.LegOrder = response.Routes.IndexOf(route) + 1;
        }

        return response;
    }
}