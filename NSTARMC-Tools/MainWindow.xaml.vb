Imports System.IO
Imports System.Net
Imports ModernWpf
Imports WPFUI
Imports System.Linq
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports dotnetCampus.FileDownloader

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
            _page_homepage.ParentWindow = Me
        End If
        If NavView.SelectedItem.Content.ToString = "帮助" Then
            frame.Content = _page_help
        End If
        If NavView.SelectedItem.Content.ToString = "设置" Or NavView.SelectedItem.Content.ToString = "Settings" Then
            frame.Content = _page_setting
            _page_setting.ParentWindow = Me
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

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

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
            If File.Exists(My.Application.Info.DirectoryPath & "\Background\bg2.png") Then
                File.Delete(My.Application.Info.DirectoryPath & "\Background\bg.png")
                Rename(My.Application.Info.DirectoryPath & "\Background\bg2.png", My.Application.Info.DirectoryPath & "\Background\bg.png")
                Dim brush As New ImageBrush()
                brush.ImageSource = New BitmapImage(New Uri(My.Application.Info.DirectoryPath & "\Background\bg.png", UriKind.Absolute))
                NavView.Background = brush
                NavView.Background.Opacity = My.Settings.bgtoumingdu

                Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/olbg.json")
                request.Method = "GET"
                Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
                Dim jsonback = sr.ReadToEnd '储存返回的json信息
                '处理json
                Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
                Dim olbg_list(888)
                Dim i = 0
                For Each x In json("img")
                    olbg_list(i) = x
                    i = i + 1
                Next

                Dim MyValue As Integer
                Randomize()
                MyValue = CInt(Int((i * Rnd()) + 0))
                Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\Background\")
                Dim info As FileInfo = New FileInfo(My.Application.Info.DirectoryPath & "\Background\bg2.png")
                Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
                Await segmentFileDownloader.DownloadFileAsync()
            Else
                Dim brush As New ImageBrush()
                brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
                NavView.Background = brush
                NavView.Background.Opacity = My.Settings.bgtoumingdu

                Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/olbg.json")
                request.Method = "GET"
                Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
                Dim jsonback = sr.ReadToEnd '储存返回的json信息
                '处理json
                Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
                Dim olbg_list(888)
                Dim i = 0
                For Each x In json("img")
                    olbg_list(i) = x
                    i = i + 1
                Next

                Dim MyValue As Integer
                Randomize()
                MyValue = CInt(Int((i * Rnd()) + 0))
                Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\Background\")
                Dim info As FileInfo = New FileInfo(My.Application.Info.DirectoryPath & "\Background\bg2.png")
                Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
                Await segmentFileDownloader.DownloadFileAsync()
            End If
            'Dim brush As New ImageBrush()
            'brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
            'frame.Background = brush
            'frame.Background.Opacity = My.Settings.bgtoumingdu

            'Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/olbg.json")
            'request.Method = "GET"
            'Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            'Dim jsonback = sr.ReadToEnd '储存返回的json信息
            ''处理json
            'Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
            'Dim olbg_list(888)
            'Dim i = 0
            'For Each x In json("img")
            '    olbg_list(i) = x
            '    i = i + 1
            'Next

            'Dim MyValue As Integer
            'Randomize()
            'MyValue = CInt(Int((i * Rnd()) + 0))
            'Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\Background\")
            'Dim info As FileInfo = New FileInfo(My.Application.Info.DirectoryPath & "\Background\bg.png")
            'Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
            'Await segmentFileDownloader.DownloadFileAsync()
            'brush.ImageSource = New BitmapImage(New Uri(My.Application.Info.DirectoryPath & "\Background\bg.png", UriKind.Absolute))
            'frame.Background = brush
            'frame.Background.Opacity = My.Settings.bgtoumingdu
        End If
    End Sub

    Public Async Sub ChangeBG(ByVal opa As String)
        If File.Exists(My.Application.Info.DirectoryPath & "\Background\bg.png") Then
            Dim brush As New ImageBrush()
            brush.ImageSource = New BitmapImage(New Uri(My.Application.Info.DirectoryPath & "\Background\bg.png", UriKind.Absolute))
            NavView.Background = brush
            NavView.Background.Opacity = My.Settings.bgtoumingdu
        Else
            Dim brush As New ImageBrush()
            brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
            NavView.Background = brush
            NavView.Background.Opacity = My.Settings.bgtoumingdu

            Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/olbg.json")
            request.Method = "GET"
            Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            Dim jsonback = sr.ReadToEnd '储存返回的json信息
            '处理json
            Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
            Dim olbg_list(888)
            Dim i = 0
            For Each x In json("img")
                olbg_list(i) = x
                i = i + 1
            Next

            Dim MyValue As Integer
            Randomize()
            MyValue = CInt(Int((i * Rnd()) + 0))
            Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\Background\")
            Dim info As FileInfo = New FileInfo(My.Application.Info.DirectoryPath & "\Background\bg.png")
            Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
            Await segmentFileDownloader.DownloadFileAsync()
            brush.ImageSource = New BitmapImage(New Uri(My.Application.Info.DirectoryPath & "\Background\bg.png", UriKind.Absolute))
            NavView.Background = brush
            NavView.Background.Opacity = My.Settings.bgtoumingdu
        End If

    End Sub
    Public Sub ChangeDW()

    End Sub
End Class
