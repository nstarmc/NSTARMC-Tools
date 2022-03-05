Imports System.IO
Imports System.Net
Imports ModernWpf
Imports WPFUI

Class MainWindow
    Dim _page_java As java = New java()
    Dim _page_homepage As Homepage = New Homepage()
    Dim _page_setting As setting = New setting()
    Dim _page_about As about = New about()
    Dim _page_download As download = New download()
    Dim _page_list As list = New list()
    Dim _page_feedback As feedback = New feedback()
    Dim _page_help As help = New help()
    Private Sub NavView_SelectionChanged(sender As ModernWpf.Controls.NavigationView, args As ModernWpf.Controls.NavigationViewSelectionChangedEventArgs) Handles NavView.SelectionChanged
        If NavView.SelectedItem.Content.ToString = "首页" Then
            frame.Content = _page_homepage
        End If
        If NavView.SelectedItem.Content.ToString = "帮助" Then
            frame.Content = _page_help
        End If
        If NavView.SelectedItem.Content.ToString = "设置" Or NavView.SelectedItem.Content.ToString = "Settings" Then
            frame.Content = _page_setting
        End If
        If NavView.SelectedItem.Content.ToString = "Java下载" Then
            frame.Content = _page_java
        End If
        If NavView.SelectedItem.Content.ToString = "关于" Then
            frame.Content = _page_about
        End If
        If NavView.SelectedItem.Content.ToString = "下载整合包" Then
            frame.Content = _page_download
        End If
        If NavView.SelectedItem.Content.ToString = "版本列表" Then
            frame.Content = _page_list
        End If
        If NavView.SelectedItem.Content.ToString = "意见反馈" Then
            frame.Content = _page_feedback
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

        If My.Settings.bg = True Then
            Dim brush As New ImageBrush()
            brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
            frame.Background = brush
        End If
    End Sub

    Private Sub ChangeBG()
        Dim brush As New ImageBrush()
        brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
        frame.Background = brush
    End Sub
End Class
