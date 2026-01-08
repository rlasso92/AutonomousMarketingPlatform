$(document).ready(function () {
    loadProfile();

    $('#avatarContainer').on('click', function () {
        $('#avatarUpload').click();
    });

    $('#avatarUpload').on('change', function () {
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#profileAvatar').attr('src', e.target.result).show();
                $('#letterAvatar').hide();
            };
            reader.readAsDataURL(this.files[0]);
        }
    });
});

function loadProfile() {
    $.get('/api/profile', function (data) {
        // Populate text fields
        const fullName = (data.firstName || '') + ' ' + (data.lastName || '');
        $('#profileFullName').text(fullName);
        $('#profileFullNameDisplay').text(fullName);
        $('#profileEmailDisplay').text(data.email);
        $('#profileDepartment').text(data.department);
        $('#profileLocation').text(data.location);

        $('#profileOrganization').val(data.organization);
        $('#profileDepartmentInput').val(data.department);
        $('#profileLocationInput').val(data.location);
        $('#profileBio').val(data.bio);

        // Handle avatar
        if (data.avatarBase64) {
            $('#profileAvatar').attr('src', "data:image/png;base64," + data.avatarBase64).show();
            $('#letterAvatar').hide();
        } else {
            const initials = (data.firstName ? data.firstName[0] : '') + (data.lastName ? data.lastName[0] : '');
            $('#letterAvatar').text(initials.toUpperCase()).show();
            $('#profileAvatar').hide();
        }

        // Handle Social Media Links
        if (data.socialMediaLinks) {
            try {
                const socialLinks = JSON.parse(data.socialMediaLinks);
                $('#socialFacebook').val(socialLinks.facebook);
                $('#socialInstagram').val(socialLinks.instagram);
                $('#socialTikTok').val(socialLinks.tiktok);
            } catch (e) {
                console.error("Could not parse social media links: ", e);
            }
        }
    });
}

function saveProfile() {
    const socialLinks = {
        facebook: $('#socialFacebook').val(),
        instagram: $('#socialInstagram').val(),
        tiktok: $('#socialTikTok').val()
    };

    const profileDto = {
        organization: $('#profileOrganization').val(),
        department: $('#profileDepartmentInput').val(),
        location: $('#profileLocationInput').val(),
        bio: $('#profileBio').val(),
        avatarBase64: $('#profileAvatar').attr('src'), // The src will be the new Base64 string from the FileReader
        socialMediaLinks: JSON.stringify(socialLinks)
    };

    $.ajax({
        url: '/api/profile',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(profileDto),
        success: function () {
            alert('Profile updated successfully!');
            loadProfile(); // Refresh data on screen
        },
        error: function () {
            alert('Failed to update profile.');
        }
    });
}

function changePassword() {
    const currentPassword = $('#currentPassword').val();
    const newPassword = $('#newPassword').val();
    const confirmNewPassword = $('#confirmNewPassword').val();

    if (newPassword !== confirmNewPassword) {
        alert("New passwords do not match.");
        return;
    }

    $.ajax({
        url: '/api/profile/change-password',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ currentPassword, newPassword }),
        success: function () {
            alert('Password changed successfully!');
            $('#changePasswordForm')[0].reset();
        },
        error: function () {
            alert('Failed to change password. Check your current password.');
        }
    });
}

function setup2FA() {
    $.get('/api/profile/setup-2fa', function (data) {
        $('#qrCodeImage').attr('src', data.qrCodeImageUrl);
        $('#manualEntryKey').text(data.manualEntryKey);
        $('#2faQrCode').show();
    });
}

function enable2FA() {
    const code = $('#2faCode').val();
    $.ajax({
        url: '/api/profile/enable-2fa',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ code: code }),
        success: function () {
            alert('2FA enabled successfully!');
            $('#2faSetup').html('<p class="text-success">Two-Factor Authentication is enabled.</p>');
        },
        error: function () {
            alert('Failed to enable 2FA. The code may be incorrect.');
        }
    });
}


