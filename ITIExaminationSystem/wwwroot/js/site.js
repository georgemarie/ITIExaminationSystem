// ─────────────────────────────────────────────────────
// ITI Examination System — site.js
// Global UI behaviours: sidebar toggle, auto-dismiss alerts
// ─────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {

    // ── Mobile sidebar toggle ──────────────────────────────────────────
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', () => {
            sidebar.classList.toggle('open');
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', (e) => {
            if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                sidebar.classList.remove('open');
            }
        });
    }

    // ── Auto-dismiss alerts after 5 seconds ───────────────────────────
    document.querySelectorAll('.alert-dismissible').forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 5000);
    });

});
