// admin-accounts.js
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        console.log('✅ Admin Accounts JS loaded');
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

        // ✅ Prepare data - GỬI CẢ Password VÀ ConfirmPassword
        const payload = {
            FirstName: data.FirstName || '',
            LastName: data.LastName || '',
            Email: email,
            PhoneNumber: data.PhoneNumber || '',
            Password: password,
            ConfirmPassword: confirmPassword,  // ← THÊM DÒNG NÀY
            Role: data.Role || 'Customer',
            Username: email.split('@')[0],
            IsActive: document.getElementById("statusSwitch")?.checked || false
        };

        console.log('Sending data:', payload);

        try {
            const response = await fetch("https://localhost:7160/api/admin/create-account", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorData = await response.json();
                console.error('Backend error:', errorData);

                // Hiển thị lỗi chi tiết từ backend
                let errorMsg = "❌ Lỗi khi thêm tài khoản:\n\n";
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
            alert("⚠️ Không thể kết nối tới máy chủ backend!");
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
        const accountId = form.dataset.accountId || data.Id;

        if (!accountId) {
            alert('❌ Không tìm thấy ID tài khoản!');
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

        if (data.ResetPassword || document.getElementById('resetPassword')?.checked) {
            payload.Password = data.Password || '';
        }

        console.log('Updating account:', accountId, payload);

        try {
            const response = await fetch(`https://localhost:7160/api/admin/account/${accountId}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

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
            alert("⚠️ Không thể kết nối tới máy chủ backend!");
        }
    };

    // Toggle Account Status
    window.toggleAccountStatus = async function (checkbox, accountId) {
        const isActive = checkbox.checked;
        const label = checkbox.nextElementSibling;

        if (!confirm(`Bạn có chắc muốn ${isActive ? 'kích hoạt' : 'khóa'} tài khoản này?`)) {
            checkbox.checked = !isActive;
            return;
        }

        try {
            const response = await fetch(`https://localhost:7160/api/admin/account/${accountId}/toggle-status`, {
                method: "PATCH",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ isActive: isActive })
            });

            if (!response.ok) {
                const err = await response.text();
                alert("❌ Lỗi: " + err);
                checkbox.checked = !isActive;
                return;
            }

            if (isActive) {
                label.classList.remove('text-danger');
                label.classList.add('text-success');
                label.querySelector('small').textContent = 'Hoạt động';
            } else {
                label.classList.remove('text-success');
                label.classList.add('text-danger');
                label.querySelector('small').textContent = 'Bị khóa';
            }

            alert('✅ Cập nhật trạng thái thành công!');

        } catch (error) {
            console.error("Error:", error);
            alert("⚠️ Không thể kết nối tới máy chủ backend!");
            checkbox.checked = !isActive;
        }
    };

    // Confirm Delete
    window.confirmDelete = async function (accountId) {
        if (!confirm('Bạn có chắc chắn muốn xóa tài khoản này? Hành động này không thể hoàn tác!')) {
            return;
        }

        try {
            const response = await fetch(`https://localhost:7160/api/admin/account/${accountId}`, {
                method: "DELETE"
            });

            if (!response.ok) {
                const err = await response.text();
                alert("❌ Lỗi khi xóa: " + err);
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
    window.loadAccountForEdit = function (accountId) {
        fetch(`https://localhost:7160/api/admin/account/${accountId}`)
            .then(response => response.json())
            .then(account => {
                const form = document.getElementById("editAccountForm");
                if (!form) {
                    console.error('Edit form not found!');
                    return;
                }

                form.dataset.accountId = accountId;

                const fields = {
                    'FirstName': account.firstName,
                    'LastName': account.lastName,
                    'Email': account.email,
                    'PhoneNumber': account.phoneNumber,
                    'Role': account.role
                };

                for (let [name, value] of Object.entries(fields)) {
                    const input = form.querySelector(`[name="${name}"]`);
                    if (input) {
                        input.value = value || '';
                    }
                }
            })
            .catch(error => {
                console.error("Error:", error);
                alert("⚠️ Không thể tải thông tin tài khoản!");
            });
    };

})();