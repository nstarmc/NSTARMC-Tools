Imports System.IO
Imports System.Net
Imports ModernWpf
Imports WPFUI

Class MainWindow
    Private Sub NavView_SelectionChanged(sender As ModernWpf.Controls.NavigationView, args As ModernWpf.Controls.NavigationViewSelectionChangedEventArgs) Handles NavView.SelectionChanged
        If NavView.SelectedItem.Content.ToString = "首页" Then
            Dim p1 As Homepage = New Homepage()
            frame.Content = p1
        End If
        If NavView.SelectedItem.Content.ToString = "设置" Then
            Dim p1 As setting = New setting()
            frame.Content = p1
        End If
        If NavView.SelectedItem.Content.ToString = "Java下载" Then
            Dim p1 As java = New java()
            frame.Content = p1
        End If
        If NavView.SelectedItem.Content.ToString = "关于" Then
            Dim p1 As about = New about()
            frame.Content = p1
        End If
        If NavView.SelectedItem.Content.ToString = "下载整合包" Then
            Dim p1 As download = New download()
            frame.Content = p1
        End If
        If NavView.SelectedItem.Content.ToString = "版本列表" Then
            Dim p1 As list = New list()
            frame.Content = p1
        End If
        If NavView.SelectedItem.Content.ToString = "意见反馈" Then
            Dim p1 As feedback = New feedback()
            frame.Content = p1
        End If
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        If My.Settings.themebysys = True Then
            If Theme.Manager.GetSystemTheme = Theme.Style.Dark Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            End If
        Else
            If My.Settings.theme = "Light" Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            End If
        End If

    End Sub
End Class
