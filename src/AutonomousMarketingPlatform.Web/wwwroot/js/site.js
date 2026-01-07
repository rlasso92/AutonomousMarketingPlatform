// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');
    const icon = document.getElementById('sidebarToggleIcon');

    sidebar.classList.toggle('collapsed');
    mainContent.classList.toggle('expanded');

    // Logic: 
    // If collapsed, we want to show 'chevron-right' (to indicate expand)
    // If expanded (default), we want to show 'chevron-left' (to indicate collapse)
    
    if (sidebar.classList.contains('collapsed')) {
        icon.classList.remove('fa-chevron-left');
        icon.classList.add('fa-chevron-right');
    } else {
        icon.classList.remove('fa-chevron-right');
        icon.classList.add('fa-chevron-left');
    }
}
