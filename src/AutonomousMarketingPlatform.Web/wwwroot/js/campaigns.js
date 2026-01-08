$(document).ready(function () {
    loadCampaigns();

    $('#campaignSearch').on('input', function () {
        loadCampaigns(1);
    });
});

let currentPage = 1;
const pageSize = 10;

function loadCampaigns(page = 1) {
    currentPage = page;
    const search = $('#campaignSearch').val();

    $.get(`/api/campaigns?page=${page}&pageSize=${pageSize}&search=${search}`, function (response) {
        const tbody = $('#campaignsTableBody');
        tbody.empty();

        response.data.forEach(campaign => {
            const statusBadge = getCampaignStatusBadge(campaign.status);
            const scheduledDate = new Date(campaign.scheduledDate).toLocaleString();

            const tr = `
                <tr>
                    <td class="ps-4 fw-bold">${campaign.name}</td>
                    <td>${campaign.subject}</td>
                    <td>${scheduledDate}</td>
                    <td>${statusBadge}</td>
                    <td class="text-end pe-4">
                        <button class="btn btn-sm btn-light text-primary border me-1 shadow-sm" onclick="editCampaign('${campaign.id}')"><i class="fas fa-pen"></i></button>
                        <button class="btn btn-sm btn-light text-danger border shadow-sm" onclick="deleteCampaign('${campaign.id}')"><i class="fas fa-trash"></i></button>
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

function getCampaignStatusBadge(status) {
    switch (status) {
        case 0: return '<span class="badge bg-secondary">Draft</span>';
        case 1: return '<span class="badge bg-info">Scheduled</span>';
        case 2: return '<span class="badge bg-success">Sent</span>';
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
        const li = `<li class="page-item"><button class="page-link ${activeClass}" onclick="loadCampaigns(${i})">${i}</button></li>`;
        pagination.append(li);
    }
}

function resetCampaignModal() {
    $('#campaignId').val('');
    $('#campaignForm')[0].reset();
    $('#campaignModalTitle').text('Add Campaign');
    $('#saveBtnText').text('Add Campaign');
    // Set default status to Draft
    $('#campaignStatus').val(0);
    // Set scheduled date to a reasonable default (e.g., now)
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset()); // Adjust for timezone
    $('#campaignScheduledDate').val(now.toISOString().slice(0, 16));
}

function editCampaign(id) {
    $.get(`/api/campaigns/${id}`, function (campaign) {
        $('#campaignId').val(campaign.id);
        $('#campaignName').val(campaign.name);
        $('#campaignSubject').val(campaign.subject);
        $('#campaignHtmlContent').val(campaign.htmlContent);
        // Format date for datetime-local input
        const scheduledDate = new Date(campaign.scheduledDate);
        scheduledDate.setMinutes(scheduledDate.getMinutes() - scheduledDate.getTimezoneOffset());
        $('#campaignScheduledDate').val(scheduledDate.toISOString().slice(0, 16));
        $('#campaignStatus').val(campaign.status);

        $('#campaignModalTitle').text('Edit Campaign');
        $('#saveBtnText').text('Save Changes');
        
        new bootstrap.Modal(document.getElementById('campaignModal')).show();
    });
}

function saveCampaign() {
    const id = $('#campaignId').val();
    const isEdit = !!id;

    const campaignDto = {
        name: $('#campaignName').val(),
        subject: $('#campaignSubject').val(),
        htmlContent: $('#campaignHtmlContent').val(),
        scheduledDate: $('#campaignScheduledDate').val(),
        status: parseInt($('#campaignStatus').val())
    };

    let url = '/api/campaigns';
    let type = 'POST';

    if (isEdit) {
        campaignDto.id = id;
        url = `/api/campaigns/${id}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(campaignDto),
        success: function () {
            const modalEl = document.getElementById('campaignModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadCampaigns(isEdit ? currentPage : 1);
        },
        error: function () {
            alert('Failed to save campaign');
        }
    });
}

function deleteCampaign(id) {
    if (confirm('Are you sure you want to delete this campaign?')) {
        $.ajax({
            url: `/api/campaigns/${id}`,
            type: 'DELETE',
            success: function () {
                loadCampaigns(currentPage);
            },
            error: function () {
                alert('Failed to delete campaign');
            }
        });
    }
}
