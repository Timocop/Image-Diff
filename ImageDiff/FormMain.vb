Public Class FormMain
    Public Const HASH_CACHE_VERSION As Integer = 1

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
        ClassTreeViewColumns_Images.m_Columns.Add("", 50)
        ClassTreeViewColumns_Images.m_Columns.Add("File", 250)
        ClassTreeViewColumns_Images.m_Columns.Add("Difference", 75)
        ClassTreeViewColumns_Images.m_Columns.Add("Size", 75)

        ClassTreeViewColumns_Images.m_TreeView.ContextMenuStrip = ContextMenuStrip_Images

        AddHandler ClassTreeViewColumns_Images.m_TreeView.AfterSelect, AddressOf TreeView_AfterNode

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

    Public Sub SetPreviewImageA(mImage As Object, sFile As String)
        If (PictureBox_ImageAPreview.Image IsNot Nothing) Then
            PictureBox_ImageAPreview.Image.Dispose()

            PictureBox_ImageAPreview.Image = Nothing
            PictureBox_ImageAPreview.Tag = Nothing
        End If

        If (mImage Is Nothing) Then
            Return
        End If

        Select Case (True)
            Case (TypeOf mImage Is Image)
                Dim mNewImage = DirectCast(mImage, Image)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Save(mStream, Imaging.ImageFormat.Jpeg)
                    mStream.Position = 0

                    PictureBox_ImageAPreview.Image = Image.FromStream(mStream)
                    PictureBox_ImageAPreview.Tag = sFile
                End Using

            Case (TypeOf mImage Is ImageMagick.MagickImage)
                Dim mNewImage = DirectCast(mImage, ImageMagick.MagickImage)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Write(mStream, ImageMagick.MagickFormat.Jpg)
                    mStream.Position = 0

                    PictureBox_ImageAPreview.Image = Image.FromStream(mStream)
                    PictureBox_ImageAPreview.Tag = sFile
                End Using
        End Select
    End Sub

    Public Sub SetPreviewImageB(mImage As Object, sFile As String)
        If (PictureBox_ImageBPreview.Image IsNot Nothing) Then
            PictureBox_ImageBPreview.Image.Dispose()

            PictureBox_ImageBPreview.Image = Nothing
            PictureBox_ImageBPreview.Tag = Nothing
        End If

        If (mImage Is Nothing) Then
            Return
        End If

        Select Case (True)
            Case (TypeOf mImage Is Image)
                Dim mNewImage = DirectCast(mImage, Image)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Save(mStream, Imaging.ImageFormat.Jpeg)
                    mStream.Position = 0

                    PictureBox_ImageBPreview.Image = Image.FromStream(mStream)
                    PictureBox_ImageBPreview.Tag = sFile
                End Using

            Case (TypeOf mImage Is ImageMagick.MagickImage)
                Dim mNewImage = DirectCast(mImage, ImageMagick.MagickImage)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Write(mStream, ImageMagick.MagickFormat.Jpg)
                    mStream.Position = 0

                    PictureBox_ImageBPreview.Image = Image.FromStream(mStream)
                    PictureBox_ImageBPreview.Tag = sFile
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

    Private Sub PictureBox_ImageAPreview_Click(sender As Object, e As EventArgs) Handles PictureBox_ImageAPreview.Click
        Try
            If (PictureBox_ImageAPreview.Tag Is Nothing) Then
                Return
            End If

            Dim sFile As String = CStr(PictureBox_ImageAPreview.Tag)
            If (Not IO.File.Exists(sFile)) Then
                Return
            End If

            Process.Start(sFile)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub PictureBox_ImageBPreview_Click(sender As Object, e As EventArgs) Handles PictureBox_ImageBPreview.Click
        Try
            If (PictureBox_ImageBPreview.Tag Is Nothing) Then
                Return
            End If

            Dim sFile As String = CStr(PictureBox_ImageBPreview.Tag)
            If (Not IO.File.Exists(sFile)) Then
                Return
            End If

            Process.Start(sFile)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Private Sub ToolStripMenuItem_Open_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_Open.Click
        Try
            Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView

            If (mTreeView.SelectedNode Is Nothing) Then
                Return
            End If

            Dim sFile As String = CType(mTreeView.SelectedNode.Tag, String())(0)
            If (Not IO.File.Exists(sFile)) Then
                Return
            End If

            Process.Start(sFile)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ToolStripMenuItem_OpenExplorer_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_OpenExplorer.Click
        Try
            Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView

            If (mTreeView.SelectedNode Is Nothing) Then
                Return
            End If

            Dim sFile As String = CType(mTreeView.SelectedNode.Tag, String())(0)
            If (Not IO.File.Exists(sFile)) Then
                Return
            End If

            Process.Start("explorer.exe", String.Format("/select,""{0}""", sFile))
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ToolStripMenuItem_Remove_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem_Remove.Click
        Try
            Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView

            If (mTreeView.SelectedNode Is Nothing) Then
                Return
            End If

            Dim sFile As String = CType(mTreeView.SelectedNode.Tag, String())(0)
            If (Not IO.File.Exists(sFile)) Then
                Return
            End If

            If (MessageBox.Show(String.Format("Do you want to delete {0}?", sFile), "Delete files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No) Then
                Return
            End If

            IO.File.Delete(sFile)

            'Remove all nodes using this file
            For i = mTreeView.Nodes.Count - 1 To 0 Step -1
                Dim mRootNode = mTreeView.Nodes(i)

                For j = mRootNode.Nodes.Count - 1 To 0 Step -1
                    Dim mSubNode = mRootNode.Nodes(j)

                    Dim sSubNodeFile As String = CType(mSubNode.Tag, String())(0)
                    If (Not String.Equals(sFile, sSubNodeFile, StringComparison.InvariantCultureIgnoreCase)) Then
                        Continue For
                    End If

                    mRootNode.Nodes.RemoveAt(j)
                Next

                If (mRootNode.Nodes.Count < 1) Then
                    mTreeView.Nodes.RemoveAt(i)
                    Continue For
                End If

                Dim sRootNodeFile As String = CType(mRootNode.Tag, String())(0)
                If (Not String.Equals(sFile, sRootNodeFile, StringComparison.InvariantCultureIgnoreCase)) Then
                    Continue For
                End If

                mTreeView.Nodes.RemoveAt(i)
            Next
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

        Private g_mThreadLock As New Object
        Private g_mHashCache As New Dictionary(Of String, Byte())(StringComparer.InvariantCultureIgnoreCase)
        Private g_mHashCacheLock As New Object

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

        Private Property m_HashCache(sFile As String) As Byte()
            Get
                SyncLock g_mHashCacheLock
                    Dim i = New Byte() {}
                    If (g_mHashCache.TryGetValue(sFile, i)) Then
                        Return i
                    Else
                        Return {}
                    End If
                End SyncLock
            End Get
            Set(value As Byte())
                SyncLock g_mHashCacheLock
                    g_mHashCache(sFile) = value
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
                Dim mImageInfo As New Dictionary(Of String, Dictionary(Of String, STRUC_IMAGE_INFO))(StringComparison.InvariantCultureIgnoreCase)
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
                                Try
                                    Select Case (j)
                                        Case 0
                                            g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = "Loading cache...")

                                            LoadCache()
                                        Case 1
                                            g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = "Saving cache...")

                                            SaveCache()
                                    End Select
                                Catch ex As Threading.ThreadAbortException
                                    Throw
                                Catch ex As Exception
                                    MessageBox.Show(ex.Message, "Unable to use save/load hash cache", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                End Try
                            End If

                            SyncLock g_mThreadLock
                                mThreadInfo("Files") = 0
                            End SyncLock

                            For i = 0 To sFiles.Length - 1
                                mFilesThreads.Enqueue(sFiles(i))
                            Next

                            For i = 0 To iThreads - 1
                                Dim mData As New Dictionary(Of String, Object)
                                mData("FilesThreads") = mFilesThreads
                                mData("TotalFiles") = sFiles
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

                                    SyncLock g_mThreadLock
                                        iFiles = CInt(mThreadInfo("Files"))
                                    End SyncLock

                                    ' Calculate time left and rate per seconds
                                    mLastFilesPerSec.Enqueue(iFiles - iFilesLast)
                                    iFilesPerSec = mLastFilesPerSec.Average()
                                    iFilesLast = iFiles

                                    Dim mTimeLeft As New TimeSpan(0, 0, CInt((sFiles.Length - iFiles) / Math.Max(iFilesPerSec, 1)))

                                    While (mLastFilesPerSec.Count > 30)
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
                    If (m_HashCache(sFile).Length > 0) Then
                        Continue For
                    End If

                    mFailedFiles.Add(sFile)
                Next

                Dim mDuplicateFiles = mImageInfo.ToArray

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
                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Visible = False
                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Nodes.Clear()

                                            Dim mRootNodeCollection As New List(Of TreeNode)

                                            For Each mFileItem In mDuplicateFiles
                                                If (mFileItem.Value Is Nothing OrElse mFileItem.Value.Count < 2) Then
                                                    Continue For
                                                End If

                                                Dim mSubNodeCollection As New List(Of TreeNode)

                                                Dim mRootFileItem = mFileItem.Value.Values(0)

                                                Dim mRootTreeNode As New TreeNode(" > ")
                                                mRootTreeNode.NodeFont = New Font(g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Font, FontStyle.Bold)
                                                mRootTreeNode.Tag = New String() {
                                                    mRootFileItem.sFile,
                                                    CStr(Math.Ceiling(mRootFileItem.iDifference * 100)),
                                                    ClassHelpers.FormatBytes(mRootFileItem.iFileSize)}

                                                For i = 1 To mFileItem.Value.Values.Count - 1
                                                    Dim mSubFileItem = mFileItem.Value.Values(i)

                                                    Dim mSubTreeNode As New TreeNode("")
                                                    mSubTreeNode.Tag = New String() {
                                                        mSubFileItem.sFile,
                                                        CStr(Math.Ceiling(mSubFileItem.iDifference * 100)),
                                                        ClassHelpers.FormatBytes(mSubFileItem.iFileSize)}

                                                    mSubNodeCollection.Add(mSubTreeNode)
                                                Next

                                                mSubNodeCollection.Sort(Function(a As TreeNode, b As TreeNode)
                                                                            Dim iDiffA As Integer = CInt(CType(a.Tag, String())(1))
                                                                            Dim iDiffB As Integer = CInt(CType(b.Tag, String())(1))

                                                                            Return iDiffB.CompareTo(iDiffA)
                                                                        End Function)


                                                mRootTreeNode.Nodes.AddRange(mSubNodeCollection.ToArray)
                                                mRootNodeCollection.Add(mRootTreeNode)
                                            Next

                                            mRootNodeCollection.Sort(Function(a As TreeNode, b As TreeNode) As Integer
                                                                         Dim sFileA As String = CType(a.Tag, String())(0)
                                                                         Dim sFileB As String = CType(b.Tag, String())(0)

                                                                         Return sFileA.CompareTo(sFileB)
                                                                     End Function)

                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Nodes.AddRange(mRootNodeCollection.ToArray)
                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.ExpandAll()

                                            If (g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Nodes.Count > 0) Then
                                                g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Nodes(0).EnsureVisible()
                                            End If

                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Visible = True
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
            Dim sTotalFiles = DirectCast(mData("TotalFiles"), String())
            Dim mImageInfo = DirectCast(mData("ImageInfo"), Dictionary(Of String, Dictionary(Of String, STRUC_IMAGE_INFO)))
            Dim mThreadInfo = DirectCast(mData("ThreadInfo"), Dictionary(Of String, Object))

            Dim iMaxImageDiff = DirectCast(mData("MaxImageDiff"), Integer)
            Dim bIsPreHashing = DirectCast(mData("IsPreHashing"), Boolean)
            Dim iHashingMethod = DirectCast(mData("HashingMethod"), ENUM_HASHING_METHOD)
            Dim iThumbSize = DirectCast(mData("ThumbSize"), Integer)

            Dim mTotalFilesList As New HashSet(Of String)(StringComparison.InvariantCultureIgnoreCase)
            For i = 0 To sTotalFiles.Length - 1
                mTotalFilesList.Add(sTotalFiles(i))
            Next

            Dim MAX_FILE_SIZE As Integer = 100 * 1024 * 1024

            While True
                Try
                    Dim sFileA As String

                    SyncLock g_mThreadLock
                        If (mFilesThreads.Count < 1) Then
                            Exit While
                        End If

                        sFileA = mFilesThreads.Dequeue()

                        mThreadInfo("Files") = CInt(mThreadInfo("Files")) + 1
                    End SyncLock

                    mTotalFilesList.Remove(sFileA)

                    If (bIsPreHashing) Then
                        If (Not IO.File.Exists(sFileA)) Then
                            Continue While
                        End If

                        Dim sHashA As Byte() = m_HashCache(sFileA)
                        If (sHashA.Length > 0) Then
                            Continue While
                        End If

                        Dim mFileAInfo As New IO.FileInfo(sFileA)
                        If (mFileAInfo.Length > MAX_FILE_SIZE) Then
                            Continue While
                        End If

                        Try
                            Select Case (iHashingMethod)
                                Case ENUM_HASHING_METHOD.MAGICK
                                    Using mImage As New ImageMagick.MagickImage(sFileA)
                                        ' Success
                                    End Using

                                Case Else
                                    Using mImage As Image = Image.FromFile(sFileA)
                                        ' Success
                                    End Using
                            End Select
                        Catch ex As Threading.ThreadAbortException
                            Throw
                        Catch ex As Exception
                            Continue While
                        End Try

                        Select Case (iHashingMethod)
                            Case ENUM_HASHING_METHOD.MAGICK
                                m_HashCache(sFileA) = CalculatePerceptualHash(sFileA, CUInt(iThumbSize))
                            Case Else
                                m_HashCache(sFileA) = CalculateAverageHash(sFileA, CUInt(iThumbSize))
                        End Select
                    Else
                        Dim sHashA As Byte() = m_HashCache(sFileA)
                        If (sHashA.Length = 0) Then
                            Continue While
                        End If

                        For Each sFileB As String In mTotalFilesList
                            Try
                                Dim sHashB As Byte() = m_HashCache(sFileB)
                                If (sHashB.Length = 0) Then
                                    Continue For
                                End If

                                Dim iAvgDiff = CalculateHashSimilarity(sHashA, sHashB)
                                If (iAvgDiff < (iMaxImageDiff / 100)) Then
                                    Continue For
                                End If

                                SyncLock g_mThreadLock
                                    If (Not mImageInfo.ContainsKey(sFileA)) Then
                                        mImageInfo(sFileA) = New Dictionary(Of String, STRUC_IMAGE_INFO)
                                        mImageInfo(sFileA)(sFileA) = New STRUC_IMAGE_INFO(sFileA, 1, New IO.FileInfo(sFileA).Length)
                                    End If

                                    mImageInfo(sFileA)(sFileB) = New STRUC_IMAGE_INFO(sFileB, iAvgDiff, New IO.FileInfo(sFileB).Length)
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

        Public Function CalculatePerceptualHash(sFile As String, Optional ByVal iThumbSize As UInteger = 8) As Byte()
            Using mImage As New ImageMagick.MagickImage(sFile)
                Return CalculatePerceptualHash(mImage, iThumbSize)
            End Using
        End Function

        Public Function CalculatePerceptualHash(mImage As ImageMagick.MagickImage, Optional ByVal iThumbSize As UInteger = 8) As Byte()
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

                Dim iHashBits(CInt(iThumbSize * iThumbSize) - 1) As Byte
                Dim iHashBitCount As Integer = 0

                For Each mVal As UShort In mPixelVal
                    If (mVal >= iAverage) Then
                        iHashBits(iHashBitCount) = 1
                        iHashBitCount += 1
                    Else
                        iHashBits(iHashBitCount) = 0
                        iHashBitCount += 1
                    End If
                Next

                Return iHashBits
            End Using
        End Function

        Public Function CalculateAverageHash(sFile As String, Optional ByVal iThumbSize As UInteger = 8) As Byte()
            Using mImage As Image = Image.FromFile(sFile)
                Return CalculateAverageHash(mImage, iThumbSize)
            End Using
        End Function

        Public Function CalculateAverageHash(mImage As Image, Optional ByVal iThumbSize As UInteger = 8) As Byte()
            Using mThumb As New Bitmap(CInt(iThumbSize), CInt(iThumbSize))
                Using mG As Graphics = Graphics.FromImage(mThumb)
                    SyncLock g_mThreadLock
                        If (m_HighQualityHashing) Then
                            mG.InterpolationMode = Drawing.Drawing2D.InterpolationMode.Bilinear
                        Else
                            mG.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                        End If
                        mG.DrawImage(mImage, 0, 0, iThumbSize, iThumbSize)
                    End SyncLock
                End Using

                Dim iAvgBrightness As Double = GetAverageBrightness(mThumb)

                Dim iHashBits(CInt(iThumbSize * iThumbSize) - 1) As Byte
                Dim iHashBitCount As Integer = 0

                For iX As Integer = 0 To CInt(iThumbSize - 1)
                    For iY As Integer = 0 To CInt(iThumbSize - 1)
                        Dim mPB As Color = mThumb.GetPixel(iY, iX)
                        Dim iBB As Double = (CInt(mPB.R) + CInt(mPB.G) + CInt(mPB.B)) / 3.0

                        If (iBB >= iAvgBrightness) Then
                            iHashBits(iHashBitCount) = 1
                            iHashBitCount += 1
                        Else
                            iHashBits(iHashBitCount) = 0
                            iHashBitCount += 1
                        End If
                    Next
                Next

                Return iHashBits
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

        Private Function CalculateHashSimilarity(iHashA As Byte(), iHashB As Byte()) As Double
            If (iHashA.Length <> iHashB.Length) Then
                Return 0.0
            End If

            Dim iMatchingBits As Integer = 0
            For i = 0 To iHashA.Length - 1
                If (iHashA(i) = iHashB(i)) Then
                    iMatchingBits += 1
                End If
            Next

            Return iMatchingBits / iHashA.Length
        End Function

        Private Sub LoadCache()
            Dim iHashingMethod As Integer = m_HashingMethod
            Dim iHashSize As Integer = m_ThumbSize
            Dim bHighQualityHashing As Boolean = m_HighQualityHashing
            Dim sCacheFile As String = IO.Path.Combine(Application.StartupPath, String.Format("hash_cache_{0}_{1}_{2}.dat", iHashingMethod, iHashSize, If(bHighQualityHashing, 1, 0)))

            If (Not IO.File.Exists(sCacheFile)) Then
                Return
            End If

            SyncLock g_mHashCacheLock
                Using mStream As New IO.MemoryStream()
                    Using mBinWriter As New IO.BinaryReader(mStream)
                        Using mFileStream As New IO.FileStream(sCacheFile, IO.FileMode.Open, IO.FileAccess.Read)
                            mFileStream.CopyTo(mStream)

                            mStream.Position = 0

                            '[Checksum:Int32][Version:Int32]
                            ' ... [FilePathLength:Int32][FilePath:Byte()][HashMethod:Int32][FileModified:Int64][HashSize:Int32][Hash:Byte()]

                            Dim iConfigChecksum As Integer = mBinWriter.ReadInt32() '[Checksum:Int32]

                            ' Check checksum
                            If (True) Then
                                Dim iPostChecksumPos As Long = mStream.Position
                                Dim iChecksum As Integer = 0

                                While (mStream.Position < mStream.Length)
                                    iChecksum = (iChecksum * 101) + mStream.ReadByte
                                End While

                                If (iConfigChecksum <> iChecksum) Then
                                    Throw New ArgumentException("Hash cache checksum failed")
                                End If

                                mStream.Position = iPostChecksumPos
                            End If

                            Dim iConfigVersion As Integer = mBinWriter.ReadInt32() '[Version:Int32]
                            If (HASH_CACHE_VERSION <> iConfigVersion) Then
                                Return
                            End If

                            While (mStream.Position < mStream.Length)
                                Dim iFilePathLen As Integer = mBinWriter.ReadInt32() '[FilePathLength:Int32]
                                If (iFilePathLen < 1) Then
                                    Throw New ArgumentException("File path size cant be zero")
                                End If

                                Dim sFilePath As String = System.Text.Encoding.UTF8.GetString(mBinWriter.ReadBytes(iFilePathLen)) '[FilePath:Byte()]
                                Dim iFileHashMethod As Integer = mBinWriter.ReadInt32() '[HashMethod:Int32]
                                Dim iFileModifiedTimestamp As Long = mBinWriter.ReadInt64() '[FileModified:Int64]
                                Dim iHashByteCount As Integer = mBinWriter.ReadInt32() '[HashSize:Int32]

                                Dim iByteCount As Integer = 0
                                Dim iHashBytes As New List(Of Byte)
                                Dim mBitReader As New ClassBitReader(mStream)
                                For i = 0 To iHashByteCount - 1
                                    iHashBytes.AddRange(mBitReader.ReadBit()) '[Hash:Byte()]
                                Next
                                iByteCount = mBitReader.m_TotalByteCount

                                If (iHashingMethod <> iFileHashMethod) Then
                                    Continue While
                                End If

                                If (iHashByteCount <> iByteCount) Then
                                    Continue While
                                End If

                                If ((iHashSize * iHashSize) <> iHashBytes.Count) Then
                                    Continue While
                                End If

                                If (Not IO.File.Exists(sFilePath)) Then
                                    Continue While
                                End If

                                Dim iFileLastModified As Long = New IO.FileInfo(sFilePath).LastWriteTime.Ticks
                                If (iFileLastModified <> iFileModifiedTimestamp) Then
                                    Continue While
                                End If

                                g_mHashCache(sFilePath) = iHashBytes.ToArray
                            End While
                        End Using
                    End Using
                End Using
            End SyncLock
        End Sub

        Private Sub SaveCache()
            Dim iHashingMethod As Integer = m_HashingMethod
            Dim iHashSize As Integer = m_ThumbSize
            Dim bHighQualityHashing As Boolean = m_HighQualityHashing
            Dim sCacheFile As String = IO.Path.Combine(Application.StartupPath, String.Format("hash_cache_{0}_{1}_{2}.dat", iHashingMethod, iHashSize, If(bHighQualityHashing, 1, 0)))

            SyncLock g_mHashCacheLock
                Using mStream As New IO.MemoryStream()
                    Using mBinReader As New IO.BinaryWriter(mStream)
                        Using mFileStream As New IO.FileStream(sCacheFile, IO.FileMode.OpenOrCreate, IO.FileAccess.ReadWrite)
                            '[Checksum:Int32][Version:Int32]
                            ' ... [FilePathLength:Int32][FilePath:Byte()][HashMethod:Int32][FileModified:Int64][HashSize:Int32][Hash:Byte()]

                            'Checksum not yet known
                            Dim iChecksum As Integer = 0
                            mBinReader.Write(iChecksum) '[Checksum:Int32] 
                            Dim iPostChecksumPos As Long = mStream.Position

                            mBinReader.Write(HASH_CACHE_VERSION) '[Version:Int32]

                            For Each mItem In g_mHashCache
                                Dim sFile As String = mItem.Key
                                Dim iHash As Byte() = mItem.Value

                                If (Not IO.File.Exists(sFile)) Then
                                    Continue For
                                End If

                                If (sFile.Length < 1) Then
                                    Continue For
                                End If

                                If (iHash.Length < 1) Then
                                    Continue For
                                End If

                                Dim iFilePath As Byte() = System.Text.Encoding.UTF8.GetBytes(sFile)
                                Dim iFilePathLen As Integer = iFilePath.Length
                                Dim iFileLastModified As Long = New IO.FileInfo(sFile).LastWriteTime.Ticks

                                mBinReader.Write(iFilePathLen) '[FilePathLength:Int32]
                                mBinReader.Write(iFilePath) '[FilePath:Byte()]
                                mBinReader.Write(iHashingMethod) '[HashMethod:Int32]
                                mBinReader.Write(iFileLastModified) '[FileModified:Int64]

                                Dim iPreHashPos As Long = mStream.Position

                                ' We dont know the bits yet
                                Dim iByteCount As Integer = 0
                                mBinReader.Write(iByteCount) ' [HashSize:Int32]

                                ' Compress byte array into bool
                                Dim mBitWriter As New ClassBitWriter(mStream)
                                For i = 0 To iHash.Length - 1
                                    mBitWriter.WriteBit(iHash(i)) '[Hash:Byte()]
                                Next
                                mBitWriter.Flush()
                                iByteCount = mBitWriter.m_TotalByteCount

                                ' Go back and set bit count
                                If (True) Then
                                    Dim iCurrentPos = mStream.Position

                                    mStream.Position = iPreHashPos

                                    mBinReader.Write(iByteCount) ' [HashSize:Int32]

                                    mStream.Position = iCurrentPos
                                End If
                            Next

                            ' Calculate the checksum 
                            If (True) Then
                                mStream.Position = iPostChecksumPos

                                iChecksum = 0
                                While (mStream.Position < mStream.Length)
                                    iChecksum = (iChecksum * 101) + mStream.ReadByte()
                                End While

                                mStream.Position = 0

                                ' Go back and set checksum
                                mBinReader.Write(iChecksum) '[Checksum:Int32] 
                            End If

                            mStream.Position = 0

                            mStream.CopyTo(mFileStream)
                        End Using
                    End Using
                End Using
            End SyncLock
        End Sub

        Class ClassBitWriter
            Private g_mStream As IO.Stream
            Private g_iCurrentByte As Byte = 0
            Private g_iBitIndex As Integer = 0
            Private g_iTotalByteCount As Integer = 0

            Sub New(_Stream As IO.Stream)
                g_mStream = _Stream
            End Sub

            Public Sub WriteBit(iValue As Byte)
                If (iValue > 0) Then
                    g_iCurrentByte = g_iCurrentByte Or CByte(1 << g_iBitIndex)
                End If

                g_iBitIndex += 1

                If (g_iBitIndex = 8) Then
                    g_mStream.WriteByte(g_iCurrentByte)
                    g_iTotalByteCount += 1
                    g_iCurrentByte = 0
                    g_iBitIndex = 0
                End If
            End Sub

            Public Sub Flush()
                If (g_iBitIndex > 0) Then
                    g_mStream.WriteByte(g_iCurrentByte)
                    g_iTotalByteCount += 1
                    g_iCurrentByte = 0
                    g_iBitIndex = 0
                End If
            End Sub

            ReadOnly Property m_TotalByteCount As Integer
                Get
                    Return g_iTotalByteCount
                End Get
            End Property
        End Class

        Class ClassBitReader
            Private g_mStream As IO.Stream
            Private g_iTotalByteCount As Integer = 0

            Sub New(_Stream As IO.Stream)
                g_mStream = _Stream
            End Sub

            Public Function ReadBit() As Byte()
                Dim iByteSize = 8
                Dim iCompressedByte As Byte = CByte(g_mStream.ReadByte)
                Dim iByte(iByteSize - 1) As Byte

                For i = 0 To iByte.Length - 1
                    If ((iCompressedByte And (1 << i)) = (1 << i)) Then
                        iByte(i) = 1
                    Else
                        iByte(i) = 0
                    End If
                Next

                g_iTotalByteCount += 1

                Return iByte
            End Function

            ReadOnly Property m_TotalByteCount As Integer
                Get
                    Return g_iTotalByteCount
                End Get
            End Property
        End Class
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

    Structure STRUC_IMAGE_INFO
        Dim sFile As String
        Dim iDifference As Double
        Dim iFileSize As Double

        Sub New(_File As String, _Difference As Double, _FileSize As Double)
            sFile = _File
            iDifference = _Difference
            iFileSize = _FileSize
        End Sub
    End Structure

    Private Sub TreeView_AfterNode(sender As Object, e As TreeViewEventArgs)
        Try
            Dim mFileNode = e.Node
            Dim mParentFileNode = mFileNode.Parent

            If (mFileNode Is Nothing) Then
                Return
            End If

            Dim sFileA As String = Nothing
            Dim sFileB As String = Nothing

            If (mFileNode IsNot Nothing) Then
                If (mParentFileNode Is Nothing) Then
                    sFileA = DirectCast(mFileNode.Tag, String())(0)
                    sFileB = Nothing
                Else
                    sFileB = DirectCast(mFileNode.Tag, String())(0)
                    sFileA = DirectCast(mParentFileNode.Tag, String())(0)
                End If
            End If

            If (sFileA IsNot Nothing) Then
                If (IO.File.Exists(sFileA)) Then
                    Try
                        Using i As New Bitmap(sFileA)
                            SetPreviewImageA(i, sFileA)
                        End Using
                    Catch ex As Exception
                        Try
                            ' Unsupported image, try Magick
                            Using i As New ImageMagick.MagickImage(sFileA)
                                SetPreviewImageA(i, sFileA)
                            End Using
                        Catch ex2 As Exception
                            SetPreviewImageA(Nothing, Nothing)
                        End Try
                    End Try
                Else
                    SetPreviewImageA(Nothing, Nothing)
                End If
            Else
                SetPreviewImageA(Nothing, Nothing)
            End If

            If (sFileB IsNot Nothing) Then
                If (IO.File.Exists(sFileB)) Then
                    Try
                        Using i As New Bitmap(sFileB)
                            SetPreviewImageB(i, sFileB)
                        End Using
                    Catch ex As Exception
                        Try
                            ' Unsupported image, try Magick
                            Using i As New ImageMagick.MagickImage(sFileB)
                                SetPreviewImageB(i, sFileB)
                            End Using
                        Catch ex2 As Exception
                            SetPreviewImageB(Nothing, Nothing)
                        End Try
                    End Try
                Else
                    SetPreviewImageB(Nothing, Nothing)
                End If
            Else
                SetPreviewImageB(Nothing, Nothing)
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FormMain_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        CleanUp()
    End Sub

    Private Sub CleanUp()
        RemoveHandler ClassTreeViewColumns_Images.m_TreeView.AfterSelect, AddressOf TreeView_AfterNode

        SetPreviewImageA(Nothing, Nothing)
        SetPreviewImageB(Nothing, Nothing)

        If (g_ClassScanner IsNot Nothing) Then
            g_ClassScanner.Abort()
            g_ClassScanner = Nothing
        End If
    End Sub
End Class
