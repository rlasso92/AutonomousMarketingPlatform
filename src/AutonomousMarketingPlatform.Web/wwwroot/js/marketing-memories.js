$(document).ready(function () {
    loadMemories();

    $('#memorySearch').on('input', function () {
        loadMemories(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadMemories(page = 1) {
    currentPage = page;
    const search = $('#memorySearch').val();

    $.get(`/api/marketingmemories?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#memoriesTableBody');
        tbody.empty();

        response.data.forEach(memory => {
            const lastAccessed = new Date(memory.lastAccessed).toLocaleString();
            const valuePreview = memory.value.length > 100 ? memory.value.substring(0, 100) + '...' : memory.value;

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${memory.key}</td>
                    <td class="text-muted">${valuePreview}</td>
                    <td>${lastAccessed}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editMemory('${memory.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteMemory('${memory.id}')"><i class="fas fa-trash"></i></button>
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadMemories(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetMemoryModal() {
    $('#memoryId').val('');
    $('#memoryForm')[0].reset();
    $('#memoryModalTitle').text('Add Memory');
    $('#saveBtnText').text('Add Memory');
}

function editMemory(id) {
    $.get(`/api/marketingmemories/${id}`, function (memory) {
        $('#memoryId').val(memory.id);
        $('#memoryKey').val(memory.key);
        $('#memoryValue').val(memory.value);
        $('#memoryNotes').val(memory.notes);

        $('#memoryModalTitle').text('Edit Memory');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('memoryModal')).show();
    });
}

function saveMemory() {
    const id = $('#memoryId').val();
    const isEdit = !!id;

    const memoryDto = {
        key: $('#memoryKey').val(),
        value: $('#memoryValue').val(),
        notes: $('#memoryNotes').val()
    };

    let url = '/api/marketingmemories';
    let type = 'POST';

    if (isEdit) {
        memoryDto.id = id;
        url = `/api/marketingmemories/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(memoryDto),
        success: function () {
            const modalEl = document.getElementById('memoryModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadMemories(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save memory');
        }
    });
}

function deleteMemory(id) {
    if (confirm('Are you sure you want to delete this memory?')) {
        $.ajax({
            url: `/api/marketingmemories/${id}`,
            type: 'DELETE',
            success: function () {
                loadMemories(currentPage);
            },
            error: function () {
                alert('Failed to delete memory');
            }
        });
    }
}
