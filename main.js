$(function() {
    var $spinner = $(new Spinner().spin().el).css({
        left: 50,
        top: 50,
        height: 100
    });
    $('.content').append($spinner);

    $.getJSON('packages.json', function(data) {
        var date, $lastUpdate, $table, $input;

        $.each(data.packages, function(i, p) {
            p[6] = '<a class="npm" href="http://search.npmjs.org/#/' + p[0] + '">/\\</a>';
            if (data.repoUrls[i]) {
                p[0] = '<a href="https://github.com/' + data.repoUrls[i] + '">' + p[0] + '</a>';
            }
            if (data.authorUrls[i]) {
                p[2] = '<a href="' + data.authorUrls[i] + '">' + p[2] + '</a>';
            }
        });

        $table = $('table').dataTable({
            aaSorting: [[4, 'desc']],
            bLengthChange: false,
            bInfo: false,
            sPaginationType: 'full_numbers',
            iDisplayLength: 21,
            bProcessing: true,
            aaData: data.packages,
            fnInitComplete: function() {
                $spinner.remove();
            }
        });

        date = new Date(data.end);

        $lastUpdate = $('<label>').addClass('lastUpdate');
        $lastUpdate.text('Last update: ' + date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate());
        $('div.dataTables_filter').append($lastUpdate);

        $input = $(':input[type=text]').focus();
        if (window.location.hash.length > 1) {
            $input.val(window.location.hash.slice(1));
            $table.fnFilter($input.val());
        }
        $input.keyup(function(e) {
            if (e.keyCode === 27) {
                $table.fnFilter('');
                $input.val('');
                $input.click();
            }
            window.location.hash = $input.val();
        });
    });
});

