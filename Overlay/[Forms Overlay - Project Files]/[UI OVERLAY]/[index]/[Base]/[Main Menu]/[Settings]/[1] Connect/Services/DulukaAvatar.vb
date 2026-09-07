







Imports System.Diagnostics
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text.RegularExpressions

Friend Module DulukaAvatar

    
    
    Private Const MaxProfileImageBytes As Integer = 256 * 1024

    
    
    Private Const EncodedEdge As Integer = 256

    
    
    Private Const MaxSourceFileBytes As Integer = 8 * 1024 * 1024

    
    
    Private Const MaxSourceEdge As Integer = 4096

    Private ReadOnly DataUrlPattern As New Regex(
        "^data:image/(png|jpeg|webp);base64,([A-Za-z0-9+/]*={0,2})$",
        RegexOptions.Compiled)

    
    
    
    
    Public Function FromDataUrl(dataUrl As String) As Image
        If String.IsNullOrWhiteSpace(dataUrl) Then Return Nothing
        Try
            Dim m As Match = DataUrlPattern.Match(dataUrl)
            If Not m.Success Then Return Nothing
            Dim payload As String = m.Groups(2).Value
            If payload.Length = 0 OrElse payload.Length > (MaxProfileImageBytes \ 3 + 1) * 4 Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(payload)
            If bytes.Length = 0 OrElse bytes.Length > MaxProfileImageBytes Then Return Nothing
            
            
            
            Dim stream As New MemoryStream(bytes, writable:=False)
            Return Image.FromStream(stream)
        Catch ex As Exception
            
            Debug.WriteLine($"DulukaAvatar.FromDataUrl failed: {ex.GetType().Name}")
            Return Nothing
        End Try
    End Function

    
    
    
    
    Public Function EncodeFromFile(path As String, ByRef errorReason As String) As String
        errorReason = ""
        Try
            Dim info As New FileInfo(path)
            If Not info.Exists Then
                errorReason = "That file no longer exists."
                Return Nothing
            End If
            If info.Length > MaxSourceFileBytes Then
                errorReason = "That image is too large — pick one under 8 MB."
                Return Nothing
            End If

            Dim source As Image = Nothing
            Try
                source = Image.FromFile(path)
                If source.Width > MaxSourceEdge OrElse source.Height > MaxSourceEdge Then
                    errorReason = "That image is too big (over 4096 px on a side)."
                    Return Nothing
                End If

                Dim fit As Integer = EncodedEdge
                Dim scale As Double = Math.Min(fit / CDbl(source.Width), fit / CDbl(source.Height))
                If scale > 1.0 Then scale = 1.0 
                Dim w As Integer = Math.Max(1, CInt(Math.Round(source.Width * scale)))
                Dim h As Integer = Math.Max(1, CInt(Math.Round(source.Height * scale)))

                Using scaled As New Bitmap(w, h, PixelFormat.Format32bppArgb)
                    Using g As Graphics = Graphics.FromImage(scaled)
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic
                        g.SmoothingMode = SmoothingMode.HighQuality
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality
                        g.Clear(Color.Transparent)
                        g.DrawImage(source, New Rectangle(0, 0, w, h))
                    End Using

                    Using outStream As New MemoryStream()
                        scaled.Save(outStream, ImageFormat.Png)
                        Dim bytes As Byte() = outStream.ToArray()
                        
                        
                        
                        
                        If bytes.Length > MaxProfileImageBytes Then
                            errorReason = "That image encodes too large — pick a simpler one."
                            Return Nothing
                        End If
                        Return "data:image/png;base64," & Convert.ToBase64String(bytes)
                    End Using
                End Using
            Finally
                If source IsNot Nothing Then source.Dispose()
            End Try
        Catch ex As Exception
            
            Debug.WriteLine($"DulukaAvatar.EncodeFromFile failed: {ex.GetType().Name}")
            errorReason = "That file could not be read as an image."
            Return Nothing
        End Try
    End Function

    
    
    
    Public Sub SetPreview(box As PictureBox, dataUrl As String, letterFallback As Label)
        Dim img As Image = FromDataUrl(dataUrl)
        Dim old As Image = box.Image
        box.Image = img
        If img IsNot Nothing Then
            box.Visible = True
            If letterFallback IsNot Nothing Then letterFallback.Visible = False
        Else
            box.Visible = False
            If letterFallback IsNot Nothing Then letterFallback.Visible = True
        End If
        If old IsNot Nothing Then old.Dispose()
    End Sub

End Module
