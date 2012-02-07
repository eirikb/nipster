var fs = require('fs'),
path = require('path'),
request = require('request');

var raw, file = './packages.json',
fileRaw = './packages-raw.json',
start = new Date();

console.log('Nipster! - %s-%s-%s', start.getFullYear(), start.getMonth() + 1, start.getDate());

try {
    raw = require(fileRaw);
} catch(e) {
    raw = {
        packages: {}
    };
}

updatePackages(raw, function(err, raw) {
    console.log('Total packages: %d', Object.keys(raw.packages).length);

    var repos = getRepositories(raw.packages);
    repos = filterRepoUrls(repos);

    githubSync(repos, function(repos) {
        var packages = {
            start: start
        },
        repoUrls = [],
        authorUrls = [];

        packages.packages = repos.map(function(r, i) {
            var repo = raw.packages[r.name],
            author = repo.author;

            if (r.url) {
                repoUrls[i] = r.url;
            } else {
                repoUrls[i] = 0;
            }
            if (author) {
                if (author.url) authorUrls[i] = repo.author.url;
                author = author.name;
            }

            return [r.name, repo.description, author, r.forks, r.watchers];
        });

        packages.repoUrls = repoUrls;
        packages.authorUrls = authorUrls;
        packages.end = Date.now();

        fs.writeFile(file, JSON.stringify(packages), function() {
            console.log('Done');
        });
    });
});

function updatePackages(raw, cb) {
    var path = '/-/all/';
    if (raw.timestamp) path += 'since?startkey=' + raw.timestamp;

    request.get({
        url: 'http://registry.npmjs.org' + path,
        json: true
    },
    function(err, res, data) {
        if (!err) {
            Object.keys(data).forEach(function(key) {
                raw.packages[key] = data[key];
            });
            raw.timestamp = Date.now();
            fs.writeFile(fileRaw, JSON.stringify(raw), function(err) {
                cb(err, raw);
            });
        } else {
            cb(err);
        }
    });
}

function getRepositories(rawPackages) {
    return Object.keys(rawPackages).map(function(k) {
        var p = rawPackages[k],
        urls = [];

        function parseRepo(repo) {
            if (repo) {

                ['private', 'url', 'web', 'path'].forEach(function(t) {
                    var r = repo[t];
                    if (r) urls.push(r);
                });
                if (typeof repo === 'string') urls.push(repo);
            }
        }

        if (Array.isArray(p.repository)) {
            p.repository.forEach(parseRepo);
        } else {
            parseRepo(p.repository);
        }
        if (p.url) urls.push(p.url);

        return {
            name: k,
            url: urls
        };
    });
}

function filterRepoUrls(repos) {
    return repos.filter(function(repo) {
        var urls = repo.url.filter(function(url) {
            return ('' + url).match(/github/);
        }).map(function(url) {
            return url.replace(/(^.*\.com.)|\.git$/g, '');
        });

        if (urls.length > 0) {
            repo.url = urls[0];
            return true;
        }
    });
}

function githubSync(repos, cb) {
    function sync(repos, i) {
        var repo, sleep;

        if (!i) i = 0;
        if (i < repos.length - 1) {
            repo = repos[i];
            console.log('%d - %s - %s', repos.length - i, repo.name, repo.url);
            github(repo.url, function(err, data, limit) {
                if (!err) {
                    repo.forks = data.forks;
                    repo.watchers = data.watchers;
                } else {
                    repo.error = err;
                    repo.errorMsg = data;
                }

                if (limit > 0) {
                    sync(repos, i + 1);
                } else {
                    sleep = (60 * 60) - (Date.now() - start.getTime()) / 1000;
                    sleep = Math.floor(sleep);

                    console.log('Limit reached, sleeping for %d seconds', sleep);
                    setTimeout(function() {
                        sync(repos, i + 1);
                    },
                    sleep * 1000);
                }
            });
        } else {
            cb(repos);
        }
    }
    sync(repos);
}

function github(url, cb) {
    request.get({
        url: 'https://api.github.com/repos/' + url,
        json: true
    },
    function(err, res, data) {
        var limit = parseInt(res.headers['x-ratelimit-remaining'], 10);
        if (!err) {
            err = ! data || ! data.html_url;
            cb(err, data, limit);
        } else {
            cb(err, null, limit);
        }
    });
}

