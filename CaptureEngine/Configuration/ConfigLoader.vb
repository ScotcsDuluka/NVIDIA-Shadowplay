Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports CaptureEngine.Configuration.Schema

Namespace CaptureEngine.Configuration
    ''' <summary>
    ''' Loads and saves EngineConfigV2 to/from a single JSON file
    ''' (default name: engine-config.v2.json).
    '''
    ''' JSON serialization options:
    '''   - PropertyNameCaseInsensitive = True (tolerant of camelCase/PascalCase)
    '''   - AllowTrailingCommas = True
    '''   - ReadCommentHandling = Skip (tolerant of // comments in config)
    '''   - DefaultIgnoreCondition = WhenWritingNull (no null fields in output)
    '''
    ''' On load:
    '''   - File missing → returns Nothing (caller decides whether to migrate from V1)
    '''   - File present but malformed → throws JsonException
    '''   - File present and valid → returns EngineConfigV2 instance
    ''' </summary>
    Public NotInheritable Class ConfigLoader
        Private Sub New()
            ' Static helper class — no instances.
        End Sub

        Public Const DefaultFileName As String = "engine-config.v2.json"

        Private Shared ReadOnly _jsonOpts As New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True,
            .AllowTrailingCommas = True,
            .ReadCommentHandling = JsonCommentHandling.Skip,
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            .WriteIndented = True
        }

        ''' <summary>Load V2 config from a specific file path. Returns Nothing if file does not exist.</summary>
        Public Shared Function Load(filePath As String) As EngineConfigV2
            If String.IsNullOrEmpty(filePath) Then Return Nothing
            If Not File.Exists(filePath) Then Return Nothing

            Dim json As String = File.ReadAllText(filePath)
            If String.IsNullOrWhiteSpace(json) Then Return Nothing

            Dim cfg As EngineConfigV2 = JsonSerializer.Deserialize(Of EngineConfigV2)(json, _jsonOpts)
            If cfg Is Nothing Then Return Nothing

            ' Always normalize Version field after load (defensive against manual edits).
            If cfg.Version <> EngineConfigV2.SchemaVersion Then
                cfg.Version = EngineConfigV2.SchemaVersion
            End If

            Return cfg
        End Function

        ''' <summary>Save V2 config to a specific file path.</summary>
        Public Shared Sub Save(cfg As EngineConfigV2, filePath As String)
            If cfg Is Nothing Then Throw New ArgumentNullException(NameOf(cfg))
            If String.IsNullOrEmpty(filePath) Then Throw New ArgumentException("filePath is empty.", NameOf(filePath))

            cfg.GeneratedAt = DateTime.UtcNow
            cfg.Version = EngineConfigV2.SchemaVersion

            Dim dir As String = Path.GetDirectoryName(filePath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            Dim json As String = JsonSerializer.Serialize(cfg, _jsonOpts)
            File.WriteAllText(filePath, json)
        End Sub

        ''' <summary>Load V2 config from the default location next to the Engine binary.</summary>
        Public Shared Function LoadDefault(appBaseDir As String) As EngineConfigV2
            If String.IsNullOrEmpty(appBaseDir) Then
                appBaseDir = AppDomain.CurrentDomain.BaseDirectory
            End If
            Dim path As String = Path.Combine(appBaseDir, DefaultFileName)
            Return Load(path)
        End Function

        ''' <summary>Save V2 config to the default location next to the Engine binary.</summary>
        Public Shared Sub SaveDefault(cfg As EngineConfigV2, appBaseDir As String)
            If String.IsNullOrEmpty(appBaseDir) Then
                appBaseDir = AppDomain.CurrentDomain.BaseDirectory
            End If
            Dim path As String = Path.Combine(appBaseDir, DefaultFileName)
            Save(cfg, path)
        End Sub

        ''' <summary>
        ''' Serialize V2 config to a JSON string. Useful for snapshot tests.
        ''' </summary>
        Public Shared Function SerializeToJson(cfg As EngineConfigV2) As String
            If cfg Is Nothing Then Throw New ArgumentNullException(NameOf(cfg))
            Return JsonSerializer.Serialize(cfg, _jsonOpts)
        End Function

        ''' <summary>
        ''' Deserialize V2 config from a JSON string. Useful for snapshot tests.
        ''' </summary>
        Public Shared Function DeserializeFromJson(json As String) As EngineConfigV2
            If String.IsNullOrEmpty(json) Then Throw New ArgumentException("json is empty.", NameOf(json))
            Return JsonSerializer.Deserialize(Of EngineConfigV2)(json, _jsonOpts)
        End Function
    End Class
End Namespace
