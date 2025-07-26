// ===== DATA MANAGEMENT FUNCTIONS =====

// Tải dữ liệu tuyến đường khi trang được tải
async function loadRouteData() {
  try {
    // Kiểm tra dữ liệu đã lưu trong localStorage trước
    if (loadDataFromStorage()) {
      populateRouteDropdown();
      return;
    }
    
    // Nếu không có dữ liệu đã lưu, tải từ file
    const response = await fetch(DATA_FILE_NAME);
    routeData = await response.json();
    
    // Lưu vào localStorage lần đầu
    saveDataToFileInstantly();
    
    populateRouteDropdown();
  } catch (error) {
    console.error('Error loading route data:', error);
    // Dự phòng: ẩn phần dropdown nếu không tải được dữ liệu
    document.getElementById('routeDropdownSection').style.display = 'none';
    document.querySelector('input[value="dropdown"]').disabled = true;
  }
}

// Hàm để tải dữ liệu từ localStorage khi khởi động
function loadDataFromStorage() {
  try {
    const savedData = localStorage.getItem(STORAGE_KEY);
    if (savedData) {
      const parsedData = JSON.parse(savedData);
      // Merge với dữ liệu hiện tại, ưu tiên dữ liệu đã lưu
      routeData = parsedData;
      console.log('Data loaded from localStorage');
      return true;
    }
  } catch (error) {
    console.error('Error loading data from storage:', error);
  }
  return false;
}

// Hàm để lưu dữ liệu ngay lập tức (không download)
function saveDataToFileInstantly() {
  if (!routeData) {
    console.error('No data to save');
    return;
  }
  
  try {
    // Lưu vào localStorage để persist data
    localStorage.setItem(STORAGE_KEY, JSON.stringify(routeData));
    console.log('Data saved to localStorage successfully');
    
    // Hiển thị thông báo nhỏ
    showSaveNotification();
  } catch (error) {
    console.error('Error saving data:', error);
  }
}

// Hàm export dữ liệu (tùy chọn cho người dùng muốn download)
function exportData() {
  if (!routeData) {
    alert('No data to export');
    return;
  }
  
  try {
    // Tạo dữ liệu JSON
    const jsonData = JSON.stringify(routeData, null, 2);
    
    // Tạo blob và download
    const blob = new Blob([jsonData], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    
    // Tạo link tạm thời để download
    const a = document.createElement('a');
    a.href = url;
    a.download = EXPORT_FILE_NAME;
    document.body.appendChild(a);
    a.click();
    
    // Cleanup
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    showSaveNotification('Data exported successfully!');
  } catch (error) {
    alert('Error exporting data: ' + error.message);
    console.error('Export error:', error);
  }
}
