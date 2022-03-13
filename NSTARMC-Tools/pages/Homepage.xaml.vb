Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports dotnetCampus.FileDownloader
Imports ModernWpf.Controls
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Class Homepage
    Dim checkupdate = 0
    Dim local_id, url_online, year_ol, month_ol, day_ol, year_l, month_l, day_l, upd_dir '定义检查更新部分使用变量
    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
        homepagestart.Start()
    End Sub
    Private Async Function CheckupdatethreadAsync(ByVal objParamReport As Object) As Task(Of String)

        Try
            File.Delete(My.Application.Info.DirectoryPath & "\upd.bat")
            File.Delete(My.Application.Info.DirectoryPath & "\upd.vbs")
        Catch ex As Exception

        End Try
        If My.Computer.FileSystem.DirectoryExists(My.Application.Info.DirectoryPath & "\file\") = False Then
            Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\file\")
        End If
        '更新工具
        If checkupdate = 0 Then
            checkupdate = 1
            Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/tool.xml")
            request.Method = "GET"
            Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
            Dim tool_xml As XElement = XElement.Parse(sr.ReadToEnd)
            If My.Application.Info.Version.ToString < tool_xml.<version>.Value Then
                notice1.Dispatcher.Invoke(New Action(Sub()
                                                         upinfo.Visibility = Visibility.Visible
                                                         card_lauch.Visibility = Visibility.Collapsed
                                                         card_info.Visibility = Visibility.Collapsed
                                                         card_list.Visibility = Visibility.Collapsed
                                                         upinfo.Subtitle = "正在下载新版本工具···" & vbCrLf & "更新内容：" & vbCrLf & tool_xml.<content>.Value
                                                     End Sub))
                Dim info As FileInfo = New FileInfo(My.Application.Info.DirectoryPath & "\main_new.exe")
                Dim segmentFileDownloader = New SegmentFileDownloader(tool_xml.<update>.Value, info)
                Await segmentFileDownloader.DownloadFileAsync()
                '写入bat替换脚本
                Dim file As System.IO.StreamWriter
                file = My.Computer.FileSystem.OpenTextFileWriter(My.Application.Info.DirectoryPath & "\upd.bat", False, Encoding.GetEncoding("GB2312"))
                file.WriteLine("@echo off")
                file.WriteLine("TIMEOUT /T 1")
                file.WriteLine("taskkill /F /IM " & Replace(Me.GetType().Assembly.Location, My.Application.Info.DirectoryPath & "\", ""))
                file.WriteLine("TIMEOUT /T 1")
                file.WriteLine("copy """ & My.Application.Info.DirectoryPath & "\main_new.exe"" """ & Me.GetType().Assembly.Location & """")
                file.WriteLine("start """" """ & Me.GetType().Assembly.Location & """")
                file.WriteLine("del """ & My.Application.Info.DirectoryPath & "\main_new.exe" & """")
                file.WriteLine("exit")
                file.Close()
                '写入bat替换脚本
                file = My.Computer.FileSystem.OpenTextFileWriter(My.Application.Info.DirectoryPath & "\upd.vbs", False, Encoding.GetEncoding("GB2312"))
                file.WriteLine("set shell=wscript.createObject(""wscript.shell"")")
                file.WriteLine("run=shell.Run(""" & My.Application.Info.DirectoryPath & "\upd.bat" & """,0)")
                file.Close()
                mclist.Dispatcher.Invoke(New Action(Async Sub()
                                                        ring1.IsActive = False
                                                        Dim dialog As ContentDialog = New ContentDialog() With {
                                                            .Title = "更新",
                                                            .CloseButtonText = "更新",
                                                            .PrimaryButtonText = "Or 更新",
                                                            .DefaultButton = ContentDialogButton.Close,
                                                            .Content = "有新版本的整合包工具！" & vbCrLf & "新版本已经下载完成，你只需要点击更新按钮，" & vbCrLf & "我们将为您替换为新版本，并重启工具！" & vbCrLf & "快去更新吧！" & vbCrLf & "#更新内容：" & vbCrLf & tool_xml.<content>.Value
                                                        }
                                                        Await dialog.ShowAsync()
                                                        Dim p As New Process()
                                                        p.StartInfo.FileName = "cmd.exe"
                                                        p.StartInfo.UseShellExecute = False
                                                        p.StartInfo.RedirectStandardInput = True
                                                        p.StartInfo.RedirectStandardOutput = True
                                                        p.StartInfo.RedirectStandardError = True
                                                        p.StartInfo.CreateNoWindow = True
                                                        p.Start()
                                                        p.StandardInput.WriteLine("start "" "" upd.vbs") '这个Data就是cmd命令
                                                        End
                                                    End Sub))
            End If
        End If

    End Function
    Private Function onesay_thr(ByVal objParamReport As Object) As String
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/onesay.json")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim jsonback = sr.ReadToEnd '储存返回的json信息
        '处理json
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
        Dim onesay_list(888)
        Dim i = 0
        For Each x In json("onesay")
            onesay_list(i) = x
            i = i + 1
        Next

        notice1.Dispatcher.Invoke(New Action(Sub()
                                                 Dim MyValue As Integer
                                                 Randomize()
                                                 MyValue = CInt(Int((i * Rnd()) + 0))
                                                 one_say_card.Subtitle = onesay_list(MyValue)
                                             End Sub))
    End Function

    Private Function Homepagestartthread(ByVal objParamReport As Object) As String

        '检查工具更新
        Dim checkupdate As Thread = New Thread(AddressOf CheckupdatethreadAsync)
        checkupdate.Start()
        '检查工具更新
        Dim onesay As Thread = New Thread(AddressOf onesay_thr)
        onesay.Start()
        '获取公告内容
        '发送get请求，获取xml
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/notice.xml")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim notice As XElement = XElement.Parse(sr.ReadToEnd)
        notice1.Dispatcher.Invoke(New Action(Sub()
                                                 notice1.Subtitle = notice.<announcement>.Value & vbCrLf & "公告发布日期：" & notice.<date>.Value
                                             End Sub))
        If My.Settings.dialogv < notice.<dialogv>.Value.ToString Then
            notice1.Dispatcher.Invoke(New Action(Async Sub()
                                                     Dim dialog As ContentDialog = New ContentDialog() With {
                                                            .Title = notice.<dialogt>.Value,
                                                            .CloseButtonText = "我知道啦",
                                                            .PrimaryButtonText = "不再提示",
                                                            .DefaultButton = ContentDialogButton.Primary,
                                                            .Content = notice.<dialog>.Value
                                                        }
                                                     Dim result As ContentDialogResult = Await dialog.ShowAsync()
                                                     If result = ContentDialogResult.Primary Then
                                                         My.Settings.dialogv = notice.<dialogv>.Value.ToString
                                                         My.Settings.Save()
                                                     End If
                                                 End Sub))
        End If
        '读取整合包
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                mclist.Items.Clear()
                                            End Sub))
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                mclist.Dispatcher.Invoke(New Action(Sub()
                                                        mclist.Items.Add(info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")")

                                                    End Sub))
            End If
        Next
        If mclist.Items.Count = 0 Then
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    mclist.IsEnabled = False
                                                    Dim dialog As ContentDialog = New ContentDialog() With {
                                                        .Title = "本地未检测到整合包文件~",
                                                        .CloseButtonText = "我知道啦",
                                                        .IsPrimaryButtonEnabled = False,
                                                        .DefaultButton = ContentDialogButton.Close,
                                                        .Content = "无法在工具下的File检测到有效格式的整合包！" & vbCrLf & "请在左侧导航栏进入下载页面下载整合包！" & vbCrLf & "或者请正确放置整合包文件后，重启工具再试！" & vbCrLf & "如还是无法检测，请在Q群求助！"
                                                    }
                                                    dialog.ShowAsync()
                                                    'ParentWindow.ChangeDW()
                                                End Sub))

        Else

            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    mclist.SelectedIndex = 0
                                                    mclist.IsEnabled = True
                                                End Sub))

        End If
    End Function
    Private _parentWin As MainWindow

    Public Property ParentWindow As MainWindow
        Get
            Return _parentWin
        End Get
        Set(ByVal value As MainWindow)
            _parentWin = value
        End Set
    End Property

    Private Sub mclist_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles mclist.SelectionChanged
        Dim choose_dir
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim ol_ok = 0
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    choose_dir = dirlist
                    mclist.Dispatcher.Invoke(New Action(Sub()
                                                            verinfolabel.Content = "版本号：" & info_xml.<mcversion>.Value & vbCrLf &
                                                            "模组加载器：" & info_xml.<modloader>.Value & vbCrLf &
                                                            "光影加载器：" & info_xml.<sharder>.Value & vbCrLf &
                                                            "打包日期：" & info_xml.<packdate>.<year>.Value & "年" & info_xml.<packdate>.<month>.Value & "月" & info_xml.<packdate>.<day>.Value & "日" & vbCrLf &
                                                            "整合包ID：" & info_xml.<packid>.Value
                                                        End Sub))

                    '从资源服务器获取该版本信息
                    '发送get请求，请求版本表
                    Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
                    request.Method = "GET"
                    Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
                    Dim jsonback = sr.ReadToEnd '储存返回的json信息
                    '处理json
                    Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
                    For Each x In json("group")
                        mclist.Dispatcher.Invoke(New Action(Sub()
                                                                For Each x2 In json("group")(0)("list")
                                                                    Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                                    If json2("id").ToString > 10000 Then
                                                                        If json2("id").ToString < 20000 Then
                                                                            If json2("id").ToString = Int(info_xml.<packid>.Value) Then
                                                                                ol_ok = 1
                                                                                year_ol = json2("update_time")("year").ToString
                                                                                month_ol = json2("update_time")("month").ToString
                                                                                day_ol = json2("update_time")("date").ToString
                                                                                url_online = json2("download")
                                                                                Exit For
                                                                            End If
                                                                        End If
                                                                    End If

                                                                Next
                                                            End Sub))
                    Next
                    If ol_ok = 1 Then
                        '比对本地与服务器日期
                        Dim update_yes = 0
                        If Int(year_ol) > Int(info_xml.<packdate>.<year>.Value.ToString) Then
                            update_yes = 1
                        Else
                            If Int(month_ol) > Int(info_xml.<packdate>.<month>.Value.ToString) Then
                                update_yes = 1
                            Else
                                If Int(month_ol) = Int(info_xml.<packdate>.<month>.Value.ToString) And Int(day_ol) > Int(info_xml.<packdate>.<day>.Value.ToString) Then
                                    update_yes = 1
                                Else
                                    update_yes = 0
                                End If
                            End If
                        End If
                        If update_yes = 1 Then
                            '检测到更新，弹窗
                            mclist.Dispatcher.Invoke(New Action(Sub()

                                                                    verinfolabel.Content += vbCrLf & "该版本整合包存在新版本！"
                                                                    Dim dialog As ContentDialog = New ContentDialog() With {
                                                                        .Title = "有新的整合包版本！",
                                                                        .CloseButtonText = "我知道啦",
                                                                        .DefaultButton = ContentDialogButton.Close,
                                                                        .Content = "检测到选中的整合包有新的版本！" & vbCrLf & "建议前往版本列表页面更新整合包~" & vbCrLf & "如果数据重要，建议先备份!"
                                                                    }
                                                                    dialog.ShowAsync()

                                                                End Sub))
                        End If
                    End If

                End If

            End If
        Next
    End Sub

    Private Sub lauch_1_Click(sender As Object, e As RoutedEventArgs) Handles lauch_1.Click
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    '启动客户端
                    Dim apppath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                    Try
                        Dim p As New Process()
                        p.StartInfo.FileName = "cmd.exe"
                        p.StartInfo.UseShellExecute = False
                        p.StartInfo.RedirectStandardInput = True
                        p.StartInfo.RedirectStandardOutput = True
                        p.StartInfo.RedirectStandardError = True
                        p.StartInfo.CreateNoWindow = True
                        p.Start()
                        p.StandardInput.WriteLine("cd " & dirlist) '这个Data就是cmd命令
                        p.StandardInput.WriteLine("PCL2.exe") '这个Data就是cmd命令
                        p.StandardInput.WriteLine("exit") '这个Data就是cmd命令
                        p.WaitForExit()
                        If My.Settings.endwhengamestart = True Then
                            End
                        End If
                    Catch ex As Exception

                    End Try
                End If

            End If
        Next
    End Sub

    Private Sub lauch_2_Click(sender As Object, e As RoutedEventArgs) Handles lauch_2.Click
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    '启动客户端
                    Dim apppath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                    Try
                        Dim p As New Process()
                        p.StartInfo.FileName = "cmd.exe"
                        p.StartInfo.UseShellExecute = False
                        p.StartInfo.RedirectStandardInput = True
                        p.StartInfo.RedirectStandardOutput = True
                        p.StartInfo.RedirectStandardError = True
                        p.StartInfo.CreateNoWindow = True
                        p.Start()
                        p.StandardInput.WriteLine("cd " & dirlist) '这个Data就是cmd命令
                        p.StandardInput.WriteLine("HMCL.exe") '这个Data就是cmd命令
                        p.StandardInput.WriteLine("exit") '这个Data就是cmd命令
                        p.WaitForExit()
                        If My.Settings.endwhengamestart = True Then
                            End
                        End If
                    Catch ex As Exception

                    End Try
                End If

            End If
        Next
    End Sub
End Class
