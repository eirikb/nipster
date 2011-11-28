$(function() {
    var $spinner = $(new Spinner().spin().el).css({
        left: 50,
        top: 50,
        height: 100
    });
    $('.content').append($spinner);

    $.getJSON('json/packages.json', function(data) {
        var $includeNongithub, date, $lastUpdate, $table, $input;

        $.each(data.packages, function(i, p) {
            p[0] = '<a href="https://github.com/' + data.urls[i] + '">' + p[0] + '</a>';
        });

        $table = $('table').dataTable({
            aaSorting: [[3, 'desc']],
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

        $includeNongithub = $('<div>').append($('<input id="includeNongithub" type="checkbox">').change(function() {
            var $this = $(this),
            nongithub;

            $table.fnClearTable();
            $table.fnAddData(data.packages);

            if ($this.is(':checked')) {
                nongithub = [];
                $.each(data.nongithub, function(i, n) {
                    nongithub.push([n[0], n[1], '', '']);
                });
                $table.fnAddData(nongithub);
            }
        })).append('<label for="includeNongithub">Include non-github');

        date = new Date(data.end);

        $lastUpdate = $('<label>').addClass('lastUpdate').text('Last update: ' + date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate());

        $('div.dataTables_filter').append($includeNongithub).prepend($lastUpdate);

        $input = $(':input[type=text]').focus();
        if (window.location.hash.length > 1) {
            $input.val(window.location.hash.slice(1));
        }
        $input.keyup(function(e) {
            if (e.keyCode === 27) {
                $table.fnFilter('');
                $input.val('');
            }
            window.location.hash = $input.val();
        });
    });
});

