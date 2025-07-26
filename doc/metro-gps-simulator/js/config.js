// ===== CONFIGURATION & GLOBAL VARIABLES =====

// Tên file dữ liệu JSON
const DATA_FILE_NAME = './data-demo.json';

// Key cho localStorage
const STORAGE_KEY = 'gps-simulator-data';

// Exported file name cho dữ liệu
const EXPORT_FILE_NAME = 'data-demo-exported.json';

// Biến toàn cục để điều khiển mô phỏng
let currentSimulation = null;
let currentPointIndex = 0;
let allPoints = [];
let simulationRunning = false;
let routeData = null;

// Cấu hình mặc định cho API và thông báo
const DEFAULT_CONFIG = {
  // ID của tàu (để gửi đến API)
  TRAIN_ID: '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0',

  // Thời gian hiển thị thông báo (ms)
  NOTIFICATION_DURATION: 1500,
  
  // Delay trước khi bắt đầu hiển thị tọa độ (ms)
  COORDINATE_DISPLAY_DELAY: 500,
  
  // Notification position
  NOTIFICATION_POSITION: {
    top: '20px',
    right: '20px'
  },
  
  // Colors
  COLORS: {
    SUCCESS: '#28a745',
    WARNING: '#ffc107',
    DANGER: '#dc3545',
    INFO: '#17a2b8'
  }
};
