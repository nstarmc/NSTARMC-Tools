Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports dotnetCampus.FileDownloader
Imports ModernWpf.Controls

Class Homepage
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
    End Function




    Private Function Homepagestartthread(ByVal objParamReport As Object) As String
        '检查工具更新
        Dim checkupdate As Thread = New Thread(AddressOf CheckupdatethreadAsync)
        checkupdate.Start()
        '获取公告内容
        '发送get请求，获取xml
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/notice.xml")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim notice As XElement = XElement.Parse(sr.ReadToEnd)
        notice1.Dispatcher.Invoke(New Action(Sub()
                                                 notice1.Subtitle = notice.<announcement>.Value & vbCrLf & "公告发布日期：" & notice.<date>.Value
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
                                                        .Title = "错误",
                                                        .CloseButtonText = "我知道了",
                                                        .IsPrimaryButtonEnabled = False,
                                                        .DefaultButton = ContentDialogButton.Close,
                                                        .Content = "无法在工具下的File检测到有效格式的整合包！" & vbCrLf & "请正确放置整合包文件后，重启工具再试！" & vbCrLf & "如还是无法检测，请在Q群求助！"
                                                    }
                                                    dialog.ShowAsync()
                                                End Sub))

        Else
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    mclist.SelectedIndex = 0
                                                End Sub))

        End If
    End Function

    Private Sub mclist_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles mclist.SelectionChanged
        Dim choose_dir
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    choose_dir = dirlist
                    mclist.Dispatcher.Invoke(New Action(Sub()
                                                            verinfolabel.Content = "版本号：" & info_xml.<mcversion>.Value & vbCrLf &
                                                            "模组加载器：" & info_xml.<modloader>.Value & vbCrLf &
                                                            "光影加载器：" & info_xml.<sharder>.Value & vbCrLf &
                                                            "打包日期：" & info_xml.<packdate>.<year>.Value & "年" & info_xml.<packdate>.<month>.Value & "月" & info_xml.<packdate>.<day>.Value & "日" & vbCrLf &
                                                            "整合包ID（日后检查更新用）：" & info_xml.<packid>.Value
                                                        End Sub))
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
