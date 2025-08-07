// ===== UI MANAGEMENT FUNCTIONS =====

// Điền dropdown với các tuyến đường có sẵn
function populateRouteDropdown() {
  const select = document.getElementById('routeSelect');
  select.innerHTML = '<option value="">-- Select Route --</option>';
  
  if (routeData && Array.isArray(routeData)) {
    routeData.forEach((route, index) => {
      const option = document.createElement('option');
      option.value = index;
      option.textContent = route.routeName;
      select.appendChild(option);
    });
  }
}

// Chuyển đổi giữa chế độ thủ công và dropdown
function toggleRouteMode() {
  const mode = document.querySelector('input[name="routeMode"]:checked').value;
  const manualSection = document.getElementById('manualPointsSection');
  const dropdownSection = document.getElementById('routeDropdownSection');
  
  if (mode === 'manual') {
    manualSection.style.display = 'block';
    dropdownSection.style.display = 'none';
  } else {
    manualSection.style.display = 'none';
    dropdownSection.style.display = 'block';
  }
}

// Cập nhật danh sách ga khi chọn tuyến đường
function updateStationList() {
  const routeIndex = document.getElementById('routeSelect').value;
  const direction = parseInt(document.getElementById('directionSelect').value);
  const stationsList = document.getElementById('stationsList');
  
  if (!routeIndex || !routeData) {
    stationsList.innerHTML = '';
    return;
  }
  
  const selectedRoute = routeData[routeIndex];
  stationsList.innerHTML = '';
  
  if (selectedRoute && selectedRoute.stations) {
    // Lấy tất cả stations (không lọc)
    let allStations = [...selectedRoute.stations];
    
    // Đảo ngược thứ tự nếu hướng là ngược lại
    if (direction === 1) {
      allStations = allStations.reverse();
    }
    
    allStations.forEach((station, index) => {
      const stationDiv = document.createElement('div');
      stationDiv.className = `station-item ${station.IsActive ? 'active' : 'inactive'}`;
      
      // Tạo toggle button cho IsActive
      const toggleButton = document.createElement('button');
      toggleButton.className = `toggle-btn ${station.IsActive ? 'active' : 'inactive'}`;
      toggleButton.innerHTML = station.IsActive ? '●' : '○';
      toggleButton.title = station.IsActive ? 'Click to deactivate' : 'Click to activate';
      toggleButton.onclick = () => toggleStationActive(routeIndex, station.StationCode, !station.IsActive);
      
      stationDiv.innerHTML = `
        <div style="display: flex; align-items: center; gap: 8px;">
          <span style="flex: 1;">
            ${index + 1}. ${station.StationNameVi} ${station.IsActive ? '(Active)' : '(Inactive)'}
            <br><small>Lat: ${station.Latitude}, Lon: ${station.Longitude}</small>
          </span>
        </div>
      `;
      
      // Thêm toggle button vào đầu div
      stationDiv.querySelector('div').insertBefore(toggleButton, stationDiv.querySelector('span'));
      
      stationsList.appendChild(stationDiv);
    });
    
    if (allStations.length > 0) {
      const activeCount = allStations.filter(station => station.IsActive).length;
      const infoDiv = document.createElement('div');
      infoDiv.style.marginTop = '0.5em';
      infoDiv.style.fontWeight = 'bold';
      infoDiv.style.color = DEFAULT_CONFIG.COLORS.SUCCESS;
      infoDiv.innerHTML = `Total: ${allStations.length} stations (${activeCount} active, ${allStations.length - activeCount} inactive)`;
      stationsList.appendChild(infoDiv);
    }
  }
}

// Hàm để toggle trạng thái IsActive của ga
function toggleStationActive(routeIndex, stationCode, newActiveStatus) {
  if (!routeData || !routeData[routeIndex]) {
    alert('Route data not found');
    return;
  }
  
  const route = routeData[routeIndex];
  const station = route.stations.find(s => s.StationCode === stationCode);
  
  if (station) {
    station.IsActive = newActiveStatus;
    
    // Tự động lưu dữ liệu ngay lập tức
    saveDataToFileInstantly();
    
    // Cập nhật lại danh sách để hiển thị thay đổi
    updateStationList();
    
    console.log(`Station ${station.StationNameVi} (${stationCode}) is now ${newActiveStatus ? 'Active' : 'Inactive'}`);
  } else {
    alert('Station not found');
  }
}
