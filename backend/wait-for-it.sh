#!/bin/bash
# wait-for-it.sh - Wait for a service to be available

set -e

host="$1"
shift
cmd="$@"

until nc -z -v -w30 ${host%:*} ${host#*:}; do
  echo "Waiting for $host..."
  sleep 1
done

echo "$host is available, starting application..."
exec $cmd
