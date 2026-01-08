$(document).ready(function () {
    loadRoles();
    loadUsers();
    loadTenantsForSelect();

    $('#userSearch').on('input', function() {
        loadUsers(1);
    });
});
let currentPage = 1;
const pageSize = 10;

function loadUsers(page = 1) {
    currentPage = page;
    const search = $('#userSearch').val();

    $.get(`/api/users?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#usersTableBody');
        tbody.empty();

        response.data.forEach(user => {
            const roleBadge = user.roles && user.roles.length > 0
                ? `<span class="badge badge-role ${getRoleBadgeClass(user.roles[0])}">${user.roles[0]}</span>`
                : '<span class="text-muted small">No Role</span>';

            const initials = (user.firstName ? user.firstName[0] : '') + (user.lastName ? user.lastName[0] : '');
            const avatarHtml = user.avatarBase64 
                ? `<img src="data:image/png;base64,${user.avatarBase64}" class="avatar" alt="Avatar">`
                : `<div class="avatar">${initials.toUpperCase()}</div>`;
            
            const tr = `
                <tr>
                    <td class="ps-4">
                        ${avatarHtml}
                    </td>
                    <td class="fw-bold">${user.firstName}</td>
                    <td class="fw-bold">${user.lastName}</td>
                    <td>${user.email}</td>
                    <td>${roleBadge}</td>
                    <td>${user.isDisabled ? 'Yes' : 'No'}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editUser('${user.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteUser('${user.id}')"><i class="fas fa-trash"></i></button>
                    </td>
                </tr>
            `;
            tbody.append(tr);
        });

        // Update Pagination
        $('#startRange').text((page - 1) * pageSize + 1);
        $('#endRange').text(Math.min(page * pageSize, response.total));
        $('#totalRecords').text(response.total);
        renderPagination(response.total, page);
    });
}

function loadRoles() {
    $.get('/api/roles', function (response) {
        const select = $('#roleSelect');
        select.find('option:not(:first)').remove();
        response.data.forEach(role => {
            select.append(`<option value="${role.name}">${role.name}</option>`);
        });
    });
}

function loadTenantsForSelect() {
    $.get('/api/tenants', function (response) {
        const select = $('#tenantSelect');
        select.find('option:not(:first)').remove();
        response.data.forEach(tenant => {
            select.append(`<option value="${tenant.id}">${tenant.name}</option>`);
        });
    });
}

function getRoleBadgeClass(role) {
    if (role === 'Admin') return 'badge-role-admin';
    if (role === 'Software Engineer') return 'badge-role-engineer';
    return 'badge-role-admin'; // Default
}

function renderPagination(total, page) {
    const totalPages = Math.ceil(total / pageSize);
    const pagination = $('#pagination');
    pagination.empty();

    if (totalPages <= 1) return;

    for (let i = 1; i <= totalPages; i++) {
        const activeClass = i === page ? 'active bg-primary border-primary' : 'bg-dark border-secondary text-muted';
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadUsers(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetUserModal() {
    $('#userId').val('');
    $('#userForm')[0].reset();
    $('#userModalTitle').text('Add User');
    $('#saveBtnText').text('Add User');
    $('#passwordGroup').show();
    $('#isDisabled').prop('checked', false);
}

function editUser(id) {
    $.get(`/api/users/${id}`, function (user) {
        $('#userId').val(user.id);
        $('#firstName').val(user.firstName);
        $('#lastName').val(user.lastName);
        $('#email').val(user.email);
        $('#isDisabled').prop('checked', user.isDisabled);
        if (user.roles && user.roles.length > 0) {
            $('#roleSelect').val(user.roles[0]);
        }
        $('#tenantSelect').val(user.tenantId);
        
        $('#passwordGroup').hide(); // Don't allow password edit here for simplicity
        $('#userModalTitle').text('Edit User');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('userModal')).show();
    });
}

function saveUser() {
    const id = $('#userId').val();
    const isEdit = !!id;
    
    const userDto = {
        firstName: $('#firstName').val(),
        lastName: $('#lastName').val(),
        email: $('#email').val(),
        phoneNumber: "1234567890", // Default for now
        role: $('#roleSelect').val(),
        isDisabled: $('#isDisabled').prop('checked'),
        tenantId: $('#tenantSelect').val() || null
    };

    if (isEdit) {
        userDto.id = id;
        $.ajax({
            url: `/api/users/${id}`,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(userDto),
            success: function () {
                $('#userModal').modal('hide'); // Close modal properly (Bootstrap 5 might need instance)
                const modalEl = document.getElementById('userModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();
                loadUsers(currentPage);
            },
            error: function () {
                alert('Failed to update user');
            }
        });
    } else {
        userDto.password = $('#password').val();
        if (!userDto.password) {
            alert('Password is required');
            return;
        }

        $.ajax({
            url: '/api/users',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(userDto),
            success: function () {
                // Close modal
                 const modalEl = document.getElementById('userModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();
                loadUsers(1);
            },
            error: function () {
                alert('Failed to create user');
            }
        });
    }
}

function deleteUser(id) {
    if (confirm('Are you sure you want to delete this user?')) {
        $.ajax({
            url: `/api/users/${id}`,
            type: 'DELETE',
            success: function () {
                loadUsers(currentPage);
            },
            error: function () {
                alert('Failed to delete user');
            }
        });
    }
}
