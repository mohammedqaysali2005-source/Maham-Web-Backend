// تهيئة DataTables
function initDataTable(selector, options = {}) {
    if ($(selector).length) {
        $(selector).DataTable({
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/ar.json'
            },
            responsive: true,
            pageLength: 10,
            ...options
        });
    }
}

$(document).ready(function() {
    initDataTable('.dt-table');
});

// تأكيد الحذف
function confirmDelete(url, name) {
    if (confirm(`هل أنت متأكد من حذف "${name}"؟ لا يمكن التراجع عن هذا الإجراء.`)) {
        document.getElementById('delete-form').action = url;
        document.getElementById('delete-form').submit();
    }
}
