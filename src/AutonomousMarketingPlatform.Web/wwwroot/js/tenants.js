$(document).ready(function () {
    loadTenants();

    $('#tenantSearch').on('input', function () {
        loadTenants(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadTenants(page = 1) {
    currentPage = page;
    const search = $('#tenantSearch').val();

    $.get(`/api/tenants?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#tenantsTableBody');
        tbody.empty();

        response.data.forEach(tenant => {
            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${tenant.name}</td>
                    <td>${tenant.schemaName}</td>
                    <td>${tenant.isDisabled ? '<span class="badge bg-danger">Disabled</span>' : '<span class="badge bg-success">Enabled</span>'}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editTenant('${tenant.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteTenant('${tenant.id}')"><i class="fas fa-trash"></i></button>
                    </td>
                </tr>
            `;
            tbody.append(tr);
        });

        $('#startRange').text((page - 1) * pageSize + 1);
        $('#endRange').text(Math.min(page * pageSize, response.total));
        $('#totalRecords').text(response.total);
        renderPagination(response.total, page);
    });
}

function renderPagination(total, page) {
    const totalPages = Math.ceil(total / pageSize);
    const pagination = $('#pagination');
    pagination.empty();

    if (totalPages <= 1) return;

    for (let i = 1; i <= totalPages; i++) {
        const activeClass = i === page ? 'active bg-primary border-primary' : 'bg-dark border-secondary text-muted';
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadTenants(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetTenantModal() {
    $('#tenantId').val('');
    $('#tenantForm')[0].reset();
    $('#tenantModalTitle').text('Add Tenant');
    $('#saveBtnText').text('Add Tenant');
}

function editTenant(id) {
    $.get(`/api/tenants/${id}`, function (tenant) {
        $('#tenantId').val(tenant.id);
        $('#tenantName').val(tenant.name);
        $('#schemaName').val(tenant.schemaName);
        $('#isDisabled').prop('checked', tenant.isDisabled);

        $('#tenantModalTitle').text('Edit Tenant');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('tenantModal')).show();
    });
}

function saveTenant() {
    const id = $('#tenantId').val();
    const isEdit = !!id;

    const tenantDto = {
        name: $('#tenantName').val(),
        schemaName: $('#schemaName').val(),
        isDisabled: $('#isDisabled').is(':checked')
    };

    let url = '/api/tenants';
    let type = 'POST';

    if (isEdit) {
        tenantDto.id = id;
        url = `/api/tenants/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(tenantDto),
        success: function () {
            const modalEl = document.getElementById('tenantModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadTenants(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save tenant');
        }
    });
}

function deleteTenant(id) {
    if (confirm('Are you sure you want to delete this tenant?')) {
        $.ajax({
            url: `/api/tenants/${id}`,
            type: 'DELETE',
            success: function () {
                loadTenants(currentPage);
            },
            error: function () {
                alert('Failed to delete tenant');
            }
        });
    }
}
