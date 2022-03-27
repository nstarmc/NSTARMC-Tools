Imports System.IO
Imports System.Net
Imports System.Threading
Imports IniParser
Imports IniParser.Model
Imports ModernWpf.Controls

Class about
    Dim parser = New FileIniDataParser()

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        If Not System.IO.File.Exists(My.Application.Info.DirectoryPath & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\NSTARMC-Tools\")
            System.IO.File.Create(My.Application.Info.DirectoryPath & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim pagestart As Thread = New Thread(AddressOf pagestartthread)
        pagestart.Start()

    End Sub

    Private Sub pagestartthread()

        Dim data As IniData = parser.ReadFile(My.Application.Info.DirectoryPath & "\NSTARMC-Tools\Configuration.ini")
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
            parser.WriteFile(My.Application.Info.DirectoryPath & "\NSTARMC-Tools\Configuration.ini", data)
        End If
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 '获取版本信息
                                                 verinfo.Subtitle = "工具版本：" & My.Application.Info.Version.ToString & vbCrLf & "版本状态：" & My.Application.Info.Description & vbCrLf & "运行路径：" & My.Application.Info.DirectoryPath & vbCrLf & "GUID:" & data("Tools")("GUID")
                                             End Sub))

        '加密测试模块
        Dim currentUser As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim sid As String = currentUser.User.ToString()
        Dim dataToHash As Byte() = (New System.Text.ASCIIEncoding).GetBytes(sid)
        Dim hashvalue As Byte() = CType(System.Security.Cryptography.CryptoConfig.CreateFromName("MD5"), System.Security.Cryptography.HashAlgorithm).ComputeHash(dataToHash)
        Dim ATR As String = ""
        Dim i As Integer
        '选择32位字符的加密结果   
        For i = 0 To 15
            ATR &= Hex(hashvalue(i)).PadLeft(2, "0").ToLower
        Next
        verinfo.Dispatcher.Invoke(New Action(Sub()
                                                 text_sid.Text = ATR.ToUpper
                                                 text_guid.Text = data("Tools")("GUID")
                                             End Sub))
    End Sub

    Private Sub bt_copy_Click(sender As Object, e As RoutedEventArgs) Handles bt_copy.Click
        Dim data As IniData = parser.ReadFile(My.Application.Info.DirectoryPath & "\NSTARMC-Tools\Configuration.ini")
        Clipboard.Clear() ' 清除剪贴板
        Clipboard.SetText("工具版本：" & My.Application.Info.Version.ToString & vbCrLf & "版本状态：" & My.Application.Info.Description & vbCrLf & "运行路径：" & My.Application.Info.DirectoryPath & vbCrLf & "GUID:" & Data("Tools")("GUID")) ' 拷贝数据到粘贴板
        Dim dialog As ContentDialog = New ContentDialog() With {
                                           .Title = "复制成功",
                                           .CloseButtonText = "我知道了",
                                           .IsPrimaryButtonEnabled = False,
                                           .DefaultButton = ContentDialogButton.Close,
                                           .Content = "工具信息复制成功！"
                                       }
        dialog.ShowAsync()
    End Sub

    Private Sub button_key_Click(sender As Object, e As RoutedEventArgs) Handles button_key.Click
        Dim key
        key = text_sid.Text.ToLower & text_guid.Text & text_pwd.Text & "nstarmctools"
        Dim currentUser As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim sid As String = currentUser.User.ToString()
        Dim dataToHash As Byte() = (New System.Text.ASCIIEncoding).GetBytes(key)
        Dim hashvalue As Byte() = CType(System.Security.Cryptography.CryptoConfig.CreateFromName("MD5"), System.Security.Cryptography.HashAlgorithm).ComputeHash(dataToHash)
        Dim ATR As String = ""
        Dim i As Integer
        '选择32位字符的加密结果   
        For i = 0 To 15
            ATR &= Hex(hashvalue(i)).PadLeft(2, "0").ToLower
        Next
        text_key.Text = ATR.ToUpper
    End Sub
End Class
