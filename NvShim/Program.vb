Imports System
Imports System.Diagnostics
Imports System.IO

Friend Module Program

    Sub Main(args As String())
        Dim baseDir As String = AppContext.BaseDirectory
        ShimLog.Init(Path.Combine(baseDir, "Logs", "NvShim.log"))

        Dim role As String = ""
        Dim extraArgs As Integer = 0
        Dim i As Integer = 0
        While i < args.Length
            If String.Equals(args(i), "--help", StringComparison.OrdinalIgnoreCase) OrElse args(i) = "-?" Then
                PrintUsage()
                Environment.ExitCode = 0
                Return
            ElseIf args(i).StartsWith("--role=", StringComparison.OrdinalIgnoreCase) Then
                role = args(i).Substring(7).Trim()
                extraArgs += Math.Max(0, args.Length - i - 1)
                Exit While
            ElseIf String.Equals(args(i), "--role", StringComparison.OrdinalIgnoreCase) Then
                If i + 1 < args.Length Then
                    role = args(i + 1).Trim()
                    extraArgs += Math.Max(0, args.Length - i - 2)
                End If
                Exit While
            ElseIf role = "" AndAlso Not args(i).StartsWith("-", StringComparison.Ordinal) Then
                role = args(i).Trim()
                extraArgs += Math.Max(0, args.Length - i - 1)
                Exit While
            Else
                extraArgs += 1
            End If
            i += 1
        End While

        If role = "" Then
            PrintUsage()
            Environment.ExitCode = 0
            Return
        End If

        Dim config As ShimConfig = ShimConfig.Load(Path.Combine(baseDir, "Config", "NvShim.json"), baseDir)
        Dim spec As ShimRole = config.FindRole(role)
        If spec Is Nothing Then
            Console.Error.WriteLine("Unknown role: " & role)
            Console.Error.WriteLine("Known roles: " & String.Join(", ", config.RoleNames()))
            Environment.ExitCode = 2
            Return
        End If

        If extraArgs > 0 Then ShimLog.Log("warning: extra arguments ignored: " & extraArgs.ToString())

        If Not File.Exists(spec.ExePath) Then
            ShimLog.Log("role missing: " & role & " exe=" & spec.ExePath)
            Console.Error.WriteLine("Role executable not found: " & spec.ExePath)
            Environment.ExitCode = 3
            Return
        End If

        ShimLog.Log("spawn role=" & role & " exe=" & spec.ExePath)
        Try
            Dim startInfo As New ProcessStartInfo With {
                .FileName = spec.ExePath,
                .Arguments = spec.Args,
                .WorkingDirectory = Path.GetDirectoryName(spec.ExePath),
                .UseShellExecute = False
            }
            Process.Start(startInfo)
            Environment.ExitCode = 0
        Catch ex As Exception
            ShimLog.Log("spawn failed: " & ex.Message)
            Console.Error.WriteLine("Failed to start role '" & role & "': " & ex.Message)
            Environment.ExitCode = 3
        End Try
    End Sub

    Private Sub PrintUsage()
        Console.WriteLine("NVIDIA Share role dispatcher")
        Console.WriteLine("Usage: NVIDIA Share.exe --role=<name>")
        Console.WriteLine("       NVIDIA Share.exe --role <name>")
        Console.WriteLine("       NVIDIA Share.exe <name>")
        Console.WriteLine("Roles: coordinator, winform, webview, hook")
        Console.WriteLine("Example: NVIDIA Share.exe --role=coordinator")
    End Sub

End Module
