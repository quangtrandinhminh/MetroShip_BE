using MetroShip.Repository.Models;
using MetroShip.Repository.Repositories;
using MetroShip.Service.ApiModels.Graph;
using MetroShip.Service.ApiModels.MetroLine;
using MetroShip.Service.ApiModels.Route;
using MetroShip.Service.ApiModels.Shipment;
using MetroShip.Service.ApiModels.Station;
using MetroShip.Service.Mapper;
using MetroShip.Utility.Constants;
using MetroShip.Utility.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

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
            // Thêm ga vào stations map nếu chưa có
            if (!_adjacencyList.ContainsKey(route.FromStationId))
            {
                _adjacencyList[route.FromStationId] = new List<string>();
            }

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
    public List<string> FindShortestPathByBFS(string fromStationId, string toStationId)
    {
        // Nếu ga xuất phát hoặc ga đích không tồn tại
        if (!_stationsMap.ContainsKey(fromStationId) || !_stationsMap.ContainsKey(toStationId))
                throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageStation.STATION_NOT_FOUND,
                StatusCodes.Status404NotFound
                );

        /*// Khởi tạo queue và dictionaries cho BFS
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
        path.Reverse();*/
        var path = BFS(fromStationId, toStationId);

        return path;
    }

    /// <summary>
    /// Tìm đường đi ngắn nhất giữa hai ga theo số lượng ga bằng thuật toán DFS, khi mà BFS không khả thi (ví dụ: đồ thị có chu trình)
    /// </summary>
    /// <param name="fromStationId"></param>
    /// <param name="toStationId"></param>
    /// <returns>List station Id on path, or null if not found</returns>
    /// <exception cref="AppException"></exception>
    public List<string> FindShortestPathByDFS(string fromStationId, string toStationId)
    {
        // Nếu ga xuất phát hoặc ga đích không tồn tại
        if (!_stationsMap.ContainsKey(fromStationId) || !_stationsMap.ContainsKey(toStationId))
            throw new AppException(
            ErrorCode.NotFound,
            ResponseMessageStation.STATION_NOT_FOUND,
            StatusCodes.Status404NotFound                                                                                                                                                                                                                   );

        // Tìm đường đi bằng DFS
        var path = DFS(fromStationId, toStationId);

        return path;
    }

    /// <summary>
    /// Tìm đường đi ngắn nhất giữa hai ga bằng thuật toán Dijkstra, ưu tiên giá và thời gian
    /// </summary>
    /// <param name="fromStationId"></param>
    /// <param name="toStationId"></param>
    /// <returns></returns>
    /// <exception cref="AppException"></exception>
    public List<string> FindShortestPathByDijkstra(
               string fromStationId,
                      string toStationId)
    {
        // Nếu ga xuất phát hoặc ga đích không tồn tại
        if (!_stationsMap.ContainsKey(fromStationId) || !_stationsMap.ContainsKey(toStationId))
                throw new AppException(
                ErrorCode.NotFound,
                ResponseMessageStation.STATION_NOT_FOUND,
                StatusCodes.Status404NotFound
            );

        Expression<Func<Route, object>>[] weights =
        {
            //x => x.BasePriceVndPerKm * x.LengthKm, // Ưu tiên giá
            x => x.TravelTimeMin // Ưu tiên thời gian
        };


        // Tìm đường đi bằng Dijkstra
        var path = Dijkstra(fromStationId, toStationId, weights);

        return path;
    }

    /// <summary>
    /// Tạo một MetroGraph mới chỉ chứa các ga và tuyến đường trong path
    /// </summary>
    /// <param name="path">Đường đi dưới dạng danh sách ID ga</param>
    /// <returns>MetroGraph mới chứa các ga và tuyến đường trong path</returns>
    public BestPathGraphResponse CreateResponseFromPath(List<string> path, IMapperlyMapper _mapper)
    {
        if (!path.Any())
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
            //var basePricePerKm = _linesMap[route.LineId].BasePriceVndPerKm;
            route.LegOrder = response.Routes.IndexOf(route) + 1;
            //route.BasePriceVndPerKg = basePricePerKm;
        }

        return response;
    }

    public BestPathGraphResponse CreateResponseFromPath(
        List<string> path, IMapperlyMapper _mapper, DateTimeOffset schduleDate)
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
            //route.BasePriceVndPerKg = basePricePerKm;
        }

        return response;
    }

    private List<string> BFS (string start, string end)
    {
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        var parentMap = new Dictionary<string, string>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Nếu đã đến ga đích, xây dựng đường đi
            if (current == end)
            {
                var path = new List<string>();
                while (current != null)
                {
                    path.Add(current);
                    parentMap.TryGetValue(current, out current);
                }
                path.Reverse();
                return path;
            }

            // Duyệt các ga kề
            foreach (var neighbor in _adjacencyList[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parentMap[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Không tìm thấy đường đi
        return null;
    }

    private List<string> DFS (string start, string end)
    {
        var stack = new Stack<string>();
        var visited = new HashSet<string>();
        var parentMap = new Dictionary<string, string>();

        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            // Nếu đã đến đích, xây dựng đường đi
            if (current == end)
            {
                var path = new List<string>();
                while (current != null)
                {
                    path.Add(current);
                    parentMap.TryGetValue(current, out current);
                }
                path.Reverse();
                return path;
            }

            // Duyệt các ga kề
            foreach (var neighbor in _adjacencyList[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parentMap[neighbor] = current;
                    stack.Push(neighbor);
                }
            }
        }

        // Không tìm thấy đường đi
        return null;
    }

    // Chọn weight cho route dựa trên tỉ lệ các weights, weight nào vào trước thì độ ưu tiên cao hơn
    // Ưu tiên price (MetroBasePriceVndPerKm * Length) > EstMinute
    private List<string> Dijkstra(string start, string end, 
        params Expression<Func<Route, object>>[] weights)
    {                                                                                                                                                                                                               
        // Khởi tạo cấu trúc dữ liệu
        var priorityQueue = new SortedSet<(decimal cost, string station)>();
        var visited = new HashSet<string>();
        var parentMap = new Dictionary<string, string>();
        var costMap = new Dictionary<string, decimal>();

        // Thêm ga xuất phát vào queue
        priorityQueue.Add((0, start));
        costMap[start] = 0;

        while (priorityQueue.Count > 0)
        {
            var (currentCost, currentStation) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);

            // Nếu đã đến đích, xây dựng đường đi
            if (currentStation == end)
            {
                var path = new List<string>();
                while (currentStation != null)
                {
                    path.Add(currentStation);
                    parentMap.TryGetValue(currentStation, out currentStation);
                }
                path.Reverse();
                return path;
            }

            // Nếu đã thăm rồi thì bỏ qua
            if (visited.Contains(currentStation))
                continue;

            visited.Add(currentStation);

            // Duyệt các ga kề
            if (_adjacencyList.TryGetValue(currentStation, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (visited.Contains(neighbor))
                        continue;

                    // Tính toán chi phí cho tuyến đường này
                    if (_routesMap.TryGetValue((currentStation, neighbor), out var route))
                    {
                        decimal weight = 0;
                        foreach (var weightExpression in weights)
                        {
                            var value = weightExpression.Compile().Invoke(route);
                            weight += Convert.ToDecimal(value);
                        }

                        var newCost = currentCost + weight;

                        // Nếu chưa có hoặc chi phí mới thấp hơn chi phí cũ
                        if (!costMap.ContainsKey(neighbor) || newCost < costMap[neighbor])
                        {
                            costMap[neighbor] = newCost;
                            parentMap[neighbor] = currentStation;
                            priorityQueue.Add((newCost, neighbor));
                        }
                    }
                }
            }
        }

        // Không tìm thấy đường đi
        return null;
    }
}