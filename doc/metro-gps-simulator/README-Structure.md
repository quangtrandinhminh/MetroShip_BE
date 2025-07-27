# GPS Simulator - Cấu trúc File

## Chạy ứng dung
Để chạy ứng dụng, bạn cần mở file `index.html` trong trình duyệt.

## Cấu trúc File

### 📁 js/
Thư mục chứa tất cả các module JavaScript

#### 🔧 config.js
- **Chức năng**: Cấu hình và biến toàn cục
- **Nội dung**:
  - Tên file dữ liệu (`DATA_FILE_NAME`)
  - Key localStorage (`STORAGE_KEY`)
  - Tên file export (`EXPORT_FILE_NAME`)
  - Biến toàn cục mô phỏng
  - Cấu hình mặc định (thời gian, màu sắc, vị trí)

#### 🛠️ utils.js
- **Chức năng**: Các hàm tiện ích toán học và UI
- **Nội dung**:
  - `deg2rad()` - Chuyển đổi độ sang radian
  - `haversine()` - Tính khoảng cách GPS
  - `interpolate()` - Nội suy tuyến tính
  - `showSaveNotification()` - Hiển thị thông báo

#### 🌐 api.js
- **Chức năng**: Quản lý API calls
- **Nội dung**:
  - `sendCoordinateToAPI()` - Gửi tọa độ đến API endpoint

#### 💾 data-manager.js
- **Chức năng**: Quản lý dữ liệu (load/save/export)
- **Nội dung**:
  - `loadRouteData()` - Tải dữ liệu tuyến đường
  - `loadDataFromStorage()` - Tải từ localStorage
  - `saveDataToFileInstantly()` - Lưu tức thì
  - `exportData()` - Xuất dữ liệu

#### 🎨 ui-manager.js
- **Chức năng**: Quản lý giao diện người dùng
- **Nội dung**:
  - `populateRouteDropdown()` - Điền dropdown tuyến đường
  - `toggleRouteMode()` - Chuyển đổi chế độ
  - `updateStationList()` - Cập nhật danh sách ga
  - `toggleStationActive()` - Bật/tắt trạng thái ga

#### 📍 point-manager.js
- **Chức năng**: Quản lý điểm và tọa độ
- **Nội dung**:
  - `addPoint()` - Thêm điểm mới
  - `removePoint()` - Xóa điểm
  - `updatePointLabels()` - Cập nhật nhãn
  - `getAllPoints()` - Lấy tất cả điểm
  - `loadRouteStations()` - Tải ga từ tuyến

#### 🚂 simulation-engine.js
- **Chức năng**: Công cụ mô phỏng chính
- **Nội dung**:
  - `simulateRoute()` - Mô phỏng tuyến đường
  - `simulateSegment()` - Mô phỏng đoạn
  - `simulate()` - Mô phỏng đơn giản A→B

#### 🎮 simulation-controls.js
- **Chức năng**: Điều khiển mô phỏng
- **Nội dung**:
  - `startSimulation()` - Bắt đầu mô phỏng
  - `continueToNext()` - Tiếp tục điểm tiếp theo
  - `stopSimulation()` - Dừng mô phỏng
  - `resetSimulation()` - Reset mô phỏng

#### 🚀 main.js
- **Chức năng**: Khởi tạo ứng dụng
- **Nội dung**:
  - Event listener DOMContentLoaded
  - Export functions cho global scope

## Thứ tự load File

Các file được load theo thứ tự trong `index.html`:

1. **config.js** - Cấu hình và biến toàn cục
2. **utils.js** - Hàm tiện ích cơ bản
3. **api.js** - Chức năng API
4. **data-manager.js** - Quản lý dữ liệu
5. **ui-manager.js** - Quản lý UI
6. **point-manager.js** - Quản lý điểm
7. **simulation-engine.js** - Engine mô phỏng
8. **simulation-controls.js** - Điều khiển mô phỏng
9. **main.js** - Khởi tạo cuối cùng

## Lợi ích của cấu trúc mới

### ✅ Ưu điểm
- **Modular**: Mỗi file có chức năng riêng biệt
- **Dễ bảo trì**: Tìm và sửa lỗi dễ dàng hơn
- **Tái sử dụng**: Có thể sử dụng lại các module
- **Cấu hình tập trung**: Tất cả config ở `config.js`
- **Comments đầy đủ**: Giữ nguyên tất cả comments

### 🔧 Cách chỉnh sửa

#### Thay đổi cấu hình:
- Mở `js/config.js`
- Chỉnh sửa `DATA_FILE_NAME`, `STORAGE_KEY`, etc.

#### Thay đổi API:
- Mở `js/api.js`
- Sửa function `sendCoordinateToAPI()`

#### Thay đổi UI:
- Mở `js/ui-manager.js`
- Sửa các function UI liên quan

#### Thay đổi logic mô phỏng:
- Mở `js/simulation-engine.js`
- Sửa function `simulateRoute()`
