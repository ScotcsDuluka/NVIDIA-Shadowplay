#!/bin/bash
f="$1"
n=$(cat "$f")
echo $((n+1)) > "$f"
