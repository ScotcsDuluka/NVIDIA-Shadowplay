Imports Newtonsoft.Json.Linq
Imports System.IO

Module LangHelper
    Private lang As JObject

    Public Sub LoadLang(langFile As String)
        If File.Exists(langFile) Then
            Dim jsonText As String = File.ReadAllText(langFile)
            lang = JObject.Parse(jsonText)
        Else
            lang = New JObject()
        End If
    End Sub

    Public Function GetText(key As String, ParamArray args() As String) As String
        If lang IsNot Nothing AndAlso lang(key) IsNot Nothing Then
            Dim text = lang(key).ToString()
            ' แทนที่ arg1, arg2, arg3 ...
            For i = 0 To args.Length - 1
                text = text.Replace("{{arg" & (i + 1) & "}}", args(i))
            Next
            Return text
        Else
            Return key
        End If
    End Function
End Module