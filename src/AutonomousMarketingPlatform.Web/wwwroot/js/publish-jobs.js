$(document).ready(function () {
    loadJobs();

    $('#jobSearch').on('input', function () {
        loadJobs(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadJobs(page = 1) {
    currentPage = page;
    const search = $('#jobSearch').val();

    $.get(`/api/publishjobs?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#jobsTableBody');
        tbody.empty();

        response.data.forEach(job => {
            const scheduledTime = new Date(job.scheduledTime).toLocaleString();
            const statusBadge = getJobStatusBadge(job.status);

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${job.contentTitle}</td>
                    <td>${job.channel}</td>
                    <td>${scheduledTime}</td>
                    <td>${statusBadge}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editJob('${job.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteJob('${job.id}')"><i class="fas fa-trash"></i></button>
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

function getJobStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-secondary">Pending</span>';
        case 1: return '<span class="badge bg-info">In Progress</span>';
        case 2: return '<span class="badge bg-success">Completed</span>';
        case 3: return '<span class="badge bg-danger">Failed</span>';
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadJobs(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetJobModal() {
    $('#jobId').val('');
    $('#jobForm')[0].reset();
    $('#jobModalTitle').text('Add Publishing Job');
    $('#saveBtnText').text('Add Job');
    $('#jobStatus').val(0);
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    $('#jobScheduledTime').val(now.toISOString().slice(0, 16));
}

function editJob(id) {
    $.get(`/api/publishjobs/${id}`, function (job) {
        $('#jobId').val(job.id);
        $('#contentId').val(job.contentId);
        $('#jobChannel').val(job.channel);
        const scheduledTime = new Date(job.scheduledTime);
        scheduledTime.setMinutes(scheduledTime.getMinutes() - scheduledTime.getTimezoneOffset());
        $('#jobScheduledTime').val(scheduledTime.toISOString().slice(0, 16));
        $('#jobStatus').val(job.status);

        $('#jobModalTitle').text('Edit Publishing Job');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('jobModal')).show();
    });
}

function saveJob() {
    const id = $('#jobId').val();
    const isEdit = !!id;

    const jobDto = {
        contentId: $('#contentId').val(),
        channel: $('#jobChannel').val(),
        scheduledTime: $('#jobScheduledTime').val(),
        status: parseInt($('#jobStatus').val())
    };

    let url = '/api/publishjobs';
    let type = 'POST';

    if (isEdit) {
        jobDto.id = id;
        url = `/api/publishjobs/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(jobDto),
        success: function () {
            const modalEl = document.getElementById('jobModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadJobs(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save publishing job');
        }
    });
}

function deleteJob(id) {
    if (confirm('Are you sure you want to delete this publishing job?')) {
        $.ajax({
            url: `/api/publishjobs/${id}`,
            type: 'DELETE',
            success: function () {
                loadJobs(currentPage);
            },
            error: function () {
                alert('Failed to delete publishing job');
            }
        });
    }
}
