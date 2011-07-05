$(function() {
    $('table').dataTable({
        aaSorting: [[3, 'desc']],
        bLengthChange: false,
        bInfo: false,
        sPaginationType: 'full_numbers',
        iDisplayLength: 30,
        bProcessing: true,
        sAjaxSource: 'packages.json'
    });
});

