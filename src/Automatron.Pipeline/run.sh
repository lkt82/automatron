#!/usr/bin/env bash

bash --version 2>&1 | head -n 1

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_MULTILEVEL_LOOKUP=0

dotnet build /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
dotnet run --no-build -- "$@"
