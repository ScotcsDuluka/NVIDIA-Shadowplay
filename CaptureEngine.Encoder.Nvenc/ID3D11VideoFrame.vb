Option Strict On
Option Explicit On
Option Infer On

' ID3D11VideoFrame.vb
'
' Extension of CaptureEngine.Video.IVideoFrame that exposes the underlying
' D3D11 texture pointer for zero-copy GPU encoding.
'
' BACKGROUND:
'   The Foundation IVideoFrame contract (CaptureEngine.Video.IVideoFrame)
'   does NOT expose native resource handles — it only carries Origin,
'   PixelFormat, Dimensions, and Diagnostics. This is intentional:
'   the Foundation contract is backend-agnostic and supports CPU-memory
'   frames as well as GPU textures.
'
'   NvencEncoderBackend needs the D3D11 texture pointer to call
'   CopyResource (frame texture → encoder texture). Without this interface,
'   the encoder would need reflection or a TryCast hack — both rejected
'   per OWNER requirement (no reflection in production path).
'
' SOLUTION:
'   Define ID3D11VideoFrame in CaptureEngine.Encoder.Nvenc (NOT Foundation).
'   D3D11-producing backends (DdagrabBackend) emit frames that implement BOTH
'   CaptureEngine.Video.IVideoFrame AND ID3D11VideoFrame.
'
'   Encoder pipeline:
'     Dim d3d11Frame As ID3D11VideoFrame = DirectCast(frame, ID3D11VideoFrame)
'     Dim texPtr As IntPtr = d3d11Frame.NativeTexture
'     Dim frameTexture As ID3D11Texture2D = ID3D11Texture2D.FromPointer(texPtr)
'     deviceCtx.CopyResource(_encoderTexture, frameTexture)
'
'   This is a clean DirectCast (no reflection, no TryCast hack) and the
'   contract is owned by the encoder project (not Foundation).
'
' CONTRACT:
'   - Inherits CaptureEngine.Video.IVideoFrame (so Foundation contract is unchanged)
'   - Adds NativeTexture As IntPtr (the ID3D11Texture2D native pointer)
'   - Frame is still BORROWED — encoder MUST NOT dispose the texture
'     (the frame owner — typically DdagrabBackend — disposes it)
'
' HARD RULES COMPLIANCE:
'   ✅ Foundation contract unchanged (CaptureEngine.Video.IVideoFrame FROZEN)
'   ✅ No reflection in production path
'   ✅ No TryCast hack (DirectCast is type-safe at compile time)
'   ✅ Frame ownership = BORROW (encoder MUST NOT dispose texture)

Imports CaptureEngine.Video

Namespace CaptureEngine.Encoder.Nvenc

    ''' <summary>
    ''' Extension of IVideoFrame that exposes the D3D11 texture pointer.
    '''
    ''' D3D11-producing backends (DdagrabBackend) emit frames implementing
    ''' BOTH IVideoFrame AND ID3D11VideoFrame. The encoder casts to this
    ''' interface to access the native texture without reflection.
    '''
    ''' Ownership: the texture is BORROWED. The encoder MUST NOT dispose it.
    ''' The frame owner (DdagrabBackend) disposes the texture when the
    ''' frame is disposed.
    ''' </summary>
    Public Interface ID3D11VideoFrame
        Inherits IVideoFrame

        ''' <summary>
        ''' Native ID3D11Texture2D pointer. The encoder wraps this via
        ''' Vortice.Direct3D11.ID3D11Texture2D.FromPointer (does NOT take
        ''' ownership — the wrapper is GC-eligible, the texture lives as
        ''' long as the frame).
        '''
        ''' Returns IntPtr.Zero only if the frame has been disposed (defensive).
        ''' </summary>
        ReadOnly Property NativeTexture As IntPtr

    End Interface

End Namespace
