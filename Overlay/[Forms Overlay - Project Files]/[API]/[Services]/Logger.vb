Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Public Module Logger

#Region "Custom TraceListener — รับ Debug.WriteLine มาเขียนที่ console"

    Private Class LoggerTraceListener
        Inherits System.Diagnostics.TraceListener

        Public Overrides Sub Write(message As String)
            If Not _isStarted Then Return
            SyncLock _lock
                Try
                    Console.Write(message)
                Catch
                End Try
            End SyncLock
        End Sub

        Public Overrides Sub WriteLine(message As String)
            If Not _isStarted Then Return
            SyncLock _lock
                Try
                    Dim ts As String = DateTime.Now.ToString("HH:mm:ss.fff")
                    Console.ForegroundColor = ConsoleColor.DarkGray
                    Console.Write(ts & " ")
                    Console.ForegroundColor = ConsoleColor.White
                    Console.WriteLine(message)
                Catch
                End Try
            End SyncLock
        End Sub

    End Class


    Private Sub RedirectDebugToConsole()
        Debug.Listeners.Clear()
        Debug.Listeners.Add(New LoggerTraceListener())
    End Sub

#End Region

#Region "WinAPI — Console"

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function AllocConsole() As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function FreeConsole() As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function GetConsoleWindow() As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Private Function SetConsoleCtrlHandler(handler As HandlerRoutine, add As Boolean) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr,
                                   X As Integer, Y As Integer, cx As Integer, cy As Integer,
                                   uFlags As UInteger) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    Private Delegate Function HandlerRoutine(ctrlType As UInteger) As Boolean
    Private _ctrlHandler As HandlerRoutine

    Private Const CTRL_C_EVENT As UInteger = 0
    Private Const CTRL_BREAK_EVENT As UInteger = 1
    Private Const CTRL_CLOSE_EVENT As UInteger = 2

    Private Const SW_HIDE As Integer = 0
    Private Const SW_SHOW As Integer = 5
    Private Const SWP_NOSIZE As UInteger = &H1
    Private Const SWP_NOMOVE As UInteger = &H2
    Private ReadOnly HWND_TOPMOST As IntPtr = New IntPtr(-1)

    Private Function ConsoleCtrlHandler(ctrlType As UInteger) As Boolean
        If ctrlType = CTRL_CLOSE_EVENT OrElse ctrlType = CTRL_C_EVENT OrElse ctrlType = CTRL_BREAK_EVENT Then
            Try
                FreeConsole()
                _isStarted = False
            Catch
            End Try
            Return True
        End If
        Return False
    End Function

#End Region

#Region "Fields"

    Private _consoleOut As StreamWriter
    Private _isStarted As Boolean = False
    Private _lock As New Object

    Private _tagColor As ConsoleColor = ConsoleColor.Cyan
    Private _msgColor As ConsoleColor = ConsoleColor.White
    Private _errorColor As ConsoleColor = ConsoleColor.Red
    Private _successColor As ConsoleColor = ConsoleColor.Green
    Private _warningColor As ConsoleColor = ConsoleColor.Yellow
    Private _systemColor As ConsoleColor = ConsoleColor.DarkGray

#End Region

