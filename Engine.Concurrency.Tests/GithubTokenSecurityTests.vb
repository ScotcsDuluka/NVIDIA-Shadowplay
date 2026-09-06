Option Strict On
Option Explicit On
Option Infer On

' GithubTokenSecurityTests.vb — F-S2: truthful at-rest contract for the
' GitHub OAuth token (S2-class: config.json is synced/backed-up freely, so
' the token must NEVER rest in plaintext on disk).
'
' Verified production chain (all REAL code, no fakes):
'   Connect.vb OAuth code → GetAccessToken → GetUser(token)
'     → SaveGitHubUser(username, avatarUrl, token)
'     → GitHubToken setter → EncryptToken → DPAPI ProtectedData(CurrentUser)
'     → GitHubTokenEncrypted (the ONLY persisted form) → Save()
'
' Consumers that JUSTIFY persistence (why no-persist was rejected):
'   Main Menu.vb:432 → LoadGitHubUser() re-fetches the profile from
'   api.github.com/user with the persisted token at startup — no-persist
'   would force a re-login on every launch. Engine-side OverlayConfig reads
'   GitHubTokenEncrypted as an opaque mirror (never decrypts).
'
' Tests (regression proof against the REAL Overlay AppSettings, linked):
'   GHS-1  SaveGitHubUser → config.json contains NO plaintext token, only a
'          non-empty GitHubTokenEncrypted blob; in-memory decrypt round-trips.
'   GHS-2  a LEGACY config.json with a plain-text "GitHubToken" (pre-DPAPI
'          world) is migrated on Load: token stays usable AND the plaintext
'          field is wiped from disk by the migration save.
'   GHS-3  Load round-trip after save (simulated restart) — token decrypts
'          to the original value.

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading

