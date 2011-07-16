Developing
---
This is how I develop on nipster (dev.sh):

    mkdir nipster
    cd nipster
    git clone git@github.com:eirikb/nipster
    git clone git@github.com:eirikb/nipster static/nipster
    cd static/nipster
    git checkout gh-pages
    mv json ../../nipster/
    ln -s ../../nipster/json ./json
    simple-server
