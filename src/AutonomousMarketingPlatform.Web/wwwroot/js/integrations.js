const SERVICE_DEFINITIONS = {
    // Storage Services
    'OneDrive': { category: 'Storage', displayName: 'OneDrive', iconClass: 'fab fa-microsoft', fields: [{ name: 'ClientId', label: 'Client ID', type: 'text', required: true }, { name: 'ClientSecret', label: 'Client Secret', type: 'password', required: true }, { name: 'TenantId', label: 'Tenant ID', type: 'text', required: true }] },
    'GoogleDrive': { category: 'Storage', displayName: 'Google Drive', iconClass: 'fab fa-google-drive', fields: [{ name: 'ClientId', label: 'Client ID', type: 'text', required: true }, { name: 'ClientSecret', label: 'Client Secret', type: 'password', required: true }, { name: 'RefreshToken', label: 'Refresh Token', type: 'password', required: true }] },
    'Dropbox': { category: 'Storage', displayName: 'Dropbox', iconClass: 'fab fa-dropbox', fields: [{ name: 'AccessToken', label: 'Access Token', type: 'password', required: true }] },
    'AWSS3': { category: 'Storage', displayName: 'AWS S3', iconClass: 'fab fa-aws', fields: [{ name: 'AccessKeyId', label: 'Access Key ID', type: 'text', required: true }, { name: 'SecretAccessKey', label: 'Secret Access Key', type: 'password', required: true }, { name: 'Region', label: 'Region', type: 'text', required: true }, { name: 'BucketName', label: 'Bucket Name', type: 'text', required: true }] },
    'Cloudflare': { category: 'Storage', displayName: 'Cloudflare R2', iconClass: 'fab fa-cloudflare', fields: [{ name: 'AccountId', label: 'Account ID', type: 'text', required: true }, { name: 'AccessKeyId', label: 'Access Key ID', type: 'text', required: true }, { name: 'SecretAccessKey', label: 'Secret Access Key', type: 'password', required: true }, { name: 'BucketName', label: 'Bucket Name', type: 'text', required: true }] },
    'Box': { category: 'Storage', displayName: 'Box', iconClass: 'fas fa-box', fields: [{ name: 'ClientId', label: 'Client ID', type: 'text', required: true }, { name: 'ClientSecret', label: 'Client Secret', type: 'password', required: true }, { name: 'DeveloperToken', label: 'Developer Token', type: 'password', required: true }] },

    // SMS Services
    'Telegram': { category: 'SMS', displayName: 'Telegram', iconClass: 'fab fa-telegram', fields: [{ name: 'BotToken', label: 'Bot Token', type: 'password', required: true }, { name: 'ChatId', label: 'Chat ID', type: 'text', required: true }] },
    'Slack': { category: 'SMS', displayName: 'Slack', iconClass: 'fab fa-slack', fields: [{ name: 'BotToken', label: 'Bot Token', type: 'password', required: true }, { name: 'ChannelId', label: 'Channel ID', type: 'text', required: true }] },
    'MSTeams': { category: 'SMS', displayName: 'MS Teams', iconClass: 'fab fa-microsoft', fields: [{ name: 'WebhookUrl', label: 'Webhook URL', type: 'text', required: true }] },

    // SMTP Services
    'SendGrid': { category: 'SMTP', displayName: 'SendGrid', iconClass: 'fas fa-envelope', fields: [{ name: 'ApiKey', label: 'API Key', type: 'password', required: true }, { name: 'SenderEmail', label: 'Sender Email', type: 'email', required: true }] },
    'Mailgun': { category: 'SMTP', displayName: 'Mailgun', iconClass: 'fas fa-envelope-open-text', fields: [{ name: 'ApiKey', label: 'API Key', type: 'password', required: true }, { name: 'Domain', label: 'Domain', type: 'text', required: true }, { name: 'SenderEmail', label: 'Sender Email', type: 'email', required: true }] },
    'SMTPcom': { category: 'SMTP', displayName: 'SMTP.com', iconClass: 'fas fa-mail-bulk', fields: [{ name: 'ApiKey', label: 'API Key', type: 'password', required: true }, { name: 'SenderEmail', label: 'Sender Email', type: 'email', required: true }] },
    'GenericSMTP': { category: 'SMTP', displayName: 'Generic SMTP', iconClass: 'fas fa-server', fields: [
        { name: 'Host', label: 'SMTP Host', type: 'text', required: true },
        { name: 'Port', label: 'SMTP Port', type: 'number', required: true },
        { name: 'Username', label: 'SMTP Username', type: 'text', required: true },
        { name: 'Password', label: 'SMTP Password', type: 'password', required: true },
        { name: 'SenderEmail', label: 'Sender Email', type: 'email', required: true }
    ] }
};

let allIntegrations = [];

