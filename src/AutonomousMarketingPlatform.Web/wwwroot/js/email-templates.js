$(document).ready(function () {
    loadEmailTemplates();

    $('#emailTemplateSearch').on('input', function () {
        loadEmailTemplates(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadEmailTemplates(page = 1) {
    currentPage = page;
    const search = $('#emailTemplateSearch').val();

    $.get(`/api/emailtemplates?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#emailTemplatesTableBody');
        tbody.empty();

        response.data.forEach(template => {
            const lastUpdated = new Date(template.updatedAt).toLocaleString();
            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${template.name}</td>
                    <td>${template.subject}</td>
                    <td>${lastUpdated}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editEmailTemplate('${template.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteEmailTemplate('${template.id}')"><i class="fas fa-trash"></i></button>
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadEmailTemplates(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetEmailTemplateModal() {
    $('#emailTemplateId').val('');
    $('#emailTemplateForm')[0].reset();
    $('#emailTemplateModalTitle').text('Add Email Template');
    $('#saveBtnText').text('Add Template');
}

function editEmailTemplate(id) {
    $.get(`/api/emailtemplates/${id}`, function (template) {
        $('#emailTemplateId').val(template.id);
        $('#emailTemplateName').val(template.name);
        $('#emailTemplateSubject').val(template.subject);
        $('#emailTemplateHtmlBody').val(template.htmlBody);

        $('#emailTemplateModalTitle').text('Edit Email Template');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('emailTemplateModal')).show();
    });
}

function saveEmailTemplate() {
    const id = $('#emailTemplateId').val();
    const isEdit = !!id;

    const templateDto = {
        name: $('#emailTemplateName').val(),
        subject: $('#emailTemplateSubject').val(),
        htmlBody: $('#emailTemplateHtmlBody').val()
    };

    let url = '/api/emailtemplates';
    let type = 'POST';

    if (isEdit) {
        templateDto.id = id;
        url = `/api/emailtemplates/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(templateDto),
        success: function () {
            const modalEl = document.getElementById('emailTemplateModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadEmailTemplates(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save email template');
        }
    });
}

function deleteEmailTemplate(id) {
    if (confirm('Are you sure you want to delete this email template?')) {
        $.ajax({
            url: `/api/emailtemplates/${id}`,
            type: 'DELETE',
            success: function () {
                loadEmailTemplates(currentPage);
            },
            error: function () {
                alert('Failed to delete email template');
            }
        });
    }
}
