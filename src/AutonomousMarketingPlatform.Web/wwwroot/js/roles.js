$(document).ready(function () {
    loadRolesList();
});

function loadRolesList() {
    $.get('/api/roles', function (response) {
        const tbody = $('#rolesTableBody');
        tbody.empty();

        response.data.forEach(role => {
            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${role.name}</td>
                    <td class="text-muted">Role description placeholder</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editRole('${role.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteRole('${role.id}')"><i class="fas fa-trash"></i></button>
                    </td>
                </tr>
            `;
            tbody.append(tr);
        });
    });
}

function resetRoleModal() {
    $('#roleId').val('');
    $('#roleForm')[0].reset();
    $('#roleModalTitle').text('Add Role');
    $('#saveRoleBtnText').text('Add Role');
}

function editRole(id) {
    $.get(`/api/roles/${id}`, function (role) {
        $('#roleId').val(role.id);
        $('#roleName').val(role.name);
        $('#roleModalTitle').text('Edit Role');
        $('#saveRoleBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('roleModal')).show();
    });
}

function saveRole() {
    const id = $('#roleId').val();
    const isEdit = !!id;
    
    const roleDto = {
        name: $('#roleName').val()
    };

    if (isEdit) {
        roleDto.id = id;
        $.ajax({
            url: `/api/roles/${id}`,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(roleDto),
            success: function () {
                const modalEl = document.getElementById('roleModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();
                loadRolesList();
            },
            error: function () {
                alert('Failed to update role');
            }
        });
    } else {
        $.ajax({
            url: '/api/roles',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(roleDto),
            success: function () {
                const modalEl = document.getElementById('roleModal');
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();
                loadRolesList();
            },
            error: function () {
                alert('Failed to create role');
            }
        });
    }
}

function deleteRole(id) {
    if (confirm('Are you sure you want to delete this role?')) {
        $.ajax({
            url: `/api/roles/${id}`,
            type: 'DELETE',
            success: function () {
                loadRolesList();
            },
            error: function () {
                alert('Failed to delete role');
            }
        });
    }
}
