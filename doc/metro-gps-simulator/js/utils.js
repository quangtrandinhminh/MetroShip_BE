// ===== UTILITY FUNCTIONS =====

// Chuyển đổi độ sang radian
function deg2rad(deg) {
  return deg * Math.PI / 180;
}

// Tính toán khoảng cách thực tế giữa hai điểm GPS sử dụng công thức Haversine
function haversine(lat1, lon1, lat2, lon2) {
  const R = 6371000; // Bán kính Trái Đất tính bằng mét
  const dLat = deg2rad(lat2 - lat1); // Chênh lệch vĩ độ
  const dLon = deg2rad(lon2 - lon1); // Chênh lệch kinh độ
  
  // Công thức Haversine để tính khoảng cách trên hình cầu
  const a = Math.sin(dLat/2) ** 2 +
    Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) *
    Math.sin(dLon/2) ** 2;
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  return R * c; // Khoảng cách tính bằng mét
}

// Nội suy tuyến tính giữa hai điểm GPS
// Tìm tọa độ trung gian dọc theo đường thẳng từ A đến B
// frac = 0 có nghĩa là chúng ta đang ở điểm A, frac = 1 có nghĩa là chúng ta đang ở điểm B
// frac = 0.5 có nghĩa là chúng ta đang ở chính giữa giữa A và B
function interpolate(a, b, frac) {
  return [
    a[0] + (b[0] - a[0]) * frac, // Vĩ độ được nội suy
    a[1] + (b[1] - a[1]) * frac  // Kinh độ được nội suy
  ];
}

// Hiển thị thông báo lưu thành công
function showSaveNotification(message = '✓ Saved') {
  // Tạo thông báo nhỏ
  const notification = document.createElement('div');
  notification.style.cssText = `
    position: fixed;
    top: ${DEFAULT_CONFIG.NOTIFICATION_POSITION.top};
    right: ${DEFAULT_CONFIG.NOTIFICATION_POSITION.right};
    background-color: ${DEFAULT_CONFIG.COLORS.SUCCESS};
    color: white;
    padding: 8px 16px;
    border-radius: 4px;
    font-size: 12px;
    z-index: 1000;
    opacity: 0;
    transition: opacity 0.3s ease;
  `;
  notification.textContent = message;
  
  document.body.appendChild(notification);
  
  // Hiệu ứng fade in
  setTimeout(() => notification.style.opacity = '1', 10);
  
  // Tự động ẩn sau thời gian đã cấu hình
  setTimeout(() => {
    notification.style.opacity = '0';
    setTimeout(() => document.body.removeChild(notification), 300);
  }, DEFAULT_CONFIG.NOTIFICATION_DURATION);
}
