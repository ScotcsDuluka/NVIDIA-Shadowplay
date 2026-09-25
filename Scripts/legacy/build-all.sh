#!/usr/bin/env bash
# build-all.sh — Phase 12b compile verification on Linux CI.
#
# net10.0-windows projects cannot RUN here (D3D11/NVENC/WASAPI), but they
# MUST COMPILE. dotnet supports cross-targeting via EnableWindowsTargeting.
# The pure net8.0 test suites DO run (with real ffmpeg) — that is the
# runtime evidence half available on Linux; GPU-path evidence must come
# from scripts/validate-phase12b.ps1 on a Windows machine.
set -euo pipefail
cd "$(dirname "$0")/.."

echo "=================================================="
echo " Phase 12b — Linux compile verification"
echo "=================================================="

dotnet build "Overlay/NVIDIA Overlay.sln" -c Release \
    -p:EnableWindowsTargeting=true --nologo -v q

echo "BUILD OK (all projects, including net10.0-windows)"

SUITES="CaptureEngine.Tests CaptureEngine.FFmpegTests CaptureEngine.FrameContractTests CaptureEngine.ConfigTests CaptureEngine.Encoder.Tests CaptureEngine.Recording.Tests"
FAILED=0
for s in $SUITES; do
    echo ""
    echo "──── $s ────"
    if ! dotnet run --project "$s/$s.vbproj" -c Release --no-build; then
        FAILED=1
        echo "SUITE FAILED: $s"
    fi
done

if [ "$FAILED" -ne 0 ]; then
    echo "SOME SUITES FAILED"
    exit 2
fi
echo ""
echo "ALL LINUX-RUNNABLE SUITES PASSED"
