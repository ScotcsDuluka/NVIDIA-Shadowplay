$env:OSC_DIR = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\osc'
Start-Process -FilePath 'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA NodeAPI\NvNode.exe' -ArgumentList 'index.js' -WorkingDirectory 'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA NodeAPI'
