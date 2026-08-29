' AppConfigShared.vb — SHARED config.json ACCESS FOR NON-OVERLAY PROCESSES
'
' config.json (Config\config.json) is THE single user-facing settings file.
' The full typed model lives in the Overlay project (AppSettings), but three
' other processes need to touch individual keys without owning the schema:
'
'   NVIDIA Experience (Launcher)  — writes Overlay.UseOverlayEnabled (toggle)
'   NVIDIA API (hub)              — reads  Overlay.UseOverlayEnabled every
'                                   second to start/keep-alive or kill the
'                                   overlay stack (Notifier/ShadowPlay/Capture)
'   NVIDIA Notifier               — reads  UI.Language for localized toasts
'
' This module gives them a tiny, dependency-free read/patch API. It NEVER
' rewrites the file from a cached model: every write is a parse of the
' CURRENT file content, a one-key mutation, and an immediate write-back, so
' sections owned by other processes survive untouched.
'
'   ReadBool / ReadString  — missing file / corrupt JSON / wrong type →
'                            the caller's fallback. Never creates the file.
'   WriteBool              — creates the file (and section) on demand via
'                            AppLayout.EnsureParentDir; never throws.
'
' This file is deliberately dependency-free (besides AppLayout) and
' Option Strict On-clean so it compiles identically inside every app
' project — same contract as AppLayout.vb.

Imports System
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes

Public Module AppConfigShared

    Private _configPath As String = Nothing

    ''' <summary>Path to the unified config file (Config\config.json).</summary>
    Public Function ConfigPath() As String
        If _configPath Is Nothing Then
            _configPath = AppLayout.P("Config", "config.json")
        End If
        Return _configPath
    End Function

    ''' <summary>
    ''' Reads a boolean key from [section]. Falls back when the file, the
    ''' section, or the key is missing — or when the value is not a boolean.
    ''' </summary>
    Public Function ReadBool(sectionName As String, keyName As String, fallback As Boolean) As Boolean
        Try
            If Not File.Exists(ConfigPath()) Then Return fallback

            Dim sectionObj As JsonObject = FindSection(sectionName)
            If sectionObj Is Nothing Then Return fallback

            Dim valueNode As JsonNode = FindMember(sectionObj, keyName)
            If valueNode Is Nothing Then Return fallback

            Dim value As JsonValue = TryCast(valueNode, JsonValue)
            Dim result As Boolean = fallback
            If value IsNot Nothing AndAlso value.TryGetValue(Of Boolean)(result) Then
                Return result
            End If
            Return fallback
        Catch
            Return fallback
        End Try
    End Function

    ''' <summary>
    ''' Reads a string key from [section]. Falls back when the file, the
    ''' section, or the key is missing — or when the value is not a string.
    ''' </summary>
    Public Function ReadString(sectionName As String, keyName As String, fallback As String) As String
        Try
            If Not File.Exists(ConfigPath()) Then Return fallback

            Dim sectionObj As JsonObject = FindSection(sectionName)
            If sectionObj Is Nothing Then Return fallback

            Dim valueNode As JsonNode = FindMember(sectionObj, keyName)
            If valueNode Is Nothing Then Return fallback

            Dim value As JsonValue = TryCast(valueNode, JsonValue)
            Dim result As String = fallback
            If value IsNot Nothing AndAlso value.TryGetValue(Of String)(result) Then
                Return result
            End If
            Return fallback
        Catch
            Return fallback
        End Try
    End Function

    ''' <summary>
    ''' Writes a boolean key into [section] using read-modify-write on the
    ''' CURRENT file content, so keys owned by other processes are preserved.
    ''' Creates the file and section on demand. Never throws.
    ''' </summary>
    Public Sub WriteBool(sectionName As String, keyName As String, value As Boolean)
        Try
            Dim configPath As String = ConfigPath()
            AppLayout.EnsureParentDir(configPath)

            Dim rootObj As JsonObject = Nothing
            If File.Exists(configPath) Then
                Dim jsonText As String = File.ReadAllText(configPath)
                If Not String.IsNullOrWhiteSpace(jsonText) Then
                    rootObj = TryCast(JsonNode.Parse(jsonText), JsonObject)
                End If
            End If
            If rootObj Is Nothing Then rootObj = New JsonObject()

            Dim sectionObj As JsonObject = TryCast(FindMember(rootObj, sectionName), JsonObject)
            If sectionObj Is Nothing Then
                sectionObj = New JsonObject()
                rootObj(sectionName) = sectionObj
            End If

            sectionObj(keyName) = value

            Dim options As New JsonSerializerOptions With {.WriteIndented = True}
            File.WriteAllText(configPath, rootObj.ToJsonString(options))
        Catch
            ' config.json being locked/corrupt must never take the caller down
            ' (the Launcher tick and the API hub call this on a timer).
        End Try
    End Sub

    ''' <summary>Finds [section] at the root, tolerating casing differences.</summary>
    Private Function FindSection(rootObj As JsonObject, sectionName As String) As JsonObject
        Return TryCast(FindMember(rootObj, sectionName), JsonObject)
    End Function

    ''' <summary>Case-insensitive member lookup on a JSON object
    ''' (System.Text.Json node indexing is case-sensitive; the Overlay's
    ''' typed model deserializes case-insensitively, so hand-edited or
    ''' re-cased files must still resolve).</summary>
    Private Function FindMember(obj As JsonObject, memberName As String) As JsonNode
        For Each kvp As KeyValuePair(Of String, JsonNode) In obj
            If String.Equals(kvp.Key, memberName, StringComparison.OrdinalIgnoreCase) Then
                Return kvp.Value
            End If
        Next
        Return Nothing
    End Function

End Module
