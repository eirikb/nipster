$(function() {
    var $table = $('table').dataTable({
        sAjaxSource: 'packages.json',
        fnServerData: function(sSource, aoData, fnCallback, oSettings) {
            oSettings.jqXHR = $.ajax({
                url: sSource,
                success: fnCallback
            });
        },
        aaSorting: [
            [5, 'desc']
        ],
        aoColumnDefs: [{
            sType: 'html',
            aTargets: [0, 2]
        }, {
            asSorting: ['desc', 'asc'],
            aTargets: ['_all']
        }],
        aoColumns: [
        null, null, null, null, null, null, null, null,
        {
            bVisible: false
        }],
        bLengthChange: false,
        sPaginationType: 'full_numbers',
        iDisplayLength: 21,
        bProcessing: true,
        bAutoWidth: false,
        bDeferRender: true,
        fnRowCallback: function(tr, data, i) {
            var $td = $('td', tr);
            var $npm = $td.last();
            if ($npm.html().length > 0) return;

            var $gh = $td.first();
            var name = $gh.text().split(' ');
            var url = name[0];
            name = name[1];
            var $gha = $('<a>').attr('href', 'https://github.com/' + url).text(name);
            $gh.text('').append($gha);

            $npm.html('<a class="npm" href="http://npmjs.org/package/' + name + '" title="' + data[8] + '">△</a>');
            $td.eq(1).prop('title', data[1]);
        }
    }).fnSetFilteringDelay(200);

    var $input = $(':input[type=text]').focus();

    $input.keyup(function(e) {
        if (e.keyCode === 27) {
            $table.fnFilter('');
            $input.val('');
            $input.click();
        }
        if (window.location.hash.length > 1) {
            window.History.replaceState({}, '', '#' + $input.val());
        }
        window.location.hash = $input.val();
    });

    var hash = window.location.hash.slice(1);
    if (hash.length > 0) {
        hash = decodeURIComponent(hash);
        $input.val(hash);
        $table.fnFilter($input.val());
    } else {
        $input.val('');
        $table.fnFilter('');
    }
});
