' AppLayout.vb — ROOT-FIXED LAYOUT SUPPORT (custom app tree, 2026-08-28)
'
' The deployed product tree is:
'
'   NVIDIA ShadowPlay\
'     NVIDIA Experience.*            (root app, exe+dll adjacent)
'     Application\*.exe              (thin native hosts for the 3 services)
'     Services\*.dll + deps/runtimeconfig   (the 3 services' managed apps)
'     Overlay\NVIDIA ShadowPlay.*    (overlay app)
'     Engine\   CaptureEngine.*.dll  (engine libraries)
'     Core\     System.*/WinRT/SharpGen runtime libs
'     Audio\    NAudio.*
'     Graphics\ Vortice.*
'     Libraries\ Newtonsoft.Json
'     FFmpeg\ Config\ Logs\ Data\ Languages\ Resources\ Redist\ Flags\ Runtimes\
'
' This module (one copy LINKED into every app project) provides:
'
'   1. Dir  — the layout ROOT. Derived from the EXE location so it works
'      no matter which process asks: Application\* and Overlay\* walk one
'      level up; anything else (root app, dev bin\) is its own root.
'      Override for exotic installs: env NVIDIA_SHADOWPLAY_APP_ROOT.
'
'   2. Assembly resolution — the native host starts ..\Services\<app>.dll
'      fine (hostfxr combines host dir + embedded relative path), but the
'      default context only probes the app dir for dependency assemblies.
'      Shared-family folders (Engine/Core/Audio/Graphics/Libraries) are
'      found by the Resolving handler below. Zero config, no probing
'      XML: the folder map IS the layout.
'
'   3. Initialize() — call once at app startup (MyApplication.Startup):
'      installs the resolver and points the process CWD at the root so
'      any relative-path leftover code keeps working in dev layouts.
'
' Dev mode: running from a normal bin\ folder, none of the family
' folders exist, the resolver finds nothing (harmless — deps are local
' there) and Dir == exe dir == today's behaviour. This file is
' deliberately dependency-free and Option Strict On-clean so it compiles
' identically inside all five app projects.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.Loader

Public Module AppLayout

    Private _initialized As Boolean = False
    Private _dir As String = Nothing

    ''' <summary>Raw directory of the running executable (the apphost).</summary>
    Public ReadOnly Property ExeDir As String
        Get
            Try
                Dim exe As String = Process.GetCurrentProcess().MainModule.FileName
                If Not String.IsNullOrEmpty(exe) Then
                    Return Path.GetDirectoryName(Path.GetFullPath(exe))
                End If
            Catch
            End Try
            ' Fallback: the managed app dll's directory (split apps: Services\).
            Return AppContext.BaseDirectory
        End Get
    End Property

    ''' <summary>The layout ROOT (see header). Computed once per process.</summary>
    Public ReadOnly Property Dir As String
        Get
            If _dir Is Nothing Then
                Dim rootEnv As String = Environment.GetEnvironmentVariable("NVIDIA_SHADOWPLAY_APP_ROOT")
                If Not String.IsNullOrEmpty(rootEnv) AndAlso Directory.Exists(rootEnv) Then
                    _dir = Path.GetFullPath(rootEnv)
                Else
                    ' NOTE: VB is case-insensitive — a local named `exeDir`
                    ' would SHADOW the ExeDir property here (BC42104), so
                    ' this local deliberately carries a distinct name.
                    Dim exeFolder As String = ExeDir
                    Dim leaf As String = New DirectoryInfo(exeFolder).Name
                    ' Application\ (split hosts) and Overlay\ (overlay app) live
                    ' one level under the product root.
                    If String.Equals(leaf, "Application", StringComparison.OrdinalIgnoreCase) OrElse
                       String.Equals(leaf, "Overlay", StringComparison.OrdinalIgnoreCase) Then
                        _dir = Path.GetFullPath(Path.Combine(exeFolder, ".."))
                    Else
                        _dir = exeFolder
                    End If
                End If
            End If
            Return _dir
        End Get
    End Property

    ''' <summary>Path under the layout root: AppLayout.P("Config", "engine.json").</summary>
    Public Function P(ParamArray parts As String()) As String
        Dim acc As String = Dir
        Dim i As Integer
        For i = 0 To parts.Length - 1
            acc = Path.Combine(acc, parts(i))
        Next i
        Return acc
    End Function

    ''' <summary>
    ''' Full path of a companion app executable. Deployed tree: Application\&lt;name&gt;.
    ''' Dev bin\: all app exes build FLAT into one output folder (no Application\
    ''' subdir), so fall back to the layout root when the staged location does
    ''' not exist. Callers keep their own final File.Exists guards — this only
    ''' picks WHERE to look first.
    ''' </summary>
    Public Function ExePath(appExeName As String) As String
        Dim staged As String = P("Application", appExeName)
        If File.Exists(staged) Then Return staged
        Return P(appExeName)
    End Function

    ''' <summary>Call once at startup: CWD = root + install the assembly
    ''' resolver. Idempotent and safe to call again.</summary>
    Public Sub Initialize()
        If _initialized Then Return
        _initialized = True
        Try
            Environment.CurrentDirectory = Dir
        Catch
        End Try
        AddHandler AssemblyLoadContext.Default.Resolving, AddressOf OnDefaultResolving
    End Sub

    ''' <summary>Family folders probed for dependency assemblies, in order.
    ''' The last entry keeps DEV runs (plain bin\) working unchanged.</summary>
    Private Function ProbeFolders() As List(Of String)
        Dim folders As New List(Of String)(8)
        folders.Add(P("Engine"))
        folders.Add(P("Core"))
        folders.Add(P("Audio"))
        folders.Add(P("Graphics"))
        folders.Add(P("Libraries"))
        folders.Add(P("Runtimes", "win", "lib", "net10.0"))
        folders.Add(ExeDir)
        Return folders
    End Function

    Private Function OnDefaultResolving(context As AssemblyLoadContext,
                                        assemblyName As System.Reflection.AssemblyName) As System.Reflection.Assembly
        If assemblyName Is Nothing Then Return Nothing
        Dim simple As String = assemblyName.Name
        If String.IsNullOrEmpty(simple) Then Return Nothing
        ' Satellite resources are not file-probed here.
        If simple.EndsWith(".resources", StringComparison.OrdinalIgnoreCase) Then Return Nothing
        Dim candidate As String
        For Each folder In ProbeFolders()
            candidate = Path.Combine(folder, simple & ".dll")
            If File.Exists(candidate) Then
                Try
                    Return context.LoadFromAssemblyPath(candidate)
                Catch
                    ' Wrong-architecture or corrupt file: keep probing.
                End Try
            End If
            candidate = Path.Combine(folder, simple & ".exe")
            If File.Exists(candidate) Then
                Try
                    Return context.LoadFromAssemblyPath(candidate)
                Catch
                End Try
            End If
        Next folder
        Return Nothing
    End Function

End Module