#Region "Start / Stop"

    Public Sub Start(Optional title As String = "Logger")
        If _isStarted Then Return
        SyncLock _lock
            If _isStarted Then Return

            Try
                AllocConsole()
                Console.Title = title
                Console.OutputEncoding = System.Text.Encoding.UTF8

                _consoleOut = New StreamWriter(Console.OpenStandardOutput()) With {.AutoFlush = True}
                Console.SetOut(_consoleOut)

                _ctrlHandler = New HandlerRoutine(AddressOf ConsoleCtrlHandler)
                SetConsoleCtrlHandler(_ctrlHandler, True)

                RedirectDebugToConsole()

                _isStarted = True

                Console.ForegroundColor = ConsoleColor.Cyan
                Console.WriteLine("╔═══════════════════════════════════════╗")
                Console.WriteLine("║        Logger Console Started         ║")
                Console.WriteLine("║  D(""msg"") or Logger.Log(""tag"",""msg"")   ║")
                Console.WriteLine("║  Press X to close console only        ║")
                Console.WriteLine("╚═══════════════════════════════════════╝")
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine("")

            Catch ex As Exception
                _isStarted = True
            End Try
        End SyncLock
    End Sub

    Public Sub [Stop]()
        SyncLock _lock
            If Not _isStarted Then Return
            Try
                Console.Out.Flush()
                If _consoleOut IsNot Nothing Then
                    _consoleOut.Close()
                    _consoleOut.Dispose()
                    _consoleOut = Nothing
                End If
                SetConsoleCtrlHandler(_ctrlHandler, False)
                FreeConsole()
            Catch
            End Try
            _isStarted = False
        End SyncLock
    End Sub

    Public Sub Hide()
        If Not _isStarted Then Return
        Dim hwnd = GetConsoleWindow()
        If hwnd <> IntPtr.Zero Then ShowWindow(hwnd, SW_HIDE)
    End Sub

    Public Sub [Show]()
        Try
            If Not _isStarted OrElse GetConsoleWindow() = IntPtr.Zero Then
                AllocConsole()
                Console.Title = "Logger"
                Console.OutputEncoding = System.Text.Encoding.UTF8
                _consoleOut = New StreamWriter(Console.OpenStandardOutput()) With {.AutoFlush = True}
                Console.SetOut(_consoleOut)
                _ctrlHandler = New HandlerRoutine(AddressOf ConsoleCtrlHandler)
                SetConsoleCtrlHandler(_ctrlHandler, True)
                RedirectDebugToConsole()
                _isStarted = True
            End If
        Catch
        End Try
        Dim hwnd = GetConsoleWindow()
        If hwnd <> IntPtr.Zero Then
            ShowWindow(hwnd, SW_SHOW)
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE Or SWP_NOMOVE)
        End If
    End Sub

    Public Sub TopMost()
        If Not _isStarted Then Return
        Dim hwnd = GetConsoleWindow()
        If hwnd <> IntPtr.Zero Then SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE Or SWP_NOMOVE)
    End Sub

    Public Sub NotTopMost()
        If Not _isStarted Then Return
        Dim hwnd = GetConsoleWindow()
        If hwnd <> IntPtr.Zero Then SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE Or SWP_NOMOVE)
    End Sub

    Public Sub Clear()
        If Not _isStarted Then Return
        Console.Clear()
    End Sub

    Public ReadOnly Property IsStarted As Boolean
        Get
            Return _isStarted
        End Get
    End Property

    ''' <summary>Auto-start the console unless config.json says DebugEnabled = false.</summary>
    Public Sub AutoStart(Optional title As String = "Logger")
        Try
            Dim enabled As Boolean = True   ' default: console on
            Dim cfg As String = AppLayout.P("Config", "config.json")
            If IO.File.Exists(cfg) Then
                Dim json As String = IO.File.ReadAllText(cfg)
                If Regex.IsMatch(json, """DebugEnabled""\s*:\s*false", RegexOptions.IgnoreCase) Then
                    enabled = False
                End If
            End If

            If enabled Then
                Start(title)
                D("Logger", "AutoStart: DebugEnabled = true")
            Else
                D("Logger", "AutoStart: DebugEnabled = false — console disabled")
            End If
        Catch
            Start(title)
        End Try
    End Sub

#End Region

#Region "Log Methods"

    Public Sub Log(tag As String, msg As String)
        Write(tag, msg, _msgColor)
    End Sub

    Public Sub Sys(msg As String)
        Write("SYS", msg, _systemColor)
    End Sub

    Public Sub Success(tag As String, msg As String)
        Write(tag, msg, _successColor)
    End Sub

    Public Sub Warn(tag As String, msg As String)
        Write(tag, msg, _warningColor)
    End Sub

    Public Sub [Error](tag As String, msg As String)
        Write(tag, msg, _errorColor)
    End Sub

    Public Sub [Error](tag As String, ex As Exception)
        Write(tag, ex.Message, _errorColor)
        If ex.StackTrace IsNot Nothing Then
            Write("STACK", ex.StackTrace, ConsoleColor.DarkRed)
        End If
    End Sub

    ''' <summary>
    ''' shortcut แทน Debug.WriteLine — เรียก D("ข้อความ") เลย
    ''' </summary>
    Public Sub D(msg As String)
        Write("", msg, _msgColor)
    End Sub

    ''' <summary>
    ''' shortcut แทน Debug.WriteLine พร้อม tag — เรียก D("tag", "ข้อความ")
    ''' </summary>
    Public Sub D(tag As String, msg As String)
        Write(tag, msg, _msgColor)
    End Sub

#End Region

#Region "Core Write"

    Private Sub Write(tag As String, msg As String, color As ConsoleColor)
        If Not _isStarted Then Return
        SyncLock _lock
            Try
                Dim timestamp As String = DateTime.Now.ToString("HH:mm:ss.fff")

                Console.ForegroundColor = ConsoleColor.DarkGray
                Console.Write(timestamp & " ")

                If Not String.IsNullOrEmpty(tag) Then
                    Console.ForegroundColor = _tagColor
                    Console.Write("[" & tag & "] ")
                End If

                Console.ForegroundColor = color
                Console.WriteLine(msg)
                Console.ForegroundColor = ConsoleColor.White
            Catch
            End Try
        End SyncLock
    End Sub

#End Region

End Module
