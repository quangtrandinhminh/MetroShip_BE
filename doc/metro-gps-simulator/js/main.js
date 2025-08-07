// ===== MAIN APPLICATION INITIALIZATION =====

// Khởi tạo khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
  loadRouteData();
});

// Export các hàm global cần thiết để sử dụng trong HTML
window.startSimulation = startSimulation;
window.continueToNext = continueToNext;
window.stopSimulation = stopSimulation;
window.resetSimulation = resetSimulation;
window.simulate = simulate;
window.addPoint = addPoint;
window.removePoint = removePoint;
window.toggleRouteMode = toggleRouteMode;
window.updateStationList = updateStationList;
window.exportData = exportData;
