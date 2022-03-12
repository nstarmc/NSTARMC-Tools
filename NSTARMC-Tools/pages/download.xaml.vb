Imports System.IO
Imports System.Net
Imports System.Threading
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports ModernWpf.Controls
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Class download
    Dim down_url, packid, down_ver
    Dim allow_download = 0

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
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
                                                           downlod_group.Items.Add(json("group")(0)("name"))
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
                                                           If json("group")(0)("name") = downlod_group.SelectedItem Then
                                                               list_ver.Items.Clear()
                                                               For Each x2 In json("group")(0)("list")
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
                                                           If json("group")(0)("name") = downlod_group.SelectedItem Then
                                                               For Each x2 In json("group")(0)("list")
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
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
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
                                                       End Sub))
        Else

            Dim dw_thr2 As Thread = New Thread(AddressOf Dwfile_thread)
            dw_thr2.Start()

        End If
    End Function

    Private Function Dwfile_thread(ByVal objParamReport As Object) As String
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 Or SecurityProtocolType.Tls Or SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12
        Try

            '下载
            Dim hwq As HttpWebRequest
            Dim hwp As HttpWebResponse
            Dim colHeader As WebHeaderCollection  '响应头信息集合
            Dim lngSize As Int64                  '要下载文件的总大小
            Dim lngCurSize As Int64               '已经下载大小
            Dim lngNet As Int64                   '计算网速用

            Dim stRespones As Stream              '响应流
            Dim st As FileStream                  '本地流
            Dim intCurSize As Int64
            Dim bytBuffer(512) As Byte           '缓存大小

            Dim datLast As DateTime               '最后一次时间
            Dim intDiff As Int32                  '两次时间差（秒）

            datLast = Now   '取得开始时间
            hwq = CType(HttpWebRequest.Create(down_url.ToString), HttpWebRequest) '请求对象创建
            hwp = hwq.GetResponse        '取得响应对象
            colHeader = hwp.Headers      '取得响应头
            lngSize = colHeader.Get("Content-Length")  '取得要下载文件的大小

            stRespones = hwp.GetResponseStream '取得响应流
            st = New FileStream(My.Application.Info.DirectoryPath & "\file\download.zip", FileMode.Create) '本地保存文件

            intCurSize = stRespones.Read(bytBuffer, 0, bytBuffer.Length) '响应流中读取

            Do While (intCurSize > 0) '只要有数据就继续
                st.Write(bytBuffer, 0, intCurSize)     '写入本地文件
                intDiff = DateDiff(DateInterval.Second, datLast, Now)


                lngCurSize = lngCurSize + intCurSize
                lngNet = lngNet + intCurSize              '单位时间内的下载量
                If intDiff >= 1 Then
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_info.Text =
                                                         "文件大小：" & FormatNumber(lngSize / 1024 / 1024, 2, vbTrue).ToString & " MB" &
                                                         "/" & FormatNumber(lngCurSize / 1024 / 1024, 2, vbTrue).ToString & " MB" & vbCrLf &
                                                         "当前速度：" & Math.Round(lngNet / intDiff / 1024 / 1024, 2).ToString & "MB/s"
                                                             dw_pro.IsEnabled = True
                                                         End Sub))
                    datLast = Now
                    lngNet = 0
                End If
                intCurSize = stRespones.Read(bytBuffer, 0, bytBuffer.Length) '继续读取
            Loop
            st.Close()
            stRespones.Close()
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "下载完成"
                                                 End Sub))
            lngSize = 0
            lngCurSize = 0
            lngNet = 0
            intDiff = 0
            lngCurSize = 0

            '解压进程
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "正在进行解压···"
                                                 End Sub))
            Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\file\unzip_dir_nstarmctools")
            Dim zf As ZipFile = Nothing
            Try
                Dim fs As FileStream = File.OpenRead(My.Application.Info.DirectoryPath & "\file\download.zip")
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
                    Dim fullZipToPath As [String] = Path.Combine(My.Application.Info.DirectoryPath & "\file\unzip_dir_nstarmctools", entryFileName)
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
                    For Each Dirfile In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\unzip_dir_nstarmctools\file\")
                        Directory.Move(Dirfile, My.Application.Info.DirectoryPath & "\file\" & down_ver & "_" & Now.Year & Now.Month & Now.Day & "\")
                    Next
                    Directory.Delete(My.Application.Info.DirectoryPath & "\file\unzip_dir_nstarmctools\", True)
                    File.Delete(My.Application.Info.DirectoryPath & "\file\download.zip")
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
                                                         End Sub))
                End If
            End Try
        Catch ex As Exception
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "错误信息:" + ex.ToString
                                                 End Sub))
        End Try
    End Function
End Class