$(document).ready(function () {
    loadIntegrations();
});

function loadIntegrations() {
    $.get('/api/integrationsettings', function (data) {
        allIntegrations = data;
        renderServiceCards();
    });
}

function renderServiceCards() {
    const storageContainer = $('#storageServicesContainer');
    const smsContainer = $('#smsServicesContainer');
    const smtpContainer = $('#smtpServicesContainer');

    storageContainer.empty();
    smsContainer.empty();
    smtpContainer.empty();

    for (const serviceKey in SERVICE_DEFINITIONS) {
        const serviceDef = SERVICE_DEFINITIONS[serviceKey];
        const existingIntegration = allIntegrations.find(i => i.service === serviceKey);
        const isActive = existingIntegration ? existingIntegration.isEnabled : false;
        const cardHtml = `
            <div class="col">
                <div class="card service-card ${isActive ? 'active' : ''}" 
                     data-service-key="${serviceKey}"
                     onclick="openIntegrationModal('${serviceKey}')">
                    <div class="card-body">
                        <i class="service-logo ${serviceDef.iconClass} fa-3x"></i>
                        <p class="service-name">${serviceDef.displayName}</p>
                        <div class="form-check form-switch toggle-switch">
                            <input class="form-check-input" type="checkbox" id="toggle-${serviceKey}" ${isActive ? 'checked' : ''} disabled>
                            <label class="form-check-label" for="toggle-${serviceKey}">${isActive ? 'Enabled' : 'Disabled'}</label>
                        </div>
                    </div>
                </div>
            </div>
        `;

        if (serviceDef.category === 'Storage') {
            storageContainer.append(cardHtml);
        } else if (serviceDef.category === 'SMS') {
            smsContainer.append(cardHtml);
        } else if (serviceDef.category === 'SMTP') {
            smtpContainer.append(cardHtml);
        }
    }
}

function openIntegrationModal(serviceKey) {
    const serviceDef = SERVICE_DEFINITIONS[serviceKey];
    const existingIntegration = allIntegrations.find(i => i.service === serviceKey);

    $('#integrationModalTitle').text(`Configure ${serviceDef.displayName}`);
    $('#integrationId').val(existingIntegration ? existingIntegration.id : '');
    $('#serviceCategory').val(serviceDef.category);
    $('#serviceName').val(serviceKey);
    $('#serviceIconClass').val(serviceDef.iconClass);
    $('#displayName').val(serviceDef.displayName);
    $('#isEnabled').prop('checked', existingIntegration ? existingIntegration.isEnabled : false);

    const configFieldsContainer = $('#configFieldsContainer');
    configFieldsContainer.empty();

    let existingConfig = {};
    if (existingIntegration && existingIntegration.configJson) {
        try {
            existingConfig = JSON.parse(existingIntegration.configJson);
        } catch (e) {
            console.error("Error parsing existing config JSON: ", e);
        }
    }

    serviceDef.fields.forEach(field => {
        const fieldValue = existingConfig[field.name] || '';
        const fieldHtml = `
            <div class="mb-3">
                <label for="config-${field.name}" class="form-label">${field.label} ${field.required ? '*' : ''}</label>
                <input type="${field.type}" class="form-control" id="config-${field.name}" 
                       value="${fieldValue}" ${field.required ? 'required' : ''}>
            </div>
        `;
        configFieldsContainer.append(fieldHtml);
    });

    new bootstrap.Modal(document.getElementById('integrationModal')).show();
}

function saveIntegration() {
    const serviceKey = $('#serviceName').val();
    const serviceDef = SERVICE_DEFINITIONS[serviceKey];

    const integrationId = $('#integrationId').val();
    const isEdit = !!integrationId;

    const config = {};
    serviceDef.fields.forEach(field => {
        config[field.name] = $(`#config-${field.name}`).val();
    });

    const integrationDto = {
        id: integrationId || '00000000-0000-0000-0000-000000000000', // Backend will generate if new
        category: serviceDef.category, // Pass enum string, backend will convert
        service: serviceKey, // Pass enum string, backend will convert
        displayName: $('#displayName').val(),
        iconClass: serviceDef.iconClass,
        isEnabled: $('#isEnabled').is(':checked'),
        configJson: JSON.stringify(config)
    };

    let url = '/api/integrationsettings';
    let type = 'POST';

    if (isEdit) {
        url += `/${integrationId}`;
        type = 'PUT';
    }

    $.ajax({
        url: url,
        type: type,
        contentType: 'application/json',
        data: JSON.stringify(integrationDto),
        success: function () {
            const modalEl = document.getElementById('integrationModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
            loadIntegrations(); // Reload cards to reflect changes
        },
        error: function (xhr) {
            alert('Failed to save integration: ' + (xhr.responseJSON ? xhr.responseJSON.message : xhr.responseText));
        }
    });
}
