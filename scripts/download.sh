#!/bin/bash

if [[ $# -eq 0 ]] ; then
    echo "Usage: $0 /target/dir"
    exit 1
fi

target_dir=$1

function main() {
    download "win-x64"
    download "linux-x64"
    download "linux-arm"

    extract "linux-x64.zip"
    extract "linux-arm.zip"
}

function download(){
    filter=".artifacts[] | select( .name | contains(\"$1\")) | .archive_download_url"
    link=$(curl -s https://api.github.com/repos/sparten11740/allmylights/actions/artifacts | jq -cr "$filter" | head -n 1)
   
    curl -L -H "Authorization: Bearer $GITHUB_PAT" $link --output "$target_dir/$1.zip"
}

function extract(){
    pushd "$target_dir" || exit
        unzip "$1"
        rm "$1"
    popd || exit
}

main
