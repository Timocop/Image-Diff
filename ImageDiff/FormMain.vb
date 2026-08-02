#Const USE_MAGICK = 0

Imports System.Security.Cryptography

Public Class FormMain
    Private g_ClassScanner As ClassScanner

    Private g_bIgnoreSelection As Boolean = False

    Enum ENUM_HASHING_METHOD
        GDI
        MAGICK

        __MAX
    End Enum

    Structure STRUC_HASHING_SIZE_ITEM
        Dim iSize As Integer

        Sub New(_Size As Integer)
            iSize = _Size
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0}x{0} px", iSize)
        End Function
    End Structure

    Structure STRUC_HASHING_METHOD_ITEM
        Dim iMethod As ENUM_HASHING_METHOD

        Sub New(_Method As ENUM_HASHING_METHOD)
            iMethod = _Method
        End Sub

        Public Overrides Function ToString() As String
            Select Case (iMethod)
                Case ENUM_HASHING_METHOD.GDI
                    Return ".NET GDI"

                Case ENUM_HASHING_METHOD.MAGICK
                    Return "Magick Lib"
            End Select

            Return "Unknown"
        End Function
    End Structure

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        NumericUpDown_Threads.Minimum = 1
        NumericUpDown_Threads.Maximum = 64
        NumericUpDown_Threads.Value = Math.Max(Math.Min(Environment.ProcessorCount, NumericUpDown_Threads.Maximum), NumericUpDown_Threads.Minimum)

        ComboBox_HashingSize.Items.Clear()
        ComboBox_HashingSize.Items.Add(New STRUC_HASHING_SIZE_ITEM(8))
        ComboBox_HashingSize.Items.Add(New STRUC_HASHING_SIZE_ITEM(16))
        ComboBox_HashingSize.Items.Add(New STRUC_HASHING_SIZE_ITEM(32))
        ComboBox_HashingSize.Items.Add(New STRUC_HASHING_SIZE_ITEM(64))
        ComboBox_HashingSize.Items.Add(New STRUC_HASHING_SIZE_ITEM(128))
        ComboBox_HashingSize.Items.Add(New STRUC_HASHING_SIZE_ITEM(256))
        ComboBox_HashingSize.SelectedIndex = 0

        ComboBox_HashingMethod.Items.Clear()
        For i As Integer = 0 To ENUM_HASHING_METHOD.__MAX - 1
            ComboBox_HashingMethod.Items.Add(New STRUC_HASHING_METHOD_ITEM(CType(i, ENUM_HASHING_METHOD)))
        Next
        ComboBox_HashingMethod.SelectedIndex = ENUM_HASHING_METHOD.GDI

        ComboBox_HashingQuality.Items.Clear()
        ComboBox_HashingQuality.Items.Add("High Quality, Slow")
        ComboBox_HashingQuality.Items.Add("Low Quality, Fast")
        ComboBox_HashingQuality.SelectedIndex = 0

        ImageMagick.MagickNET.Initialize()
        ImageMagick.OpenCL.IsEnabled = False
    End Sub

    Public Sub SetPreviewImageA(mImage As Object)
        If (PictureBox_ImageAPreview.Image IsNot Nothing) Then
            PictureBox_ImageAPreview.Image.Dispose()
            PictureBox_ImageAPreview.Image = Nothing
        End If

        If (mImage Is Nothing) Then
            Return
        End If

        Select Case (True)
            Case (TypeOf mImage Is Image)
                Dim mNewImage = DirectCast(mImage, Image)

                PictureBox_ImageAPreview.Image = mNewImage

            Case (TypeOf mImage Is ImageMagick.MagickImage)
                Dim mNewImage = DirectCast(mImage, ImageMagick.MagickImage)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Write(mStream, ImageMagick.MagickFormat.Jpg)
                    mStream.Position = 0
                    PictureBox_ImageAPreview.Image = Image.FromStream(mStream)
                End Using
        End Select
    End Sub

    Public Sub SetPreviewImageB(mImage As Object)
        If (PictureBox_ImageBPreview.Image IsNot Nothing) Then
            PictureBox_ImageBPreview.Image.Dispose()
            PictureBox_ImageBPreview.Image = Nothing
        End If

        If (mImage Is Nothing) Then
            Return
        End If

        Select Case (True)
            Case (TypeOf mImage Is Image)
                Dim mNewImage = DirectCast(mImage, Image)

                PictureBox_ImageBPreview.Image = mNewImage

            Case (TypeOf mImage Is ImageMagick.MagickImage)
                Dim mNewImage = DirectCast(mImage, ImageMagick.MagickImage)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Write(mStream, ImageMagick.MagickFormat.Jpg)
                    mStream.Position = 0
                    PictureBox_ImageBPreview.Image = Image.FromStream(mStream)
                End Using
        End Select
    End Sub

    Private Sub Button_Select_Click(sender As Object, e As EventArgs) Handles Button_Select.Click
        Try
            If (g_ClassScanner Is Nothing OrElse Not g_ClassScanner.m_Scanning) Then
                Dim sDirectory As String
                Dim bIncludeSubDirectories As Boolean = CheckBox_CheckSubDirectorys.Checked
                Dim iMaxImageDiff As Integer = CInt(NumericUpDown_MaxImageDiff.Value)
                Dim iThreads As Integer = CInt(NumericUpDown_Threads.Value)
                Dim iThumbSize As Integer = DirectCast(ComboBox_HashingSize.SelectedItem, STRUC_HASHING_SIZE_ITEM).iSize
                Dim iHashingMethod As ENUM_HASHING_METHOD = DirectCast(ComboBox_HashingMethod.SelectedItem, STRUC_HASHING_METHOD_ITEM).iMethod
                Dim bHighQualityHashing As Boolean = (ComboBox_HashingQuality.SelectedIndex = 0)
                Dim bUseCaching As Boolean = CheckBox_Caching.Checked

                Using i As New SaveFileDialog
                    i.FileName = "Select folder..."

                    If (i.ShowDialog = DialogResult.OK) Then
                        sDirectory = IO.Path.GetDirectoryName(i.FileName)
                        TextBox_Path.Text = sDirectory
                    Else
                        Return
                    End If
                End Using

                g_ClassScanner = New ClassScanner(Me, sDirectory, bIncludeSubDirectories, iMaxImageDiff, iThreads, iThumbSize, iHashingMethod, bHighQualityHashing, bUseCaching)
                g_ClassScanner.Start()
            Else
                g_ClassScanner.Abort()

                ToolStripStatusLabel_Progress.Visible = False
                ToolStripProgressBar_Progress.Visible = False

                Button_Select.Text = "Select"
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ListViewEx_Images_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListViewEx_Images.SelectedIndexChanged
        Try
            If (g_bIgnoreSelection) Then
                Return
            End If

            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)

            If (IO.File.Exists(mSelectedItem.m_ImageInfo.sFileA)) Then
                Try
                    SetPreviewImageA(New Bitmap(mSelectedItem.m_ImageInfo.sFileA))
                Catch ex As Exception
                    ' Unsupported image, try Magick
                    SetPreviewImageA(New ImageMagick.MagickImage(mSelectedItem.m_ImageInfo.sFileA))
                End Try
            End If

            If (IO.File.Exists(mSelectedItem.m_ImageInfo.sFileB)) Then
                Try
                    SetPreviewImageB(New Bitmap(mSelectedItem.m_ImageInfo.sFileB))
                Catch ex As Exception
                    ' Unsupported image, try Magick
                    SetPreviewImageB(New ImageMagick.MagickImage(mSelectedItem.m_ImageInfo.sFileB))
                End Try
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FileAToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles FileAToolStripMenuItem1.Click
        Try
            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)
            If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileA)) Then
                Return
            End If

            Process.Start(mSelectedItem.m_ImageInfo.sFileA)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FileBToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles FileBToolStripMenuItem1.Click
        Try
            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)
            If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileB)) Then
                Return
            End If

            Process.Start(mSelectedItem.m_ImageInfo.sFileB)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FileAToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles FileAToolStripMenuItem2.Click
        Try
            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)
            If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileA)) Then
                Return
            End If

            Process.Start("explorer.exe", String.Format("/select,{0}", mSelectedItem.m_ImageInfo.sFileA))
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FileBToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles FileBToolStripMenuItem2.Click
        Try
            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)
            If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileB)) Then
                Return
            End If

            Process.Start("explorer.exe", String.Format("/select,{0}", mSelectedItem.m_ImageInfo.sFileB))
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FileAToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FileAToolStripMenuItem.Click
        Try
            If (MessageBox.Show(String.Format("Do you want to delete {0} files?", ListViewEx_Images.SelectedItems.Count), "Delete files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No) Then
                Return
            End If

            'Remove image from preview because the file will be in use
            SetPreviewImageA(Nothing)
            SetPreviewImageB(Nothing)

            Try
                g_bIgnoreSelection = True

                Try
                    ListViewEx_Images.BeginUpdate()

                    For Each mItem As ListViewItem In ListViewEx_Images.SelectedItems
                        If (TypeOf mItem IsNot ClassListViewItemImage) Then
                            Return
                        End If

                        Dim mSelectedItem = DirectCast(mItem, ClassListViewItemImage)
                        If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileA)) Then
                            Return
                        End If

                        IO.File.Delete(mSelectedItem.m_ImageInfo.sFileA)

                        ListViewEx_Images.Items.Remove(mItem)
                    Next
                Finally
                    ListViewEx_Images.EndUpdate()
                End Try
            Finally
                g_bIgnoreSelection = False
            End Try
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FileBToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FileBToolStripMenuItem.Click
        Try
            If (MessageBox.Show(String.Format("Do you want to delete {0} files?", ListViewEx_Images.SelectedItems.Count), "Delete files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No) Then
                Return
            End If

            'Remove image from preview because the file will be in use
            SetPreviewImageA(Nothing)
            SetPreviewImageB(Nothing)

            Try
                g_bIgnoreSelection = True

                Try
                    ListViewEx_Images.BeginUpdate()

                    For Each mItem As ListViewItem In ListViewEx_Images.SelectedItems
                        If (TypeOf mItem IsNot ClassListViewItemImage) Then
                            Return
                        End If

                        Dim mSelectedItem = DirectCast(mItem, ClassListViewItemImage)
                        If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileB)) Then
                            Return
                        End If

                        IO.File.Delete(mSelectedItem.m_ImageInfo.sFileB)

                        ListViewEx_Images.Items.Remove(mItem)
                    Next
                Finally
                    ListViewEx_Images.EndUpdate()
                End Try
            Finally
                g_bIgnoreSelection = False
            End Try
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ContextMenuStrip_Images_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip_Images.Opening
        ToolStripMenuItem_Open.Enabled = (ListViewEx_Images.SelectedItems.Count = 1)
        ToolStripMenuItem_OpenExplorer.Enabled = (ListViewEx_Images.SelectedItems.Count = 1)
        ToolStripMenuItem_Remove.Enabled = (ListViewEx_Images.SelectedItems.Count > 0)
    End Sub

    Private Sub PictureBox_ImageAPreview_Click(sender As Object, e As EventArgs) Handles PictureBox_ImageAPreview.Click
        Try
            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)
            If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileA)) Then
                Return
            End If

            Process.Start(mSelectedItem.m_ImageInfo.sFileA)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub PictureBox_ImageBPreview_Click(sender As Object, e As EventArgs) Handles PictureBox_ImageBPreview.Click
        Try
            If (ListViewEx_Images.SelectedItems.Count <> 1) Then
                Return
            End If

            If (TypeOf ListViewEx_Images.SelectedItems(0) IsNot ClassListViewItemImage) Then
                Return
            End If

            Dim mSelectedItem = DirectCast(ListViewEx_Images.SelectedItems(0), ClassListViewItemImage)
            If (Not IO.File.Exists(mSelectedItem.m_ImageInfo.sFileB)) Then
                Return
            End If

            Process.Start(mSelectedItem.m_ImageInfo.sFileB)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Class ClassScanner
        Private g_fFormMain As FormMain

        Private g_mScannerThread As Threading.Thread

        Property m_Directory As String
        Property m_IncludeSubDirectories As Boolean
        Property m_MaxImageDiff As Integer
        Property m_Threads As Integer
        Property m_ThumbSize As Integer
        Property m_HashingMethod As ENUM_HASHING_METHOD
        Property m_HighQualityHashing As Boolean
        Property m_UseCaching As Boolean

        Private g_mLock As New Object
        Private g_mHashCache As New Dictionary(Of String, String)
        Private g_mFileState As New Dictionary(Of String, ENUM_FILE_CHECK)

        Public Sub New(_FormMain As FormMain,
                       _Directory As String,
                       _IncludeSubDirectories As Boolean,
                       _MaxImageDiff As Integer,
                       _Threads As Integer,
                       _ThumbSize As Integer,
                       _HashingMethod As ENUM_HASHING_METHOD,
                       _HighQualityHashing As Boolean,
                       _UseCaching As Boolean)
            g_fFormMain = _FormMain
            m_Directory = _Directory
            m_IncludeSubDirectories = _IncludeSubDirectories
            m_MaxImageDiff = _MaxImageDiff
            m_Threads = _Threads
            m_ThumbSize = _ThumbSize
            m_HashingMethod = _HashingMethod
            m_HighQualityHashing = _HighQualityHashing
            m_UseCaching = _UseCaching
        End Sub

        Public Sub Start()
            If (m_Scanning) Then
                Return
            End If

            g_mScannerThread = New Threading.Thread(AddressOf ScannerThread) With {
                .IsBackground = True
            }
            g_mScannerThread.Start()
        End Sub

        Public Sub Abort()
            If (m_Scanning) Then
                g_mScannerThread.Abort()
                g_mScannerThread.Join()
                g_mScannerThread = Nothing
            End If
        End Sub

        ReadOnly Property m_Scanning As Boolean
            Get
                Return (g_mScannerThread IsNot Nothing AndAlso g_mScannerThread.IsAlive)
            End Get
        End Property

        Private Property m_HashCache(sFile As String) As String
            Get
                SyncLock g_mLock
                    If (Not g_mHashCache.ContainsKey(sFile.ToLowerInvariant)) Then
                        Return Nothing
                    End If

                    Return g_mHashCache(sFile.ToLowerInvariant)
                End SyncLock
            End Get
            Set(value As String)
                SyncLock g_mLock
                    g_mHashCache(sFile.ToLowerInvariant) = value
                End SyncLock
            End Set
        End Property

        Enum ENUM_FILE_CHECK
            NOT_CHECKED = 0
            PASSED
            FAILED
        End Enum

        Private Property m_FileState(sFile As String) As ENUM_FILE_CHECK
            Get
                SyncLock g_mLock
                    If (Not g_mFileState.ContainsKey(sFile.ToLowerInvariant)) Then
                        Return ENUM_FILE_CHECK.NOT_CHECKED
                    End If

                    Return g_mFileState(sFile.ToLowerInvariant)
                End SyncLock
            End Get
            Set(value As ENUM_FILE_CHECK)
                SyncLock g_mLock
                    g_mFileState(sFile.ToLowerInvariant) = value
                End SyncLock
            End Set
        End Property

        Private Sub ScannerThread()
            Try
                Dim sDirectory As String = m_Directory
                Dim bIncludeSubDirectories As Boolean = m_IncludeSubDirectories
                Dim iMaxImageDiff As Integer = m_MaxImageDiff
                Dim iThreads As Integer = m_Threads
                Dim iThumbSize As Integer = m_ThumbSize
                Dim iHashingMethod As ENUM_HASHING_METHOD = m_HashingMethod
                Dim bUseCaching As Boolean = m_UseCaching

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.Button_Select.Text = "Abort")

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = "Searching files...")
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Minimum = 0)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Maximum = 100)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Value = 0)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Visible = True)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Visible = True)

                Dim sFiles = IO.Directory.GetFiles(sDirectory, "*.*", If(bIncludeSubDirectories, IO.SearchOption.AllDirectories, IO.SearchOption.TopDirectoryOnly))
                Dim mImageInfo As New List(Of STRUC_IMAGE_INFO)
                Dim mThreads As New List(Of Threading.Thread)
                Dim mFilesThreads As New Queue(Of String)
                Dim mThreadInfo As New Dictionary(Of String, Object)

                Dim mTimeTaken As New Stopwatch
                mTimeTaken.Start()

                Try
                    If (sFiles.Length > 0) Then
                        g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Maximum = sFiles.Length)

                        Dim j As Integer
                        For j = 0 To 1
                            Dim mLastFilesPerSec As New Queue(Of Integer)
                            Dim iFilesLast As Integer = 0
                            Dim iFilesPerSec As Double = 0.0

                            If (bUseCaching) Then
                                Select Case (j)
                                    Case 0
                                        g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = "Loading cache...")

                                        LoadCache()
                                    Case 1
                                        g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = "Saving cache...")

                                        SaveCache()
                                End Select
                            End If

                            SyncLock g_mLock
                                mThreadInfo("Files") = 0
                            End SyncLock

                            For i = 0 To sFiles.Length - 1
                                mFilesThreads.Enqueue(sFiles(i))
                            Next

                            For i = 0 To iThreads - 1
                                Dim mData As New Dictionary(Of String, Object)
                                mData("FilesThreads") = mFilesThreads
                                mData("TotalFiles") = New List(Of String)(sFiles)
                                mData("ImageInfo") = mImageInfo
                                mData("ThreadInfo") = mThreadInfo

                                mData("MaxImageDiff") = iMaxImageDiff
                                mData("IsPreHashing") = (j = 0)
                                mData("HashingMethod") = iHashingMethod
                                mData("ThumbSize") = iThumbSize

                                Dim tThread As New Threading.Thread(AddressOf SubScanner) With {
                                    .IsBackground = True
                                }
                                tThread.Start(mData)

                                mThreads.Add(tThread)
                            Next

                            Try
                                While True
                                    Threading.Thread.Sleep(1000)

                                    Dim iFiles As Integer

                                    SyncLock g_mLock
                                        iFiles = CInt(mThreadInfo("Files"))
                                    End SyncLock

                                    ' Calculate time left and rate per seconds
                                    mLastFilesPerSec.Enqueue(iFiles - iFilesLast)
                                    iFilesPerSec = mLastFilesPerSec.Average()
                                    iFilesLast = iFiles
                                    Dim mTimeLeft As New TimeSpan(0, 0, CInt((sFiles.Length - iFiles) / iFilesPerSec))

                                    While (mLastFilesPerSec.Count > 100)
                                        mLastFilesPerSec.Dequeue()
                                    End While


                                    g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Value = iFiles)
                                    g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = String.Format("{0} files {1}/{2} - {3}/s - {4} left - {5}%...",
                                                                                                                                 If(j = 0, "Hashing", "Comparing"),
                                                                                                                                 iFiles,
                                                                                                                                 sFiles.Length,
                                                                                                                                 CInt(iFilesPerSec),
                                                                                                                                 mTimeLeft.ToString,
                                                                                                                                 CInt(Math.Floor((iFiles / sFiles.Length) * 100))))

                                    For Each mThread In mThreads
                                        If (mThread.IsAlive) Then
                                            Continue While
                                        End If
                                    Next

                                    Exit While
                                End While
                            Catch ex As Threading.ThreadAbortException
                                Throw
                            End Try
                        Next
                    End If
                Finally
                    For Each mThread In mThreads
                        mThread.Abort()
                    Next

                    For Each mThread In mThreads
                        mThread.Join()
                    Next
                End Try

                mTimeTaken.Stop()

                ' Get failed files to display later
                Dim mFailedFiles As New List(Of String)
                For Each sFile As String In sFiles
                    If (m_FileState(sFile) <> ENUM_FILE_CHECK.FAILED) Then
                        Continue For
                    End If

                    mFailedFiles.Add(sFile)
                Next

                ' Sort by difference
                mImageInfo.Sort(Function(a As STRUC_IMAGE_INFO, b As STRUC_IMAGE_INFO)
                                    Return -a.iDifference.CompareTo(b.iDifference)
                                End Function)

                g_fFormMain.BeginInvoke(Sub()
                                            Try
                                                g_fFormMain.ClassListViewEx_FailedFiles.BeginUpdate()
                                                g_fFormMain.ClassListViewEx_FailedFiles.Items.Clear()

                                                For Each sFile In mFailedFiles.ToArray
                                                    g_fFormMain.ClassListViewEx_FailedFiles.Items.Add(New ListViewItem(New String() {sFile}))
                                                Next
                                            Finally
                                                g_fFormMain.ClassListViewEx_FailedFiles.EndUpdate()
                                            End Try
                                        End Sub)


                g_fFormMain.BeginInvoke(Sub()
                                            Try
                                                g_fFormMain.ListViewEx_Images.BeginUpdate()
                                                g_fFormMain.ListViewEx_Images.Items.Clear()

                                                For Each mItem In mImageInfo.ToArray
                                                    g_fFormMain.ListViewEx_Images.Items.Add(New ClassListViewItemImage(mItem))
                                                Next
                                            Finally
                                                g_fFormMain.ListViewEx_Images.EndUpdate()
                                            End Try
                                        End Sub)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Value = g_fFormMain.ToolStripProgressBar_Progress.Maximum)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = String.Format("Comparing Finished! Time taken: {0}", New TimeSpan(mTimeTaken.Elapsed.Hours, mTimeTaken.Elapsed.Minutes, mTimeTaken.Elapsed.Seconds).ToString))

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Visible = False)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.Button_Select.Text = "Select")
            Catch ex As Threading.ThreadAbortException
                Throw
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Visible = False)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Visible = False)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.Button_Select.Text = "Select")
            End Try
        End Sub

        Private Sub SubScanner(x As Object)
            Dim mData = DirectCast(x, Dictionary(Of String, Object))

            Dim mFilesThreads = DirectCast(mData("FilesThreads"), Queue(Of String))
            Dim mTotalFiles = DirectCast(mData("TotalFiles"), List(Of String))
            Dim mImageInfo = DirectCast(mData("ImageInfo"), List(Of STRUC_IMAGE_INFO))
            Dim mThreadInfo = DirectCast(mData("ThreadInfo"), Dictionary(Of String, Object))

            Dim iMaxImageDiff = DirectCast(mData("MaxImageDiff"), Integer)
            Dim bIsPreHashing = DirectCast(mData("IsPreHashing"), Boolean)
            Dim iHashingMethod = DirectCast(mData("HashingMethod"), ENUM_HASHING_METHOD)
            Dim iThumbSize = DirectCast(mData("ThumbSize"), Integer)

            Dim sTotalFiles As String() = {}

            Dim MAX_FILE_SIZE As Integer = 100 * 1024 * 1024

            SyncLock g_mLock
                sTotalFiles = mTotalFiles.ToArray
            End SyncLock

            While True
                Try
                    Dim sFileA As String

                    SyncLock g_mLock
                        If (mFilesThreads.Count < 1) Then
                            Exit While
                        End If

                        sFileA = mFilesThreads.Dequeue()

                        mThreadInfo("Files") = CInt(mThreadInfo("Files")) + 1
                    End SyncLock

                    If (m_FileState(sFileA) = ENUM_FILE_CHECK.FAILED) Then
                        Continue While
                    End If

                    If (Not IO.File.Exists(sFileA)) Then
                        Continue While
                    End If

                    Dim mFileAInfo As New IO.FileInfo(sFileA)
                    If (mFileAInfo.Length > MAX_FILE_SIZE) Then
                        Continue While
                    End If

                    If (m_HashCache(sFileA) Is Nothing AndAlso
                        m_FileState(sFileA) <> ENUM_FILE_CHECK.PASSED) Then
                        Try
                            Select Case (iHashingMethod)
                                Case ENUM_HASHING_METHOD.MAGICK
                                    Using mImage As New ImageMagick.MagickImage(sFileA)
                                        m_FileState(sFileA) = ENUM_FILE_CHECK.PASSED
                                    End Using
                                Case Else
                                    Using mImage As Image = Image.FromFile(sFileA)
                                        m_FileState(sFileA) = ENUM_FILE_CHECK.PASSED
                                    End Using
                            End Select
                        Catch ex As Threading.ThreadAbortException
                            Throw
                        Catch ex As Exception
                            m_FileState(sFileA) = ENUM_FILE_CHECK.FAILED
                            Continue While
                        End Try
                    End If

                    If (bIsPreHashing) Then
                        Select Case (iHashingMethod)
                            Case ENUM_HASHING_METHOD.MAGICK
                                CalculatePerceptualHash(sFileA, CUInt(iThumbSize))
                            Case Else
                                CalculateAverageHash(sFileA, CUInt(iThumbSize))
                        End Select
                    Else
                        For Each sFileB As String In sTotalFiles
                            Try
                                If (sFileA.ToLowerInvariant = sFileB.ToLowerInvariant) Then
                                    Continue For
                                End If

                                If (m_FileState(sFileB) = ENUM_FILE_CHECK.FAILED) Then
                                    Continue For
                                End If

                                If (Not IO.File.Exists(sFileB)) Then
                                    Continue For
                                End If

                                Dim mFileBInfo As New IO.FileInfo(sFileB)
                                If (mFileBInfo.Length > MAX_FILE_SIZE) Then
                                    m_FileState(sFileB) = ENUM_FILE_CHECK.FAILED
                                    Continue For
                                End If

                                If (m_HashCache(sFileB) Is Nothing AndAlso
                                    m_FileState(sFileB) <> ENUM_FILE_CHECK.PASSED) Then
                                    Try
                                        Select Case (iHashingMethod)
                                            Case ENUM_HASHING_METHOD.MAGICK
                                                Using mImage As New ImageMagick.MagickImage(sFileB)
                                                    m_FileState(sFileB) = ENUM_FILE_CHECK.PASSED
                                                End Using
                                            Case Else
                                                Using mImage As Image = Image.FromFile(sFileB)
                                                    m_FileState(sFileB) = ENUM_FILE_CHECK.PASSED
                                                End Using
                                        End Select
                                    Catch ex As Threading.ThreadAbortException
                                        Throw
                                    Catch ex As Exception
                                        m_FileState(sFileB) = ENUM_FILE_CHECK.FAILED
                                        Continue For
                                    End Try
                                End If

                                Dim iAvgDiff As Double = 0.0

                                Select Case (iHashingMethod)
                                    Case ENUM_HASHING_METHOD.MAGICK
                                        iAvgDiff = ImageCompareAverageMagick(sFileA, sFileB, CUInt(iThumbSize))
                                    Case Else
                                        iAvgDiff = ImageCompareAverageImage(sFileA, sFileB, CUInt(iThumbSize))
                                End Select

                                If (iAvgDiff < (iMaxImageDiff / 100)) Then
                                    Continue For
                                End If

                                SyncLock g_mLock
                                    Dim bSkip As Boolean = False
                                    For Each mItem In mImageInfo
                                        If (sFileA.ToLowerInvariant = mItem.sFileB.ToLowerInvariant OrElse
                                            sFileB.ToLowerInvariant = mItem.sFileA.ToLowerInvariant) Then
                                            bSkip = True
                                            Exit For
                                        End If
                                    Next

                                    If (Not bSkip) Then
                                        mImageInfo.Add(New STRUC_IMAGE_INFO(sFileA, sFileB, iAvgDiff, mFileAInfo.Length, mFileBInfo.Length))
                                    End If
                                End SyncLock

                            Catch ex As Threading.ThreadAbortException
                                Throw
                            Catch ex As Exception
                                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            End Try
                        Next
                    End If
                Catch ex As Threading.ThreadAbortException
                    Throw
                Catch ex As Exception
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End While
        End Sub

        Public Function ImageCompareAverageMagick(sFileA As String, sFileB As String, Optional ByVal iThumbSize As UInteger = 8) As Double
            Dim sHashA As String = m_HashCache(sFileA)
            Dim sHashB As String = m_HashCache(sFileB)

            If (sHashA Is Nothing) Then
                Using mHashA As New ImageMagick.MagickImage(sFileA)
                    sHashA = CalculatePerceptualHash(mHashA, iThumbSize)
                    m_HashCache(sFileA) = sHashA
                End Using
            End If

            If (sHashB Is Nothing) Then
                Using mHashB As New ImageMagick.MagickImage(sFileB)
                    sHashB = CalculatePerceptualHash(mHashB, iThumbSize)
                    m_HashCache(sFileB) = sHashB
                End Using
            End If

            Return CalculateHashSimilarity(sHashA, sHashB)
        End Function

        Public Function CalculatePerceptualHash(sFile As String, Optional ByVal iThumbSize As UInteger = 8) As String
            Dim sHash As String = m_HashCache(sFile)

            If (sHash Is Nothing) Then
                Using mImage As New ImageMagick.MagickImage(sFile)
                    sHash = CalculatePerceptualHash(mImage, iThumbSize)
                    m_HashCache(sFile) = sHash
                End Using
            End If

            Return sHash
        End Function

        Public Function CalculatePerceptualHash(mImage As ImageMagick.MagickImage, Optional ByVal iThumbSize As UInteger = 8) As String
            Using mThumbImage As New ImageMagick.MagickImage(mImage)
                Dim mThumbGeo As New ImageMagick.MagickGeometry(iThumbSize, iThumbSize)
                mThumbGeo.IgnoreAspectRatio = True

                If (m_HighQualityHashing) Then
                    mThumbImage.Resize(mThumbGeo, ImageMagick.FilterType.Lanczos)
                Else
                    mThumbImage.Resize(mThumbGeo, ImageMagick.FilterType.Triangle)
                End If

                mThumbImage.Grayscale(ImageMagick.PixelIntensityMethod.Average)

                Dim mPixels = mThumbImage.GetPixels()

                Dim mPixelVal As New List(Of UShort)()

                ' Iterate through all pixels
                For y As Integer = 0 To CInt(mThumbImage.Height - 1)
                    For x As Integer = 0 To CInt(mThumbImage.Width - 1)
                        Dim pixel = mPixels.GetPixel(x, y)
                        mPixelVal.Add(pixel.GetChannel(0))
                    Next
                Next

                Dim iTotal As ULong = 0
                For Each i As UShort In mPixelVal
                    iTotal += i
                Next
                Dim iAverage As Double = iTotal / mPixelVal.Count

                Dim sHashBits As New Text.StringBuilder(CInt(iThumbSize * iThumbSize))
                For Each mVal As UShort In mPixelVal
                    If (mVal >= iAverage) Then
                        sHashBits.Append("1")
                    Else
                        sHashBits.Append("0")
                    End If
                Next

                Return sHashBits.ToString()
            End Using
        End Function

        Public Function ImageCompareAverageImage(sFileA As String, sFileB As String, Optional ByVal iThumbSize As UInteger = 8) As Double
            Dim sHashA As String = m_HashCache(sFileA)
            Dim sHashB As String = m_HashCache(sFileB)

            If (sHashA Is Nothing) Then
                Using mImageA As Image = Image.FromFile(sFileA)
                    sHashA = CalculateAverageHash(mImageA, iThumbSize)
                    m_HashCache(sFileA) = sHashA
                End Using
            End If

            If (sHashB Is Nothing) Then
                Using mImageB As Image = Image.FromFile(sFileB)
                    sHashB = CalculateAverageHash(mImageB, iThumbSize)
                    m_HashCache(sFileB) = sHashB
                End Using
            End If

            Return CalculateHashSimilarity(sHashA, sHashB)
        End Function

        Public Function CalculateAverageHash(sFile As String, Optional ByVal iThumbSize As UInteger = 8) As String
            Dim sHash As String = m_HashCache(sFile)

            If (sHash Is Nothing) Then
                Using mImage As Image = Image.FromFile(sFile)
                    sHash = CalculateAverageHash(mImage, iThumbSize)
                    m_HashCache(sFile) = sHash
                End Using
            End If

            Return sHash
        End Function

        Public Function CalculateAverageHash(mImage As Image, Optional ByVal iThumbSize As UInteger = 8) As String
            Using mThumb As New Bitmap(CInt(iThumbSize), CInt(iThumbSize))
                Using mG As Graphics = Graphics.FromImage(mThumb)
                    SyncLock g_mLock
                        If (m_HighQualityHashing) Then
                            mG.InterpolationMode = Drawing.Drawing2D.InterpolationMode.Bilinear
                        Else
                            mG.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                        End If
                        mG.DrawImage(mImage, 0, 0, iThumbSize, iThumbSize)
                    End SyncLock
                End Using

                Dim iAvgBrightness As Double = GetAverageBrightness(mThumb)

                Dim sHashBits As New Text.StringBuilder(CInt(iThumbSize * iThumbSize))

                For iX As Integer = 0 To CInt(iThumbSize - 1)
                    For iY As Integer = 0 To CInt(iThumbSize - 1)
                        Dim mPB As Color = mThumb.GetPixel(iY, iX)
                        Dim iBB As Double = (CInt(mPB.R) + CInt(mPB.G) + CInt(mPB.B)) / 3.0

                        If (iBB >= iAvgBrightness) Then
                            sHashBits.Append("1")
                        Else
                            sHashBits.Append("0")
                        End If
                    Next
                Next

                Return sHashBits.ToString
            End Using

        End Function

        Private Function GetAverageBrightness(mImage As Bitmap) As Double
            Dim iTotal As Integer = 0
            For iY As Integer = 0 To mImage.Height - 1
                For iX As Integer = 0 To mImage.Width - 1
                    Dim mColor As Color = mImage.GetPixel(iX, iY)
                    iTotal += CInt((CInt(mColor.R) + CInt(mColor.G) + CInt(mColor.B)) / 3)
                Next
            Next
            Return iTotal / (mImage.Width * mImage.Height)
        End Function

        Private Function CalculateHashSimilarity(sHashA As String, sHashB As String) As Double
            If (sHashA.Length <> sHashB.Length) Then
                Return 0.0
            End If

            Dim iMatchingChars As Integer = 0
            For i As Integer = 0 To sHashA.Length - 1
                If (sHashA(i) = sHashB(i)) Then
                    iMatchingChars += 1
                End If
            Next

            Return iMatchingChars / sHashA.Length
        End Function

        Private Sub LoadCache()
            Dim iHashingMethod As Integer = m_HashingMethod
            Dim iHashSize As Integer = m_ThumbSize
            Dim sCacheFile As String = IO.Path.Combine(Application.StartupPath, String.Format("hash_cache_{0}_{1}.dat", iHashingMethod, iHashSize))

            If (Not IO.File.Exists(sCacheFile)) Then
                Return
            End If

            SyncLock g_mLock
                For Each sLine As String In IO.File.ReadAllLines(sCacheFile)
                    Dim sSplitLine As String() = sLine.Split(";"c)
                    If (sSplitLine.Length <> 3) Then
                        Continue For
                    End If

                    Dim sFileLastModified As String = sSplitLine(0)
                    Dim sFile As String = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(sSplitLine(1)))
                    Dim sImageHash As String = sSplitLine(2)

                    If (Not IO.File.Exists(sFile)) Then
                        Continue For
                    End If

                    Dim sNewFileLastModified As String = CStr(New IO.FileInfo(sFile).LastWriteTime.Ticks)
                    If (sNewFileLastModified <> sFileLastModified) Then
                        Continue For
                    End If

                    g_mHashCache(sFile) = sImageHash
                Next
            End SyncLock
        End Sub

        Private Sub SaveCache()
            Dim iHashingMethod As Integer = m_HashingMethod
            Dim iHashSize As Integer = m_ThumbSize
            Dim sCacheFile As String = IO.Path.Combine(Application.StartupPath, String.Format("hash_cache_{0}_{1}.dat", iHashingMethod, iHashSize))

            Dim sCacheBuilder As New Text.StringBuilder

            SyncLock g_mLock
                For Each mItem In g_mHashCache
                    Dim sFile As String = mItem.Key
                    Dim sImageHash As String = mItem.Value

                    If (Not IO.File.Exists(sFile)) Then
                        Continue For
                    End If

                    Dim sFileLastModified As String = CStr(New IO.FileInfo(mItem.Key).LastWriteTime.Ticks)
                    Dim sFilePath As String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sFile))

                    sCacheBuilder.AppendFormat("{0};{1};{2}", sFileLastModified, sFilePath, sImageHash).AppendLine()
                Next
            End SyncLock

            IO.File.WriteAllText(sCacheFile, sCacheBuilder.ToString)
        End Sub
    End Class

    Class ClassListViewItemImage
        Inherits ListViewItem

        ReadOnly Property m_ImageInfo As STRUC_IMAGE_INFO

        Public Sub New(_ImageInfo As STRUC_IMAGE_INFO)
            MyBase.New(New String() {
                       _ImageInfo.sFileA,
                       _ImageInfo.sFileB,
                       CStr(Math.Ceiling(_ImageInfo.iDifference * 100)),
                       ClassHelpers.FormatBytes(_ImageInfo.iFileASize),
                       ClassHelpers.FormatBytes(_ImageInfo.iFileBSize)
                       })

            m_ImageInfo = _ImageInfo
        End Sub
    End Class

    Class ClassHelpers
        Public Shared Function FormatBytes(lBytes As Double) As String
            Try
                Dim aPosForm() As String = {"Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}
                For i As Integer = aPosForm.Length - 1 To 0 Step -1
                    If lBytes > 1024 ^ i Then
                        lBytes = lBytes / (1024 ^ i)
                        Return lBytes.ToString("0.00") & " " & aPosForm(i)
                    End If
                Next i
            Catch : End Try
            Return lBytes.ToString("N") & " Bytes"
        End Function


    End Class

    Class STRUC_IMAGE_INFO
        Public sFileA As String
        Public sFileB As String
        Public iDifference As Double
        Public iFileASize As Double
        Public iFileBSize As Double

        Public Sub New(_FileA As String, _FileB As String, _Difference As Double, _FileASize As Double, _FileBSize As Double)
            sFileA = _FileA
            sFileB = _FileB
            iDifference = _Difference
            iFileASize = _FileASize
            iFileBSize = _FileBSize
        End Sub
    End Class

    Private Sub FormMain_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        CleanUp()
    End Sub

    Private Sub CleanUp()
        SetPreviewImageA(Nothing)
        SetPreviewImageB(Nothing)

        If (g_ClassScanner IsNot Nothing) Then
            g_ClassScanner.Abort()
            g_ClassScanner = Nothing
        End If
    End Sub
End Class
