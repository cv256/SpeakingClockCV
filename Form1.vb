Imports System.Net.NetworkInformation


Public Class Form1

    Private s As Speech.Synthesis.SpeechSynthesizer
    Private Timer1 As New Windows.Forms.Timer()
    Private TimeLastTime As Date
    Private VPNLastTime As Date, VPNLastText As String

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Timer1.Interval = 30500
        Timer1.Enabled = True
        AddHandler Timer1.Tick, AddressOf Timer1_Tick

        s = New Speech.Synthesis.SpeechSynthesizer
        For Each a As Speech.Synthesis.InstalledVoice In s.GetInstalledVoices()
            'If not a.SayTime Then Continue for
            ComboBox1.Items.Add(a.VoiceInfo.Description)
            If Not String.IsNullOrEmpty(My.Settings.Language) AndAlso a.VoiceInfo.Description = My.Settings.Language Then ComboBox1.SelectedIndex = ComboBox1.Items.Count - 1
        Next

        CheckBox1.Checked = My.Settings.SayTime
        CheckBox2.Checked = My.Settings.SayVPN
        txtTimeEvery.Text = My.Settings.TimeEvery
        txtVPNEvery.Text = My.Settings.VPNEvery
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs)

        With Now
            lbTest.Text = .ToString("HH:mm")
            If CheckBox1.Checked Then
                Dim interval As Integer = 15
                Integer.TryParse(txtTimeEvery.Text, interval)
                If Now.Subtract(TimeLastTime).TotalSeconds > interval * 60 / 2 AndAlso (Now.Minute Mod interval) = 0 Then
                    If SayTime() Then TimeLastTime = Now
                    Return ' when I say the Time I dont want to also say the VPN , its too much precision, it will be said in 30 seconds anyway
                End If
            End If
        End With

        If CheckBox2.Checked Then
            Dim interval As Integer = 1
            Integer.TryParse(txtVPNEvery.Text, interval)
            If Now.Subtract(VPNLastTime).TotalMinutes >= interval Then
                If SayVPN() Then VPNLastTime = Now
            End If
        End If
    End Sub

    Private Function SayTime() As Boolean
        If Not SetVoice() Then Return False
        Dim what As String = ""
        With Now
            If s.Voice.Culture.Name.StartsWith("pt") Then
                what = .Hour & If(.Minute = 0, " horas", ",  E " & .Minute)
            Else
                If .Minute = 0 Then
                    what = .Hour & " o clock"
                Else
                    what = .Minute & " past " & .Hour
                End If
            End If
        End With
        s.SpeakAsync(what)
        lbError.Text = what
        Return True
    End Function

    Private Function SayVPN() As Boolean
        Dim what As String = ""
        For Each n In NetworkInterface.GetAllNetworkInterfaces()
            If n.NetworkInterfaceType = NetworkInterfaceType.Ppp Then
                what &= " " & n.Name & "."
            End If
        Next

        If Not String.IsNullOrEmpty(what) Then
            If Not SetVoice() Then Return False
            what = "VPN, " & what
            s.SpeakAsync(what)
            lbError.Text = what
        End If

        If VPNLastText <> what Then
            VPNLastText = what
            txtLog.Text = Now.ToString("ddd d MMM HH:mm") & "     " & If(what = "", "(no VPN connected)", what.Replace(",", "")) & vbCrLf & txtLog.Text
        End If

        Return True
    End Function

    Private Function SetVoice() As Boolean
        lbError.Text = ""
        Try
            Dim descr As String = ComboBox1.SelectedItem
            Dim a As Speech.Synthesis.InstalledVoice
            If Not String.IsNullOrEmpty(descr) Then
                a = s.GetInstalledVoices().FirstOrDefault(Function(f) f.VoiceInfo.Description.Equals(descr))
            Else
                a = s.GetInstalledVoices().FirstOrDefault(Function(f) f.VoiceInfo.Culture.Equals(Globalization.CultureInfo.CurrentCulture))
            End If
            If a IsNot Nothing Then
                s.SelectVoice(a.VoiceInfo.Name)
                s.Volume = 100
                s.Rate = -2
                If a.VoiceInfo.Culture.Name.StartsWith("pt") Then s.Rate = -3
            End If
        Catch ex As Exception
            lbError.Text = ex.Message
            Return False
        End Try
        Return True
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SayTime()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        SayVPN()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        My.Settings.SayTime = CheckBox1.Checked
        My.Settings.SayVPN = CheckBox2.Checked
        My.Settings.TimeEvery = txtTimeEvery.Text
        My.Settings.VPNEvery = txtVPNEvery.Text
        My.Settings.Language = ComboBox1.SelectedItem

        My.Settings.Save()
    End Sub
End Class
