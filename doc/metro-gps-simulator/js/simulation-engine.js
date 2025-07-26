// ===== SIMULATION ENGINE =====

// Hàm chính để tính toán và hiển thị mô phỏng cho một đoạn đường
function simulateRoute(pointA, pointB, segmentName = '', isSegment = false) {
  // Lấy các giá trị đầu vào từ form HTML
  const maxSpeed = parseFloat(document.getElementById('maxSpeed').value) / 3.6;
  const acc = parseFloat(document.getElementById('acc').value);
  const interval = parseFloat(document.getElementById('interval').value);
  const apiUrl = document.getElementById('apiUrl').value.trim();

  // Xác thực URL API nếu được cung cấp
  let isValidApi = false;
  if (apiUrl) {
    try {
      new URL(apiUrl);
      isValidApi = true;
    } catch (e) {
      console.log('Invalid API URL provided, will skip API calls');
    }
  }

  const A = [pointA.lat, pointA.lon];
  const B = [pointB.lat, pointB.lon];
  
  // Sử dụng khoảng cách tùy chỉnh nếu có (chuyển km thành mét), không thì dùng Haversine
  const distance = pointB.km > 0 ? pointB.km * 1000 : haversine(pointA.lat, pointA.lon, pointB.lat, pointB.lon);

  // === TÍNH TOÁN VẬT LÝ CHO CHUYỂN ĐỘNG THỰC TẾ ===
  let t_acc = maxSpeed / acc;
  let d_acc = 0.5 * acc * t_acc * t_acc;
  let t_total, t_cruise = 0, max_reached_speed = maxSpeed;

  if (2 * d_acc > distance) {
    t_acc = Math.sqrt(distance / acc);
    t_total = 2 * t_acc;
    max_reached_speed = acc * t_acc;
  } else {
    const d_cruise = distance - 2 * d_acc;
    t_cruise = d_cruise / maxSpeed;
    t_total = 2 * t_acc + t_cruise;
  }

  // Hiển thị thông tin mô phỏng
  let out = '';
  if (isSegment) {
    out += `\n=== Segment ${segmentName} ===\n`;
  }
  out += `Distance: ${distance.toFixed(0)}m\n`;
  if (!isSegment) {
    out += `Acceleration time: ${t_acc.toFixed(1)}s\n`;
    out += `Cruise time: ${t_cruise.toFixed(1)}s\n`;
    out += `Max speed reached: ${max_reached_speed.toFixed(1)} m/s (${(max_reached_speed * 3.6).toFixed(1)} km/h)\n`;
    out += `API Endpoint: ${apiUrl || 'None (coordinates will not be sent to API)'}\n\n`;
  }
  out += `Total time: ${t_total.toFixed(1)}s\n\n`;
  document.getElementById('output').value += out;

  // === TÍNH TOÁN TRƯỚC TẤT CẢ VỊ TRÍ GPS VÀ TỐC ĐỘ ===
  let s = 0, t = 0;
  let positions = [];
  let speeds = [];

  // Đảm bảo vị trí đầu tiên luôn là tọa độ chính xác của station A
  positions.push([pointA.lat, pointA.lon]);
  speeds.push(0);

  while (s < distance && t <= t_total) {
    t += interval;
    let v;
    if (t < t_acc) v = acc * t;
    else if (t < t_acc + t_cruise) v = maxSpeed;
    else if (t < t_total) v = maxSpeed - acc * (t - t_acc - t_cruise);
    else v = 0;

    if (v < 0) v = 0;
    let ds = v * interval;
    if (s + ds > distance) ds = distance - s;
    s += ds;
    let frac = s / distance;
    let pos = interpolate(A, B, frac);
    positions.push(pos);
    speeds.push(v);
  }

  // Đảm bảo vị trí cuối cùng luôn là tọa độ chính xác của station B
  if (positions.length > 1) {
    positions[positions.length - 1] = [pointB.lat, pointB.lon];
    speeds[speeds.length - 1] = 0; // Tốc độ = 0 khi dừng ở ga
  }

  // === HIỂN THỊ THEO THỜI GIAN THỰC CỦA TỌA ĐỘ GPS ===
  if (!isSegment) {
    document.getElementById('output').value += `GPS Coordinates (updating every ${interval}s):\n`;
  }

  let currentIndex = 0;
  const printNextCoordinate = () => {
    // Kiểm tra nếu đang trong chế độ segment và mô phỏng đã dừng
    if (isSegment && !simulationRunning) return;
    
    if (currentIndex < positions.length) {
      const p = positions[currentIndex];
      const speed = speeds[currentIndex];
      const timestamp = currentIndex * interval;
      
      const coordText = `t=${timestamp}s: ${p[0].toFixed(8)}, ${p[1].toFixed(8)} | Speed: ${speed.toFixed(1)} m/s\n`;
      document.getElementById('output').value += coordText;
      document.getElementById('output').scrollTop = document.getElementById('output').scrollHeight;
      
      // Gửi API call đồng thời (không chờ response) - concurrent API calls
      if (isValidApi) {
        let stationId = null;
        
        // Gửi stationId của station A chỉ ở lần đầu tiên của segment đầu tiên
        if (isSegment && currentIndex === 0 && currentPointIndex === 0 && pointA.stationId) {
          stationId = pointA.stationId;
        }
        // Gửi stationId của station B chỉ ở lần cuối cùng của mỗi segment
        else if (isSegment && currentIndex === positions.length - 1 && pointB.stationId) {
          stationId = pointB.stationId;
        }
        
        sendCoordinateToAPI(apiUrl, p[0], p[1], timestamp, speed, stationId)
          .catch(error => {
            console.error('Failed to send coordinate to API:', error);
          });
      }
      
      currentIndex++;
      
      if (currentIndex < positions.length) {
        if (isSegment) {
          currentSimulation = setTimeout(printNextCoordinate, interval * 1000);
        } else {
          setTimeout(printNextCoordinate, interval * 1000);
        }
      } else {
        // Mô phỏng hoàn thành
        if (isSegment) {
          const stationName = pointB.stationName && pointB.stationName !== pointB.label 
            ? `${pointB.label} (${pointB.stationName})` 
            : pointB.label;
          document.getElementById('output').value += `\nReached ${stationName}!\n`;
          
          if (currentPointIndex + 1 < allPoints.length - 1) {
            document.getElementById('continueSection').style.display = 'block';
          } else {
            document.getElementById('output').value += '\n=== All destinations reached! ===\n';
            stopSimulation();
          }
        } else {
          document.getElementById('output').value += '\nSimulation completed!';
          if (isValidApi) {
            document.getElementById('output').value += `\nTotal coordinates sent to API: ${positions.length}`;
          }
        }
      }
    }
  };
  
  setTimeout(printNextCoordinate, DEFAULT_CONFIG.COORDINATE_DISPLAY_DELAY);
}

