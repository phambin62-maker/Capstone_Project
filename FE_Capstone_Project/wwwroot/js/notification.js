// wwwroot/js/notification.js

document.addEventListener('DOMContentLoaded', () => {

    // Tìm chuông và badge
    const BELL_TRIGGER = document.getElementById('notification-bell-trigger');

    // Nếu không tìm thấy chuông (người dùng chưa đăng nhập), thì dừng lại
    if (!BELL_TRIGGER) {
        return;
    }

    const BELL_BADGE = document.getElementById('notification-count-badge');

    /**
     * 1. Hàm tải số đếm thông báo chưa đọc
     * Gọi API FE: GET /api/notificationweb/unread
     */
    async function loadUnreadCount() {
        try {
            const response = await fetch('/api/notificationweb/unread');
            if (!response.ok) return;

            const data = await response.json();
            const count = data.count || 0;

            if (count > 0) {
                BELL_BADGE.textContent = count;
                BELL_BADGE.classList.remove('d-none');
            } else {
                BELL_BADGE.classList.add('d-none');
                BELL_BADGE.textContent = 0;
            }
        } catch (error) {
            console.error("Error loading notification count:", error);
        }
    }

    // Khởi tạo:
    loadUnreadCount();

    // (Đã xóa logic loadNotificationList() và markAllAsRead() 
    //  vì chúng đã được chuyển vào Controller Index)
});