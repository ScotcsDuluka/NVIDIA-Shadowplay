Imports Newtonsoft.Json.Linq

Public Module ObsEventMap

    Public Const OBS_STATE_STARTING As String = "OBS_WEBSOCKET_OUTPUT_STARTING"
    Public Const OBS_STATE_STARTED  As String = "OBS_WEBSOCKET_OUTPUT_STARTED"
    Public Const OBS_STATE_STOPPING As String = "OBS_WEBSOCKET_OUTPUT_STOPPING"
    Public Const OBS_STATE_STOPPED  As String = "OBS_WEBSOCKET_OUTPUT_STOPPED"

    Public Class MappedNotification
        Public Property Key As String
        Public Property Ico As String = ""
        Public Property Png As Boolean = False
        Public Property Color As Color = Color.White
        Public Property Args As String() = {}
    End Class

    Private ReadOnly greenColor As Color = ColorTranslator.FromHtml("#76B900")

    Public Function TryMap(eventType As String, eventData As JObject) As MappedNotification
        If eventData Is Nothing Then Return Nothing

        Select Case eventType
            Case "RecordStateChanged"
                Return MapRecordState(eventData)

            Case "ReplayBufferStateChanged"
                Return MapReplayState(eventData)

            Case "ScreenshotSaved"
                Return New MappedNotification With {
                    .Key = "l10n.notificationScreenshotSavedToGallery",
                    .Ico = "",
                    .Color = greenColor
                }

            Case Else
                Return Nothing
        End Select
    End Function

    Private Function MapRecordState(d As JObject) As MappedNotification
        Dim outputStateTok As JToken = d("outputState")
        If outputStateTok Is Nothing Then Return Nothing
        Dim outputState As String = outputStateTok.Value(Of String)()
        If outputState Is Nothing Then Return Nothing

        Select Case outputState
            Case OBS_STATE_STARTED
                Return New MappedNotification With {
                    .Key = "l10n.recording_started",
                    .Ico = "",
                    .Color = greenColor
                }
            Case OBS_STATE_STOPPED
                Return New MappedNotification With {
                    .Key = "l10n.recording_saved",
                    .Ico = ""
                }
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function MapReplayState(d As JObject) As MappedNotification
        Dim outputStateTok As JToken = d("outputState")
        If outputStateTok Is Nothing Then Return Nothing
        Dim outputState As String = outputStateTok.Value(Of String)()
        If outputState Is Nothing Then Return Nothing

        Select Case outputState
            Case OBS_STATE_STARTED
                Return New MappedNotification With {
                    .Key = "l10n.instant_replay_on",
                    .Ico = "",
                    .Color = greenColor
                }
            Case OBS_STATE_STOPPED
                Return New MappedNotification With {
                    .Key = "l10n.instant_replay_off",
                    .Ico = ""
                }
            Case Else
                Return Nothing
        End Select
    End Function

End Module
