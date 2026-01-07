$(document).ready(function () {
    // Password Toggle Logic
    $('#togglePassword').on('click', function() {
        const passwordInput = $('#password');
        const icon = $(this).find('i');
        
        if (passwordInput.attr('type') === 'password') {
            passwordInput.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordInput.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    $('#loginForm').on('submit', function (e) {
        e.preventDefault();
        
        const email = $('#email').val();
        const password = $('#password').val();
        const btn = $('#loginBtn');
        const errorAlert = $('#loginError');

        // Simple Client Validation
        if (!email || !password) {
            errorAlert.text('Please enter both email and password.').removeClass('d-none');
            return;
        }

        // Loading State
        btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i> Signing in...');
        errorAlert.addClass('d-none');

        $.ajax({
            url: '/api/Auth/login',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ email: email, password: password, rememberMe: false }),
            success: function (response) {
                if (response.success) {
                    window.location.href = '/Home/Index'; // Redirect to Dashboard
                }
            },
            error: function (xhr) {
                btn.prop('disabled', false).text('Sign in');
                let msg = 'Login failed. Please check your credentials.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg = xhr.responseJSON.message;
                }
                errorAlert.text(msg).removeClass('d-none');
            }
        });
    });
});