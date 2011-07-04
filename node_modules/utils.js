var http = require('https'),
fs = require('fs');

exports.getJSON = function(options, callback) {
    http.get(options, function(res) {
        var data = '';
        res.on('data', function(d) {
            data += d;
        });
        res.on('end', function() {
            try {
                data = JSON.parse(data);
                callback(data);
            } catch(e) {
                console.log('Parse error (%s) for data: %s', e, data);
            }
        });
    });
};

exports.loadJSON = function(file, callback) {
    fs.readFile(file, function(err, data) {
        try {
            if (!err) {
                data = JSON.parse(data);
                callback(err, data);
            } else {
                callback(true);
            }
        } catch(e) {
            console.log('Parse error (%s) for data: %s', e, data);
            callback(true);
        }
    });
};

exports.saveJSON = function(file, data, callback) {
    try  {
        data = JSON.stringify(data);
        fs.writeFile(file, data, function(err) {
            if (err) {
                console.log(err);
            }
            callback();
        });
    } catch(e) {
        console.log(e);
        callback();
    }
};

