var utils = require('utils.js'),
fileAll = 'json/packages-all.json',
file = 'json/public/packages.json',
all = {},
packages = {},
updateGithub = function() {
    var keys = Object.keys(all).filter(function(key) {
        var p = all[key];
        return ! p.repo && p.repository && p.repository.url && p.repository.url.match(/github/i);
    });

    if (keys.length > 0) {
        var key = keys[0],
        origUrl = all[key].repository.url,
        url = origUrl.replace(/(^.*\.com\/)|:|.git$/g, '');

        console.log('%d - %s - %s', keys.length, key, url);

        utils.getJSON({
            host: 'api.github.com',
            path: '/repos/' + url
        },
        function(repo) {
            all[key].repo = repo;
            utils.saveJSON(fileAll, all, function() {
                updateGithub();
            });
        });
    } else {
        packages = [];
        Object.keys(all).forEach(function(key) {
            var a = all[key];
            packages.push([a.name, a.description, a.repo ? a.repo.forks: '', a.repo ? a.repo.watchers: '']);
        });
        packages = {
            aaData: packages
        };
        utils.saveJSON(file, packages, function() {
            console.log('DONE!');
        });
    }
};

utils.loadJSON(fileAll, function(err, data) {
    Object.keys(data).forEach(function(key) {
        all[key] = data[key];
    });
    utils.getJSON({
        host: 'registry.npmjs.org'
    },
    function(data) {
        Object.keys(data).forEach(function(key) {
            if (!all[key]) {
                all[key] = data[key];
            }
        });
        updateGithub();
    });
});