// Hàm để mô phỏng một đoạn đường từ điểm A đến điểm B (sử dụng hàm chung)
function simulateSegment(fromIndex, toIndex) {
  if (!simulationRunning || toIndex >= allPoints.length) {
    stopSimulation();
    return;
  }
  
  const pointA = allPoints[fromIndex];
  const pointB = allPoints[toIndex];
  
  // Tạo tên segment với tên ga nếu có
  const fromName = pointA.stationName && pointA.stationName !== pointA.label 
    ? `${pointA.label} (${pointA.stationName})` 
    : pointA.label;
  const toName = pointB.stationName && pointB.stationName !== pointB.label 
    ? `${pointB.label} (${pointB.stationName})` 
    : pointB.label;
  
  // Gọi hàm chung với chế độ segment
  simulateRoute(pointA, pointB, `${fromName} → ${toName}`, true);
}

// Hàm để mô phỏng đơn giản từ A đến B (dùng cho nút cũ nếu cần)
function simulate() {
  const latA = parseFloat(document.getElementById('latA').value);
  const lonA = parseFloat(document.getElementById('lonA').value);
  const latB = parseFloat(document.getElementById('latB').value);
  const lonB = parseFloat(document.getElementById('lonB').value);
  
  const pointA = { label: 'A', lat: latA, lon: lonA };
  const pointB = { label: 'B', lat: latB, lon: lonB };
  
  document.getElementById('output').value = '';
  simulateRoute(pointA, pointB, '', false);
}
