Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::SetHighDpiMode([System.Windows.Forms.HighDpiMode]::PerMonitorV2)
$form = New-Object System.Windows.Forms.Form
$form.Text = 'NVIDIA Shadowplay MOTION PROBE'
$form.StartPosition = 'Manual'
$form.Location = New-Object System.Drawing.Point(300,180)
$form.Size = New-Object System.Drawing.Size(1000,500)
$form.FormBorderStyle = 'FixedSingle'
$form.TopMost = $true
$form.BackColor = [System.Drawing.Color]::Black
$label = New-Object System.Windows.Forms.Label
$label.Text = 'MOTION PROBE 0000'
$label.ForeColor = [System.Drawing.Color]::White
$label.BackColor = [System.Drawing.Color]::Black
$label.Font = New-Object System.Drawing.Font('Consolas',32,[System.Drawing.FontStyle]::Bold)
$label.AutoSize = $true
$label.Location = New-Object System.Drawing.Point(20,20)
$form.Controls.Add($label)
$box = New-Object System.Windows.Forms.Panel
$box.Size = New-Object System.Drawing.Size(220,220)
$box.BackColor = [System.Drawing.Color]::White
$box.Location = New-Object System.Drawing.Point(20,120)
$form.Controls.Add($box)
$state = @{ x = 20; dx = 17; n = 0 }
$timer = New-Object System.Windows.Forms.Timer
$timer.Interval = 30
$timer.Add_Tick({
  $state.n++
  $state.x += $state.dx
  if ($state.x -le 20 -or $state.x -ge 730) { $state.dx = -$state.dx }
  $box.Location = New-Object System.Drawing.Point($state.x,120)
  $label.Text = ('MOTION PROBE {0:D5}' -f $state.n)
  $form.Invalidate()
})
$form.Add_Shown({ $timer.Start() })
$form.Add_FormClosed({ $timer.Stop() })
[System.Windows.Forms.Application]::Run($form)
