// wwwroot/js/notification.js

document.addEventListener('DOMContentLoaded', () => {

    const BELL_TRIGGER = document.getElementById('notification-bell-trigger');
    if (!BELL_TRIGGER) {
        return;
    }

    const BELL_BADGE = document.getElementById('notification-count-badge');
    const DROPDOWN_LIST = document.getElementById('notification-dropdown-list');

    /**
     * HÀM 1: Tải số đếm chưa đọc (Giữ nguyên)
     */
    async function loadUnreadCount() {
        try {
            const response = await fetch('/api/notificationweb/unread', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                console.error("Error loading count:", response.statusText);
                return;
            }

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
            console.error("Fetch Error (loadUnreadCount):", error);
        }
    }

    async function loadNotificationList() {
        DROPDOWN_LIST.innerHTML = '<li><div class="dropdown-item text-center text-muted">Loading...</div></li>';

        try {
            const response = await fetch('/api/notificationweb/recent', {
                method: 'GET',
                credentials: 'include'
            });

            if (!response.ok) {
                let errorMsg = `Error: ${response.statusText}`;
                try {
                    const errData = await response.json();
                    errorMsg = errData.message;
                } catch (e) { }
                DROPDOWN_LIST.innerHTML = `<li><div class="dropdown-item text-danger p-3">${errorMsg}</div></li>`;
                return;
            }

            const notifications = await response.json();
            DROPDOWN_LIST.innerHTML = '';

            if (notifications.length === 0) {
                DROPDOWN_LIST.innerHTML = '<li><div class="dropdown-item text-muted text-center p-3">No new notifications.</div></li>';
            } else {
                notifications.forEach(noti => {
                    const timeAgo = new Date(noti.createdDate).toLocaleDateString("vi-VN");
                    const li = document.createElement('li');

                    // === BẮT ĐẦU SỬA (Thêm Icon và Layout) ===

                    // 1. Chọn icon dựa trên trạng thái IsRead
                    const iconHtml = noti.isRead
                        ? '<i class="bi bi-check2-circle text-muted fs-4"></i>'
                        : '<i class="bi bi-bell-fill text-primary fs-4"></i>';

                    // 2. Chỉ in đậm Title (nếu chưa đọc)
                    const titleClass = noti.isRead ? '' : 'fw-bold';

                    // 3. Tạo layout (row/col)
                    li.innerHTML = `
                        <div class="dropdown-item" style="white-space: normal; cursor: pointer;" 
                             data-notification-id="${noti.id}" 
                             data-is-read="${noti.isRead}">
                             
                            <div class="row g-2 align-items-center">
                                <div class="col-auto">
                                    ${iconHtml}
                                </div>
                                <div class="col">
                                    <strong class="${titleClass}">${noti.title}</strong>
                                    <p class="mb-0 small">${noti.message}</p>
                                    <small class="text-muted">${timeAgo}</small>
                                </div>
                            </div>
                        </div>
                    `;
                    // === KẾT THÚC SỬA ===

                    DROPDOWN_LIST.appendChild(li);
                });
            }

            DROPDOWN_LIST.appendChild(document.createElement('hr'));
            DROPDOWN_LIST.innerHTML += '<li><a class="dropdown-item text-center" href="/NotificationWeb/Index">View all notifications</a></li>';

        } catch (error) {
            console.error("Fetch Error (loadNotificationList):", error);
        }
    }

    /**
     * HÀM 3: Đánh dấu TẤT CẢ đã đọc (Sửa: Đổi tên)
     */
    async function markAllNotificationsAsRead() {
        try {
            const response = await fetch('/api/notificationweb/mark-dropdown-read', {
                method: 'POST',
                credentials: 'include'
            });

            if (!response.ok) {
                console.error("Error marking all as read:", response.status, response.statusText);
                return;
            }

            BELL_BADGE.classList.add('d-none');
            BELL_BADGE.textContent = 0;

            // Cập nhật UI: bỏ hết in đậm
            DROPDOWN_LIST.querySelectorAll('.dropdown-item[data-notification-id]').forEach(item => {
                item.querySelector('strong')?.classList.remove('fw-bold');
                item.dataset.isRead = 'true';
            });

        } catch (error) {
            console.error("Fetch Error (markAllNotificationsAsRead):", error);
        }
    }

    /**
     * === THÊM MỚI: HÀM 4: Xử lý click vào MỘT thông báo ===
     */
    async function handleNotificationClick(event) {
        const clickedItem = event.target.closest('.dropdown-item'); // Tìm <div>.dropdown-item

        // Bấm vào "Loading", "View all", hoặc khoảng trống
        if (!clickedItem || !clickedItem.dataset.notificationId) {
            return;
        }

        const notificationId = clickedItem.dataset.notificationId;
        const isRead = (clickedItem.dataset.isRead === 'true');

        // 1. Nếu đã đọc rồi, không làm gì (chỉ cho phép click 1 lần)
        if (isRead) {
            // (Tùy chọn: bạn có thể thêm chuyển trang ở đây)
            // ví dụ: window.location.href = "/TourWeb/TourDetails/..."
            return;
        }

        // 2. Gọi API (FE) mới
        try {
            const response = await fetch(`/api/notificationweb/mark-read-single/${notificationId}`, {
                method: 'POST',
                credentials: 'include'
            });
            if (!response.ok) return;

            const data = await response.json();
            if (data.success) {
                // 3. Cập nhật UI
                clickedItem.querySelector('strong')?.classList.remove('fw-bold');
                clickedItem.dataset.isRead = 'true'; // Đánh dấu là đã đọc

                // 4. Giảm số đếm
                const currentCount = parseInt(BELL_BADGE.textContent || '0');
                const newCount = Math.max(0, currentCount - 1); // Tránh số âm
                BELL_BADGE.textContent = newCount;
                if (newCount === 0) {
                    BELL_BADGE.classList.add('d-none');
                }
            }
        } catch (error) {
            console.error("Error in handleNotificationClick:", error);
        }
    }

    // === KHỞI CHẠY (Sửa: Thêm bộ lắng nghe click) ===
    loadUnreadCount();

    BELL_TRIGGER.addEventListener('show.bs.dropdown', () => {
        loadNotificationList(); // Tải danh sách khi mở
    });

    // Khi menu dropdown bị đóng, chúng ta gọi hàm 'MarkAll'
    // (Bạn có thể TẮT dòng này nếu chỉ muốn đọc khi click)
    BELL_TRIGGER.addEventListener('hide.bs.dropdown', () => {
        // Tạm thời tắt. Bấm "View all" sẽ gọi hàm MarkAll.
        // loadUnreadCount(); // Cập nhật lại số đếm
    });

    // 4. THÊM BỘ LẮNG NGHE SỰ KIỆN CLICK (EVENT DELEGATION)
    // Nó sẽ bắt mọi cú click bên trong dropdown-list
    DROPDOWN_LIST.addEventListener('click', handleNotificationClick);
});