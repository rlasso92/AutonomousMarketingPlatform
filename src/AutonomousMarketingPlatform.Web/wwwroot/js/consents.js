$(document).ready(function () {
    loadConsents();

    $('#consentSearch').on('input', function () {
        loadConsents(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadConsents(page = 1) {
    currentPage = page;
    const search = $('#consentSearch').val();

    $.get(`/api/consents?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#consentsTableBody');
        tbody.empty();

        response.data.forEach(item => {
            const grantedAt = new Date(item.grantedAt).toLocaleDateString();
            const typeBadge = getConsentTypeBadge(item.consentType);
            const statusBadge = getConsentStatusBadge(item.status);

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${item.contactEmail}</td>
                    <td>${typeBadge}</td>
                    <td>${statusBadge}</td>
                    <td>${grantedAt}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editConsent('${item.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteConsent('${item.id}')"><i class="fas fa-trash"></i></button>
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

function getConsentTypeBadge(type) {
    switch (type) {
        case 0: return '<span class="badge bg-primary">Email Newsletter</span>';
        case 1: return '<span class="badge bg-info">SMS Alerts</span>';
        case 2: return '<span class="badge bg-warning text-dark">3rd Party Mkt</span>';
        default: return '<span class="badge bg-light text-muted">Unknown</span>';
    }
}

function getConsentStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-success">Granted</span>';
        case 1: return '<span class="badge bg-danger">Revoked</span>';
        default: return '<span class="badge bg-light text-muted">Unknown</span>';
    }
}

function renderPagination(total, page) {
    const totalPages = Math.ceil(total / pageSize);
    const pagination = $('#pagination');
    pagination.empty();

    if (totalPages <= 1) return;

    for (let i = 1; i <= totalPages; i++) {
        const activeClass = i === page ? 'active bg-primary border-primary' : 'bg-dark border-secondary text-muted';
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadConsents(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetConsentModal() {
    $('#consentId').val('');
    $('#consentForm')[0].reset();
    $('#consentModalTitle').text('Add Consent');
    $('#saveBtnText').text('Add Consent');
    $('#consentType').val(0);
    $('#consentStatus').val(0);
}

function editConsent(id) {
    $.get(`/api/consents/${id}`, function (consent) {
        $('#consentId').val(consent.id);
        $('#contactId').val(consent.contactId);
        $('#consentType').val(consent.consentType);
        $('#consentStatus').val(consent.status);
        
        // Disable contactId field on edit
        $('#contactId').prop('disabled', true);

        $('#consentModalTitle').text('Edit Consent');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('consentModal')).show();
    });
}

function saveConsent() {
    const id = $('#consentId').val();
    const isEdit = !!id;

    const consentDto = {
        contactId: $('#contactId').val(),
        consentType: parseInt($('#consentType').val()),
        status: parseInt($('#consentStatus').val())
    };

    let url = '/api/consents';
    let type = 'POST';

    if (isEdit) {
        consentDto.id = id;
        url = `/api/consents/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(consentDto),
        success: function () {
            const modalEl = document.getElementById('consentModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadConsents(isEdit ? currentPage : 1);
        },
        error: function (err) {
            alert('Failed to save consent: ' + err.responseJSON.message);
        }
    });
}

function deleteConsent(id) {
    if (confirm('Are you sure you want to delete this consent record?')) {
        $.ajax({
            url: `/api/consents/${id}`,
            type: 'DELETE',
            success: function () {
                loadConsents(currentPage);
            },
            error: function () {
                alert('Failed to delete consent record');
            }
        });
    }
}
