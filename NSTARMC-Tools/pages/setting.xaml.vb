Imports System.Text.RegularExpressions
Imports IniParser
Imports IniParser.Model
Imports ModernWpf
Imports ModernWpf.Controls
Imports WPFUI
Imports System.IO
Imports System.Reflection

Class setting

    Private Sub sw1_Toggled(sender As Object, e As RoutedEventArgs) Handles sw1.Toggled
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If sw1.IsOn = True Then
            data("UI")("Theme") = "Dark"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.theme = "Dark"
            'My.Settings.Save()
            If Not data("UI")("AutoTheme") = "True" Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            End If

        Else
            data("UI")("Theme") = "Light"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.theme = "Light"
            'My.Settings.Save()
            If Not data("UI")("AutoTheme") = "True" Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            End If

        End If
    End Sub

    Private Sub sw2_Toggled(sender As Object, e As RoutedEventArgs) Handles sw2.Toggled
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If sw2.IsOn = True Then
            data("UI")("AutoTheme") = "True"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.themebysys = True
            'My.Settings.Save()
            sw1.IsEnabled = False
            If Theme.Manager.GetSystemTheme = Theme.Style.Dark Then
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark
                Theme.Manager.Switch(Theme.Style.Dark)
            Else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light
                Theme.Manager.Switch(Theme.Style.Light)
            End If
        Else
            data("UI")("AutoTheme") = "False"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.themebysys = False
            'My.Settings.Save()
            sw1.IsEnabled = True
        End If
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")

        If data("UI")("Sidebar") = "Top" Then
            combobox_sidebar.SelectedIndex = 1
        Else
            combobox_sidebar.SelectedIndex = 0

        End If

        If data("UI")("AutoTheme") = "False" Then
            sw2.IsOn = False
        Else
            sw2.IsOn = True

        End If

        If data("UI")("Theme") = "Light" Then
            sw1.IsOn = False
        Else
            sw1.IsOn = True
        End If

        If data("Tools")("AutoClose") = "False" Then
            sw_autoclose.IsOn = False
        Else
            sw_autoclose.IsOn = True
        End If

        If data("Tools")("Update_channel") = "Beta" Then
            update_to_choose.SelectedIndex = 1
        Else
            update_to_choose.SelectedIndex = 0
        End If

        If MainWindow.shouquan = False Then
            update_to_choose.IsEnabled = False
            update_to_choose.SelectedIndex = 0
        Else
            update_to_choose.IsEnabled = True
        End If

        If data("UI")("Background") = "False" Then
            sw_bg.IsOn = False
            bg_op.IsEnabled = False
        Else
            sw_bg.IsOn = True
            bg_op.IsEnabled = True

        End If
        If data("UI")("Background_Opacity") = "" Then
            data("UI")("Background_Opacity") = "0.68"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            bg_op.Value = data("UI")("Background_Opacity")
        Else
            bg_op.Value = data("UI")("Background_Opacity")
        End If
        'bg_op.Value = My.Settings.bgtoumingdu
        If data("Download")("Thread") = "" Then
            data("Download")("Thread") = "8"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            dw_thread_value.Text = 8
        Else
            dw_thread_value.Text = Int(data("Download")("Thread"))
        End If
        'dw_thread_value.Text = My.Settings.dw_thread

    End Sub

    Private Sub sw_autoclose_Toggled(sender As Object, e As RoutedEventArgs) Handles sw_autoclose.Toggled
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")

        If sw_autoclose.IsOn = True Then
            data("Tools")("AutoClose") = "True"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.endwhengamestart = True
            'My.Settings.Save()
        Else
            data("Tools")("AutoClose") = "False"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.endwhengamestart = False
            'My.Settings.Save()
        End If
    End Sub

    Private Sub sw_bg_Toggled(sender As Object, e As RoutedEventArgs) Handles sw_bg.Toggled
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")

        If sw_bg.IsOn = True Then
            data("UI")("Background") = "True"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.bg = True
            'My.Settings.Save()
            bg_op.IsEnabled = True
        Else
            data("UI")("Background") = "False"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.bg = False
            'My.Settings.Save()
            bg_op.IsEnabled = False
        End If
    End Sub
    Private _parentWin As MainWindow

    Public Property ParentWindow As MainWindow
        Get
            Return _parentWin
        End Get
        Set(ByVal value As MainWindow)
            _parentWin = value
        End Set
    End Property
    Private Sub bg_op_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles bg_op.ValueChanged
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")

        If data("UI")("Background") = "False" Then

        Else

            ParentWindow.ChangeBG(bg_op.Value)
            data("UI")("Background_Opacity") = bg_op.Value
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.bgtoumingdu = bg_op.Value
            'My.Settings.Save()
            toumingdu_card.Subtitle = "设置背景透明度" & vbCrLf & "当前不透明度：" & Math.Round(bg_op.Value, 2) * 100 & "%"
        End If

    End Sub

    Private Sub TextBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Dim re As Regex = New Regex("[^0-9]+")
        e.Handled = re.IsMatch(e.Text)
    End Sub

    Private Sub dw_thread_value_TextChanged(sender As Object, e As TextChangedEventArgs) Handles dw_thread_value.TextChanged
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")

        If dw_thread_value.Text = "" Then
            dw_thread_value.Text = 8
            data("Download")("Thread") = "8"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.dw_thread = 8
            'My.Settings.Save()
        Else
            data("Download")("Thread") = Int(dw_thread_value.Text)
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            'My.Settings.dw_thread = Int(dw_thread_value.Text)
            'My.Settings.Save()
        End If

    End Sub

    Private Sub combobox_sidebar_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles combobox_sidebar.SelectionChanged
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If combobox_sidebar.SelectedIndex = 0 Then
            data("UI")("Sidebar") = "Left"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            ParentWindow.Changesidebar()
        Else
            data("UI")("Sidebar") = "Top"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            ParentWindow.Changesidebar()
        End If
    End Sub

    Private Sub update_to_choose_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles update_to_choose.SelectionChanged
        '配置文件前置
        If Not System.IO.File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini") Then
            IO.Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools")
            System.IO.File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini").Dispose()
        End If
        Dim parser = New FileIniDataParser()
        Dim data As IniData = parser.ReadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini")
        If update_to_choose.SelectedIndex = 0 Then
            data("Tools")("Update_channel") = "Release"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            ParentWindow.Changesidebar()
        Else
            data("Tools")("Update_channel") = "Beta"
            parser.WriteFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\NSTARMC-Tools\Configuration.ini", data)
            ParentWindow.Changesidebar()
        End If
    End Sub
End Class
