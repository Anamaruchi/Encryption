
Imports System.Security.Cryptography
Imports System.IO
Public Class Encryption
    Inherits System.Windows.Forms.Form
    Implements IMessageFilter

    Private Shared clientDESCryptoServiceProvider As TripleDESCryptoServiceProvider
    Dim strLokasi As String

    Private Function SetEncKey(ByVal pwd As String) As Byte()
        Dim drive As Long = 0
        Dim rnd As Random

        rnd = New Random

        Dim byts(7) As Byte '8 bytes
        rnd.NextBytes(byts)

        SetKey(pwd, byts)

        Return byts
    End Function

    Private Sub SetDecKey(ByVal pwd As String, ByVal salt As Byte())
        Dim drive As Long = 0

        SetKey(pwd, salt)
    End Sub

    Private Sub SetKey(ByVal pwd As String, ByVal salt As Byte())
        Dim deriver As PasswordDeriveBytes
        deriver = New PasswordDeriveBytes(pwd, salt)

        clientDESCryptoServiceProvider.Key = deriver.GetBytes(clientDESCryptoServiceProvider.LegalKeySizes(0).MaxSize / 8)
        clientDESCryptoServiceProvider.IV = deriver.GetBytes(clientDESCryptoServiceProvider.BlockSize / 8)
    End Sub

    Private Sub ProteksiFile(ByVal pwd As String, ByVal fin As String, ByVal feout As String)
        Dim fileStream As FileStream
        fileStream = File.OpenWrite(feout)

        Dim writeStream As BinaryWriter
        writeStream = New BinaryWriter(fileStream)

        Dim myByte As Byte
        Dim salt() As Byte
        salt = SetEncKey(pwd)

        'Write data to file
        Try
            'Write salt to file
            Dim i As Integer
            For i = 0 To salt.Length - 1
                writeStream.Write(salt(i))
            Next
            writeStream.Flush()
        Catch
        End Try

        'Create the encrypter
        Dim encryptor As ICryptoTransform
        encryptor = clientDESCryptoServiceProvider.CreateEncryptor()

        'Create the Encrypter Stream
        Dim encStream As CryptoStream
        encStream = New CryptoStream(fileStream, encryptor, CryptoStreamMode.Write)

        'Read the input file
        Dim readStream As BinaryReader
        readStream = New BinaryReader(File.OpenRead(fin))

        Try
            Do
                myByte = readStream.ReadByte()
                encStream.WriteByte(myByte)
            Loop
        Catch
            encStream.FlushFinalBlock()
            encStream.Flush()
        End Try

        encStream.Close()
        fileStream.Close()
        readStream.Close()
        'MsgBox("File " & TextBox1.Text & " File Successfully Encrypted", MsgBoxStyle.Information, "Message")
        MsgBox("File " & TextBox1.Text & " File Berhasil di Enkripsi", MsgBoxStyle.Information, "Pesan")
    End Sub

    Private Sub DeProteksiFile(ByVal pwd As String, ByVal fin As String, ByVal fdout As String)
        Dim fileStream As FileStream
        fileStream = File.OpenRead(fin)

        'Load Salt
        Dim salt(7) As Byte
        Try
            Dim i As Integer
            For i = 0 To 7
                salt(i) = fileStream.ReadByte()
            Next
            SetDecKey(pwd, salt)
        Catch
            fileStream.Close()
            Return
        End Try

        'Create the decryptor
        Dim encryptor As ICryptoTransform
        encryptor = clientDESCryptoServiceProvider.CreateDecryptor()

        'Create the decryptor stream
        Dim decStream As CryptoStream
        decStream = New CryptoStream(fileStream, encryptor, CryptoStreamMode.Read)

        'Stream to read the from decryptor stream
        Dim readStream As BinaryReader
        readStream = New BinaryReader(decStream)

        'The output file
        Dim writeStream As BinaryWriter
        writeStream = New BinaryWriter(File.OpenWrite(fdout))

        Dim myByte As Byte
        Try
            'Copy data
            Do
                myByte = readStream.ReadByte()
                writeStream.Write(myByte)
            Loop
        Catch
            'Flush stream
            writeStream.Flush()
        End Try

        writeStream.Close()
        fileStream.Close()
        readStream.Close()
        'MsgBox("File " & TextBox1.Text & " File Successfully Decryption", MsgBoxStyle.Information, "Message")
        MsgBox("File " & TextBox1.Text & " File Berhasil di Deskripsi", MsgBoxStyle.Information, "Pesan")
    End Sub

    Private Sub KeluarToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        End
    End Sub

    Function DriveSerialNumber(ByVal Drive As String) As String
        Dim fso = CreateObject("Scripting.FileSystemObject")
        Dim drv
        Dim driveLetter, driveSerial As String
        Dim driveGUID As Guid = Nothing

        For Each drv In fso.Drives
            Try
                driveLetter = drv.DriveLetter
                driveSerial = drv.SerialNumber

                If Drive.ToUpper.Trim = driveLetter.ToUpper.Trim Then
                    DriveSerialNumber = driveSerial
                    Exit Function
                End If
            Catch
            End Try
        Next drv

        DriveSerialNumber = ""
    End Function

    Private Sub cmdencrypt_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdencrypt.Click
        Dim drive, pwd, feout As String

        ' USB Drive
        pwd = TextBox2.Text
        If GetSetting("USBProtect", "Settings", "UseUSBDrive", False) = True Then
            drive = DriveSerialNumber(Mid(GetSetting("USBProtect", "Settings", "USBDriveLetter", "F"), 1, 1))
            Debug.Print(drive)
            If drive.Trim = "" And GetSetting("USBProtect", "Settings", "UseUSBDrive", False) = True Then
                'MsgBox("Disk Not Available, Please Select Another Disk!", MsgBoxStyle.Exclamation, "Disk Error")
                MsgBox("Disk Tidak Ada, Silahkan Pilih Disk Yang Lain!", MsgBoxStyle.Exclamation, "Disk Error")
                Exit Sub
            End If

            pwd = pwd & drive
            Debug.Print(pwd)
        End If

        feout = strLokasi & ".anamaruchi"

        ProteksiFile(pwd, strLokasi, feout)
        File.Delete(strLokasi)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim xbesarfile

        OpenFileDialog1.Filter = "File Selection|*.doc;*.docx;*.PDF;*.JPG;*.JPEG;*.PNG;*.TXT;*.EXE;*.XLS;*.PPT;*.PPTX;*.MP3;*.MP4;*.MKV;*.AVI|File Encryption|*.anamaruchi"
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            TextBox1.Text = OpenFileDialog1.FileName
            strLokasi = OpenFileDialog1.FileName

            Dim fFile As New FileInfo(OpenFileDialog1.FileName)
            Dim fSize As Integer = CInt(fFile.Length)
            xbesarfile = CInt(Microsoft.VisualBasic.Int(fSize / 1024) + 1)
            lblbesar.Text = CStr(Microsoft.VisualBasic.Int(fSize / 1024) + 1) & " KB"
        Else
            Return
        End If
    End Sub

    Private Sub cmdekripsi_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdekripsi.Click
        Dim drive, pwd, fdout As String

        If InStr(strLokasi.ToLower, ".anamaruchi") = 0 Then
            'MessageBox.Show("This file is not a protection file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            MessageBox.Show("File ini bukan file hasil proteksi.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If

        pwd = TextBox2.Text

        ' USB Drive
        If GetSetting("USBProtect", "Settings", "UseUSBDrive", False) = True Then
            drive = DriveSerialNumber(Mid(GetSetting("USBProtect", "Settings", "USBDriveLetter", "E"), 1, 1))

            If drive.Trim = "" And GetSetting("ColdSteelDefender", "Settings", "UseUSBDrive", False) = True Then
                'MsgBox("Disk Not Available, Please Select Another Disk!", MsgBoxStyle.Exclamation, "Disk Error")
                MsgBox("Disk Tidak Ada, Silahkan Pilih Disk Yang Lain!", MsgBoxStyle.Exclamation, "Disk Error")
                Exit Sub
            End If

            pwd = pwd & drive
        End If

        fdout = Mid(strLokasi.ToLower, 1, InStr(strLokasi, ".anamaruchi") - 1)

        Try
            DeProteksiFile(pwd, strLokasi, fdout)
            File.Delete(strLokasi)
        Catch
            'MessageBox.Show("Error not same Configuration File.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            MessageBox.Show("Kesalahan tidak sama dengan File Konfigurasi.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

    Private Sub PengaturanUSBFlashDiskToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        DiskKey.Show()
    End Sub

    Public Function PreFilterMessage(ByRef m As System.Windows.Forms.Message) As Boolean Implements System.Windows.Forms.IMessageFilter.PreFilterMessage

    End Function

    Private Sub FormUtama_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        clientDESCryptoServiceProvider = New TripleDESCryptoServiceProvider()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        DiskKey.Show()

    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click

    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk

    End Sub
End Class