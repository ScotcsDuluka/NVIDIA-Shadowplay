Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$f=New-Object Windows.Forms.Form
$f.Text='NVENC Moving Probe'; $f.StartPosition='Manual'; $f.Location=New-Object Drawing.Point(100,100); $f.ClientSize=New-Object Drawing.Size(900,500); $f.FormBorderStyle='FixedSingle'
$f.BackColor=[Drawing.Color]::White
$box=New-Object Windows.Forms.Panel; $box.Size=New-Object Drawing.Size(120,120); $box.BackColor=[Drawing.Color]::Black; $box.Location=New-Object Drawing.Point(0,190); $f.Controls.Add($box)
$label=New-Object Windows.Forms.Label; $label.Text='MOVING-PROBE'; $label.Font=New-Object Drawing.Font('Arial',24,[Drawing.FontStyle]::Bold); $label.AutoSize=$true; $label.Location=New-Object Drawing.Point(300,20); $f.Controls.Add($label)
$x=0; $dx=12
$t=New-Object Windows.Forms.Timer; $t.Interval=16; $t.Add_Tick({ $x += $dx; if($x -ge 760 -or $x -le 0){$dx=-$dx}; $box.Left=$x; $label.Text=('MOVING-PROBE X='+$x) }); $t.Start()
$f.Add_Shown({$f.Activate()})
[Windows.Forms.Application]::Run($f)
