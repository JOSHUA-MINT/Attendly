/* ═══════════════════════════════════════════════
 ATTENDLY - Main JavaScript
 ═══════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', function () {

 // ── Navbar active link ──
 const currentPath = window.location.pathname;
 document.querySelectorAll('.nav-link').forEach(link => {
 if (link.getAttribute('href') && currentPath.includes(link.getAttribute('href').replace('~', ''))) {
 link.classList.add('active');
 }
 });

 // ── Auto-dismiss alerts ──
 setTimeout(() => {
 const alerts = document.querySelectorAll('.alert');
 alerts.forEach(alert => {
 alert.style.transition = 'opacity 0.3s';
 alert.style.opacity = '0';
 setTimeout(() => alert.remove(), 300);
 });
 }, 5000);

 // ── Confirm delete ──
 document.querySelectorAll('form[onscreen*="confirm"]').forEach(form => {
 form.addEventListener('submit', function (e) {
 if (!confirm('Are you sure?')) e.preventDefault();
 });
 });

 // ── Chat scroll to bottom ──
 const chatContainer = document.getElementById('chatMessages');
 if (chatContainer) {
 chatContainer.scrollTop = chatContainer.scrollHeight;
 }
});

// ── Form validation ──
document.querySelectorAll('form[required]').forEach(form => {
 form.addEventListener('submit', function () {
 const required = form.querySelectorAll('[required]');
 let valid = true;
 required.forEach(field => {
 if (!field.value.trim()) {
 field.classList.add('is-invalid');
 valid = false;
 } else {
 field.classList.remove('is-invalid');
 }
 });
 return valid;
 });
});

// ── Loading state for buttons ──
document.querySelectorAll('button[type="submit"]').forEach(btn => {
 btn.addEventListener('click', function () {
 this.disabled = true;
 const originalText = this.innerHTML;
 this.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Processing...';
 setTimeout(() => {
 this.disabled = false;
 this.innerHTML = originalText;
 }, 2000);
 });
});
