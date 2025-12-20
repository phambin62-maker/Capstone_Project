// admin-accounts.js
(function () {
    'use strict';

    const API_BASE_URL = 'https://localhost:7160/api/admin';

    // Helper: Get JWT token from cookie or meta tag
    function getAuthToken() {
        // Try to get token from cookie
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'JwtToken' || name === 'jwtToken') {
                return decodeURIComponent(value);
            }
        }
        
        // Try to get token from meta tag (if injected by server)
        const tokenMeta = document.querySelector('meta[name="jwt-token"]');
        if (tokenMeta) {
            return tokenMeta.getAttribute('content');
        }
        
        // Try to get token from hidden input (if injected by server)
        const tokenInput = document.getElementById('jwtToken');
        if (tokenInput) {
            return tokenInput.value;
        }
        
        return null;
    }

    // Helper: Get auth headers
    function getAuthHeaders() {
        const token = getAuthToken();
        const headers = {
            "Content-Type": "application/json"
        };
        
        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }
        
        return headers;
    }

    // Helper: Handle API errors (401/403)
    function handleApiError(response) {
        if (response.status === 401) {
            alert('⚠️ Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
            window.location.href = '/AuthWeb/Login';
            return true;
        }
        
        if (response.status === 403) {
            alert('⚠️ Bạn không có quyền truy cập tài nguyên này.');
            window.location.href = '/Home/Forbidden';
            return true;
        }
        
        return false;
    }

    document.addEventListener('DOMContentLoaded', function () {
        console.log('✅ Admin Accounts JS loaded');
        // Load statistics when page loads
        loadStatistics();
        
        // Initialize filter event listeners
        const filterRole = document.getElementById('filterRole');
        const filterStatus = document.getElementById('filterStatus');
        
        if (filterRole) {
            filterRole.addEventListener('change', function() {
                // Auto apply filter on change (optional)
                // applyFilters();
            });
        }
        
        if (filterStatus) {
            filterStatus.addEventListener('change', function() {
                // Auto apply filter on change (optional)
                // applyFilters();
            });
        }
    });

    // Helper: Get form data safely
    function getFormData(form) {
        const formData = new FormData(form);
        const data = {};

        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }

        return data;
    }

    // Save Account
    window.saveAccount = async function () {
        const form = document.getElementById("addAccountForm");
        if (!form) {
            console.error('Form not found!');
            return;
        }

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const data = getFormData(form);

        // Validate required fields
        const email = data.Email || data.email;
        if (!email) {
            alert('❌ Email không được để trống!');
            return;
        }

        const password = data.Password || data.password;
        const confirmPassword = data.ConfirmPassword || data.confirmPassword;

        // ✅ Kiểm tra password match TRƯỚC KHI GỬI
        if (!password || !confirmPassword) {
            alert('❌ Vui lòng nhập mật khẩu và xác nhận mật khẩu!');
            return;
        }

        if (password !== confirmPassword) {
            alert('❌ Mật khẩu xác nhận không khớp!');
            return;
        }
        const payload = {
            FirstName: data.FirstName || '',
            LastName: data.LastName || '',
            Email: email,
            PhoneNumber: data.PhoneNumber || '',
            Password: password,
            ConfirmPassword: confirmPassword,  
            Role: data.Role || 'Customer',
            Username: email.split('@')[0],
            IsActive: document.getElementById("statusSwitch")?.checked || false
        };

        console.log('Sending data:', payload);

        try {
            const response = await fetch(`${API_BASE_URL}/create-account`, {
                method: "POST",
                headers: getAuthHeaders(),
                body: JSON.stringify(payload)
            });

            if (handleApiError(response)) {
                return;
            }

            if (!response.ok) {
                const errorData = await response.json();
                console.error('Backend error:', errorData);

                // Hiển thị lỗi chi tiết từ backend
                let errorMsg = " Lỗi khi thêm tài khoản:\n\n";
                if (errorData.errors) {
                    for (let field in errorData.errors) {
                        errorMsg += `• ${field}: ${errorData.errors[field].join(', ')}\n`;
                    }
                } else {
                    errorMsg += errorData.title || 'Có lỗi xảy ra!';
                }
                alert(errorMsg);
                return;
            }

            const result = await response.json();
            alert("✅ " + (result.message || "Thêm tài khoản thành công!"));

            form.reset();

            const modalElement = document.getElementById("addAccountModal");
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }

            setTimeout(() => location.reload(), 500);

        } catch (error) {
            console.error("Error:", error);
            alert("⚠ Không thể kết nối tới máy chủ backend!");
        }
    };

    // Update Account
    window.updateAccount = async function () {
        const form = document.getElementById("editAccountForm");
        if (!form) {
            console.error('Edit form not found!');
            return;
        }

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const data = getFormData(form);
        const accountId = form.dataset.accountId || data.Id || document.getElementById("editAccountId")?.value;

        if (!accountId) {
            alert(' Không tìm thấy ID tài khoản!');
            return;
        }

        const email = data.Email || data.email;

        const payload = {
            FirstName: data.FirstName || '',
            LastName: data.LastName || '',
            Email: email,
            PhoneNumber: data.PhoneNumber || '',
            Role: data.Role || 'Customer'
        };

        // Add password if reset password is checked
        const resetPasswordCheckbox = document.getElementById('resetPassword');
        if (resetPasswordCheckbox && resetPasswordCheckbox.checked) {
            const password = data.Password || document.getElementById("editPassword")?.value;
            if (!password) {
                alert(' Vui lòng nhập mật khẩu mới!');
                return;
            }
            payload.Password = password;
        }

        console.log('Updating account:', accountId, payload);

        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                method: "PUT",
                headers: getAuthHeaders(),
                body: JSON.stringify(payload)
            });

            if (handleApiError(response)) {
                return;
            }

            if (!response.ok) {
                const err = await response.text();
                alert("❌ Lỗi khi cập nhật tài khoản:\n" + err);
                return;
            }

            const result = await response.json();
            alert("✅ " + (result.message || "Cập nhật tài khoản thành công!"));

            const modalElement = document.getElementById("editAccountModal");
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }

            setTimeout(() => location.reload(), 500);

        } catch (error) {
            console.error("Error:", error);
            alert("cant access");
        }
    };

    // Toggle Account Status
    window.toggleAccountStatus = async function (checkbox, userId) {
        const isActive = checkbox.checked;
        const label = checkbox.nextElementSibling;

        if (!confirm(`Are you sure want ${isActive ? 'active' : 'inactive'} this account?`)) {
            checkbox.checked = !isActive;
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/account/${userId}/status?isActive=${isActive}`, {
                method: "PUT",
                headers: getAuthHeaders()
            });

            if (handleApiError(response)) {
                checkbox.checked = !isActive;
                return;
            }

            if (!response.ok) {
                const err = await response.text();
                alert("❌ Lỗi: " + err);
                checkbox.checked = !isActive;
                return;
            }

            if (isActive) {
                label.classList.remove('text-danger');
                label.classList.add('text-success');
                label.querySelector('small').textContent = 'Active';
            } else {
                label.classList.remove('text-success');
                label.classList.add('text-danger');
                label.querySelector('small').textContent = 'Locked';
            }

            alert(' Update status successfully!');

        } catch (error) {
            console.error("Error:", error);
            alert(" Không thể kết nối tới máy chủ backend!");
            checkbox.checked = !isActive;
        }
    };

    // Confirm Delete
    window.confirmDelete = async function (accountId) {
        if (!confirm('Bạn có chắc chắn muốn xóa tài khoản này? Hành động này không thể hoàn tác!')) {
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                method: "DELETE",
                headers: getAuthHeaders()
            });

            if (handleApiError(response)) {
                return;
            }

            if (!response.ok) {
                const err = await response.json();
                alert("❌ Lỗi khi xóa: " + (err.message || JSON.stringify(err)));
                return;
            }

            alert('✅ Xóa tài khoản thành công!');
            setTimeout(() => location.reload(), 500);

        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Không thể kết nối tới máy chủ backend!");
        }
    };

    // Load Account For Edit
    window.loadAccountForEdit = async function (accountId) {
        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                headers: getAuthHeaders()
            });
            
            if (handleApiError(response)) {
                return;
            }
            
            if (!response.ok) {
                throw new Error('Failed to load account');
            }

            const account = await response.json();
                const form = document.getElementById("editAccountForm");
            
                if (!form) {
                    console.error('Edit form not found!');
                    return;
                }

            // Set account ID
                form.dataset.accountId = accountId;
            const idInput = document.getElementById("editAccountId");
            if (idInput) {
                idInput.value = accountId;
            }

            // Populate form fields
            const firstNameInput = form.querySelector('input[name="FirstName"]');
            if (firstNameInput) firstNameInput.value = account.firstName || '';

            const lastNameInput = form.querySelector('input[name="LastName"]');
            if (lastNameInput) lastNameInput.value = account.lastName || '';

            const emailInput = form.querySelector('input[name="Email"]');
            if (emailInput) emailInput.value = account.email || '';

            const phoneInput = form.querySelector('input[name="PhoneNumber"]');
            if (phoneInput) phoneInput.value = account.phoneNumber || '';

            // Set role select
            const roleSelect = form.querySelector('select[name="Role"]');
            if (roleSelect) {
                roleSelect.value = account.role || '';
            }

            // Reset password field
            const passwordInput = document.getElementById("editPassword");
            if (passwordInput) {
                passwordInput.value = '';
                passwordInput.required = false;
            }
            document.getElementById("resetPassword").checked = false;
            document.getElementById("passwordResetSection").style.display = 'none';
        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Không thể tải thông tin tài khoản!");
        }
    };

    // Toggle Password Field
    window.togglePasswordField = function () {
        const resetCheckbox = document.getElementById("resetPassword");
        const passwordSection = document.getElementById("passwordResetSection");
        const passwordInput = document.getElementById("editPassword");
        
        if (resetCheckbox && passwordSection && passwordInput) {
            if (resetCheckbox.checked) {
                passwordSection.style.display = 'block';
                passwordInput.required = true;
            } else {
                passwordSection.style.display = 'none';
                passwordInput.required = false;
                passwordInput.value = '';
            }
        }
    };

    // Load Statistics
    async function loadStatistics() {
        try {
            const response = await fetch(`${API_BASE_URL}/accounts/statistics`, {
                headers: getAuthHeaders()
            });
            
            if (handleApiError(response)) {
                return;
            }
            
            if (!response.ok) {
                console.error('Failed to load statistics');
                return;
            }

            const stats = await response.json();
            
            // Update statistics cards - find by parent structure
            const statCards = document.querySelectorAll('.stat-card');
            
            if (statCards.length >= 1) {
                const totalElement = statCards[0].querySelector('h3');
                if (totalElement) totalElement.textContent = stats.totalAccounts || 0;
            }
            
            if (statCards.length >= 2) {
                const activeElement = statCards[1].querySelector('h3');
                if (activeElement) {
                    activeElement.textContent = stats.activeAccounts || 0;
                    activeElement.className = 'mb-0 text-success';
                }
            }
            
            if (statCards.length >= 3) {
                const inactiveElement = statCards[2].querySelector('h3');
                if (inactiveElement) {
                    inactiveElement.textContent = stats.inactiveAccounts || 0;
                    inactiveElement.className = 'mb-0 text-danger';
                }
            }
            
            // Hide or update 4th card if newAccountsThisMonth is not available
            if (statCards.length >= 4) {
                const newThisMonthElement = statCards[3].querySelector('h3');
                if (newThisMonthElement) {
                    newThisMonthElement.textContent = stats.newAccountsThisMonth || 0;
                    newThisMonthElement.className = 'mb-0 text-info';
                }
            }
        } catch (error) {
            console.error("Error loading statistics:", error);
        }
    }

    // Load Filtered Accounts
    window.loadFilteredAccounts = async function (roleId = null, status = null, search = null, page = 1, pageSize = 10) {
        try {
            let url = `${API_BASE_URL}/accounts?page=${page}&pageSize=${pageSize}`;
            
            if (roleId) url += `&roleId=${roleId}`;
            if (status) url += `&status=${encodeURIComponent(status)}`;
            if (search) url += `&search=${encodeURIComponent(search)}`;

            const response = await fetch(url, {
                headers: getAuthHeaders()
            });
            
            if (handleApiError(response)) {
                return;
            }
            
            if (!response.ok) {
                throw new Error('Failed to load accounts');
            }

            const result = await response.json();
            
            // Update table with filtered results
            updateAccountsTable(result.data);
            
            // Update pagination
            updatePagination(result.page, result.totalPages);
            
            return result;
        } catch (error) {
            console.error("Error loading filtered accounts:", error);
            alert("⚠️ Không thể tải danh sách tài khoản!");
        }
    };

    // Update Accounts Table
    function updateAccountsTable(accounts) {
        const tbody = document.querySelector('table tbody');
        if (!tbody) return;

        tbody.innerHTML = '';

        if (!accounts || accounts.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Không có tài khoản nào.</td></tr>';
            return;
        }

        accounts.forEach(user => {
            const fullName = `${user.lastName || ''} ${user.firstName || ''}`.trim();
            const roleName = user.roleId === 1 ? 'Admin' : user.roleId === 2 ? 'Staff' : 'Customer';
            const bgColor = user.roleId === 1 ? 'bg-danger' : user.roleId === 2 ? 'bg-primary' : 'bg-secondary';
            const active = user.userStatus === 'Active';
            const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(user.firstName || '')}+${encodeURIComponent(user.lastName || '')}&background=0D8ABC&color=fff`;

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>#${user.id}</td>
                <td>
                    <div class="d-flex align-items-center gap-2">
                        <img src="${avatarUrl}" class="rounded-circle" style="width:40px;height:40px;" alt="Avatar">
                        <strong>${fullName}</strong>
                    </div>
                </td>
                <td>${user.email || ''}</td>
                <td><span class="badge ${bgColor}">${roleName}</span></td>
                <td>
                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" ${active ? 'checked' : ''} 
                               onchange="toggleAccountStatus(this, ${user.id})">
                        <label class="form-check-label ${active ? 'text-success' : 'text-danger'}">
                            <small>${active ? 'Hoạt động' : 'Bị khóa'}</small>
                        </label>
                    </div>
                </td>
                <td class="text-center">
                    <button class="btn btn-sm btn-info" title="Xem chi tiết" onclick="viewAccountDetail(${user.id})">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" title="Chỉnh sửa" 
                            onclick="loadAccountForEdit(${user.id})" data-bs-toggle="modal" data-bs-target="#editAccountModal">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" title="Xóa" onclick="confirmDelete(${user.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });
    }

    // Helper function to get current filter values
    function getCurrentFilters() {
        const roleSelect = document.getElementById('filterRole');
        const statusSelect = document.getElementById('filterStatus');
        const searchInput = document.getElementById('searchInput');
        
        const roleValue = roleSelect?.value || '';
        const statusValue = statusSelect?.value || '';
        const searchValue = searchInput?.value || '';
        
        // Convert role name to roleId
        let roleId = null;
        if (roleValue === 'Admin') roleId = 1;
        else if (roleValue === 'Staff') roleId = 2;
        else if (roleValue === 'Customer') roleId = 3;
        
        return { roleId, status: statusValue, search: searchValue };
    }

    // Helper function to go to page with current filters
    window.goToPage = function (page) {
        const filters = getCurrentFilters();
        loadFilteredAccounts(filters.roleId, filters.status, filters.search, page, 10);
    };

    // Update Pagination
    function updatePagination(currentPage, totalPages) {
        const pagination = document.querySelector('.pagination');
        if (!pagination) return;

        pagination.innerHTML = '';

        // Previous button
        const prevLi = document.createElement('li');
        prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
        if (currentPage > 1) {
            const prevLink = document.createElement('a');
            prevLink.className = 'page-link';
            prevLink.href = '#';
            prevLink.textContent = 'Trước';
            prevLink.onclick = (e) => { e.preventDefault(); goToPage(currentPage - 1); return false; };
            prevLi.appendChild(prevLink);
        } else {
            prevLi.innerHTML = `<a class="page-link" href="#" tabindex="-1">Trước</a>`;
        }
        pagination.appendChild(prevLi);

        // Page numbers (show max 5 pages)
        const maxPages = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxPages / 2));
        let endPage = Math.min(totalPages, startPage + maxPages - 1);
        
        if (endPage - startPage < maxPages - 1) {
            startPage = Math.max(1, endPage - maxPages + 1);
        }

        if (startPage > 1) {
            const firstLi = document.createElement('li');
            firstLi.className = 'page-item';
            const firstLink = document.createElement('a');
            firstLink.className = 'page-link';
            firstLink.href = '#';
            firstLink.textContent = '1';
            firstLink.onclick = (e) => { e.preventDefault(); goToPage(1); return false; };
            firstLi.appendChild(firstLink);
            pagination.appendChild(firstLi);
            
            if (startPage > 2) {
                const ellipsisLi = document.createElement('li');
                ellipsisLi.className = 'page-item disabled';
                ellipsisLi.innerHTML = `<span class="page-link">...</span>`;
                pagination.appendChild(ellipsisLi);
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            const li = document.createElement('li');
            li.className = `page-item ${i === currentPage ? 'active' : ''}`;
            const link = document.createElement('a');
            link.className = 'page-link';
            link.href = '#';
            link.textContent = i.toString();
            link.onclick = (e) => { e.preventDefault(); goToPage(i); return false; };
            li.appendChild(link);
            pagination.appendChild(li);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                const ellipsisLi = document.createElement('li');
                ellipsisLi.className = 'page-item disabled';
                ellipsisLi.innerHTML = `<span class="page-link">...</span>`;
                pagination.appendChild(ellipsisLi);
            }
            
            const lastLi = document.createElement('li');
            lastLi.className = 'page-item';
            const lastLink = document.createElement('a');
            lastLink.className = 'page-link';
            lastLink.href = '#';
            lastLink.textContent = totalPages.toString();
            lastLink.onclick = (e) => { e.preventDefault(); goToPage(totalPages); return false; };
            lastLi.appendChild(lastLink);
            pagination.appendChild(lastLi);
        }

        // Next button
        const nextLi = document.createElement('li');
        nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
        if (currentPage < totalPages) {
            const nextLink = document.createElement('a');
            nextLink.className = 'page-link';
            nextLink.href = '#';
            nextLink.textContent = 'Sau';
            nextLink.onclick = (e) => { e.preventDefault(); goToPage(currentPage + 1); return false; };
            nextLi.appendChild(nextLink);
        } else {
            nextLi.innerHTML = `<a class="page-link" href="#" tabindex="-1">Sau</a>`;
        }
        pagination.appendChild(nextLi);
    }

    // View Account Detail
    window.viewAccountDetail = async function (accountId) {
        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                headers: getAuthHeaders()
            });
            
            if (handleApiError(response)) {
                return;
            }
            
            if (!response.ok) {
                throw new Error('Failed to load account');
            }

            const account = await response.json();
            
            // Show account details in a modal or alert
            alert(`Chi tiết tài khoản:\n\n` +
                  `ID: ${account.id}\n` +
                  `Họ tên: ${account.lastName} ${account.firstName}\n` +
                  `Email: ${account.email}\n` +
                  `Số điện thoại: ${account.phoneNumber || 'N/A'}\n` +
                  `Vai trò: ${account.role}\n` +
                  `Trạng thái: ${account.userStatus}`);
        } catch (error) {
                console.error("Error:", error);
                alert("⚠️ Không thể tải thông tin tài khoản!");
        }
    };

    // Apply Filters
    window.applyFilters = function () {
        const roleSelect = document.getElementById('filterRole');
        const statusSelect = document.getElementById('filterStatus');
        const searchInput = document.getElementById('searchInput');

        const roleValue = roleSelect?.value || '';
        const statusValue = statusSelect?.value || '';
        const searchValue = searchInput?.value || '';

        // Convert role name to roleId
        let roleId = null;
        if (roleValue === 'Admin') roleId = 1;
        else if (roleValue === 'Staff') roleId = 2;
        else if (roleValue === 'Customer') roleId = 3;

        loadFilteredAccounts(roleId, statusValue, searchValue, 1, 10);
    };

    // Handle Search Key Press (Enter key)
    window.handleSearchKeyPress = function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            applyFilters();
        }
    };

})();
