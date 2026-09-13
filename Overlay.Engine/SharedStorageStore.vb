' SharedStorageStore.vb — backing store for the cefQuery
' QUERY_READ/WRITE_SHARED_STORAGE pair (the page's host-persistent KV,
' e.g. "ModsEnableStatus"). GFE keeps this in the host; we keep a JSON
' file under the product Data\ folder. Never throws on corrupt content —
' a broken store reads as empty, the next write rebuilds it.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json

Public Class SharedStorageStore

    Private ReadOnly _path As String
    Private ReadOnly _lock As New Object()

    Public Sub New(Optional storagePath As String = Nothing)
        If String.IsNullOrEmpty(storagePath) Then
            Dim dir As String = AppLayout.P("Data", "osc-shared-storage.json")
            If Not IO.Directory.Exists(IO.Path.GetDirectoryName(dir)) Then
                Try : IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(dir)) : Catch : End Try
            End If
            _path = dir
        Else
            _path = storagePath
        End If
    End Sub

    Public Function Read(key As String) As String
        If String.IsNullOrEmpty(key) Then Return ""
        SyncLock _lock
            Dim dict As Dictionary(Of String, String) = Load()
            Dim value As String = Nothing
            If dict.TryGetValue(key, value) Then Return value
            Return ""
        End SyncLock
    End Function

    Public Sub Write(key As String, value As String)
        If String.IsNullOrEmpty(key) Then Return
        SyncLock _lock
            Dim dict As Dictionary(Of String, String) = Load()
            If value Is Nothing Then
                dict.Remove(key)
            Else
                dict(key) = value
            End If
            Save(dict)
        End SyncLock
    End Sub

    Private Function Load() As Dictionary(Of String, String)
        Dim dict As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Try
            If Not File.Exists(_path) Then Return dict
            Dim text As String = File.ReadAllText(_path)
            If String.IsNullOrWhiteSpace(text) Then Return dict
            Dim el As JsonElement = JsonDocument.Parse(text).RootElement
            If el.ValueKind = JsonValueKind.Object Then
                For Each prop As JsonProperty In el.EnumerateObject()
                    If prop.Value.ValueKind = JsonValueKind.String Then
                        dict(prop.Name) = prop.Value.GetString()
                    End If
                Next
            End If
        Catch
            ' corrupt store reads as empty
        End Try
        Return dict
    End Function

    Private Sub Save(dict As Dictionary(Of String, String))
        Try
            Dim options As New JsonSerializerOptions With {.WriteIndented = True}
            Dim tmp As String = _path & "." & Process.GetCurrentProcess().Id.ToString() & ".tmp"
            File.WriteAllText(tmp, JsonSerializer.Serialize(dict, options))
            If File.Exists(_path) Then
                File.Copy(_path, _path & ".bak", True)
            End If
            File.Move(tmp, _path, True)
        Catch
            ' storage failures must never take the overlay down
        End Try
    End Sub

End Class
