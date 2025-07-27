// ===== API FUNCTIONS =====

// Hàm gửi tọa độ GPS đến API endpoint (concurrent)
async function sendCoordinateToAPI(apiUrl, latitude, longitude, timestamp, currentSpeed, stationId = null) {
  try {
    // Lấy trainId từ UI input thay vì config
    const trainId = document.getElementById('trainId').value.trim() || DEFAULT_CONFIG.TRAIN_ID;
    
    console.log(`Sending coordinate to API (t=${timestamp}s): ${latitude}, ${longitude}${stationId ? ` - Station: ${stationId}` : ''}`);
    
    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        trainId: trainId,
        trackingCode: null, // nullable: true
        latitude: latitude,
        longitude: longitude,
        stationId: stationId, // nullable: true - sẽ có giá trị khi tàu dừng ở ga
        timestamp: new Date().toISOString(), // string($date-time) format
        // speed: currentSpeed, // Tốc độ hiện tại tính bằng m/s
        // speedKmh: (currentSpeed * 3.6).toFixed(1) // Tốc độ tính bằng km/h để tiện lợi
      })
    });
    
    if (!response.ok) {
      console.warn(`API call failed (t=${timestamp}s): ${response.status} ${response.statusText}`);
    } else {
      console.log(`API call successful (t=${timestamp}s)`);
    }
  } catch (error) {
    console.error(`Error sending coordinate to API (t=${timestamp}s):`, error);
  }
}
