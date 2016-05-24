Imports System
Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions

Public Class main

    Dim PathToGDAL As String
    Dim DriveOfGDAL As String
    Dim WFS_URL As String
    Dim ShapeOutput As String

    Public Function URLEncode(StringToEncode As String, Optional UsePlusRatherThanHexForSpace As Boolean = False) As String
        Dim TempAns As String
        Dim CurChr As Integer
        CurChr = 1
        TempAns = ""
        Do Until CurChr - 1 = Len(StringToEncode)
            Select Case Asc(Mid$(StringToEncode, CurChr, 1))
                Case 48 To 57, 65 To 90, 97 To 122
                    TempAns = TempAns & Mid$(StringToEncode, CurChr, 1)
                Case 32
                    If UsePlusRatherThanHexForSpace = True Then
                        TempAns = TempAns & "+"
                    Else
                        TempAns = TempAns & "%" & Hex(32)
                    End If
                Case Else
                    TempAns = TempAns & "%" & Hex(Asc(Mid$(StringToEncode, _
                      CurChr, 1)))
            End Select
            CurChr = CurChr + 1
        Loop
        URLEncode = TempAns
    End Function

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If CheckBox1.Checked Then
            Dim sURL As String
            WFS_URL = Trim(ComboBox1.Text)
            sURL = "http://wfs.geoclub.de/add.php?url=" + URLEncode(WFS_URL)
            Console.WriteLine(sURL)
            Dim wrGETURL As WebRequest
            wrGETURL = WebRequest.Create(sURL)
            Dim objStream As Stream
            objStream = wrGETURL.GetResponse.GetResponseStream()
            Dim objReader As New StreamReader(objStream)
            Dim sLine As String = ""
            Dim i As Integer = 0
            ListBox1.Items.Clear()
            Do While Not sLine Is Nothing
                i += 1
                sLine = objReader.ReadLine
                If Not sLine Is Nothing Then
                    ListBox1.Items.Add(sLine)
                End If
            Loop
        Else
            Dim Zeile As String
            Dim LayerArray() As String
            If My.Computer.FileSystem.FileExists(PathToGDAL + "\ogrinfo.exe") Then
                ListBox1.Items.Clear()
                Dim proc As ProcessStartInfo = New ProcessStartInfo("cmd.exe")
                Dim pr As Process
                Cursor.Current = Cursors.WaitCursor
                proc.CreateNoWindow = True
                proc.UseShellExecute = False
                proc.RedirectStandardInput = True
                proc.RedirectStandardOutput = True
                pr = Process.Start(proc)
                DriveOfGDAL = Microsoft.VisualBasic.Left(PathToGDAL, 2)
                pr.StandardInput.WriteLine(DriveOfGDAL)
                pr.StandardInput.WriteLine("cd """ + PathToGDAL + """")
                pr.StandardInput.WriteLine("ogrinfo.exe -ro -so -q WFS:""" + WFS_URL + """")
                pr.StandardInput.Close()
                Do Until pr.StandardOutput.EndOfStream()
                    Zeile = pr.StandardOutput.ReadLine()
                    LayerArray = Split(Zeile, " ")
                    If Val(LayerArray(0)) > 0 Then
                        ListBox1.Items.Add(LayerArray(1))
                        REM ListBox1.Refresh()
                    End If
                Loop
                pr.StandardOutput.Close()
                Cursor.Current = Cursors.Default
            Else
                Dim response = MsgBox("GDAL not found. Download now?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Confirm")
                If response = MsgBoxResult.Yes Then
                    Process.Start("http://download.gisinternals.com/sdk/downloads/release-1800-x64-gdal-1-11-1-mapserver-6-4-1/gdal-111-1800-x64-core.msi")
                End If
            End If
        End If
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        PathToGDAL = Trim(TextBox1.Text)
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If (FolderBrowserDialog1.ShowDialog() = DialogResult.OK) Then
            TextBox3.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If ListBox1.SelectedIndex >= 0 Then
            Dim LayerSelected As String = ListBox1.Items(ListBox1.SelectedIndex).ToString
            If ShapeOutput <> "" And LayerSelected <> "" Then
                Dim DriveOfShape = Microsoft.VisualBasic.Left(ShapeOutput, 2)
                Dim ShapeLayer As String
                If My.Computer.FileSystem.FileExists(PathToGDAL + "\ogr2ogr.exe") Then
                    Dim proc As ProcessStartInfo = New ProcessStartInfo("cmd.exe")
                    Dim pr As Process
                    Cursor.Current = Cursors.WaitCursor
                    proc.CreateNoWindow = True
                    proc.UseShellExecute = False
                    proc.RedirectStandardInput = True
                    proc.RedirectStandardOutput = True
                    pr = Process.Start(proc)
                    pr.StandardInput.WriteLine(DriveOfShape)
                    pr.StandardInput.WriteLine("cd """ + ShapeOutput + """")
                    pr.StandardInput.WriteLine("set GDAL_DATA=" + PathToGDAL + "\gdal-data\")
                    ShapeLayer = Regex.Replace(LayerSelected, "[^\w]", "_")
                    Dim OGRBefehl As String = """" + PathToGDAL + "\ogr2ogr.exe"" -overwrite -f ""ESRI Shapefile"" """ + ShapeLayer + """ -nln """ + ShapeLayer + """ WFS:" + WFS_URL + " """ + LayerSelected + """"
                    pr.StandardInput.WriteLine(OGRBefehl)
                    pr.StandardInput.Close()
                    Console.WriteLine(pr.StandardOutput.ReadToEnd())
                    pr.StandardOutput.Close()
                    Cursor.Current = Cursors.Default
                    REM TextBox1.Text = OGRBefehl
                    Process.Start("explorer.exe", ShapeOutput)
                Else
                    MsgBox("GDAL not found.")
                End If
            End If
        Else
            MsgBox("No layer selected.")
        End If
    End Sub

    Private Sub TextBox3_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox3.TextChanged
        ShapeOutput = Trim(TextBox3.Text)
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        If (FolderBrowserDialog1.ShowDialog() = DialogResult.OK) Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        Dim response As MsgBoxResult
        response = MsgBox("Do you want to exit?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Confirm")
        If response = MsgBoxResult.Yes Then
            Me.Dispose()
        ElseIf response = MsgBoxResult.No Then
            Exit Sub
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        WFS_URL = Trim(ComboBox1.Text)
    End Sub

    Private Sub main_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        WFS_URL = "http://maps.geo.hu-berlin.de/geoserver/wfs"
        ComboBox1.Text = WFS_URL
        Dim sURL As String
        sURL = "http://wfs.geoclub.de/urllist.php"
        Console.WriteLine(sURL)
        Dim wrGETURL As WebRequest
        wrGETURL = WebRequest.Create(sURL)
        Dim objStream As Stream
        objStream = wrGETURL.GetResponse.GetResponseStream()
        Dim objReader As New StreamReader(objStream)
        Dim sLine As String = ""
        Dim i As Integer = 0
        Do While Not sLine Is Nothing
            i += 1
            sLine = objReader.ReadLine
            If Not sLine Is Nothing Then
                ComboBox1.Items.Add(sLine)
            End If
        Loop
    End Sub

End Class
