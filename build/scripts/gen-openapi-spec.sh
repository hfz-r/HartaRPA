#!/bin/bash
set -ex

readonly ROOT_DIR=`pwd`
readonly PROTO_PATH=${ROOT_DIR}/src/Proto

api=''
proto_file=''
target_dir=''

while [[ $# -gt 0 ]]; do
    case "$1" in
        -p | --project )
          api="$2"; shift 2;;
        *)
          echo "Unknown option $1"
          usage; exit 2 ;;
    esac
done

if [[ $api == "file" ]]; then
    echo "Generating OpenAPI spec for File API..."
    target_dir=${ROOT_DIR}/src/Services/File/File.API
    proto_file="$api".proto
elif [[ $api == "ordering" ]]; then
    echo "Generating OpenAPI spec for Ordering API..."
    # target_dir=${ROOT_DIR}/src/Services/File/File.API
    # proto_file="$api".proto
fi

protoc -I$PROTO_PATH \
    --openapiv2_out ${target_dir}/wwwroot \
    --openapiv2_opt logtostderr=true,allow_merge=true \
    $PROTO_PATH/$proto_file
