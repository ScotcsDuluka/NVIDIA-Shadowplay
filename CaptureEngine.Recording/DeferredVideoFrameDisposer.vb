Imports System.Collections.Concurrent
Imports System.Threading
Imports CaptureEngine.Video

Namespace CaptureEngine.Recording

    ''' <summary>
    ''' Dedicated single-consumer disposal queue for GPU-backed video frames.
    ''' Keeps D3D11/COM Release work off the CFR presentation thread while
    ''' preserving deterministic cleanup at session end.
    ''' </summary>
    Friend NotInheritable Class DeferredVideoFrameDisposer
        Implements IDisposable

        Private ReadOnly _queue As New ConcurrentQueue(Of IVideoFrame)()
        Private ReadOnly _wake As New AutoResetEvent(False)
        Private ReadOnly _worker As Thread
        Private _stopping As Integer = 0
        Private _disposed As Integer = 0

        Public Sub New()
            _worker = New Thread(AddressOf WorkerLoop) With {
                .IsBackground = True,
                .Name = "CaptureEngine-GpuDisposer"
            }
            _worker.Start()
        End Sub

        Public Sub Enqueue(frame As IVideoFrame)
            If frame Is Nothing Then Return
            If Volatile.Read(_disposed) <> 0 OrElse Volatile.Read(_stopping) <> 0 Then
                Try
                    frame.Dispose()
                Catch
                End Try
                Return
            End If
            _queue.Enqueue(frame)
            _wake.Set()
        End Sub

        Public Sub CompleteAndWait()
            If Interlocked.Exchange(_stopping, 1) = 0 Then _wake.Set()
            If Not _worker.Join(5000) Then
                Throw New TimeoutException("GPU disposer did not drain within 5 seconds.")
            End If
            DrainSynchronously()
        End Sub

        Private Sub WorkerLoop()
            Do
                _wake.WaitOne(50)
                DrainSynchronously()
                If Volatile.Read(_stopping) <> 0 AndAlso _queue.IsEmpty Then Exit Do
            Loop
        End Sub

        Private Sub DrainSynchronously()
            Dim frame As IVideoFrame = Nothing
            While _queue.TryDequeue(frame)
                Try
                    frame.Dispose()
                Catch
                    ' Never let one bad frame kill the disposer thread.
                End Try
                frame = Nothing
            End While
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.Exchange(_disposed, 1) <> 0 Then Return
            CompleteAndWait()
            _wake.Dispose()
        End Sub

    End Class

End Namespace
