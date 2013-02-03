$(function() {
    var $table = $('table').dataTable({
        sAjaxSource: 'arrays.txt',
        aaSorting: [
            [5, 'desc']
        ],
        aoColumnDefs: [{
            sType: 'html',
            aTargets: [0]
        }],
        bLengthChange: false,
        sPaginationType: 'full_numbers',
        iDisplayLength: 21,
        bProcessing: true,
        bAutoWidth: false,
        bDeferRender: true,
        fnRowCallback: function(tr, data, i) {
            var $gh = $('td:first a', tr);
            var href = '' + $gh.attr('href');
            if (!href.match(/github\.com/)) $gh.attr('href', 'https://github.com/' + href);

            var $npm = $('td:last', tr);
            if ($npm.html().length > 0) return;
            var name = $('td:first', tr).text();
            $npm.html('<a class="npm" href="http://npmjs.org/package/' + name + '">△</a>');
        }
    });

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

    $(window).on('hashchange', function() {
        var hash = window.location.hash.slice(1);
        if (hash.length > 0) {
            hash = decodeURIComponent(hash);
            $input.val(hash);
            $table.fnFilter($input.val());
        } else {
            $input.val('');
            $table.fnFilter('');
        }
    }).trigger('hashchange');
});
