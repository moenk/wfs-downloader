Imports System.Text.RegularExpressions

Public Class main

    Dim PathToGDAL As String
    Dim DriveOfGDAL As String
    Dim WFS_URL As String
    Dim ShapeOutput As String

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
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

    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        PathToGDAL = Trim(TextBox1.Text)
    End Sub

    Private Sub TextBox2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged
        WFS_URL = Trim(TextBox2.Text)
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
End Class
