Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$wav='C:\My Project\NVIDIA-Shadowplay\evidence\p13-device-runtime\beep.wav'
$form=New-Object Windows.Forms.Form
$form.FormBorderStyle='None'; $form.WindowState='Maximized'; $form.TopMost=$true
$form.BackColor=[Drawing.Color]::Black; $form.ShowInTaskbar=$false
$form.KeyPreview=$true
$g=$form.CreateGraphics()
$player=New-Object System.Media.SoundPlayer($wav)
$form.Show(); [Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 300
$sw=[Diagnostics.Stopwatch]::StartNew()
$next=0.0
while($sw.Elapsed.TotalSeconds -lt 30){
  $t=$sw.Elapsed.TotalSeconds
  $on=([Math]::Floor($t/3.0) -eq [Math]::Floor(($t+0.001)/3.0))
  $phase=$t%3.0
  if($phase -lt 0.18){
    if($form.BackColor -ne [Drawing.Color]::White){$form.BackColor=[Drawing.Color]::White; $form.Invalidate(); [Windows.Forms.Application]::DoEvents()}
    if($next -le $t){$next=$next+3.0; try{$player.Play()}catch{}}
  } elseif($form.BackColor -ne [Drawing.Color]::Black){$form.BackColor=[Drawing.Color]::Black; $form.Invalidate(); [Windows.Forms.Application]::DoEvents()}
  Start-Sleep -Milliseconds 5
}
try{$player.Stop()}catch{}
$form.Close(); $form.Dispose(); $g.Dispose()
