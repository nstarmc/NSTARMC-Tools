Imports System.IO
Imports System.Net
Imports ModernWpf
Imports WPFUI
Imports System.Linq
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports dotnetCampus.FileDownloader
Imports IniParser
Imports IniParser.Model
Imports System.Reflection

Class MainWindow
    Dim _page_java As java = New java()
    Dim _page_homepage As Homepage = New Homepage()
    Dim _page_setting As setting = New setting()
    Dim _page_about As about = New about()
    Dim _page_download As download = New download()
    Dim _page_list As list = New list()
    Dim _page_feedback As feedback = New feedback()
    Dim _page_help As help = New help()
    Public Shared shouquan As Boolean
    Private Sub NavView_SelectionChanged(sender As ModernWpf.Controls.NavigationView, args As ModernWpf.Controls.NavigationViewSelectionChangedEventArgs) Handles NavView.SelectionChanged
        If args.IsSettingsSelected Then
            frame.Content = _page_setting
            _page_setting.ParentWindow = Me
        Else
            If NavView.SelectedItem.Content.ToString = "首页" Then
                frame.Content = _page_homepage
                _page_homepage.ParentWindow = Me
            End If
            If NavView.SelectedItem.Content.ToString = "帮助" Then
                frame.Content = _page_help
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
        End If

    End Sub

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim currentUser As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim sid As String = currentUser.User.ToString()

        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If data("UI")("Windows_Size_Width") = "" Then
        Else
            Me.Width = data("UI")("Windows_Size_Width")
            Me.Height = data("UI")("Windows_Size_Height")
        End If

        If data("UI")("Sidebar") = "Top" Then
            NavView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top
        Else
            NavView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left

        End If
        If data("UI")("AutoTheme") = "False" Then
            If data("UI")("Theme") = "Dark" Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)

            End If
        Else
            If Theme.Manager.GetSystemTheme = Theme.Style.Dark Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            End If

        End If
        '标定背景透明度（初始化）
        If data("UI")("Background_Opacity") = "" Then
            data("UI")("Background_Opacity") = "0.68"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
        End If

        If data("UI")("Background") = "False" Then

        Else
            If File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg2.png") Then
                If data("UI")("Background_download") = "True" Then
                    Try
                        File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png")
                    Catch ex As Exception

                    End Try


                    Rename(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg2.png", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png")
                    Dim brush As New ImageBrush()
                    brush.ImageSource = New BitmapImage(New Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png", UriKind.Absolute))
                    NavView.Background = brush
                    NavView.Background.Opacity = data("UI")("Background_Opacity")
                    '————下载下一次的bg————
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
                    Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\")
                    Dim info As FileInfo = New FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg2.png")
                    Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
                    data("UI")("Background_download") = "False"
                    parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                    Await segmentFileDownloader.DownloadFileAsync()
                    data("UI")("Background_download") = "True"
                    parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                Else '下载未完成，加载bg/默认
                    If File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png") Then
                        Dim brush As New ImageBrush()
                        brush.ImageSource = New BitmapImage(New Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png", UriKind.Absolute))
                        NavView.Background = brush
                        NavView.Background.Opacity = data("UI")("Background_Opacity")
                        '————下载下一次的bg————
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
                        Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\")
                        Dim info As FileInfo = New FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg2.png")
                        Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
                        data("UI")("Background_download") = "False"
                        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                        Await segmentFileDownloader.DownloadFileAsync()
                        data("UI")("Background_download") = "True"
                        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                    Else '加载默认
                        Dim brush As New ImageBrush()
                        brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
                        NavView.Background = brush
                        NavView.Background.Opacity = data("UI")("Background_Opacity")

                        '————下载下一次的bg————
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
                        Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\")
                        Dim info As FileInfo = New FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg2.png")
                        Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
                        data("UI")("Background_download") = "False"
                        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                        Await segmentFileDownloader.DownloadFileAsync()
                        data("UI")("Background_download") = "True"
                        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                    End If
                End If
            Else
                Dim brush As New ImageBrush()
                brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
                NavView.Background = brush
                NavView.Background.Opacity = data("UI")("Background_Opacity")
                '————下载下一次的bg————
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
                Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\")
                Dim info As FileInfo = New FileInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg2.png")
                Dim segmentFileDownloader = New SegmentFileDownloader(olbg_list(MyValue), info)
                data("UI")("Background_download") = "False"
                parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
                Await segmentFileDownloader.DownloadFileAsync()
                data("UI")("Background_download") = "True"
                parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)

            End If

        End If
    End Sub
    Public Sub Changesidebar()

        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If data("UI")("Sidebar") = "Left" Then
            NavView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left
        Else
            NavView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top
        End If
    End Sub
    Public Sub ChangeBG(ByVal opa As String)

        If File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png") Then
            Dim brush As New ImageBrush()
            brush.ImageSource = New BitmapImage(New Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Background\bg.png", UriKind.Absolute))
            NavView.Background = brush
            NavView.Background.Opacity = opa
        Else
            Dim brush As New ImageBrush()
            brush.ImageSource = New BitmapImage(New Uri("pack://application:,,,/NSTARMC-Tools;component/res/mc1.jpg", UriKind.Absolute))
            NavView.Background = brush
            NavView.Background.Opacity = opa
        End If

    End Sub
    Public Sub ChangeDW()

    End Sub

    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If data("UI")("Windows_Size_Width") = "" Then
        Else
            Me.Width = data("UI")("Windows_Size_Width")
            Me.Height = data("UI")("Windows_Size_Height")
        End If
    End Sub

    Private Sub Window_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        data("UI")("Windows_Size_Width") = Me.Width
        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
        data("UI")("Windows_Size_Height") = Me.Height
        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        End

    End Sub
End Class
