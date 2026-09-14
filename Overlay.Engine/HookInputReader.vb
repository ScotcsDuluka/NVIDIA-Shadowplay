' HookInputReader.vb — reads in-game hook input events from a small
' shared-memory ring ("NVIDIA_Share_Overlay_Input_v1"). Replaces the old
' HTTP path: the game's online-fix layer intercepts WinHTTP inside the
' game process (only the first POST ever reached us), while shared memory
' cannot be intercepted. Event format (16 bytes each, ring of 512):
'   int type (1=move 2=down 3=up 4=keydown 5=keyup), int a, int b, int c
'   move/down/up: a=x b=y · keys: a=vk b=shift c=ctrl

Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Threading

Public Class HookInputReader

    Public Event Input(bodyJson As String)

    Private Const MmfName As String = "NVIDIA_Share_Overlay_Input_v1"
    Private Const RingSize As Integer = 512
    Private Const RingBytes As Integer = 16 + RingSize * 16

    Private _mmf As MemoryMappedFile
    Private _view As MemoryMappedViewAccessor
    Private _readIdx As Integer
    Private _thread As Thread
    Private _stopFlag As Boolean
    Private _events As Long

    Public Sub Start()
        If _thread IsNot Nothing Then Return
        _stopFlag = False
        _thread = New Thread(AddressOf LoopRead) With {.IsBackground = True, .Name = "HookInputReader"}
        _thread.Start()
    End Sub

    Public Sub [Stop]()
        _stopFlag = True
        Try : _thread?.Join(300) : Catch : End Try
    End Sub

    Private Sub LoopRead()
        ' create OUR side first so the DLL always finds it
        Try
            _mmf = MemoryMappedFile.CreateOrOpen(MmfName, RingBytes, MemoryMappedFileAccess.ReadWrite)
            _view = _mmf.CreateViewAccessor(0, RingBytes)
            _view.Write(0, &H4E49534E)          ' "NSIN"
            _view.Write(4, 0)                    ' writeIdx
            _view.Write(8, 0)                    ' readIdx (engine-acked)
        Catch
            Return
        End Try

        Dim lastLog As Integer = Environment.TickCount
        While Not _stopFlag
            Try
                Dim writeIdx As Integer = _view.ReadInt32(4)
                Dim behind As Integer = writeIdx - _readIdx
                If behind > RingSize Then _readIdx = writeIdx - RingSize   ' overflow: skip
                While _readIdx < writeIdx
                    Dim slot As Integer = _readIdx Mod RingSize
                    Dim baseOff As Integer = 16 + slot * 16
                    Dim typ As Integer = _view.ReadInt32(baseOff)
                    Dim a As Integer = _view.ReadInt32(baseOff + 4)
                    Dim b As Integer = _view.ReadInt32(baseOff + 8)
                    Dim c As Integer = _view.ReadInt32(baseOff + 12)
                    Dim json As String = Nothing
                    Select Case typ
                        Case 1 : json = "{""type"":""mousemove"",""x"":" & a & ",""y"":" & b & "}"
                        Case 2 : json = "{""type"":""mousedown"",""x"":" & a & ",""y"":" & b & ",""button"":0}"
                        Case 3 : json = "{""type"":""mouseup"",""x"":" & a & ",""y"":" & b & ",""button"":0}"
                        Case 4 : json = "{""type"":""keydown"",""vk"":" & a & ",""shift"":" & b & ",""ctrl"":" & c & "}"
                        Case 5 : json = "{""type"":""keyup"",""vk"":" & a & ",""shift"":" & b & ",""ctrl"":" & c & "}"
                    End Select
                    If json IsNot Nothing Then
                        RaiseEvent Input(json)
                        _events += 1L
                    End If
                    _readIdx += 1
                End While
                _view.Write(8, _readIdx)
                If _events > 0 AndAlso Environment.TickCount - lastLog > 10000 Then
                    HookCdpCapture.LogLine("input ring: " & _events.ToString() & " events read")
                    lastLog = Environment.TickCount
                End If
            Catch
                Try : _view = Nothing : _mmf = Nothing : Catch : End Try
                Thread.Sleep(1000)
            End Try
            Thread.Sleep(8)
        End While
    End Sub

End Class
