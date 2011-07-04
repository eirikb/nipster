var express = require('express'),
path = require('path'),
rated = require('rated.js'),
app = module.exports = express.createServer();

app.configure(function() {
    app.set('views', __dirname + '/views');
    app.set('view engine', 'jade');
    app.use(app.router);
    app.use(express.static(path.join(__dirname + '/public')));
});

app.get('/', function(req, res) {
    res.render('index', {
        title: 'Nipster!',
        packages: rated.packages
    });
});

app.listen(8080);
console.log("Express server listening on port %d in %s mode", app.address().port, app.settings.env);

