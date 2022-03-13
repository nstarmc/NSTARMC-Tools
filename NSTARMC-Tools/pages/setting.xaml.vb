Imports ModernWpf
Imports ModernWpf.Controls
Imports WPFUI

Class setting

    Private Sub sw1_Toggled(sender As Object, e As RoutedEventArgs) Handles sw1.Toggled
        If sw1.IsOn = True Then
            My.Settings.theme = "Dark"
            My.Settings.Save()
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
            Theme.Manager.Switch(Theme.Style.Dark)
        Else
            My.Settings.theme = "Light"
            My.Settings.Save()
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
            Theme.Manager.Switch(Theme.Style.Light)
        End If
    End Sub

    Private Sub sw2_Toggled(sender As Object, e As RoutedEventArgs) Handles sw2.Toggled
        If sw2.IsOn = True Then
            My.Settings.themebysys = True
            My.Settings.Save()
            sw1.IsEnabled = False
            If Theme.Manager.GetSystemTheme = Theme.Style.Dark Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            End If
        Else
            My.Settings.themebysys = False
            My.Settings.Save()
            sw1.IsEnabled = True
        End If
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        If My.Settings.themebysys = True Then
            sw2.IsOn = True
        Else
            sw2.IsOn = False
        End If

        If My.Settings.theme = "Light" Then
            sw1.IsOn = False
        Else
            sw1.IsOn = True
        End If

        If My.Settings.endwhengamestart = True Then
            sw_autoclose.IsOn = True
        Else
            sw_autoclose.IsOn = False
        End If

        If My.Settings.bg = True Then
            sw_bg.IsOn = True
            bg_op.IsEnabled = True
        Else
            sw_bg.IsOn = False
            bg_op.IsEnabled = False
        End If
        bg_op.Value = My.Settings.bgtoumingdu
    End Sub

    Private Sub sw_autoclose_Toggled(sender As Object, e As RoutedEventArgs) Handles sw_autoclose.Toggled
        If sw_autoclose.IsOn = True Then
            My.Settings.endwhengamestart = True
            My.Settings.Save()
        Else
            My.Settings.endwhengamestart = False
            My.Settings.Save()
        End If
    End Sub

    Private Sub sw_bg_Toggled(sender As Object, e As RoutedEventArgs) Handles sw_bg.Toggled
        If sw_bg.IsOn = True Then
            My.Settings.bg = True
            My.Settings.Save()
            bg_op.IsEnabled = True
        Else
            My.Settings.bg = False
            My.Settings.Save()
            bg_op.IsEnabled = False
        End If
    End Sub
    Private _parentWin As MainWindow

    Public Property ParentWindow As MainWindow
        Get
            Return _parentWin
        End Get
        Set(ByVal value As MainWindow)
            _parentWin = value
        End Set
    End Property
    Private Sub bg_op_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles bg_op.ValueChanged
        If My.Settings.bg = True Then
            ParentWindow.ChangeBG(bg_op.Value)
            My.Settings.bgtoumingdu = bg_op.Value
            My.Settings.Save()
            toumingdu_card.Subtitle = "设置背景透明度" & vbCrLf & "当前不透明度：" & Math.Round(bg_op.Value, 2) * 100 & "%"
        End If

    End Sub
End Class
