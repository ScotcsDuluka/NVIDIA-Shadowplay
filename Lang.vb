Imports Newtonsoft.Json

Public Class Lang
    Dim strings As New Dictionary(Of String, String)

    ' ฟังก์ชันโหลดข้อความจากไฟล์ JSON
    Sub LoadStrings(lang As String)
        Dim filePath As String = System.IO.Path.Combine(Application.StartupPath, "osc", $"{lang}.json")
        If System.IO.File.Exists(filePath) Then
            Dim jsonData As String = System.IO.File.ReadAllText(filePath)
            strings = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonData)
        Else
            ' ใช้ภาษาอังกฤษเป็นค่าเริ่มต้น
            Dim defaultLang As String = "en-US"
            filePath = System.IO.Path.Combine(Application.StartupPath, "osc", $"{defaultLang}.json")
            If System.IO.File.Exists(filePath) Then
                Dim jsonData As String = System.IO.File.ReadAllText(filePath)
                strings = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(jsonData)
            End If
        End If
    End Sub

    ' ฟังก์ชันสำหรับดึงข้อความตามคีย์
    Public Function GetString(key As String) As String
        If strings.ContainsKey(key) Then
            Return strings(key)
        Else
            Return key ' คืนค่าคีย์ถ้าไม่พบ
        End If
    End Function
End Class
