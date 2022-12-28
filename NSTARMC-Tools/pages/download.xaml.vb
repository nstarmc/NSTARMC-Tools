Imports System.ComponentModel
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
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Class download
    Dim down_url, packid, down_ver
    Dim allow_download = 0
    Dim parser = New FileIniDataParser()



    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim pagestart As Thread = New Thread(AddressOf Pagestartthread)
        pagestart.Start()
    End Sub
    Private Function Pagestartthread(ByVal objParamReport As Object) As String
        '发送get请求，请求版本表
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim jsonback = sr.ReadToEnd '储存返回的json信息
        '处理json
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)

        '遍历读取类别
        downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                       downlod_group.Items.Clear()
                                                   End Sub))
        For Each x In json("group")
            downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                           Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x.ToString), JObject)
                                                           downlod_group.Items.Add(json2("name"))
                                                       End Sub))
        Next
        downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                       downlod_group.SelectedIndex = 0
                                                   End Sub))
    End Function

    Private Sub downlod_group_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles downlod_group.SelectionChanged
        Dim list As Thread = New Thread(AddressOf Listver)
        list.Start()
    End Sub
    Private Function Listver(ByVal objParamReport As Object) As String
        '发送get请求，请求版本表
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim jsonback = sr.ReadToEnd '储存返回的json信息
        '处理json
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)

        '遍历读取类别

        For Each x In json("group")
            downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                           Dim json3 As JObject = CType(JsonConvert.DeserializeObject(x.ToString), JObject)
                                                           If json3("name") = downlod_group.SelectedItem Then
                                                               list_ver.Items.Clear()
                                                               For Each x2 In json3("list")
                                                                   Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                                   list_ver.Items.Add(json2("version"))
                                                               Next
                                                               list_ver.SelectedIndex = 0
                                                           End If

                                                       End Sub))
        Next
    End Function

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim dw_thr As Thread = New Thread(AddressOf Dw_thread)
        dw_thr.Start()
        dw_info.Text = "开始处理下载任务···"
        dw_pro.IsActive = True
        bt_startdw.IsEnabled = False
    End Sub
    Private Function Dw_thread(ByVal objParamReport As Object) As String


        '发送get请求，请求下载地址
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim jsonback = sr.ReadToEnd '储存返回的json信息
        sr.Close()
        '处理json
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)

        '遍历读取下载链接，id
        For Each x In json("group")
            downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                           Dim json3 As JObject = CType(JsonConvert.DeserializeObject(x.ToString), JObject)
                                                           If json3("name") = downlod_group.SelectedItem Then
                                                               For Each x2 In json3("list")
                                                                   Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                                   If json2("version") = list_ver.SelectedItem Then
                                                                       down_url = json2("download")
                                                                       packid = json2("id")
                                                                       down_ver = json2("version")
                                                                   End If

                                                               Next
                                                           End If

                                                       End Sub))
        Next

        '检查本地是否存在相应ID整合包
        allow_download = 0
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                               If packid = info_xml.<packid>.Value Then
                                                                   allow_download = 1

                                                               End If

                                                           End Sub))
            End If
        Next
        If allow_download = 1 Then
            downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                           Dim dialog As ContentDialog = New ContentDialog() With {
                                                        .Title = "错误",
                                                        .CloseButtonText = "我知道了",
                                                        .IsPrimaryButtonEnabled = False,
                                                        .DefaultButton = ContentDialogButton.Close,
                                                        .Content = "本地文件夹已经存在相应版本整合包！" & vbCrLf & "请不要重复下载哦~"
                                                            }
                                                           dialog.ShowAsync()
                                                           dw_info.Text = "没有正在进行的下载任务哦~"
                                                           dw_pro.IsActive = False
                                                           bt_startdw.IsEnabled = True
                                                       End Sub))
        Else

            Dim dw_thr2 As Thread = New Thread(AddressOf Dwfile_threadAsync)
            dw_thr2.Start()

        End If
    End Function

    Private Sub list_ver_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles list_ver.SelectionChanged
        Dim his As Thread = New Thread(AddressOf his_get)
        his.Start()
    End Sub

    Private Sub his_get()
        '发送get请求，请求下载地址
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim jsonback = sr.ReadToEnd '储存返回的json信息
        sr.Close()
        '处理json
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)

        '遍历读取下载链接，id
        For Each x In json("group")
            downlod_group.Dispatcher.Invoke(New Action(Sub()
                                                           Dim json3 As JObject = CType(JsonConvert.DeserializeObject(x.ToString), JObject)
                                                           If json3("name") = downlod_group.SelectedItem Then
                                                               For Each x2 In json3("list")
                                                                   Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                                   If json2("version") = list_ver.SelectedItem Then
                                                                       If json2("history") = "" Then
                                                                           his.Subtitle = "整合包作者尚未填写更新日志"
                                                                       Else
                                                                           his.Subtitle = json2("history")
                                                                       End If


                                                                   End If

                                                               Next
                                                           End If

                                                       End Sub))
        Next
    End Sub

    'Public Event DownloadProgressChanged As EventHandler(Of Downloader.DownloadProgressChangedEventArgs)

    Public Async Sub Dwfile_threadAsync(ByVal objParamReport As Object)
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12
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
            Await downloader.DownloadFileTaskAsync(down_url.ToString, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\download.zip")


            '解压进程
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "正在进行解压···"
                                                 End Sub))
            Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctools")
            Dim zf As ZipFile = Nothing
            Try
                Dim fs As FileStream = File.OpenRead(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\download.zip")
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
                    Dim fullZipToPath As [String] = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctools", entryFileName)
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
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_info.Text = "解压完成！正在进行文件夹移动···"
                                                         End Sub))
                    For Each Dirfile In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctools\file\")
                        Directory.Move(Dirfile, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\" & down_ver & "_" & Now.Year & Now.Month & Now.Day & "\")
                    Next
                    Directory.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctools\", True)
                    File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\download.zip")
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_info.Text = "下载完成啦~"
                                                             Dim dialog As ContentDialog = New ContentDialog() With {
                                                        .Title = "下载完成",
                                                        .CloseButtonText = "我知道了",
                                                        .IsPrimaryButtonEnabled = False,
                                                        .DefaultButton = ContentDialogButton.Close,
                                                        .Content = "你所选择的整合包下载完成啦！"
                                                    }
                                                             dialog.ShowAsync()
                                                             dw_pro.IsActive = False
                                                             bt_startdw.IsEnabled = True
                                                         End Sub))
                End If
            End Try
        Catch ex As Exception
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "错误信息:" + ex.ToString
                                                     bt_startdw.IsEnabled = True
                                                 End Sub))

        End Try
    End Sub
    Private Sub OnDownloadProgressChanged(sender As Object, e As Downloader.DownloadProgressChangedEventArgs)
        dw_info.Dispatcher.Invoke(New Action(Sub()
                                                 dw_info.Text = "文件大小：" & Math.Round(e.TotalBytesToReceive / 1024 / 1024, 2) & "MB/" &
                                                 Math.Round(e.ReceivedBytesSize / 1024 / 1024, 2) & "MB" & vbCrLf &
                                                 "下载进度：" & Math.Round(e.ProgressPercentage, 2) & "%" & vbCrLf &
                                                 "当前速度：" & Math.Round(e.BytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S" & vbCrLf &
                                                 "平均速度：" & Math.Round(e.AverageBytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S"
                                             End Sub))
    End Sub


End Class
