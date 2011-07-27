$(function() {
	$.getJSON('json/packages.json', function(data) {
		console.log(data);
		var i, l, $includeUntracked, $table;
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

		$includeUntracked = $('<div>').append($('<input id="includeUntracked" type="checkbox">').change(function() {
            var i, untracked;
			$table.fnClearTable();
			$table.fnAddData(data.packages);
			if (this.checked) {
                untracked = [];
				for (i = 0; i < data.untracked.length; i++) {
					untracked.push([data.untracked[i][0], '', '', '']);
				}
                $table.fnAddData(untracked);
			}
		})).append('<label for="includeUntracked">Include untracked');

		$('div.dataTables_filter').append($includeUntracked);
	});
});