Namespace Engine.Concurrency.Tests

    Friend Module GithubTokenSecurityTests

        Private Const SecretToken As String = "gho_F_S2SECRET1234567890abcdef"
        Private Const LegacyToken As String = "gho_F_S2LEGACY1234567890abcdef"

        Friend Sub RunAll()
            Console.WriteLine("── F-S2: GitHub token at-rest security (real AppSettings + real DPAPI) ──")
            TestRunner.RunTest("GHS-1: SaveGitHubUser → config.json has NO plaintext token, only DPAPI blob",
                               AddressOf Test_SaveNoPlaintext)
            TestRunner.RunTest("GHS-2: legacy plaintext GitHubToken in config.json → migrated + wiped from disk",
                               AddressOf Test_LegacyMigrationScrubs)
            TestRunner.RunTest("GHS-3: persistence round-trip — token decrypts after simulated restart",
                               AddressOf Test_RoundTripAfterRestart)
        End Sub

        ' ───────────────────────────────────────────────────────────────

        ''' <summary>AppSettings.ConfigPath = AppLayout.P("Config", "config.json")
        ''' — resolved through the test process's AppLayout copy (test bin dir).
        ''' The suite owns this file; other Concurrency tests never touch it.</summary>
        Private Function ConfigFilePath() As String
            Return AppLayout.P("Config", "config.json")
        End Function

        Private Sub WriteRawConfig(content As String)
            Dim path As String = ConfigFilePath()
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(path))
            IO.File.WriteAllText(path, content)
        End Sub

        Private Sub CleanConfig()
            Try
                Dim path As String = ConfigFilePath()
                If IO.File.Exists(path) Then IO.File.Delete(path)
                Dim bak As String = path & ".bak"
                If IO.File.Exists(bak) Then IO.File.Delete(bak)
            Catch
            End Try
            ClearHelperEnv()
        End Sub

        Private Sub SetEnv(name As String, value As String)
            Environment.SetEnvironmentVariable(name, value)
        End Sub

        Private Sub ClearHelperEnv()
            SetEnv("LMHLP_SRC", Nothing)
            SetEnv("LMHLP_COPYTO", Nothing)
            SetEnv("LMHLP_SLEEP", Nothing)
            SetEnv("LMHLP_EXIT", Nothing)
            SetEnv("LMHLP_MUX_EXIT", Nothing)
            SetEnv("LMHLP_MUX_SRC", Nothing)
            SetEnv("LMHLP_MUX_REAL", Nothing)
            SetEnv("LMHLP_PROBE_REAL", Nothing)
            SetEnv("LMHLP_PROBE_EXIT", Nothing)
        End Sub

        ''' <summary>The whole point of F-S2: the plaintext token string must
        ''' appear NOWHERE in the persisted file.</summary>
        Private Sub AssertNoPlaintextOnDisk(raw As String, token As String, label As String)
            TestRunner.Assert(Not raw.Contains(token),
                              label & ": PLAINTEXT TOKEN PRESENT in config.json")
            ' The pre-DPAPI schema wrote a bare "GitHubToken" key — its presence
            ' (with a value) means a plaintext token is resting on disk.
            TestRunner.Assert(Not Regex.IsMatch(raw, """GitHubToken""\s*:"),
                              label & ": legacy plain-text ""GitHubToken"" key still present in config.json")
        End Sub

        ' ───────────────────────────────────────────────────────────────
        ' Scenarios
        ' ───────────────────────────────────────────────────────────────

        ''' <summary>GHS-1: the exact production login tail — SaveGitHubUser
        ''' with the OAuth token — must persist ONLY the DPAPI blob.</summary>
        Private Sub Test_SaveNoPlaintext()
            CleanConfig()
            Dim settings As AppSettings = AppSettings.Instance

            settings.SaveGitHubUser("duluka_tester", "https://avatars.example/u.png", SecretToken)

            TestRunner.Assert(IO.File.Exists(ConfigFilePath()), "config.json not written by SaveGitHubUser")
            Dim raw As String = IO.File.ReadAllText(ConfigFilePath())
            AssertNoPlaintextOnDisk(raw, SecretToken, "GHS-1")

            ' The encrypted blob must exist and must NOT be the plaintext itself.
            Dim m As Match = Regex.Match(raw, """GitHubTokenEncrypted""\s*:\s*""([^""]*)""")
            TestRunner.Assert(m.Success AndAlso m.Groups(1).Value.Length > 0,
                              "GitHubTokenEncrypted missing or empty in config.json")
            TestRunner.Assert(m.Groups(1).Value <> SecretToken,
                              "GitHubTokenEncrypted equals the plaintext token — encryption is a no-op!")

            ' In-memory decrypt round-trip (DPAPI CurrentUser).
            TestRunner.Assert(settings.GitHubToken = SecretToken,
                              "DPAPI decrypt round-trip failed — token unusable in-session")
            TestRunner.Assert(settings.IsGitHubLoggedIn, "IsGitHubLoggedIn False after login")

            settings.ClearGitHubUser()
            Dim afterLogout As String = IO.File.ReadAllText(ConfigFilePath())
            AssertNoPlaintextOnDisk(afterLogout, SecretToken, "GHS-1 logout")
            TestRunner.Assert(Not settings.IsGitHubLoggedIn, "logout must clear the logged-in flag")
        End Sub

        ''' <summary>GHS-2: a config.json from the PRE-DPAPI world carries a
        ''' plain-text "GitHubToken" field. Load must migrate it (token stays
        ''' usable) and the migration save must WIPE the plaintext from disk.</summary>
        Private Sub Test_LegacyMigrationScrubs()
            CleanConfig()
            ' Pre-DPAPI world: flat schema with the raw token on disk.
            WriteRawConfig("{ """ & "GitHubToken" & """: """ & LegacyToken & """ }")

            Dim settings As AppSettings = AppSettings.Instance
            settings.Load()

            ' Migration must keep the token usable (no silent loss).
            TestRunner.Assert(settings.GitHubToken = LegacyToken,
                              "legacy token lost during migration — value: " &
                              If(settings.GitHubToken, "(empty)"))

            ' Migration save must have wiped the plaintext from disk.
            Dim raw As String = IO.File.ReadAllText(ConfigFilePath())
            AssertNoPlaintextOnDisk(raw, LegacyToken, "GHS-2")

            ' The atomic writer's crash-recovery .bak mirrors the PRE-save
            ' file — after the migration save it must not carry the plaintext
            ' either (config.json.bak is backed up/synced together with
            ' config.json).
            Dim bak As String = ConfigFilePath() & ".bak"
            If IO.File.Exists(bak) Then
                Dim rawBak As String = IO.File.ReadAllText(bak)
                AssertNoPlaintextOnDisk(rawBak, LegacyToken, "GHS-2 (.bak)")
            End If

            ' The encrypted form must decode back to the legacy token.
            TestRunner.Assert(settings.GitHubToken = LegacyToken AndAlso
                              Regex.IsMatch(raw, """GitHubTokenEncrypted""\s*:\s*""[^""]+"""),
                              "encrypted blob missing after migration")
            settings.ClearGitHubUser()
        End Sub

        ''' <summary>GHS-3: simulated restart — a fresh Load after Save must
        ''' decrypt the persisted blob back to the original token (this is the
        ''' contract that rules out the no-persist alternative).</summary>
        Private Sub Test_RoundTripAfterRestart()
            CleanConfig()
            Dim settings As AppSettings = AppSettings.Instance

            settings.SaveGitHubUser("duluka_tester", "https://avatars.example/u.png", SecretToken)
            Dim tokenAfterSave As String = settings.GitHubToken

            ' Simulated restart: re-load everything from disk.
            Dim fresh As New AppSettings()
            fresh.Load()

            TestRunner.Assert(fresh.GitHubToken = SecretToken,
                              "token did not survive a simulated restart (DPAPI round-trip broken)")
            TestRunner.Assert(fresh.GitHubUser.Username = "duluka_tester",
                              "username did not survive the restart")
            TestRunner.Assert(fresh.IsGitHubLoggedIn, "logged-in state lost after restart")
            TestRunner.Assert(tokenAfterSave = SecretToken, "in-memory token corrupted by save")
            settings.ClearGitHubUser()
        End Sub

    End Module

End Namespace
