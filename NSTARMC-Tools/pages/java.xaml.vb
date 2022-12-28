Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Threading
Imports Downloader
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports IniParser
Imports IniParser.Model
Imports ModernWpf.Controls

Class java
    Dim java_ver
    Dim DWFilename, DWUrl, DWFn, DWS, DWDir, DWDir_up '定义下载线程调用变量
    Dim parser = New FileIniDataParser()

    Private Sub Page_Loaded_1(sender As Object, e As RoutedEventArgs)
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
        homepagestart.Start()
    End Sub
    Private Function Homepagestartthread(ByVal objParamReport As Object) As String
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                mclist.Items.Clear()
                                            End Sub))


        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
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
                                                    mclist.IsEnabled = True
                                                    mclist.SelectedIndex = 0
                                                End Sub))

        End If
    End Function

    Private Sub mclist_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles mclist.SelectionChanged
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    mclist.Dispatcher.Invoke(New Action(Sub()
                                                            dw_java_text.Subtitle = "适配Java版本：Java" & info_xml.<Java>.Value
                                                        End Sub))
                    Exit For
                End If

            End If
        Next
    End Sub

    Private Sub bt_start_Click(sender As Object, e As RoutedEventArgs) Handles bt_start.Click
        bt_start.IsEnabled = False
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    java_ver = info_xml.<Java>.Value
                    '获取Java下载地址
                    Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/java.xml")
                    request.Method = "GET"
                    Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
                    Dim java_xml As XElement = XElement.Parse(sr.ReadToEnd)
                    '放置下载内容
                    DWS = 0
                    If java_ver = 17 Then
                        DWUrl = java_xml.<Java17>.<url>.Value
                        DWDir_up = java_xml.<Java17>.<dir>.Value
                    ElseIf java_ver = 16 Then
                        DWDir_up = java_xml.<Java16>.<dir>.Value
                        DWUrl = java_xml.<Java16>.<url>.Value
                    ElseIf java_ver = 8 Then
                        DWDir_up = java_xml.<Java8>.<dir>.Value
                        DWUrl = java_xml.<Java8>.<url>.Value
                    End If

                    DWFilename = dirlist & "\java.zip"
                    DWFn = "Java" & java_ver
                    DWDir = dirlist
                    Dim dw_java As Thread = New Thread(AddressOf Dw_java_threadAsync)
                    dw_java.Start()
                    card_dwinfo.Visibility = Visibility.Visible
                    Exit For
                End If

            End If
        Next
    End Sub

    Private Async Function Dw_java_threadAsync(ByVal objParamReport As Object) As Task(Of String)
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12
        If Directory.Exists(DWDir & "\jre-x64") = True Then
            Directory.Delete(DWDir & "\jre-x64", True)
        End If

        Try
            '新下载模块
            Dim downloadOpt = New DownloadConfiguration()
            downloadOpt.BufferBlockSize = 10240 '文件缓冲区大小
            downloadOpt.ChunkCount = data("Download")("Thread") '下载线程数量
            downloadOpt.MaximumBytesPerSecond = 0 '下载限速
            downloadOpt.Timeout = 1000 '超时
            downloadOpt.MaxTryAgainOnFailover = Integer.MaxValue
            downloadOpt.OnTheFlyDownload = False
            downloadOpt.ParallelDownload = True
            downloadOpt.TempDirectory = "C:\temp"
            downloadOpt.RequestConfiguration.Accept = "*/*"
            downloadOpt.RequestConfiguration.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
            downloadOpt.RequestConfiguration.CookieContainer = New CookieContainer()
            downloadOpt.RequestConfiguration.Headers = New WebHeaderCollection()
            downloadOpt.RequestConfiguration.KeepAlive = False
            downloadOpt.RequestConfiguration.ProtocolVersion = HttpVersion.Version11
            downloadOpt.RequestConfiguration.UseDefaultCredentials = False
            downloadOpt.RequestConfiguration.UserAgent = ""
            Dim downloader = New DownloadService(downloadOpt)
            AddHandler downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
            Await downloader.DownloadFileTaskAsync(DWUrl, DWFilename)
        Catch ex As Exception

        End Try
        Try

            '启动解压缩线程
            Dim unzip_java As Thread = New Thread(AddressOf Unzip_java_thread)
            unzip_java.Start()
        Catch ex As Exception
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "错误信息:" + ex.Message
                                                 End Sub))
        End Try

    End Function

    Private Sub OnDownloadProgressChanged(sender As Object, e As Downloader.DownloadProgressChangedEventArgs)
        dw_info.Dispatcher.Invoke(New Action(Sub()
                                                 dw_info.Text = "文件大小：" & Math.Round(e.TotalBytesToReceive / 1024 / 1024, 2) & "MB/" &
                                                 Math.Round(e.ReceivedBytesSize / 1024 / 1024, 2) & "MB" &
                                                 " 当前速度：" & Math.Round(e.BytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S"
                                                 dw_progressbar.Value = Math.Round(e.ProgressPercentage, 1)
                                             End Sub))
    End Sub

    Private Function Unzip_java_thread(ByVal objParamReport As Object) As String
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                card_unzip.Visibility = Visibility.Visible
                                                card_dwinfo.Visibility = Visibility.Collapsed
                                            End Sub))

        Dim zf As ZipFile = Nothing
        Try
            Dim fs As FileStream = File.OpenRead(DWFilename)
            zf = New ZipFile(fs)
            For Each zipEntry As ZipEntry In zf
                If Not zipEntry.IsFile Then     ' 忽略目录
                    Continue For
                End If
                Dim entryFileName As [String] = zipEntry.Name
                ' 从条目中删除文件夹：- entryFileName = Path.GetFileName（entryFileName）;
                ' （可选）将条目名称与此处的选择列表匹配，以便根据需要跳过。
                ' 解包长度在 zipEntry.Size 属性中可用.

                Dim buffer As Byte() = New Byte(4095) {}    ' 4K is optimum
                Dim zipStream As Stream = zf.GetInputStream(zipEntry)

                ' Manipulate the output filename here as desired.
                Dim fullZipToPath As [String] = Path.Combine(DWDir, entryFileName)
                Dim directoryName As String = Path.GetDirectoryName(fullZipToPath)
                If directoryName.Length > 0 Then
                    Directory.CreateDirectory(directoryName)
                End If

                ' Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                ' of the file, but does not waste memory.
                ' The "Using" will close the stream even if an exception occurs.
                Using streamWriter As FileStream = File.Create(fullZipToPath)
                    StreamUtils.Copy(zipStream, streamWriter, buffer)
                End Using
            Next
        Finally
            If zf IsNot Nothing Then
                zf.IsStreamOwner = True     ' Makes close also shut the underlying stream
                ' Ensure we release resources
                zf.Close()

                FileSystem.Rename(DWDir & "\" & DWDir_up, DWDir & "\jre-x64")
                File.Delete(DWFilename)
                mclist.Dispatcher.Invoke(New Action(Sub()
                                                        card_unzip.Visibility = Visibility.Collapsed
                                                        Dim dialog As ContentDialog = New ContentDialog() With {
                                                            .Title = "Java安装成功",
                                                            .CloseButtonText = "我知道了",
                                                            .IsPrimaryButtonEnabled = False,
                                                            .DefaultButton = ContentDialogButton.Close,
                                                            .Content = "Java已经成功安装在您选择的整合包当中啦！"
                                                        }
                                                        dialog.ShowAsync()
                                                        bt_start.IsEnabled = True
                                                    End Sub))
            End If
        End Try
    End Function
End Class
