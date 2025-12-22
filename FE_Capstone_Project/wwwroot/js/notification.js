// wwwroot/js/notification.js

document.addEventListener('DOMContentLoaded', () => {
    initCustomerNotification();
    initStaffNotification();
});

function formatVietnamTime(dateString) {
    if (!dateString) return '';
    if (dateString.indexOf('Z') === -1 && dateString.indexOf('+') === -1) {
        dateString += 'Z';
    }
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return dateString;
    return date.toLocaleString("vi-VN", {
        timeZone: "Asia/Ho_Chi_Minh",
        hour: '2-digit', minute: '2-digit', second: '2-digit',
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour12: false
    });
}

function initCustomerNotification() {
    const TRIGGER = document.getElementById('notification-bell-trigger');
    const BADGE = document.getElementById('notification-count-badge');
    const LIST = document.getElementById('notification-dropdown-list');
    if (!TRIGGER || !LIST) return;

    function hideFrame() { LIST.style.display = "none"; LIST.innerHTML = ""; }
    function showFrame() { LIST.style.display = "block"; }

    async function loadCount() {
        try {
            const res = await fetch('/api/notificationweb/unread', { method: 'GET' });
            if (!res.ok) return;
            const data = await res.json();
            const count = data.count || 0;
            if (count > 0) {
                BADGE.textContent = count;
                BADGE.classList.remove('d-none');
            } else {
                BADGE.classList.add('d-none');
            }
        } catch (e) { }
    }

    async function loadList() {
        LIST.style.removeProperty("display");
        LIST.style.padding = "";
        LIST.innerHTML = '<li><div class="dropdown-item text-center text-muted p-3">Loading...</div></li>';
        try {
            const res = await fetch('/api/notificationweb/recent', { method: 'GET' });
            if (!res.ok) throw new Error();
            const notifications = await res.json();
            LIST.innerHTML = '';
            if (notifications.length === 0) {
                LIST.innerHTML = '<li><div class="dropdown-item text-muted text-center p-3">No new notifications.</div></li>';
            } else {
                LIST.innerHTML += `<li><h6 class="dropdown-header">Recent Notifications</h6></li><li><hr class="dropdown-divider"></li>`;

                notifications.forEach(noti => {
                    const timeAgo = formatVietnamTime(noti.createdDate);
                    const iconHtml = noti.isRead ? '<i class="bi bi-check2-circle text-muted fs-4"></i>' : '<i class="bi bi-bell-fill text-primary fs-4"></i>';
                    const titleClass = noti.isRead ? '' : 'fw-bold';

                    // --- [MỚI] Tách ID Booking từ tin nhắn ---
                    let bookingId = null;
                    const combinedText = (noti.title + " " + noti.message);
                    const match = combinedText.match(/Booking #(\d+)/i);
                    if (match && match[1]) bookingId = match[1];

                    // --- [MỚI] Tạo Link đến trang My Bookings ---
                    // Thay '/BookingWeb/MyBookings' bằng URL thật của bạn nếu khác
                    const linkUrl = bookingId ? `/Profile/MyBookings#booking-${bookingId}` : '#';
                    const linkClass = bookingId ? '' : 'text-decoration-none cursor-default';

                    const li = document.createElement('li');
                    // Sử dụng thẻ <a> thay vì <div> để có thể click chuyển trang
                    li.innerHTML = `
                        <a class="dropdown-item ${linkClass}" href="${linkUrl}" 
                             data-notification-id="${noti.id}" 
                             data-is-read="${noti.isRead}"
                             style="white-space: normal; cursor: pointer;">
                            <div class="row g-2 align-items-center">
                                <div class="col-auto">${iconHtml}</div>
                                <div class="col">
                                    <strong class="${titleClass}">${noti.title}</strong>
                                    <p class="mb-0 small">${noti.message}</p>
                                    <small class="text-muted" style="font-size: 0.75rem;">${timeAgo}</small>
                                </div>
                            </div>
                        </a>`;
                    LIST.appendChild(li);
                    LIST.appendChild(document.createElement('li')).innerHTML = '<hr class="dropdown-divider">';
                });

                LIST.innerHTML += '<li><a class="dropdown-item text-center small text-primary" href="/NotificationWeb/Index">View all notifications</a></li>';
            }
        } catch (e) {
            LIST.innerHTML = `<li><div class="dropdown-item text-danger p-3">Failed to load.</div></li>`;
        }
    }

    LIST.addEventListener('click', async (e) => {
        // [SỬA] Tìm thẻ 'a' thay vì 'div'
        const item = e.target.closest('a.dropdown-item');
        if (!item || !item.dataset.notificationId) return;

        const notiId = item.dataset.notificationId;
        const targetUrl = item.getAttribute('href');
        const shouldRedirect = (targetUrl && targetUrl !== '#' && targetUrl !== '');

        // Chặn chuyển trang ngay để gọi API đánh dấu đã đọc trước
        if (shouldRedirect) e.preventDefault();

        try {
            if (item.dataset.isRead !== 'true') {
                await fetch(`/api/notificationweb/mark-read-single/${notiId}`, { method: 'POST' });

                // Cập nhật giao diện (bỏ in đậm)
                item.querySelector('strong')?.classList.remove('fw-bold');
                item.dataset.isRead = 'true';
                item.querySelector('.col-auto').innerHTML = '<i class="bi bi-check2-circle text-muted fs-4"></i>';

                const newCount = Math.max(0, parseInt(BADGE.textContent || '0') - 1);
                BADGE.textContent = newCount;
                if (newCount === 0) BADGE.classList.add('d-none');
            }
        } catch (e) { }
        finally {
            // Chuyển trang sau khi xử lý xong
            if (shouldRedirect) window.location.href = targetUrl;
        }
    });

    loadCount();
    LIST.style.padding = "0"; LIST.style.border = "none";
    TRIGGER.addEventListener('show.bs.dropdown', loadList);
    TRIGGER.addEventListener('hidden.bs.dropdown', () => {
        LIST.style.padding = "0"; LIST.style.border = "none"; LIST.innerHTML = "";
    });
}

function initStaffNotification() {
    const TRIGGER = document.getElementById('staffNotiTrigger');
    const BADGE = document.getElementById('staffNotiBadge');
    const LIST = document.getElementById('staffNotiList');

    if (!TRIGGER || !LIST) return;

    async function loadCount() {
        try {
            const res = await fetch('/api/NotificationWeb/staff-count', { method: 'GET' });
            if (!res.ok) return;
            const data = await res.json();
            const count = data.count || 0;
            if (count > 0) {
                BADGE.textContent = count;
                BADGE.classList.remove('d-none');
            } else {
                BADGE.classList.add('d-none');
            }
        } catch (e) { }
    }

    async function loadList() {
        LIST.style.removeProperty("display");
        LIST.style.padding = "";
        LIST.innerHTML = '<li><div class="dropdown-item text-center text-muted p-3">Checking...</div></li>';

        try {
            const res = await fetch('/api/NotificationWeb/staff-alerts', { method: 'GET' });
            if (!res.ok) throw new Error();
            const alerts = await res.json();
            LIST.innerHTML = '';

            if (alerts.length === 0) {
                LIST.innerHTML = '<li><div class="dropdown-item text-muted text-center p-3">No pending alerts.</div></li>';
            } else {
                LIST.innerHTML += `<li><h6 class="dropdown-header">Booking Updates</h6></li><li><hr class="dropdown-divider"></li>`;

                alerts.forEach(item => {
                    const timeAgo = formatVietnamTime(item.createdDate);

                    let bookingId = null;
                    const combinedText = (item.title + " " + item.message);
                    const match = combinedText.match(/Booking #(\d+)/i);
                    if (match && match[1]) bookingId = match[1];

                    const linkUrl = bookingId ? `/StaffBooking/Details/${bookingId}` : '#';
                    const linkClass = bookingId ? '' : 'text-decoration-none cursor-default';

                    const titleStyle = !item.isRead ? 'fw-bold text-dark' : 'fw-normal text-secondary';
                    const bgStyle = !item.isRead ? 'background-color: #f0f7ff;' : ''; 

                    const li = document.createElement('li');
                    li.innerHTML = `
                        <a class="dropdown-item py-2 ${linkClass}" href="${linkUrl}" 
                           data-notification-id="${item.id}" 
                           data-is-read="${item.isRead}"
                           style="${bgStyle}">
                            <div class="d-flex align-items-start">
                                <div class="me-2 mt-1">
                                    ${!item.isRead ? '<i class="bi bi-circle-fill text-primary" style="font-size: 8px;"></i>' : ''}
                                    <i class="bi bi-exclamation-circle-fill text-warning"></i>
                                </div>
                                <div>
                                    <div class="${titleStyle} small">${item.title}</div>
                                    <div class="small text-secondary" style="white-space: normal;">${item.message}</div>
                                    <div class="text-muted" style="font-size: 0.75rem;">${timeAgo}</div>
                                </div>
                            </div>
                        </a>`;
                    LIST.appendChild(li);
                    LIST.appendChild(document.createElement('li')).innerHTML = '<hr class="dropdown-divider">';
                });

                LIST.innerHTML += '<li><a class="dropdown-item text-center small text-primary fw-bold" href="/StaffNotificationWeb/Index">View all notifications</a></li>';
            }
        } catch (e) {
            LIST.innerHTML = `<li><div class="dropdown-item text-danger p-3">Error loading.</div></li>`;
        }
    }

    LIST.addEventListener('click', async (e) => {
        const item = e.target.closest('a.dropdown-item');
        if (!item || !item.dataset.notificationId) return;

        const notiId = item.dataset.notificationId;
        const targetUrl = item.getAttribute('href');
        const shouldRedirect = (targetUrl && targetUrl !== '#' && targetUrl !== '');

        if (shouldRedirect) e.preventDefault();

        try {
            await fetch(`/api/notificationweb/mark-read-single/${notiId}`, { method: 'POST' });

            const titleDiv = item.querySelector('.fw-bold');
            if (titleDiv) {
                titleDiv.classList.remove('fw-bold', 'text-dark');
                titleDiv.classList.add('fw-normal', 'text-secondary');
            }
            item.style.backgroundColor = ''; 
            item.dataset.isRead = "true";

            const currentCount = parseInt(BADGE.textContent || '0');
            const newCount = Math.max(0, currentCount - 1);
            BADGE.textContent = newCount;
            if (newCount === 0) BADGE.classList.add('d-none');
        } catch (err) { console.error(err); }
        finally {
            if (shouldRedirect) window.location.href = targetUrl;
        }
    });

    loadCount();
    LIST.style.padding = "0"; LIST.style.border = "none";
    TRIGGER.addEventListener('show.bs.dropdown', loadList);
    TRIGGER.addEventListener('hidden.bs.dropdown', () => {
        LIST.style.padding = "0"; LIST.style.border = "none"; LIST.innerHTML = "";
    });
}