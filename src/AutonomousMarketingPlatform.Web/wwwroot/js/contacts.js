$(document).ready(function () {
    loadContacts();

    $('#contactSearch').on('input', function () {
        loadContacts(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadContacts(page = 1) {
    currentPage = page;
    const search = $('#contactSearch').val();

    $.get(`/api/contacts?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#contactsTableBody');
        tbody.empty();

        response.data.forEach(contact => {
            const subscriptionBadge = contact.isSubscribed
                ? '<span class="badge bg-success">Subscribed</span>'
                : '<span class="badge bg-secondary">Unsubscribed</span>';

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${contact.email}</td>
                    <td>${contact.firstName}</td>
                    <td>${contact.lastName}</td>
                    <td>${subscriptionBadge}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editContact('${contact.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteContact('${contact.id}')"><i class="fas fa-trash"></i></button>
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadContacts(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetContactModal() {
    $('#contactId').val('');
    $('#contactForm')[0].reset();
    $('#contactModalTitle').text('Add Contact');
    $('#saveBtnText').text('Add Contact');
    $('#isSubscribed').prop('checked', true);
}

function editContact(id) {
    $.get(`/api/contacts/${id}`, function (contact) {
        $('#contactId').val(contact.id);
        $('#contactEmail').val(contact.email);
        $('#contactFirstName').val(contact.firstName);
        $('#contactLastName').val(contact.lastName);
        $('#isSubscribed').prop('checked', contact.isSubscribed);

        $('#contactModalTitle').text('Edit Contact');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('contactModal')).show();
    });
}

function saveContact() {
    const id = $('#contactId').val();
    const isEdit = !!id;

    const contactDto = {
        email: $('#contactEmail').val(),
        firstName: $('#contactFirstName').val(),
        lastName: $('#contactLastName').val(),
        isSubscribed: $('#isSubscribed').is(':checked')
    };

    let url = '/api/contacts';
    let type = 'POST';

    if (isEdit) {
        contactDto.id = id;
        url = `/api/contacts/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(contactDto),
        success: function () {
            const modalEl = document.getElementById('contactModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadContacts(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save contact');
        }
    });
}

function deleteContact(id) {
    if (confirm('Are you sure you want to delete this contact?')) {
        $.ajax({
            url: `/api/contacts/${id}`,
            type: 'DELETE',
            success: function () {
                loadContacts(currentPage);
            },
            error: function () {
                alert('Failed to delete contact');
            }
        });
    }
}
