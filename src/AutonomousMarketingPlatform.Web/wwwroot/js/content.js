$(document).ready(function () {
    loadContent();

    $('#contentSearch').on('input', function () {
        loadContent(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadContent(page = 1) {
    currentPage = page;
    const search = $('#contentSearch').val();

    $.get(`/api/content?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#contentTableBody');
        tbody.empty();

        response.data.forEach(item => {
            const createdAt = new Date(item.createdAt).toLocaleDateString();
            const typeBadge = getContentTypeBadge(item.contentType);
            const statusBadge = getContentStatusBadge(item.status);

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${item.title}</td>
                    <td>${typeBadge}</td>
                    <td>${statusBadge}</td>
                    <td>${createdAt}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editContent('${item.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteContent('${item.id}')"><i class="fas fa-trash"></i></button>
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

function getContentTypeBadge(type) {
    switch (type) {
        case 0: return '<span class="badge bg-info">Blog Post</span>';
        case 1: return '<span class="badge bg-primary">Social Media</span>';
        case 2: return '<span class="badge bg-warning text-dark">Ad</span>';
        case 3: return '<span class="badge bg-success">Email</span>';
        default: return '<span class="badge bg-light text-muted">Unknown</span>';
    }
}

function getContentStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-secondary">Draft</span>';
        case 1: return '<span class="badge bg-info">In Review</span>';
        case 2: return '<span class="badge bg-success">Published</span>';
        case 3: return '<span class="badge bg-danger">Archived</span>';
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadContent(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetContentModal() {
    $('#contentId').val('');
    $('#contentForm')[0].reset();
    $('#contentModalTitle').text('Add Content');
    $('#saveBtnText').text('Add Content');
    $('#contentType').val(0);
    $('#contentStatus').val(0);
}

function editContent(id) {
    $.get(`/api/content/${id}`, function (content) {
        $('#contentId').val(content.id);
        $('#contentTitle').val(content.title);
        $('#contentBody').val(content.body);
        $('#contentType').val(content.contentType);
        $('#contentStatus').val(content.status);

        $('#contentModalTitle').text('Edit Content');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('contentModal')).show();
    });
}

function saveContent() {
    const id = $('#contentId').val();
    const isEdit = !!id;

    const contentDto = {
        title: $('#contentTitle').val(),
        body: $('#contentBody').val(),
        contentType: parseInt($('#contentType').val()),
        status: parseInt($('#contentStatus').val())
    };

    let url = '/api/content';
    let type = 'POST';

    if (isEdit) {
        contentDto.id = id;
        url = `/api/content/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(contentDto),
        success: function () {
            const modalEl = document.getElementById('contentModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadContent(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save content');
        }
    });
}

function deleteContent(id) {
    if (confirm('Are you sure you want to delete this content?')) {
        $.ajax({
            url: `/api/content/${id}`,
            type: 'DELETE',
            success: function () {
                loadContent(currentPage);
            },
            error: function () {
                alert('Failed to delete content');
            }
        });
    }
}
