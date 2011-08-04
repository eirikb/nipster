$(function() {
	$.getJSON('json/packages.json', function(data) {
		var i, l, $includeNongithub, date, $lastUpdate, $table;

		for (i = 0; i < data.packages.length; i++) {
			l = data.packages[i];
			l[0] = '<a href="' + data.urls[i] + '">' + l[0] + '</a>';
		}
		$table = $('table').dataTable({
			aaSorting: [[3, 'desc']],
			bLengthChange: false,
			bInfo: false,
			sPaginationType: 'full_numbers',
			iDisplayLength: 21,
			bProcessing: true,
			aaData: data.packages
		});

		$includeNongithub = $('<div>').append($('<input id="includeNongithub" type="checkbox">').change(function() {
            var i, nongithub, p;
			$table.fnClearTable();
			$table.fnAddData(data.packages);
			if (this.checked) {
                nongithub = [];
				for (i = 0; i < data.nongithub.length; i++) {
                    p = data.nongithub[i];
					nongithub.push([p[0], p[1], '', '']);
				}
                $table.fnAddData(nongithub);
			}
		})).append('<label for="includeNongithub">Include non-github');

        date = new Date(data.end);

        $lastUpdate = $('<label>').addClass('lastUpdate').text('Last update: ' + date.getFullYear() +
                '-' + (date.getMonth() + 1) + '-' + date.getDate());

		$('div.dataTables_filter').append($includeNongithub).prepend($lastUpdate);
        
	});
});

