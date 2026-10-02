Imports Newtonsoft.Json.Linq
Imports System.IO

Module LangHelper
    Private lang As JObject

    Public Sub LoadLang(langFile As String)
        If Not File.Exists(langFile) Then
            lang = New JObject()
            Return
        End If

        Try
            Dim jsonText As String
            Using fs As New FileStream(langFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(fs)
                    jsonText = sr.ReadToEnd()
                End Using
            End Using

            lang = JObject.Parse(jsonText)
        Catch ex As Exception
            Debug.WriteLine("LoadLang Error: " & ex.Message)
            lang = New JObject()
        End Try
    End Sub

    Public Function GetText(key As String, ParamArray args() As String) As String
        If lang Is Nothing OrElse lang(key) Is Nothing Then
            Return key
        End If

        Return FormatArgs(lang(key).ToString(), args)
    End Function

    Private Function FormatArgs(template As String, args() As String) As String
        Dim text As String = template
        For i = 0 To args.Length - 1
            text = text.Replace("{{arg" & (i + 1) & "}}", args(i))
        Next
        Return text
    End Function
End Module