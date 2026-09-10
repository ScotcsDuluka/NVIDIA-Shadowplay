Option Strict On
Option Explicit On
Option Infer On

' StateMachineTests.vb — transition truth + guard rules (design doc §3.5).
'
' Owner-mandated guards proven here (pure, no threads):
'   double start / double stop / dispose-then-everything / seek-after-dispose
'   / render-after-dispose is enforced at session level; the static truth
'   table these tests pin is what the session implements.

Imports System
Imports Gallery.Video

Namespace Gallery.Video.Tests

    Friend NotInheritable Class StateMachineTests

        Public Shared Sub RunAll(runner As Action(Of String, Action))
            runner("SM: CanPlay truth table (Paused/Stopped yes; Opening/Seeking/Disposed no)",
                   AddressOf Test_CanPlay)
            runner("SM: CanSeek truth table (Playing/Paused only — seek-after-dispose rejected)",
                   AddressOf Test_CanSeek)
            runner("SM: CanStop idempotence (everything except Disposed)",
                   AddressOf Test_CanStop)
            runner("SM: terminal states (Stopped/Faulted/Disposed)",
                   AddressOf Test_Terminal)
        End Sub

        Private Shared Sub Test_CanPlay()
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanPlay(PlaybackState.Created), "Created")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanPlay(PlaybackState.Opening), "Opening")
            TestRunner.AssertEqual(True, PlaybackStateMachine.CanPlay(PlaybackState.Playing), "Playing")
            TestRunner.AssertEqual(True, PlaybackStateMachine.CanPlay(PlaybackState.Paused), "Paused")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanPlay(PlaybackState.Seeking), "Seeking")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanPlay(PlaybackState.Stopping), "Stopping")
            TestRunner.AssertEqual(True, PlaybackStateMachine.CanPlay(PlaybackState.Stopped), "Stopped")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanPlay(PlaybackState.Faulted), "Faulted")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanPlay(PlaybackState.Disposed), "Disposed")
        End Sub

        Private Shared Sub Test_CanSeek()
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanSeek(PlaybackState.Created), "Created")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanSeek(PlaybackState.Opening), "Opening")
            TestRunner.AssertEqual(True, PlaybackStateMachine.CanSeek(PlaybackState.Playing), "Playing")
            TestRunner.AssertEqual(True, PlaybackStateMachine.CanSeek(PlaybackState.Paused), "Paused")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanSeek(PlaybackState.Seeking), "Seeking")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanSeek(PlaybackState.Stopped), "Stopped")
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanSeek(PlaybackState.Faulted), "Faulted")
            ' Owner case: seek after dispose must be REJECTED, never crash.
            TestRunner.AssertEqual(False, PlaybackStateMachine.CanSeek(PlaybackState.Disposed), "Disposed")
        End Sub

        Private Shared Sub Test_CanStop()
            For Each sObj As Object In [Enum].GetValues(GetType(PlaybackState))
                Dim s = CType(sObj, PlaybackState)
                Dim expected = (s <> PlaybackState.Disposed)
                TestRunner.AssertEqual(expected, PlaybackStateMachine.CanStop(s), s.ToString())
            Next
        End Sub

        Private Shared Sub Test_Terminal()
            TestRunner.Assert(PlaybackStateMachine.IsTerminal(PlaybackState.Stopped), "Stopped")
            TestRunner.Assert(PlaybackStateMachine.IsTerminal(PlaybackState.Faulted), "Faulted")
            TestRunner.Assert(PlaybackStateMachine.IsTerminal(PlaybackState.Disposed), "Disposed")
            TestRunner.Assert(Not PlaybackStateMachine.IsTerminal(PlaybackState.Playing), "Playing not terminal")
            TestRunner.Assert(Not PlaybackStateMachine.IsTerminal(PlaybackState.Opening), "Opening not terminal")
        End Sub

    End Class

End Namespace
