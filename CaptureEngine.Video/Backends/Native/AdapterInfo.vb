Option Strict On
Option Explicit On

Namespace CaptureEngine.Video.Backends.Native
    ''' <summary>
    ''' Describes the GPU adapter that a native backend is bound to.
    '''
    ''' This is used to verify V1 zero-copy compatibility:
    ''' capture backend and NVENC encoder MUST be on the same
    ''' physical GPU (same LUID).
    ''' </summary>
    Public NotInheritable Class AdapterInfo

        Private ReadOnly _index As Integer
        Private ReadOnly _description As String
        Private ReadOnly _vendorId As UInteger
        Private ReadOnly _deviceId As UInteger
        Private ReadOnly _luidLow As UInteger
        Private ReadOnly _luidHigh As Integer
        Private ReadOnly _dedicatedVideoMemoryBytes As ULong

        Public Sub New(
            index As Integer,
            description As String,
            vendorId As UInteger,
            deviceId As UInteger,
            luidLow As UInteger,
            luidHigh As Integer,
            dedicatedVideoMemoryBytes As ULong)

            _index = index
            _description = description
            _vendorId = vendorId
            _deviceId = deviceId
            _luidLow = luidLow
            _luidHigh = luidHigh
            _dedicatedVideoMemoryBytes = dedicatedVideoMemoryBytes
        End Sub

        Public ReadOnly Property Index As Integer
            Get
                Return _index
            End Get
        End Property

        Public ReadOnly Property Description As String
            Get
                Return _description
            End Get
        End Property

        Public ReadOnly Property VendorId As UInteger
            Get
                Return _vendorId
            End Get
        End Property

        Public ReadOnly Property DeviceId As UInteger
            Get
                Return _deviceId
            End Get
        End Property

        ''' <summary>
        ''' LUID packed as a single Int64 for easy comparison.
        ''' Two adapters with the same LUID are the same physical GPU.
        ''' </summary>
        Public ReadOnly Property Luid As Long
            Get
                Return CLng(_luidLow) Or (CLng(_luidHigh) << 32)
            End Get
        End Property

        Public ReadOnly Property LuidLow As UInteger
            Get
                Return _luidLow
            End Get
        End Property

        Public ReadOnly Property LuidHigh As Integer
            Get
                Return _luidHigh
            End Get
        End Property

        Public ReadOnly Property DedicatedVideoMemoryBytes As ULong
            Get
                Return _dedicatedVideoMemoryBytes
            End Get
        End Property

        Public ReadOnly Property IsNvidia As Boolean
            Get
                Return _vendorId = &H10DEUI
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{_index}] {_description} (Vendor=0x{_vendorId:X4}, Device=0x{_deviceId:X4}, LUID={Luid:X16})"
        End Function
    End Class
End Namespace
