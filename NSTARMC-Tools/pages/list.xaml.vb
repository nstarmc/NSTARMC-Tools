Imports System.Threading
Imports ModernWpf.Controls
Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json
Imports ICSharpCode.SharpZipLib.Zip
Imports ICSharpCode.SharpZipLib.Core
Imports Downloader
Imports IniParser
Imports IniParser.Model
Imports System.Reflection

Class list
    Dim del_dir
    Dim local_id, url_online, year_ol, month_ol, day_ol, year_l, month_l, day_l, upd_dir '定义更新部分使用变量
    Dim parser = New FileIniDataParser()

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
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
        Dim choose_dir
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
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
                    Dim ol_ok
                    ol_ok = 0
                    '发送get请求，请求版本表
                    Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
                    request.Method = "GET"
                    Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
                    Dim jsonback = sr.ReadToEnd '储存返回的json信息
                    '处理json
                    Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
                    For Each x In json("group")
                        mclist.Dispatcher.Invoke(New Action(Sub()
                                                                For Each x2 In json("group")
                                                                    Dim json3 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                                    For Each x3 In json3("list")
                                                                        Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x3.ToString), JObject)
                                                                        If json2("id").ToString > 10000 Then
                                                                            If json2("id").ToString < 20000 Then
                                                                                If json2("id").ToString = Int(info_xml.<packid>.Value) Then
                                                                                    ol_ok = 1
                                                                                    year_ol = json2("update_time")("year").ToString
                                                                                    month_ol = json2("update_time")("month").ToString
                                                                                    day_ol = json2("update_time")("date").ToString
                                                                                    url_online = json2("download")
                                                                                    his_card.Subtitle = json2("history")
                                                                                    Exit For
                                                                                End If
                                                                            End If
                                                                        End If
                                                                    Next


                                                                Next
                                                            End Sub))
                    Next
                    If ol_ok = 1 Then
                        '比对本地与服务器日期
                        Dim update_yes = 0
                        update_yes = 0
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
                                                                        .Content = "检测到选中的整合包有新的版本！" & vbCrLf & "建议前往版本列表页面更新整合包~" & vbCrLf & "如果数据重要，建议先备份哦~"
                                                                    }
                                                                    dialog.ShowAsync()

                                                                End Sub))

                        End If
                    Else
                        mclist.Dispatcher.Invoke(New Action(Sub()
                                                                verinfolabel.Content += vbCrLf & "该版本整合包仅限于本地！"

                                                            End Sub))
                    End If

                End If

            End If
        Next
    End Sub

    Private Sub bt_opdir_Click(sender As Object, e As RoutedEventArgs) Handles bt_opdir.Click
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    Dim p As New Process()
                    p.StartInfo.FileName = "cmd.exe"
                    p.StartInfo.UseShellExecute = False
                    p.StartInfo.RedirectStandardInput = True
                    p.StartInfo.RedirectStandardOutput = True
                    p.StartInfo.RedirectStandardError = True
                    p.StartInfo.CreateNoWindow = True
                    p.Start()
                    p.StandardInput.WriteLine("explorer " & dirlist) '这个Data就是cmd命令
                    p.StandardInput.WriteLine("exit") '这个Data就是cmd命令
                    p.WaitForExit()
                    Exit For
                End If

            End If
        Next
    End Sub

    Private Async Sub bt_delpack_ClickAsync(sender As Object, e As RoutedEventArgs) Handles bt_delpack.Click
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                    Dim dialog As ContentDialog = New ContentDialog() With {
                                                        .Title = "确山删除整合包？",
                                                        .CloseButtonText = "取消",
                                                        .PrimaryButtonText = "确认删除",
                                                        .DefaultButton = ContentDialogButton.Primary,
                                                        .Content = "即将删除整合包：" & info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" & vbCrLf &
                                                        "将删除整合包内所有数据，该操作不可逆1" & vbCrLf & "是否继续删除？"
                                                    }
                    Dim result As ContentDialogResult = Await dialog.ShowAsync()
                    If result = ContentDialogResult.Primary Then
                        Dim delpack As Thread = New Thread(AddressOf Delpackthread)
                        delpack.Start()
                        del_dir = dirlist
                    End If
                    Exit For
                End If

            End If
        Next
    End Sub

    Private Sub Delpackthread(ByVal objParamReport As Object)
        Try
            Directory.Delete(del_dir, True)
            mclist.Dispatcher.Invoke(New Action(Sub()


                                                    Dim dialog As ContentDialog = New ContentDialog() With {
                .Title = "成功",
                .CloseButtonText = "我知道了",
                .IsPrimaryButtonEnabled = False,
                .DefaultButton = ContentDialogButton.Close,
                .Content = "成功删除整合包！"
            }
                                                    dialog.ShowAsync()
                                                End Sub))
            '刷新列表
            Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
            homepagestart.Start()
        Catch ex As Exception
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    Dim dialog As ContentDialog = New ContentDialog() With {
               .Title = "失败",
               .CloseButtonText = "我知道了",
               .IsPrimaryButtonEnabled = False,
               .DefaultButton = ContentDialogButton.Close,
               .Content = "删除整合包不成功或不完全！" & vbCrLf & "请尝试手动删除！"
           }
                                                    dialog.ShowAsync()
                                                End Sub))
            '刷新列表
            Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
            homepagestart.Start()
        End Try

    End Sub

    Private Sub bt_upver_Click(sender As Object, e As RoutedEventArgs) Handles bt_upver.Click
        If upd_card.Visibility = Visibility.Collapsed Then
            upd_card.Visibility = Visibility.Visible
            Dim list As Thread = New Thread(AddressOf Listver)
            list.Start()
        Else
            upd_card.Visibility = Visibility.Collapsed
        End If
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
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    choose_ver.Items.Clear()
                                                    For Each x2 In json("group")
                                                        Dim json3 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                        For Each x3 In json3("list")
                                                            Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x3.ToString), JObject)
                                                            If json2("id").ToString > 10000 Then
                                                                If json2("id").ToString < 20000 Then
                                                                    choose_ver.Items.Add(json2("version"))
                                                                End If
                                                            End If
                                                        Next


                                                    Next
                                                    choose_ver.SelectedIndex = 0

                                                End Sub))
        Next
    End Function

    Private Sub bt_upd_Click(sender As Object, e As RoutedEventArgs) Handles bt_upd.Click
        Dim upd As Thread = New Thread(AddressOf Upd_ver)
        upd.Start()
    End Sub
    Private Function Upd_ver(ByVal objParamReport As Object) As String

        '第一步，比对本地与服务器的日期，检查更新

        '获取本地整合包ID
        For Each dirlist In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\")
            If File.Exists(dirlist & "\info.xml") Then
                '检测到文件夹内为符合格式的整合包，查找ID
                Dim info_reader As String = File.ReadAllText(dirlist & "\info.xml")
                Dim info_xml As XElement = XElement.Parse(info_reader)
                mclist.Dispatcher.Invoke(New Action(Sub()
                                                        If mclist.SelectedItem = info_xml.<mcversion>.Value & " - " & info_xml.<modloader>.Value & "(" & info_xml.<sharder>.Value & ")" Then
                                                            local_id = info_xml.<packid>.Value
                                                            year_l = info_xml.<packdate>.<year>.Value.ToString
                                                            month_l = info_xml.<packdate>.<month>.Value.ToString
                                                            day_l = info_xml.<packdate>.<day>.Value.ToString
                                                            upd_dir = dirlist
                                                        End If
                                                    End Sub))

            End If
        Next

        '从资源服务器获取该版本信息
        Dim ol_ok
        ol_ok = 0
        '发送get请求，请求版本表
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/packlist.json")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim jsonback = sr.ReadToEnd '储存返回的json信息
        '处理json
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonback), JObject)
        For Each x In json("group")
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    Dim json3 As JObject = CType(JsonConvert.DeserializeObject(x.ToString), JObject)
                                                    For Each x2 In json3("list")
                                                        Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x2.ToString), JObject)
                                                        If json2("id").ToString > 10000 Then
                                                            If json2("id").ToString < 20000 Then
                                                                If json2("id").ToString = local_id Then
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
            update_yes = 0
            If Int(year_ol) > Int(year_l) Then
                update_yes = 1
            Else
                If Int(month_ol) > Int(month_l) Then
                    update_yes = 1
                Else
                    If Int(month_ol) = Int(month_l) And Int(day_ol) > Int(day_l) Then
                        update_yes = 1
                    Else
                        update_yes = 0
                    End If
                End If
            End If
            If update_yes = 1 Then
                '检测到更新，弹窗确认
                mclist.Dispatcher.Invoke(New Action(Async Sub()


                                                        Dim dialog As ContentDialog = New ContentDialog() With {
                                                            .Title = "确认更新整合包？",
                                                            .CloseButtonText = "取消",
                                                            .PrimaryButtonText = "开始更新",
                                                            .DefaultButton = ContentDialogButton.Primary,
                                                            .Content = "我们即将为您更新整合包" & vbCrLf & "更新后您的数据会被保留" & vbCrLf & "如果数据重要，建议先备份哦~"
                                                        }
                                                        Dim result As ContentDialogResult = Await dialog.ShowAsync()
                                                        If result = ContentDialogResult.Primary Then
                                                            '执行更新
                                                            Dim upd As Thread = New Thread(AddressOf Upd_ver_mainAsync)
                                                            upd.Start()
                                                        End If
                                                    End Sub))
            Else

                '没有更新，弹窗提示
                mclist.Dispatcher.Invoke(New Action(Sub()
                                                        Dim dialog As ContentDialog = New ContentDialog() With {
                                                                                                                .Title = "没有更新",
                                                                                                                .CloseButtonText = "我知道啦",
                                                                                                                .DefaultButton = ContentDialogButton.Close,
                                                                                                                .Content = "当前选中版本没有新版本哦~"
                                                                                                            }
                                                        dialog.ShowAsync()
                                                    End Sub))
            End If
        Else
            '不存在，弹窗提示
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    Dim dialog As ContentDialog = New ContentDialog() With {
                                                                                                            .Title = "资源服务器无此整合包",
                                                                                                            .CloseButtonText = "我知道啦",
                                                                                                            .DefaultButton = ContentDialogButton.Close,
                                                                                                            .Content = "没有在服务器上搜索到相应ID的整合包哦~"
                                                                                                        }
                                                    dialog.ShowAsync()
                                                End Sub))
        End If

    End Function
    '更新线程
    Private Async Function Upd_ver_mainAsync(ByVal objParamReport As Object) As Task(Of String)
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                dw_card.Visibility = Visibility.Visible
                                                dw_pro.Visibility = Visibility.Visible
                                            End Sub))
        Try
            Directory.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd", True)
        Catch ex As Exception

        End Try
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12

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
            Await downloader.DownloadFileTaskAsync(url_online, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\update_download.zip")

            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "下载完成"
                                                     dw_pro.Visibility = Visibility.Collapsed
                                                 End Sub))

            '下载完成，进入解压处理
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "正在进行解压···"
                                                 End Sub))
            Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd")
            Dim zf As ZipFile = Nothing
            Try
                Dim fs As FileStream = File.OpenRead(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\update_download.zip")
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
                    Dim fullZipToPath As [String] = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd", entryFileName)
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
                    '解压完成
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_info.Text = "正在删除旧版整合包资源···"
                                                         End Sub))
                    Try
                        '——————删掉旧的——————
                        If Directory.Exists(upd_dir & "\.minecraft\mods\") Then
                            For Each d As String In Directory.GetFileSystemEntries(upd_dir & "\.minecraft\mods\")
                                If File.Exists(d) Then
                                    Dim [me] As String = Path.GetFileNameWithoutExtension(d)
                                    If [me].StartsWith("[##模组##]") Then
                                        File.Delete(d)
                                    End If
                                End If
                            Next
                        End If

                        If Directory.Exists(upd_dir & "\.minecraft\shaderpacks\") Then
                            For Each d As String In Directory.GetFileSystemEntries(upd_dir & "\.minecraft\shaderpacks\")
                                If File.Exists(d) Then
                                    Dim [me] As String = Path.GetFileNameWithoutExtension(d)
                                    If [me].StartsWith("[##光影##]") Then
                                        File.Delete(d)
                                    End If
                                End If
                            Next
                        End If

                        If Directory.Exists(upd_dir & "\.minecraft\resourcepacks\") Then
                            For Each d As String In Directory.GetFileSystemEntries(upd_dir & "\.minecraft\resourcepacks\")
                                If File.Exists(d) Then
                                    Dim [me] As String = Path.GetFileNameWithoutExtension(d)
                                    If [me].StartsWith("[##资源包##]") Then
                                        File.Delete(d)
                                    End If
                                End If
                            Next
                        End If

                        dw_info.Dispatcher.Invoke(New Action(Sub()
                                                                 dw_info.Text = "正在更新整合包资源···"
                                                             End Sub))
                        '——————删掉旧的——————
                        '复制文件
                        For Each Dir_list In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd\file\")
                            If Directory.Exists(upd_dir & "\.minecraft\mods\") Then
                                '确保文件夹存在
                                Directory.CreateDirectory(Dir_list & "\.minecraft\mods\")
                                For Each file_list In System.IO.Directory.GetFiles(Dir_list & "\.minecraft\mods\")
                                    Dim file_name_org
                                    file_name_org = Replace(file_list, Dir_list & "\.minecraft\mods\", "")
                                    File.Copy(file_list, upd_dir & "\.minecraft\mods\" & file_name_org, True)
                                Next
                            End If

                        Next

                        For Each Dir_list In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd\file\")
                            If Directory.Exists(upd_dir & "\.minecraft\shaderpacks\") Then
                                Directory.CreateDirectory(Dir_list & "\.minecraft\shaderpacks\")
                                For Each file_list In System.IO.Directory.GetFiles(Dir_list & "\.minecraft\shaderpacks\")
                                    Dim file_name_org
                                    file_name_org = Replace(file_list, Dir_list & "\.minecraft\shaderpacks\", "")
                                    File.Copy(file_list, upd_dir & "\.minecraft\shaderpacks\" & file_name_org, True)
                                Next
                            End If

                        Next

                        For Each Dir_list In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd\file\")
                            If Directory.Exists(Dir_list & "\.minecraft\resourcepacks\") Then
                                Directory.CreateDirectory(upd_dir & "\.minecraft\resourcepacks\")
                                For Each file_list In System.IO.Directory.GetFiles(Dir_list & "\.minecraft\resourcepacks\")
                                    Dim file_name_org
                                    file_name_org = Replace(file_list, Dir_list & "\.minecraft\resourcepacks\", "")
                                    File.Copy(file_list, upd_dir & "\.minecraft\resourcepacks\" & file_name_org, True)
                                Next
                            End If

                        Next

                        For Each Dir_list In System.IO.Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd\file\")
                            File.Copy(Dir_list & "\info.xml", upd_dir & "\info.xml", True)
                            Directory.Delete(upd_dir & "\.minecraft\versions\", True)
                            Directory.Move(Dir_list & "\.minecraft\versions\", upd_dir & "\.minecraft\versions\")
                        Next

                    Catch ex As Exception

                    End Try
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_info.Text = "资源更新完成，正在删除多余的文件···"
                                                         End Sub))
                    Directory.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\unzip_dir_nstarmctoolsupd", True)
                    File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\file\update_download.zip")
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_card.Visibility = Visibility.Collapsed
                                                             Dim dialog As ContentDialog = New ContentDialog() With {
                                                                                                            .Title = "更新完成",
                                                                                                            .CloseButtonText = "我知道啦",
                                                                                                            .DefaultButton = ContentDialogButton.Close,
                                                                                                            .Content = "您选择的整合包已更新到最新版本~"
                                                                                                        }
                                                             dialog.ShowAsync()
                                                         End Sub))

                End If
            End Try
        Catch ex As Exception

        End Try
    End Function

    Private Sub OnDownloadProgressChanged(sender As Object, e As Downloader.DownloadProgressChangedEventArgs)
        dw_info.Dispatcher.Invoke(New Action(Sub()
                                                 dw_info.Text = "文件大小：" & Math.Round(e.TotalBytesToReceive / 1024 / 1024, 2) & "MB/" &
                                                 Math.Round(e.ReceivedBytesSize / 1024 / 1024, 2) & "MB" &
                                                 " 当前速度：" & Math.Round(e.BytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S"
                                                 dw_pro.Value = Math.Round(e.ProgressPercentage, 1)
                                             End Sub))
    End Sub
End Class
