// ===== POINT MANAGEMENT FUNCTIONS =====

// Hàm để thêm một điểm mới vào danh sách
function addPoint() {
  const pointsList = document.getElementById('pointsList');
  const pointCount = pointsList.children.length;
  const pointLabel = String.fromCharCode(65 + pointCount); // A, B, C, D, etc.
  
  const pointDiv = document.createElement('div');
  pointDiv.className = 'point-item';
  pointDiv.setAttribute('data-index', pointCount);
  
  pointDiv.innerHTML = `
    <span style="margin-right: 0.5em;">${pointLabel}:</span>
    <div class="point-inputs">
      <input id="lat${pointLabel}" value="" placeholder="Latitude ${pointLabel}">
      <input id="lon${pointLabel}" value="" placeholder="Longitude ${pointLabel}">
      <input id="km${pointLabel}" value="" placeholder="KM (auto)" style="width: 80px;">
    </div>
    <button onclick="removePoint(${pointCount})" class="btn-danger btn-small">×</button>
  `;
  
  pointsList.appendChild(pointDiv);
}

// Hàm để xóa một điểm khỏi danh sách (không thể xóa A và B)
function removePoint(index) {
  if (index < 2) {
    alert('Cannot remove initial points A and B');
    return;
  }
  
  const pointItem = document.querySelector(`[data-index="${index}"]`);
  if (pointItem) {
    pointItem.remove();
    updatePointLabels();
  }
}

// Hàm để cập nhật nhãn điểm sau khi thêm hoặc xóa điểm
function updatePointLabels() {
  const pointItems = document.querySelectorAll('.point-item');
  pointItems.forEach((item, index) => {
    const label = String.fromCharCode(65 + index);
    item.setAttribute('data-index', index);
    item.querySelector('span').textContent = `${label}:`;
    
    const inputs = item.querySelectorAll('input');
    if (inputs.length >= 3) {
      inputs[0].id = `lat${label}`;
      inputs[0].placeholder = `Latitude ${label}`;
      inputs[1].id = `lon${label}`;
      inputs[1].placeholder = `Longitude ${label}`;
      inputs[2].id = `km${label}`;
      if (index === 0) {
        inputs[2].value = '0';
        inputs[2].readOnly = true;
        inputs[2].style.backgroundColor = '#f0f0f0';
        inputs[2].placeholder = '0';
      } else {
        inputs[2].placeholder = 'KM (auto)';
        inputs[2].readOnly = false;
        inputs[2].style.backgroundColor = '';
      }
    }
    
    const removeBtn = item.querySelector('button');
    if (removeBtn && index >= 2) {
      removeBtn.onclick = () => removePoint(index);
    }
  });
}

// Hàm để lấy tất cả các điểm từ danh sách
function getAllPoints() {
  const points = [];
  const pointItems = document.querySelectorAll('.point-item');
  
  pointItems.forEach((item, index) => {
    const label = String.fromCharCode(65 + index);
    const latInput = document.getElementById(`lat${label}`);
    const lonInput = document.getElementById(`lon${label}`);
    const kmInput = document.getElementById(`km${label}`);
    
    if (latInput && lonInput && latInput.value && lonInput.value) {
      // Lấy tên ga nếu có
      const stationNameElement = item.querySelector('span:last-child');
      const stationName = stationNameElement && stationNameElement.style.fontSize === '0.8em' 
        ? stationNameElement.textContent : label;
      
      points.push({
        label: label,
        lat: parseFloat(latInput.value),
        lon: parseFloat(lonInput.value),
        km: kmInput ? (parseFloat(kmInput.value) || 0) : 0,
        stationName: stationName
      });
    }
  });
  
  return points;
}

// Tải các ga của tuyến đường vào danh sách điểm (deprecated - không còn sử dụng UI button)
function loadRouteStations() {
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
  
  // Xóa các điểm hiện tại
  document.getElementById('pointsList').innerHTML = '';
  
  // Thêm các ga làm điểm
  activeStations.forEach((station, index) => {
    const pointLabel = String.fromCharCode(65 + index);
    const pointDiv = document.createElement('div');
    pointDiv.className = 'point-item';
    pointDiv.setAttribute('data-index', index);
    
    pointDiv.innerHTML = `
      <span style="margin-right: 0.5em;">${pointLabel}:</span>
      <div class="point-inputs">
        <input id="lat${pointLabel}" value="${station.Latitude}" placeholder="Latitude ${pointLabel}" readonly>
        <input id="lon${pointLabel}" value="${station.Longitude}" placeholder="Longitude ${pointLabel}" readonly>
        <input id="km${pointLabel}" value="" placeholder="KM (auto)" style="width: 80px;">
      </div>
      <span style="margin-left: 0.5em; font-size: 0.8em; color: #666;">${station.StationNameVi}</span>
    `;
    
    // Đặt km = 0 cho ga đầu tiên
    if (index === 0) {
      const kmInput = pointDiv.querySelector(`#km${pointLabel}`);
      kmInput.value = '0';
      kmInput.readOnly = true;
      kmInput.style.backgroundColor = '#f0f0f0';
      kmInput.placeholder = '0';
    }
    
    document.getElementById('pointsList').appendChild(pointDiv);
  });
  
  // Chuyển về chế độ thủ công để hiển thị các điểm đã tải
  document.querySelector('input[value="manual"]').checked = true;
  toggleRouteMode();
  
  alert(`Loaded ${activeStations.length} active stations from ${selectedRoute.routeName}`);
}
