Imports System.IO
Imports System.Net
Imports System.Threading
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports ModernWpf.Controls

Class java
    Dim java_ver
    Dim DWFilename, DWUrl, DWFn, DWS, DWDir, DWDir_up '定义下载线程调用变量
    Private Sub Page_Loaded_1(sender As Object, e As RoutedEventArgs)
        Dim homepagestart As Thread = New Thread(AddressOf Homepagestartthread)
        homepagestart.Start()
    End Sub
    Private Function Homepagestartthread(ByVal objParamReport As Object) As String
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
        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
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

        For Each dirlist In System.IO.Directory.GetDirectories(My.Application.Info.DirectoryPath & "\file\")
            If My.Computer.FileSystem.FileExists(dirlist & "\info.xml") Then

                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = My.Computer.FileSystem.ReadAllText(dirlist & "\info.xml")
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
                    Dim dw_java As Thread = New Thread(AddressOf Dw_java_thread)
                    dw_java.Start()
                    card_dwinfo.Visibility = Visibility.Visible
                    Exit For
                End If

            End If
        Next
    End Sub

    Private Function Dw_java_thread(ByVal objParamReport As Object) As String
        If Directory.Exists(DWDir & "\jre-x64") = True Then
            Directory.Delete(DWDir & "\jre-x64", True)
        End If
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
            hwq = CType(HttpWebRequest.Create(DWUrl), HttpWebRequest) '请求对象创建
            hwp = hwq.GetResponse        '取得响应对象
            colHeader = hwp.Headers      '取得响应头
            lngSize = colHeader.Get("Content-Length")  '取得要下载文件的大小

            stRespones = hwp.GetResponseStream '取得响应流
            st = New FileStream(DWFilename, FileMode.Create) '本地保存文件

            intCurSize = stRespones.Read(bytBuffer, 0, bytBuffer.Length) '响应流中读取

            Do While (intCurSize > 0) '只要有数据就继续
                st.Write(bytBuffer, 0, intCurSize)     '写入本地文件
                intDiff = DateDiff(DateInterval.Second, datLast, Now)


                lngCurSize = lngCurSize + intCurSize
                lngNet = lngNet + intCurSize              '单位时间内的下载量
                If intDiff >= 1 Then
                    dw_info.Dispatcher.Invoke(New Action(Sub()
                                                             dw_info.Text = "正在下载：" & DWFn & vbCrLf &
                                                             "文件大小：" & FormatNumber(lngSize / 1024 / 1024, 2, vbTrue).ToString & " MB" &
                                                             "/" & FormatNumber(lngCurSize / 1024 / 1024, 2, vbTrue).ToString & " MB" & vbCrLf &
                                                             "当前速度：" & CInt(lngNet / intDiff / 1024 / 1024).ToString & "MB/s" & vbCrLf &
                                                             "注意：在文件下载时，请不要切换其它选项卡，否则会导致一系列问题！"
                                                             dw_progressbar.Value = Math.Round(lngCurSize / lngSize * 100, 2)
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
                                                     dw_progressbar.Value = 100
                                                 End Sub))
            lngSize = 0
            lngCurSize = 0
            lngNet = 0
            intDiff = 0
            lngCurSize = 0
            DWS = 1
            '启动解压缩线程
            Dim unzip_java As Thread = New Thread(AddressOf Unzip_java_thread)
            unzip_java.Start()
        Catch ex As Exception
            dw_info.Dispatcher.Invoke(New Action(Sub()
                                                     dw_info.Text = "错误信息:" + ex.Message
                                                 End Sub))
        End Try

    End Function
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
                                                        End Sub))
                End If
        End Try
    End Function
End Class
