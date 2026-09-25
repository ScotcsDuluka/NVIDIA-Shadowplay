$dir = "C:\My Project\NVIDIA-Shadowplay\Tester\test\Engine\ConfigTruth"
$files = "W2OverlayHonestyTests.vb","CT4ConfigTruthTests.vb","FrameRetirementContractTests.vb","SessionEndContractTests.vb","VCTVideoWiringTests.vb","GetEngineModeTests.vb","H1OutputPathTests.vb","P3UIContractTests.vb","L1ReconnectTests.vb"
foreach ($f in $files) {
    $p = Join-Path $dir $f
    if (-not (Test-Path $p)) { continue }
    $ids = Select-String -Path $p -Pattern '"((W2|CT|FR|SE|VCT|GEM|H1|P3|L1|S1)[A-Za-z0-9.\-]*)"'
    Write-Output ("== " + $f)
    foreach ($m in $ids) { Write-Output ("   " + $m.Matches[0].Groups[1].Value) }
}
