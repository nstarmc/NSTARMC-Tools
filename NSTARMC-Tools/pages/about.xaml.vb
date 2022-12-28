Imports System.IO
Imports System.Net
Imports System.Threading
Imports IniParser
Imports IniParser.Model
Imports ModernWpf.Controls
Imports Afdian.Sdk
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json
Imports System.Text.RegularExpressions
Imports System.Net.NetworkInformation
Imports System.Text
Imports System.Reflection
Imports GuerrillaNtp

Class about
    Dim parser = New FileIniDataParser()

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        '加密模块-获取设备ID
        Dim currentUser As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim sid As String = currentUser.User.ToString() & "NSTARMCToolsSID"
        Dim dataToHash As Byte() = (New System.Text.ASCIIEncoding).GetBytes(sid)
        Dim hashvalue As Byte() = CType(System.Security.Cryptography.CryptoConfig.CreateFromName("MD5"), System.Security.Cryptography.HashAlgorithm).ComputeHash(dataToHash)
        Dim ATR As String = ""
        Dim i As Integer
        '选择32位字符的加密结果   
        For i = 0 To 15
            ATR &= Hex(hashvalue(i)).PadLeft(2, "0").ToLower
        Next
        sb_id.Text = ATR.ToUpper
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim pagestart As Thread = New Thread(AddressOf pagestartthread)
        pagestart.Start()

    End Sub

    Private Sub pagestartthread()

        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        '发送get请求，获取xml
        Dim request As HttpWebRequest = WebRequest.Create("https://res.nstarmc.cn/update.xml")
        request.Method = "GET"
        Dim sr As StreamReader = New StreamReader(request.GetResponse().GetResponseStream)
        Dim upd As XElement = XElement.Parse(sr.ReadToEnd)
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 uphist.Text = upd.<history>.<text>.Value & vbCrLf & "最后更新日期：" & upd.<history>.<lastdate>.Value
                                                 upfu.Text = upd.<future>.<text>.Value & vbCrLf & "最后更新日期：" & upd.<future>.<lastdate>.Value
                                             End Sub))

        If data("Tools")("GUID") = "" Then
            Dim sGUID As String
            sGUID = System.Guid.NewGuid.ToString()
            data("Tools")("GUID") = Guid.NewGuid.ToString()
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
        End If
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 '获取版本信息
                                                 verinfo.Subtitle = "工具版本：" & Assembly.GetExecutingAssembly().GetName().Version.ToString() & vbCrLf & "版本状态：" & "" & vbCrLf & "运行路径：" & Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & vbCrLf & "GUID:" & data("Tools")("GUID")

                                                 If MainWindow.shouquan = True Then
                                                     Dim spinfo As Sponsor_info
                                                     spinfo = Sponsor_proof(data("Keys")("key"))
                                                     ver_beta.Subtitle = "授权状态：已授权" & vbCrLf & "授权用户：" & spinfo.username & vbCrLf & "到期时间：" & spinfo.outdate & vbCrLf & "授权邮箱：" & spinfo.email & vbCrLf & "用户唯一设备KEY：" & spinfo.key & vbCrLf & "感谢您对我们的支持！"
                                                     bt_shouquan.IsEnabled = False
                                                     bt_shouquan.Content = "已授权！"
                                                 Else
                                                     ver_beta.Subtitle = "授权状态：未授权" & vbCrLf & "赞助后才可以体验Beta版本噢~"
                                                 End If
                                             End Sub))

        ''加密测试模块
        'Dim currentUser As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        'Dim sid As String = currentUser.User.ToString() & "NSTARMCToolsSID"
        'Dim dataToHash As Byte() = (New System.Text.ASCIIEncoding).GetBytes(sid)
        'Dim hashvalue As Byte() = CType(System.Security.Cryptography.CryptoConfig.CreateFromName("MD5"), System.Security.Cryptography.HashAlgorithm).ComputeHash(dataToHash)
        'Dim ATR As String = ""
        'Dim i As Integer
        ''选择32位字符的加密结果   
        'For i = 0 To 15
        '    ATR &= Hex(hashvalue(i)).PadLeft(2, "0").ToLower
        'Next
        'verinfo.Dispatcher.Invoke(New Action(Sub()
        '                                         text_sid.Text = ATR.ToUpper
        '                                         text_guid.Text = data("Tools")("GUID")
        '                                     End Sub))

        '获取赞助列表
        Dim jsonStr1 As String = Aifadian.aifadian
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 zanzhulist.Text = ""
                                             End Sub))
        Dim json As JObject = CType(JsonConvert.DeserializeObject(jsonStr1), JObject)
        If json("ec") = "200" Then


            For Each x In json("data")("list")
                Dim json2 As JObject = CType(JsonConvert.DeserializeObject(x.ToString), JObject)
                Dim zanzhuname As String

                zanzhuname = json2("user")("name").ToString & " （赞助金额：" & json2("all_sum_amount").ToString & "元）"
                verinfo.Dispatcher.Invoke(New Action(Sub()
                                                         If zanzhulist.Text = "" Then
                                                             zanzhulist.Text = zanzhuname
                                                         Else
                                                             zanzhulist.Text = zanzhulist.Text & vbCrLf & zanzhuname
                                                         End If


                                                     End Sub))
            Next
        Else
            verinfo.Dispatcher.Invoke(New Action(Sub()
                                                     zanzhulist.Text = "获取失败！"
                                                 End Sub))
        End If

        '测试延迟
        Dim pingSender As Ping = New Ping()
        Dim options As PingOptions = New PingOptions()
        options.DontFragment = True
        Dim data_ping As String = "ping"
        Dim buffer As Byte() = Encoding.ASCII.GetBytes(data_ping)
        Dim timeout As Integer = 1024
        Dim reply As PingReply = pingSender.Send("res.nstarmc.cn", timeout, buffer, options)

        If reply.Status = IPStatus.Success Then
            verinfo.Dispatcher.Invoke(New Action(Sub()
                                                     ping_card.Subtitle = ""
                                                     ping_card.Subtitle += "节点IP：" & reply.Address.ToString()
                                                     ping_card.Subtitle += vbCrLf & "延迟：" & reply.RoundtripTime & "ms"
                                                 End Sub))
        End If
    End Sub

    Private Sub bt_copy_Click(sender As Object, e As RoutedEventArgs) Handles bt_copy.Click
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        Clipboard.Clear() ' 清除剪贴板
        Clipboard.SetText("工具版本：" & Assembly.GetExecutingAssembly().GetName().Version.ToString() & vbCrLf & "版本状态：" & "" & vbCrLf & "运行路径：" & Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & vbCrLf & "GUID:" & data("Tools")("GUID")) ' 拷贝数据到粘贴板
        Dim dialog As ContentDialog = New ContentDialog() With {
                                           .Title = "复制成功",
                                           .CloseButtonText = "我知道了",
                                           .IsPrimaryButtonEnabled = False,
                                           .DefaultButton = ContentDialogButton.Close,
                                           .Content = "工具信息复制成功！"
                                       }
        dialog.ShowAsync()
    End Sub



    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Clipboard.Clear() ' 清除剪贴板
        Clipboard.SetText(sb_id.Text) ' 拷贝数据到粘贴板
        Dim dialog As ContentDialog = New ContentDialog() With {
                                           .Title = "复制成功",
                                           .CloseButtonText = "我知道了",
                                           .IsPrimaryButtonEnabled = False,
                                           .DefaultButton = ContentDialogButton.Close,
                                           .Content = "您的设备ID已成功复制！"
                                       }
        dialog.ShowAsync()
    End Sub

    Private Async Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        Dim spinfo As Sponsor_info
        spinfo = Sponsor_proof(key_input.Text)

        '写入
        data("Keys")("key") = key_input.Text
        data("Keys")("Year") = spinfo.outdate.Year
        data("Keys")("Month") = spinfo.outdate.Month
        data("Keys")("Day") = spinfo.outdate.Day
        parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
        MsgBox(spinfo.success)
        If spinfo.attachid = sb_id.Text Then
            '校验是否过期
            Dim client As NtpClient = New NtpClient("ntp.aliyun.com")
            Dim clock As NtpClock = client.Query()
            Dim utc As DateTime = clock.UtcNow.UtcDateTime
            'Dim getWebDatetime As Date
            'Dim XmlHttp As Object
            'XmlHttp = CreateObject("Microsoft.XMLHTTP")
            'XmlHttp.open("POST", "https://www.baidu.com/", False)
            'XmlHttp.send()
            'getWebDatetime = (CDate(Date.FromOADate(1 / 3 + CDbl(CDate(Mid$(XmlHttp.getResponseHeader("Date"), 5, 21)).ToOADate()))))
            If spinfo.outdate.Date > utc Then
                '通过
                MainWindow.shouquan = True
                bt_shouquan.IsEnabled = False
                bt_shouquan.Content = "已授权！"
                ver_beta.Subtitle = "授权状态：已授权" & vbCrLf & "授权用户：" & spinfo.username & vbCrLf & "到期时间：" & spinfo.outdate & vbCrLf & "授权邮箱：" & spinfo.email & vbCrLf & "用户唯一设备KEY：" & spinfo.key & vbCrLf & "感谢您对我们的支持！"
                info_shouquan.Visibility = Visibility.Collapsed
                Dim dialog As ContentDialog = New ContentDialog() With {
                                   .Title = "授权码检验成功！",
                                   .CloseButtonText = "我知道了",
                                   .IsPrimaryButtonEnabled = False,
                                   .DefaultButton = ContentDialogButton.Close,
                                   .Content = "授权码检验成功！" & vbCrLf & "你可以使用NSTARMC-Tools的Beta版本啦！"
                               }
                Await dialog.ShowAsync()


            Else

                '不通过
                Dim dialog As ContentDialog = New ContentDialog() With {
           .Title = "授权码检验失败！",
           .CloseButtonText = "我知道了",
           .IsPrimaryButtonEnabled = False,
           .DefaultButton = ContentDialogButton.Close,
           .Content = "授权码检验失败！" & vbCrLf & "该授权码已经到期！"
       }
                Await dialog.ShowAsync()
            End If

        Else
            Dim dialog As ContentDialog = New ContentDialog() With {
                                   .Title = "授权码检验失败！",
                                   .CloseButtonText = "我知道了",
                                   .IsPrimaryButtonEnabled = False,
                                   .DefaultButton = ContentDialogButton.Close,
                                   .Content = "授权码检验失败！" & vbCrLf & "请检查授权码和有效日期是否与收到的一致！" & vbCrLf & "授权码建议复制粘贴！"
                               }
            Await dialog.ShowAsync()
        End If
    End Sub

    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
        If info_shouquan.Visibility = Visibility.Collapsed Then
            info_shouquan.Visibility = Visibility.Visible
        Else
            info_shouquan.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub key_day_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles key_day.PreviewTextInput
        Dim re As Regex = New Regex("[^0-9]+")
        e.Handled = re.IsMatch(e.Text)
    End Sub

    Private Sub key_day_TextChanged(sender As Object, e As TextChangedEventArgs) Handles key_day.TextChanged
        If key_day.Text = "" Then
            key_day.Text = "1"
        End If
    End Sub

    Private Sub bt_ping_Click(sender As Object, e As RoutedEventArgs) Handles bt_ping.Click
        bt_ping.IsEnabled = False
        Dim pingSender As Ping = New Ping()
        Dim options As PingOptions = New PingOptions()
        options.DontFragment = True
        Dim data As String = "ping"
        Dim buffer As Byte() = Encoding.ASCII.GetBytes(data)
        Dim timeout As Integer = 1024
        Dim reply As PingReply = pingSender.Send("res.nstarmc.cn", timeout, buffer, options)

        If reply.Status = IPStatus.Success Then
            ping_card.Subtitle = ""
            ping_card.Subtitle += "节点IP：" & reply.Address.ToString()
            ping_card.Subtitle += vbCrLf & "延迟：" & reply.RoundtripTime & "ms"
            bt_ping.IsEnabled = True
        Else

            bt_ping.IsEnabled = True
        End If
    End Sub
End Class
