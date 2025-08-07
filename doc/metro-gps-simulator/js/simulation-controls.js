// ===== SIMULATION CONTROL FUNCTIONS =====

// Hàm để bắt đầu mô phỏng
function startSimulation() {
  const routeMode = document.querySelector('input[name="routeMode"]:checked').value;
  
  if (routeMode === 'dropdown') {
    // Sử dụng predefined route
    const routeIndex = document.getElementById('routeSelect').value;
    const direction = parseInt(document.getElementById('directionSelect').value);
    
    if (!routeIndex || !routeData) {
      alert('Please select a route first');
      return;
    }
    
    const selectedRoute = routeData[routeIndex];
    if (!selectedRoute || !selectedRoute.stations) {
      alert('Invalid route data');
      return;
    }
    
    // Lọc chỉ các ga đang hoạt động
    let activeStations = selectedRoute.stations.filter(station => station.IsActive);
    
    if (activeStations.length < 2) {
      alert('Route must have at least 2 active stations');
      return;
    }
    
    // Đảo ngược thứ tự nếu hướng là ngược lại
    if (direction === 1) {
      activeStations = activeStations.reverse();
    }
    
    // Chuyển đổi stations thành points
    allPoints = activeStations.map((station, index) => ({
      label: String.fromCharCode(65 + index),
      lat: station.Latitude,
      lon: station.Longitude,
      km: index === 0 ? 0 : 0, // km sẽ được tính tự động bằng Haversine
      stationName: station.StationNameVi,
      stationId: station.Id // Thêm station ID để gửi API khi tàu dừng
    }));
    
  } else {
    // Sử dụng manual points
    allPoints = getAllPoints();
  }
  
  if (allPoints.length < 2) {
    alert('Please add at least 2 points to start simulation');
    return;
  }
  
  currentPointIndex = 0;
  simulationRunning = true;
  document.getElementById('startBtn').disabled = true;
  document.getElementById('continueSection').style.display = 'none';
  
  simulateSegment(currentPointIndex, currentPointIndex + 1);
}

// Hàm để tiếp tục đến điểm tiếp theo
function continueToNext() {
  currentPointIndex++;
  if (currentPointIndex + 1 < allPoints.length) {
    document.getElementById('continueSection').style.display = 'none';
    simulateSegment(currentPointIndex, currentPointIndex + 1);
  } else {
    stopSimulation();
  }
}

// Hàm để dừng mô phỏng
function stopSimulation() {
  simulationRunning = false;
  document.getElementById('startBtn').disabled = false;
  document.getElementById('continueSection').style.display = 'none';
  
  if (currentSimulation) {
    clearTimeout(currentSimulation);
    currentSimulation = null;
  }
  
  document.getElementById('output').value += '\n=== Simulation Stopped ===\n';
}

// Hàm để reset mô phỏng
function resetSimulation() {
  stopSimulation();
  currentPointIndex = 0;
  document.getElementById('output').value = '';
  document.getElementById('startBtn').disabled = false;
  document.getElementById('continueSection').style.display = 'none';
}
