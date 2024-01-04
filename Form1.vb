Public Class Form1

    Private s As System.Speech.Synthesis.SpeechSynthesizer
    Private Timer1 As New Windows.Forms.Timer()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        Timer1.Interval = 30500
        Timer1.Enabled = False
        AddHandler Timer1.Tick, AddressOf Timer1_Tick

        s = New System.Speech.Synthesis.SpeechSynthesizer
        For Each a As Speech.Synthesis.InstalledVoice In s.GetInstalledVoices()
            'If not a.Enabled Then Continue for
            ComboBox1.Items.Add(a.VoiceInfo.Description)
            If Not String.IsNullOrEmpty(My.Settings.Language) AndAlso a.VoiceInfo.Description = My.Settings.Language Then ComboBox1.SelectedIndex = ComboBox1.Items.Count - 1
        Next

        CheckBox1.Checked = My.Settings.Enabled
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        With Now
            lbTest.Text = .Hour & ":" & .Minute.ToString("00")
            If .Minute = 0 OrElse .Minute = 30 OrElse .Minute = 15 OrElse .Minute = 45 Then SayTime()
        End With
    End Sub

    Private Sub SayTime()
        With Now
            Dim descr As String = ComboBox1.SelectedItem
            Dim a As Speech.Synthesis.InstalledVoice
            If Not String.IsNullOrEmpty(descr) Then
                a = s.GetInstalledVoices().FirstOrDefault(Function(f) f.VoiceInfo.Description.Equals(descr))
                My.Settings.Language = descr
                My.Settings.Save()
            Else
                a = s.GetInstalledVoices().FirstOrDefault(Function(f) f.VoiceInfo.Culture.Equals(Globalization.CultureInfo.CurrentCulture))
            End If
            If a IsNot Nothing Then
                s.SelectVoice(a.VoiceInfo.Name)
                s.Volume = 100
                s.Rate = -2
                If a.VoiceInfo.Culture.Name.StartsWith("pt") Then
                    s.Rate = -3
                    s.SpeakAsync(.Hour & If(.Minute = 0, " horas", ",  E " & .Minute))
                Else
                    If .Minute = 0 Then
                        s.SpeakAsync(.Hour & " o clock")
                    Else
                        s.SpeakAsync(.Minute & " past " & .Hour)
                    End If
                End If
            End If
        End With
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SayTime()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Timer1.Enabled = CheckBox1.Checked
        My.Settings.Enabled = CheckBox1.Checked
        My.Settings.Save()
    End Sub

End Class
