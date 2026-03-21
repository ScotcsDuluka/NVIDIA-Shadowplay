Imports Newtonsoft.Json.Linq
Imports System.IO

Module LangHelper
    Private lang As JObject

    Public Sub LoadLang(langFile As String)
        If File.Exists(langFile) Then
            Try
                Dim jsonText As String
                Using fs As New FileStream(langFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using sr As New StreamReader(fs)
                        jsonText = sr.ReadToEnd()
                    End Using
                End Using
                lang = JObject.Parse(jsonText)
            Catch ex As IOException
                Debug.WriteLine("Lang file locked: " & ex.Message)
            End Try
        Else
            lang = New JObject()
        End If
    End Sub

    Public Function GetText(key As String, ParamArray args() As String) As String
        If lang IsNot Nothing AndAlso lang(key) IsNot Nothing Then
            Dim text = lang(key).ToString()
            For i = 0 To args.Length - 1
                text = text.Replace("{{arg" & (i + 1) & "}}", args(i))
            Next
            Return text
        Else
            Return key
        End If
    End Function
End Module