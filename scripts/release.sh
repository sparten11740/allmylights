#!/bin/bash

latestVersion="0"

latestTagName=$(git describe --tags "$(git rev-list --tags --max-count=1)" 2> /dev/null)
if [ "$latestTagName" ]; then
    latestVersion="${latestTagName:1}"
fi

release="v$((latestVersion + 1))"

git tag $release
if [ $? -eq 0 ]; then
    echo "Created tag for release $release"
fi
