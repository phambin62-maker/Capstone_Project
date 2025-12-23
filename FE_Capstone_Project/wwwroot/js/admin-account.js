// admin-accounts.js
(function () {
    'use strict';

    const API_BASE_URL = 'https://localhost:7160/api/admin';
    const PAGE_SIZE = 10;
    let currentPage = 1;

    /* ================= AUTH ================= */

    function getAuthToken() {
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'JwtToken' || name === 'jwtToken') {
                return decodeURIComponent(value);
            }
        }

        const meta = document.querySelector('meta[name="jwt-token"]');
        if (meta) return meta.content;

        const hidden = document.getElementById('jwtToken');
        if (hidden) return hidden.value;

        return null;
    }

    function getAuthHeaders() {
        const headers = { "Content-Type": "application/json" };
        const token = getAuthToken();
        if (token) headers.Authorization = `Bearer ${token}`;
        return headers;
    }

    function handleApiError(response) {
        if (response.status === 401) {
            alert('⚠️ Session expired. Please login again.');
            window.location.href = '/AuthWeb/Login';
            return true;
        }

        if (response.status === 403) {
            alert('⚠️ You do not have permission to access this resource.');
            window.location.href = '/Home/Forbidden';
            return true;
        }
        return false;
    }

    /* ================= HELPER ================= */

    function getFormData(form) {
        const formData = new FormData(form);
        const data = {};
        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }
        return data;
    }

    function getCurrentFilters() {
        const roleSelect = document.getElementById('filterRole');
        const statusSelect = document.getElementById('filterStatus');
        const searchInput = document.getElementById('searchInput');

        const roleValue = roleSelect?.value || '';
        const statusValue = statusSelect?.value || '';
        const searchValue = searchInput?.value || '';

        let roleId = null;
        if (roleValue === 'Admin') roleId = 1;
        else if (roleValue === 'Staff') roleId = 2;
        else if (roleValue === 'Customer') roleId = 3;

        return { roleId, status: statusValue, search: searchValue };
    }

    /* ================= INIT ================= */

    document.addEventListener('DOMContentLoaded', () => {
        console.log('✅ Admin Accounts JS loaded');
        loadStatistics();
        loadAccounts(1);

        const filterRole = document.getElementById('filterRole');
        const filterStatus = document.getElementById('filterStatus');

        if (filterRole) {
            filterRole.addEventListener('change', () => {
                // Auto apply filter on change (optional)
                // applyFilters();
            });
        }

        if (filterStatus) {
            filterStatus.addEventListener('change', () => {
                // Auto apply filter on change (optional)
                // applyFilters();
            });
        }
    });

    /* ================= LOAD DATA ================= */

    async function loadAccounts(page) {
        try {
            currentPage = page;
            const filters = getCurrentFilters();

            let url = `${API_BASE_URL}/accounts?page=${currentPage}&pageSize=${PAGE_SIZE}`;
            if (filters.roleId !== null) url += `&roleId=${filters.roleId}`;
            if (filters.status) url += `&status=${encodeURIComponent(filters.status)}`;
            if (filters.search) url += `&search=${encodeURIComponent(filters.search)}`;

            const res = await fetch(url, { headers: getAuthHeaders() });
            if (handleApiError(res)) return;

            const result = await res.json();

            renderTable(result.data);
            renderPagination(result.totalPages);
        } catch (e) {
            console.error(e);
            alert('Unable to load account list.');
        }
    }

    async function loadStatistics() {
        try {
            const response = await fetch(`${API_BASE_URL}/accounts/statistics`, {
                headers: getAuthHeaders()
            });

            if (handleApiError(response)) return;

            if (!response.ok) {
                console.error('Failed to load statistics');
                return;
            }

            const stats = await response.json();

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

    /* ================= TABLE ================= */

    function renderTable(data) {
        const tbody = document.querySelector('table tbody');
        if (!tbody) return;

        tbody.innerHTML = '';

        if (!data || data.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-muted">
                        No accounts found.
                    </td>
                </tr>`;
            return;
        }

        data.forEach(user => {
            const fullName = `${user.lastName || ''} ${user.firstName || ''}`.trim();
            const roleName = user.roleId === 1 ? 'Admin' : user.roleId === 2 ? 'Staff' : 'Customer';
            const bgColor = user.roleId === 1 ? 'bg-danger' : user.roleId === 2 ? 'bg-primary' : 'bg-secondary';
            const active = user.userStatus === 'Active';
            const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(user.firstName || '')}+${encodeURIComponent(user.lastName || '')}&background=0D8ABC&color=fff`;

            const tr = document.createElement('tr');
            tr.innerHTML = `
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
                            <small>${active ? 'Active' : 'Locked'}</small>
                        </label>
                    </div>
                </td>
                <td class="text-center">
                    <button class="btn btn-sm btn-info" title="View Details" onclick="viewAccountDetail(${user.id})">
                        <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" title="Edit" 
                            onclick="loadAccountForEdit(${user.id})" data-bs-toggle="modal" data-bs-target="#editAccountModal">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" title="Delete" onclick="confirmDelete(${user.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>`;
            tbody.appendChild(tr);
        });
    }

    /* ================= PAGINATION ================= */

    function renderPagination(totalPages) {
        const ul = document.querySelector('.pagination');
        if (!ul) return;

        ul.innerHTML = '';

        const add = (label, page, disabled = false, active = false) => {
            const li = document.createElement('li');
            li.className = `page-item ${disabled ? 'disabled' : ''} ${active ? 'active' : ''}`;
            const a = document.createElement('a');
            a.className = 'page-link';
            a.href = '#';
            a.textContent = label;
            if (!disabled) {
                a.onclick = e => {
                    e.preventDefault();
                    loadAccounts(page);
                };
            }
            li.appendChild(a);
            ul.appendChild(li);
        };

        add('Previous', currentPage - 1, currentPage === 1);

        // Show max 5 pages
        const maxPages = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxPages / 2));
        let endPage = Math.min(totalPages, startPage + maxPages - 1);

        if (endPage - startPage < maxPages - 1) {
            startPage = Math.max(1, endPage - maxPages + 1);
        }

        if (startPage > 1) {
            add('1', 1);
            if (startPage > 2) {
                const ellipsis = document.createElement('li');
                ellipsis.className = 'page-item disabled';
                ellipsis.innerHTML = `<span class="page-link">...</span>`;
                ul.appendChild(ellipsis);
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            add(i, i, false, i === currentPage);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                const ellipsis = document.createElement('li');
                ellipsis.className = 'page-item disabled';
                ellipsis.innerHTML = `<span class="page-link">...</span>`;
                ul.appendChild(ellipsis);
            }
            add(totalPages.toString(), totalPages);
        }

        add('Next', currentPage + 1, currentPage === totalPages);
    }

    window.goToPage = function (page) {
        loadAccounts(page);
    };

    /* ================= FILTER ================= */

    window.applyFilters = function () {
        currentPage = 1;
        loadAccounts(currentPage);
    };

    window.handleSearchKeyPress = function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            applyFilters();
        }
    };

    /* ================= ACTIONS ================= */

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

        const email = data.Email || data.email;
        if (!email) {
            alert('❌ Email is required!');
            return;
        }

        const password = data.Password || data.password;
        const confirmPassword = data.ConfirmPassword || data.confirmPassword;

        if (!password || !confirmPassword) {
            alert('❌ Please enter password and confirm password!');
            return;
        }

        if (password !== confirmPassword) {
            alert('❌ Passwords do not match!');
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

            if (handleApiError(response)) return;

            if (!response.ok) {
                const errorData = await response.json();
                console.error('Backend error:', errorData);

                let errorMsg = "❌ Error adding account:\n\n";
                if (errorData.errors) {
                    for (let field in errorData.errors) {
                        errorMsg += `• ${field}: ${errorData.errors[field].join(', ')}\n`;
                    }
                } else {
                    errorMsg += errorData.title || 'An error occurred!';
                }
                alert(errorMsg);
                return;
            }

            const result = await response.json();
            alert("✅ " + (result.message || "Account added successfully!"));

            form.reset();

            const modalElement = document.getElementById("addAccountModal");
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }

            setTimeout(() => location.reload(), 500);

        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Cannot connect to backend server!");
        }
    };

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
            alert('❌ Account ID not found!');
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

        const resetPasswordCheckbox = document.getElementById('resetPassword');
        if (resetPasswordCheckbox && resetPasswordCheckbox.checked) {
            const password = data.Password || document.getElementById("editPassword")?.value;
            if (!password) {
                alert('❌ Please enter new password!');
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

            if (handleApiError(response)) return;

            if (!response.ok) {
                const err = await response.text();
                alert("❌ Error updating account:\n" + err);
                return;
            }

            const result = await response.json();
            alert("✅ " + (result.message || "Account updated successfully!"));

            const modalElement = document.getElementById("editAccountModal");
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }

            setTimeout(() => location.reload(), 500);

        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Cannot connect to backend server!");
        }
    };

    window.toggleAccountStatus = async function (checkbox, userId) {
        const isActive = checkbox.checked;
        const label = checkbox.nextElementSibling;

        if (!confirm(`Are you sure you want to ${isActive ? 'activate' : 'deactivate'} this account?`)) {
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
                alert("❌ Error: " + err);
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

            alert('✅ Status updated successfully!');

        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Cannot connect to backend server!");
            checkbox.checked = !isActive;
        }
    };

    window.confirmDelete = async function (accountId) {
        if (!confirm('Are you sure you want to delete this account? This action cannot be undone!')) {
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                method: "DELETE",
                headers: getAuthHeaders()
            });

            if (handleApiError(response)) return;

            if (!response.ok) {
                const err = await response.json();
                alert("❌ Error deleting account: " + (err.message || JSON.stringify(err)));
                return;
            }

            alert('✅ Account deleted successfully!');
            setTimeout(() => location.reload(), 500);

        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Cannot connect to backend server!");
        }
    };

    window.loadAccountForEdit = async function (accountId) {
        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                headers: getAuthHeaders()
            });

            if (handleApiError(response)) return;

            if (!response.ok) {
                throw new Error('Failed to load account');
            }

            const account = await response.json();
            const form = document.getElementById("editAccountForm");

            if (!form) {
                console.error('Edit form not found!');
                return;
            }

            form.dataset.accountId = accountId;
            const idInput = document.getElementById("editAccountId");
            if (idInput) idInput.value = accountId;

            const firstNameInput = form.querySelector('input[name="FirstName"]');
            if (firstNameInput) firstNameInput.value = account.firstName || '';

            const lastNameInput = form.querySelector('input[name="LastName"]');
            if (lastNameInput) lastNameInput.value = account.lastName || '';

            const emailInput = form.querySelector('input[name="Email"]');
            if (emailInput) emailInput.value = account.email || '';

            const phoneInput = form.querySelector('input[name="PhoneNumber"]');
            if (phoneInput) phoneInput.value = account.phoneNumber || '';

            const roleSelect = form.querySelector('select[name="Role"]');
            if (roleSelect) roleSelect.value = account.role || '';

            const passwordInput = document.getElementById("editPassword");
            if (passwordInput) {
                passwordInput.value = '';
                passwordInput.required = false;
            }

            const resetCheckbox = document.getElementById("resetPassword");
            const passwordSection = document.getElementById("passwordResetSection");
            if (resetCheckbox) resetCheckbox.checked = false;
            if (passwordSection) passwordSection.style.display = 'none';
        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Cannot load account information!");
        }
    };

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

    window.viewAccountDetail = async function (accountId) {
        try {
            const response = await fetch(`${API_BASE_URL}/account/${accountId}`, {
                headers: getAuthHeaders()
            });

            if (handleApiError(response)) return;

            if (!response.ok) {
                throw new Error('Failed to load account');
            }

            const account = await response.json();

            alert(`Account Details:\n\n` +
                `ID: ${account.id}\n` +
                `Full Name: ${account.lastName} ${account.firstName}\n` +
                `Email: ${account.email}\n` +
                `Phone: ${account.phoneNumber || 'N/A'}\n` +
                `Role: ${account.role}\n` +
                `Status: ${account.userStatus}`);
        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Cannot load account information!");
        }
    };

})();