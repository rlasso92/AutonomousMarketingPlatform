$(document).ready(function () {
    loadPacks();

    $('#packSearch').on('input', function () {
        loadPacks(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadPacks(page = 1) {
    currentPage = page;
    const search = $('#packSearch').val();

    $.get(`/api/marketingpacks?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#packsTableBody');
        tbody.empty();

        response.data.forEach(pack => {
            const createdAt = new Date(pack.createdAt).toLocaleDateString();
            const statusBadge = getPackStatusBadge(pack.status);

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${pack.name}</td>
                    <td>${pack.description}</td>
                    <td>${statusBadge}</td>
                    <td>${createdAt}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editPack('${pack.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deletePack('${pack.id}')"><i class="fas fa-trash"></i></button>
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

function getPackStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-secondary">Draft</span>';
        case 1: return '<span class="badge bg-success">Ready</span>';
        case 2: return '<span class="badge bg-danger">Archived</span>';
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadPacks(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetPackModal() {
    $('#packId').val('');
    $('#packForm')[0].reset();
    $('#packModalTitle').text('Add Marketing Pack');
    $('#saveBtnText').text('Add Pack');
    $('#packStatus').val(0);
}

function editPack(id) {
    $.get(`/api/marketingpacks/${id}`, function (pack) {
        $('#packId').val(pack.id);
        $('#packName').val(pack.name);
        $('#packDescription').val(pack.description);
        $('#packStatus').val(pack.status);

        $('#packModalTitle').text('Edit Marketing Pack');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('packModal')).show();
    });
}

function savePack() {
    const id = $('#packId').val();
    const isEdit = !!id;

    const packDto = {
        name: $('#packName').val(),
        description: $('#packDescription').val(),
        status: parseInt($('#packStatus').val())
    };

    let url = '/api/marketingpacks';
    let type = 'POST';

    if (isEdit) {
        packDto.id = id;
        url = `/api/marketingpacks/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(packDto),
        success: function () {
            const modalEl = document.getElementById('packModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadPacks(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save marketing pack');
        }
    });
}

function deletePack(id) {
    if (confirm('Are you sure you want to delete this marketing pack?')) {
        $.ajax({
            url: `/api/marketingpacks/${id}`,
            type: 'DELETE',
            success: function () {
                loadPacks(currentPage);
            },
            error: function () {
                alert('Failed to delete marketing pack');
            }
        });
    }
}
