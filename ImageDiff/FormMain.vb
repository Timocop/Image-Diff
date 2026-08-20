Public Class FormMain
    Public Const HASH_CACHE_VERSION As Integer = 1

    Private g_ClassScanner As ClassScanner

    Private g_bIgnoreSelection As Boolean = False

    Enum ENUM_HASHING_METHOD
        GDI
        MAGICK
        SKIA

        __MAX
    End Enum

    Structure STRUC_DIFF_MARKING_ITEM
        Dim iAlpha As Single

        Public Sub New(_Alpha As Single)
            iAlpha = _Alpha
        End Sub

        Public Overrides Function ToString() As String
            If (iAlpha <= 0.0F) Then
                Return "Hide"
            End If

            Return String.Format("Show {0}%", CInt(iAlpha * 100))
        End Function
    End Structure

    Structure STRUC_IMAGE_INFO
        Dim sFile As String
        Dim iDifference As Double
        Dim iFileSize As Double

        Dim iHash As Byte()

        Dim bIsValid As Boolean

        Sub New(_File As String, _Difference As Double, _FileSize As Double, _Hash As Byte())
            sFile = _File
            iDifference = _Difference
            iFileSize = _FileSize

            iHash = _Hash

            bIsValid = True
        End Sub
    End Structure

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
                    Return "GDI"

                Case ENUM_HASHING_METHOD.MAGICK
                    Return "Magick"

                Case ENUM_HASHING_METHOD.SKIA
                    Return "Skia"
            End Select

            Return "Unknown"
        End Function
    End Structure

    Class ClassImageTreeNode
        Inherits TreeNode

        Property m_ImageInfo As STRUC_IMAGE_INFO
    End Class

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ClassTreeViewColumns_Images.m_Columns.Add("File", 250)
        ClassTreeViewColumns_Images.m_Columns.Add("Similarity", 75)
        ClassTreeViewColumns_Images.m_Columns.Add("Size", 75)

        ClassTreeViewColumns_Images.m_TreeView.ContextMenuStrip = ContextMenuStrip_Images

        AddHandler ClassTreeViewColumns_Images.m_TreeView.AfterSelect, AddressOf TreeView_AfterNode
        AddHandler ClassTreeViewColumns_Images.m_TreeView.NodeMouseDoubleClick, AddressOf TreeView_NodeMouseDoubleClick
        AddHandler ClassTreeViewColumns_Images.m_TreeView.NodeMouseClick, AddressOf TreeView_MouseClick

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
        ComboBox_HashingMethod.SelectedIndex = ENUM_HASHING_METHOD.SKIA

        ComboBox_HashingQuality.Items.Clear()
        ComboBox_HashingQuality.Items.Add("High Quality, Slow")
        ComboBox_HashingQuality.Items.Add("Low Quality, Fast")
        ComboBox_HashingQuality.SelectedIndex = 0

        ToolStripComboBox_ShowDiffPreview.Items.Clear()
        ToolStripComboBox_ShowDiffPreview.Items.Add(New STRUC_DIFF_MARKING_ITEM(0.0F))
        ToolStripComboBox_ShowDiffPreview.Items.Add(New STRUC_DIFF_MARKING_ITEM(0.1F))
        ToolStripComboBox_ShowDiffPreview.Items.Add(New STRUC_DIFF_MARKING_ITEM(0.25F))
        ToolStripComboBox_ShowDiffPreview.Items.Add(New STRUC_DIFF_MARKING_ITEM(0.5F))
        ToolStripComboBox_ShowDiffPreview.Items.Add(New STRUC_DIFF_MARKING_ITEM(0.75F))
        ToolStripComboBox_ShowDiffPreview.Items.Add(New STRUC_DIFF_MARKING_ITEM(1.0F))
        ToolStripComboBox_ShowDiffPreview.SelectedIndex = 0

        ImageMagick.MagickNET.Initialize()
        ImageMagick.OpenCL.IsEnabled = False
    End Sub

    Private Sub TreeView_MouseClick(sender As Object, e As TreeNodeMouseClickEventArgs)
        If (e.Node Is Nothing) Then
            Return
        End If

        Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView

        mTreeView.SelectedNode = e.Node
    End Sub

    Private Sub TreeView_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs)
        Try
            If (e.Node Is Nothing) Then
                Return
            End If

            Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView

            mTreeView.SelectedNode = e.Node

            Dim mImageTreeNode = DirectCast(e.Node, ClassImageTreeNode)
            If (Not IO.File.Exists(mImageTreeNode.m_ImageInfo.sFile)) Then
                Return
            End If

            Process.Start(mImageTreeNode.m_ImageInfo.sFile)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TreeView_AfterNode(sender As Object, e As TreeViewEventArgs)
        Try
            ShowPreviewFromNode(e.Node)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private g_mPreviousSelectedColoredNodes As New List(Of KeyValuePair(Of ClassImageTreeNode, Color))
    Private Sub ShowPreviewFromNode(mSelectedNode As TreeNode)
        Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView
        Dim mFileNode = DirectCast(mSelectedNode, ClassImageTreeNode)
        Dim mParentFileNode = DirectCast(mFileNode.Parent, ClassImageTreeNode)

        Dim iDifferenceAlpha As Single = DirectCast(ToolStripComboBox_ShowDiffPreview.SelectedItem, STRUC_DIFF_MARKING_ITEM).iAlpha

        For Each mNodePair As KeyValuePair(Of ClassImageTreeNode, Color) In g_mPreviousSelectedColoredNodes
            If (mNodePair.Key Is Nothing) Then
                Continue For
            End If

            mNodePair.Key.BackColor = mNodePair.Value
        Next
        g_mPreviousSelectedColoredNodes.Clear()

        If (mFileNode Is Nothing) Then
            Return
        End If

        If (True) Then
            Dim mFoundNodes = mTreeView.Nodes.Find(mFileNode.m_ImageInfo.sFile.ToLowerInvariant, True)

            For Each mNode As ClassImageTreeNode In mFoundNodes
                If (mFileNode Is mNode) Then
                    Continue For
                End If

                g_mPreviousSelectedColoredNodes.Add(New KeyValuePair(Of ClassImageTreeNode, Color)(mNode, mNode.BackColor))
                mNode.BackColor = Color.FromKnownColor(KnownColor.PaleGreen)
            Next

            If (mParentFileNode IsNot Nothing) Then
                g_mPreviousSelectedColoredNodes.Add(New KeyValuePair(Of ClassImageTreeNode, Color)(mParentFileNode, mParentFileNode.BackColor))
                mParentFileNode.BackColor = Color.FromKnownColor(KnownColor.Yellow)
            End If
        End If

        ' Show preview 
        If (mFileNode IsNot Nothing) Then
            If (IO.File.Exists(mFileNode.m_ImageInfo.sFile)) Then
                Try
                    Using i As New Bitmap(mFileNode.m_ImageInfo.sFile)
                        SetPreviewImage(PictureBox_ImageAPreview, i, mFileNode, mParentFileNode, iDifferenceAlpha)
                    End Using
                Catch ex As Exception
                    Try
                        ' Unsupported image, try skia
                        Using i = SkiaSharp.SKBitmap.Decode(mFileNode.m_ImageInfo.sFile)
                            SetPreviewImage(PictureBox_ImageAPreview, i, mFileNode, mParentFileNode, iDifferenceAlpha)
                        End Using
                    Catch ex2 As Exception
                        Try
                            ' Unsupported image, try Magick
                            Using i As New ImageMagick.MagickImage(mFileNode.m_ImageInfo.sFile)
                                SetPreviewImage(PictureBox_ImageAPreview, i, mFileNode, mParentFileNode, iDifferenceAlpha)
                            End Using
                        Catch ex3 As Exception
                            SetPreviewImage(PictureBox_ImageAPreview, Nothing, New STRUC_IMAGE_INFO(), New STRUC_IMAGE_INFO(), 0.0F)
                        End Try
                    End Try
                End Try
            Else
                SetPreviewImage(PictureBox_ImageAPreview, Nothing, New STRUC_IMAGE_INFO(), New STRUC_IMAGE_INFO(), 0.0F)
            End If
        Else
            SetPreviewImage(PictureBox_ImageAPreview, Nothing, New STRUC_IMAGE_INFO(), New STRUC_IMAGE_INFO(), 0.0F)
        End If

        If (mParentFileNode IsNot Nothing) Then
            If (IO.File.Exists(mParentFileNode.m_ImageInfo.sFile)) Then
                Try
                    Using i As New Bitmap(mParentFileNode.m_ImageInfo.sFile)
                        SetPreviewImage(PictureBox_ImageBPreview, i, mParentFileNode, mFileNode, iDifferenceAlpha)
                    End Using
                Catch ex As Exception
                    Try
                        ' Unsupported image, try skia
                        Using i = SkiaSharp.SKBitmap.Decode(mParentFileNode.m_ImageInfo.sFile)
                            SetPreviewImage(PictureBox_ImageBPreview, i, mParentFileNode, mFileNode, iDifferenceAlpha)
                        End Using
                    Catch ex2 As Exception
                        Try
                            ' Unsupported image, try Magick
                            Using i As New ImageMagick.MagickImage(mParentFileNode.m_ImageInfo.sFile)
                                SetPreviewImage(PictureBox_ImageBPreview, i, mParentFileNode, mFileNode, iDifferenceAlpha)
                            End Using
                        Catch ex3 As Exception
                            SetPreviewImage(PictureBox_ImageBPreview, Nothing, New STRUC_IMAGE_INFO(), New STRUC_IMAGE_INFO(), 0.0F)
                        End Try
                    End Try
                End Try
            Else
                SetPreviewImage(PictureBox_ImageBPreview, Nothing, New STRUC_IMAGE_INFO(), New STRUC_IMAGE_INFO(), 0.0F)
            End If
        Else
            SetPreviewImage(PictureBox_ImageBPreview, Nothing, New STRUC_IMAGE_INFO(), New STRUC_IMAGE_INFO(), 0.0F)
        End If
    End Sub

    Private Sub LinkLabel_CacheClear_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel_CacheClear.LinkClicked
        Dim sFiles = IO.Directory.GetFiles(Application.StartupPath, "*.dat", IO.SearchOption.TopDirectoryOnly)
        Dim iTotalSize As Double = 0.0
        Dim iTotalCacheFiles As Integer = 0

        For i = 0 To sFiles.Length - 1
            If (sFiles(i).StartsWith("hash_cache")) Then
                Continue For
            End If

            If (Not IO.File.Exists(sFiles(i))) Then
                Continue For
            End If

            iTotalSize += New IO.FileInfo(sFiles(i)).Length
            iTotalCacheFiles += 1
        Next

        If (iTotalCacheFiles = 0) Then
            MessageBox.Show("No cache found", "No cache found", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If (MessageBox.Show(String.Format("Do you really want to clear all hash caches?\nTotal cache size: {0}",
                                          ClassHelpers.ClassFormating.FormatBytes(iTotalSize)).Replace("\n", Environment.NewLine),
                            "Clear hash cache",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) <> DialogResult.Yes) Then
            Return
        End If

        For i = 0 To sFiles.Length - 1
            If (sFiles(i).StartsWith("hash_cache")) Then
                Continue For
            End If

            If (Not IO.File.Exists(sFiles(i))) Then
                Continue For
            End If

            IO.File.Delete(sFiles(i))
        Next
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

            Dim mImageTreeNode = DirectCast(mTreeView.SelectedNode, ClassImageTreeNode)
            If (Not IO.File.Exists(mImageTreeNode.m_ImageInfo.sFile)) Then
                Return
            End If

            Process.Start(mImageTreeNode.m_ImageInfo.sFile)
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

            Dim mImageTreeNode = DirectCast(mTreeView.SelectedNode, ClassImageTreeNode)
            If (Not IO.File.Exists(mImageTreeNode.m_ImageInfo.sFile)) Then
                Return
            End If

            Process.Start("explorer.exe", String.Format("/select,""{0}""", mImageTreeNode.m_ImageInfo.sFile))
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

            Dim mImageTreeNode = DirectCast(mTreeView.SelectedNode, ClassImageTreeNode)
            If (Not IO.File.Exists(mImageTreeNode.m_ImageInfo.sFile)) Then
                Return
            End If

            If (MessageBox.Show(String.Format("Do you want to delete {0}?", mImageTreeNode.m_ImageInfo.sFile), "Delete files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No) Then
                Return
            End If

            IO.File.Delete(mImageTreeNode.m_ImageInfo.sFile)

            'Remove all nodes using this file
            For i = mTreeView.Nodes.Count - 1 To 0 Step -1
                Dim mRootNode = mTreeView.Nodes(i)

                For j = mRootNode.Nodes.Count - 1 To 0 Step -1
                    Dim mSubNode = mRootNode.Nodes(j)

                    Dim sSubNodeFile As String = CType(mSubNode.Tag, String())(0)
                    If (Not String.Equals(mImageTreeNode.m_ImageInfo.sFile, sSubNodeFile, StringComparison.InvariantCultureIgnoreCase)) Then
                        Continue For
                    End If

                    mRootNode.Nodes.RemoveAt(j)
                Next

                If (mRootNode.Nodes.Count < 1) Then
                    mTreeView.Nodes.RemoveAt(i)
                    Continue For
                End If

                Dim sRootNodeFile As String = CType(mRootNode.Tag, String())(0)
                If (Not String.Equals(mImageTreeNode.m_ImageInfo.sFile, sRootNodeFile, StringComparison.InvariantCultureIgnoreCase)) Then
                    Continue For
                End If

                mTreeView.Nodes.RemoveAt(i)
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ToolStripComboBox_ShowDiffPreview_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ToolStripComboBox_ShowDiffPreview.SelectedIndexChanged
        Try
            Dim mTreeView = ClassTreeViewColumns_Images.m_TreeView

            If (mTreeView.SelectedNode Is Nothing) Then
                Return
            End If

            ShowPreviewFromNode(mTreeView.SelectedNode)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Public Sub SetPreviewImage(mImageControl As PictureBox, mImage As Object, mMainImageInfo As ClassImageTreeNode, mSecondImageInfo As ClassImageTreeNode, iDifferenceAlpha As Single)
        SetPreviewImage(mImageControl,
                        mImage,
                        If(mMainImageInfo Is Nothing, New STRUC_IMAGE_INFO(), mMainImageInfo.m_ImageInfo),
                        If(mSecondImageInfo Is Nothing, New STRUC_IMAGE_INFO(), mSecondImageInfo.m_ImageInfo),
                        iDifferenceAlpha)
    End Sub

    Public Sub SetPreviewImage(mImageControl As PictureBox, mImage As Object, mMainImageInfo As STRUC_IMAGE_INFO, mSecondImageInfo As STRUC_IMAGE_INFO, iDifferenceAlpha As Single)
        If (mImageControl.Image IsNot Nothing) Then
            mImageControl.Image.Dispose()

            mImageControl.Image = Nothing
            mImageControl.Tag = Nothing
        End If

        If (mImage Is Nothing) Then
            Return
        End If

        If (Not mMainImageInfo.bIsValid) Then
            Return
        End If

        Select Case (True)
            Case (TypeOf mImage Is Image)
                Dim mNewImage = DirectCast(mImage, Image)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Save(mStream, Imaging.ImageFormat.Jpeg)
                    mStream.Position = 0

                    Dim mPreviewImage As Image = Image.FromStream(mStream)
                    If (iDifferenceAlpha > 0.0F AndAlso mSecondImageInfo.bIsValid) Then
                        DrawImageDifferenceFromHash(mPreviewImage, mMainImageInfo.iHash, mSecondImageInfo.iHash, iDifferenceAlpha)
                    End If

                    mImageControl.Image = mPreviewImage
                    mImageControl.Tag = mMainImageInfo.sFile
                End Using

            Case (TypeOf mImage Is ImageMagick.MagickImage)
                Dim mNewImage = DirectCast(mImage, ImageMagick.MagickImage)

                Using mStream As New IO.MemoryStream()
                    mNewImage.Write(mStream, ImageMagick.MagickFormat.Jpg)
                    mStream.Position = 0

                    Dim mPreviewImage As Image = Image.FromStream(mStream)
                    If (iDifferenceAlpha > 0.0F AndAlso mSecondImageInfo.bIsValid) Then
                        DrawImageDifferenceFromHash(mPreviewImage, mMainImageInfo.iHash, mSecondImageInfo.iHash, iDifferenceAlpha)
                    End If

                    mImageControl.Image = mPreviewImage
                    mImageControl.Tag = mMainImageInfo.sFile
                End Using

            Case (TypeOf mImage Is SkiaSharp.SKBitmap)
                Dim mNewImage = DirectCast(mImage, SkiaSharp.SKBitmap)

                Using mStream As New IO.MemoryStream()
                    Dim mData = mNewImage.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 95)
                    mData.SaveTo(mStream)
                    mStream.Position = 0

                    Dim mPreviewImage As Image = Image.FromStream(mStream)
                    If (iDifferenceAlpha > 0.0F AndAlso mSecondImageInfo.bIsValid) Then
                        DrawImageDifferenceFromHash(mPreviewImage, mMainImageInfo.iHash, mSecondImageInfo.iHash, iDifferenceAlpha)
                    End If

                    mImageControl.Image = mPreviewImage
                    mImageControl.Tag = mMainImageInfo.sFile
                End Using
        End Select
    End Sub

    Private Sub DrawImageDifferenceFromHash(mImage As Image, iHashA As Byte(), iHashB As Byte(), iAlpha As Single)
        Dim iThumbSizeA As Integer = CInt(Math.Sqrt(iHashA.Length))
        Dim iThumbSizeB As Integer = CInt(Math.Sqrt(iHashB.Length))

        If (iHashA.Length <> iHashB.Length) Then
            Return
        End If

        If ((iThumbSizeA * iThumbSizeA) <> iHashA.Length) Then
            Return
        End If

        If ((iThumbSizeB * iThumbSizeB) <> iHashB.Length) Then
            Return
        End If

        Dim iBlockWidth As Single = CSng(mImage.Width / iThumbSizeA)
        Dim iBlockHeight As Single = CSng(mImage.Height / iThumbSizeA)

        Using mG As Graphics = Graphics.FromImage(mImage)
            Using mBrush As New SolidBrush(Color.FromArgb(CInt(iAlpha * 255), 255, 0, 0))
                For i = 0 To iHashA.Length - 1
                    If (iHashA(i) <> iHashB(i)) Then
                        Dim iRow As Integer = (i \ iThumbSizeA)
                        Dim iCol As Integer = (i Mod iThumbSizeA)

                        Dim iX As Single = (iCol * iBlockWidth)
                        Dim iY As Single = (iRow * iBlockHeight)

                        mG.FillRectangle(mBrush, iX, iY, iBlockWidth, iBlockHeight)
                    End If
                Next
            End Using
        End Using
    End Sub

    Class ClassScanner
        Private g_fFormMain As FormMain

        Private g_mScannerThread As ClassThread

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
        Private g_bHashCacheChanged As Boolean = False

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

            g_mScannerThread = New ClassThread(AddressOf ThreadMainScanner, Nothing)
            g_mScannerThread.Start()
        End Sub

        Public Sub Abort()
            If (m_Scanning) Then
                g_mScannerThread.Abort()
            End If
        End Sub

        Public Sub Join()
            If (m_Scanning) Then
                g_mScannerThread.Join()
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
                    Dim i As Byte() = {}
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
                    g_bHashCacheChanged = True
                End SyncLock
            End Set
        End Property

        Public Function GetHashCacheItems() As KeyValuePair(Of String, Byte())()
            SyncLock g_mHashCacheLock
                Return g_mHashCache.ToArray
            End SyncLock
        End Function

        Public Function GetHashCacheItems(bOnlyDirectory As Boolean) As KeyValuePair(Of String, Byte())()
            SyncLock g_mHashCacheLock
                If (Not bOnlyDirectory) Then
                    Return GetHashCacheItems()
                End If

                Dim mItems As New List(Of KeyValuePair(Of String, Byte()))

                Dim sDirectory As String = m_Directory

                For Each mItem In g_mHashCache
                    Dim sFileDirectory As String = IO.Path.GetDirectoryName(mItem.Key)

                    If (m_IncludeSubDirectories) Then
                        If (Not sFileDirectory.StartsWith(sDirectory, StringComparison.InvariantCultureIgnoreCase)) Then
                            Continue For
                        End If
                    Else
                        If (Not sFileDirectory.Equals(sDirectory, StringComparison.InvariantCultureIgnoreCase)) Then
                            Continue For
                        End If
                    End If

                    mItems.Add(mItem)
                Next

                Return mItems.ToArray
            End SyncLock
        End Function

        Class ClassThread
            Private g_mThread As Threading.Thread = Nothing
            Private ReadOnly g_mCancelToken As New Threading.CancellationTokenSource

            Public Delegate Sub ThreadAction(mCancelToken As Threading.CancellationTokenSource, mObject As Object)

            Public Sub New(mThreadAction As ThreadAction, mObject As Object)
                g_mThread = New Threading.Thread(Sub()
                                                     Try
                                                         mThreadAction.Invoke(g_mCancelToken, mObject)
                                                     Catch ex As ThreadAbortException
                                                     Catch ex As Exception
                                                     End Try
                                                 End Sub)
                g_mThread.IsBackground = True
            End Sub

            Public Sub Start()
                If (g_mThread Is Nothing) Then
                    Return
                End If

                g_mThread.Start()
            End Sub

            Public Sub Abort()
                If (g_mThread Is Nothing) Then
                    Return
                End If

                g_mCancelToken.Cancel()
            End Sub

            Public Sub Join()
                If (g_mThread Is Nothing) Then
                    Return
                End If

                g_mCancelToken.Cancel()

                g_mThread.Join()
            End Sub

            Public Function IsAlive() As Boolean
                If (g_mThread Is Nothing) Then
                    Return False
                End If

                Return g_mThread.IsAlive
            End Function

            Class ThreadAbortException
                Inherits ArgumentException
            End Class
        End Class

        Private Sub ThreadMainScanner(mCancelToken As Threading.CancellationTokenSource, mObject As Object)
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
                Dim mThreads As New List(Of ClassThread)
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
                                Catch ex As ClassThread.ThreadAbortException
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

                                Dim mThread As New ClassThread(AddressOf ThreadSubScanner, mData)
                                mThread.Start()

                                mThreads.Add(mThread)
                            Next

                            Try
                                While True
                                    If (mCancelToken.IsCancellationRequested) Then
                                        Throw New ClassThread.ThreadAbortException
                                    End If

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
                            Catch ex As ClassThread.ThreadAbortException
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

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Text = "Preparing...")

                Dim mRootNodeCollection As New ClassImageTreeNode

                Dim mImageInfoTotal = mImageInfo.ToArray
                For Each mInfoItem In mImageInfoTotal
                    If (mInfoItem.Value Is Nothing OrElse mInfoItem.Value.Count < 2) Then
                        Continue For
                    End If

                    Dim mRootFileItem = mInfoItem.Value.Values(0)

                    Dim mRootTreeNode As New ClassImageTreeNode

                    mRootTreeNode.BackColor = Color.FromKnownColor(KnownColor.GradientActiveCaption)
                    mRootTreeNode.Name = mRootFileItem.sFile.ToLowerInvariant
                    mRootTreeNode.Tag = New String() {
                                                        mRootFileItem.sFile,
                                                        String.Format("{0} %", CStr(Math.Floor(mRootFileItem.iDifference * 100))),
                                                        ClassHelpers.ClassFormating.FormatBytes(mRootFileItem.iFileSize)}
                    mRootTreeNode.m_ImageInfo = mRootFileItem


                    For i = 1 To mInfoItem.Value.Values.Count - 1
                        Dim mSubFileItem = mInfoItem.Value.Values(i)

                        Dim mSubTreeNode As New ClassImageTreeNode
                        mSubTreeNode.Name = mSubFileItem.sFile.ToLowerInvariant
                        mSubTreeNode.Tag = New String() {
                                                        mSubFileItem.sFile,
                                                        String.Format("{0} %", CStr(Math.Floor(mSubFileItem.iDifference * 100))),
                                                        ClassHelpers.ClassFormating.FormatBytes(mSubFileItem.iFileSize)}
                        mSubTreeNode.m_ImageInfo = mSubFileItem

                        mRootTreeNode.Nodes.Add(mSubTreeNode)
                    Next

                    mRootNodeCollection.Nodes.Add(mRootTreeNode)
                Next

                ClassHelpers.ClassTreeNodes.CompactNodeCollection(mRootNodeCollection.Nodes)
                ClassHelpers.ClassTreeNodes.SortNodeCollection(mRootNodeCollection.Nodes)

                g_fFormMain.BeginInvoke(Sub()
                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Visible = False
                                            g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Nodes.Clear()

                                            For Each mNode As ClassImageTreeNode In mRootNodeCollection.Nodes
                                                g_fFormMain.ClassTreeViewColumns_Images.m_TreeView.Nodes.Add(mNode)
                                            Next

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
            Catch ex As ClassThread.ThreadAbortException
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Visible = False)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Visible = False)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.Button_Select.Text = "Select")

                Throw
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripStatusLabel_Progress.Visible = False)
                g_fFormMain.BeginInvoke(Sub() g_fFormMain.ToolStripProgressBar_Progress.Visible = False)

                g_fFormMain.BeginInvoke(Sub() g_fFormMain.Button_Select.Text = "Select")
            End Try
        End Sub

        Private Sub ThreadSubScanner(mCancelToken As Threading.CancellationTokenSource, mObject As Object)
            Dim mData = DirectCast(mObject, Dictionary(Of String, Object))

            Dim mFilesThreads = DirectCast(mData("FilesThreads"), Queue(Of String))
            Dim sTotalFiles = DirectCast(mData("TotalFiles"), String())
            Dim mImageInfo = DirectCast(mData("ImageInfo"), Dictionary(Of String, Dictionary(Of String, STRUC_IMAGE_INFO)))
            Dim mThreadInfo = DirectCast(mData("ThreadInfo"), Dictionary(Of String, Object))

            Dim iMaxImageDiff = DirectCast(mData("MaxImageDiff"), Integer)
            Dim bIsPreHashing = DirectCast(mData("IsPreHashing"), Boolean)
            Dim iHashingMethod = DirectCast(mData("HashingMethod"), ENUM_HASHING_METHOD)
            Dim iThumbSize = DirectCast(mData("ThumbSize"), Integer)

            Dim MAX_FILE_SIZE As Integer = 100 * 1024 * 1024

            Dim mHashCacheItems As New Dictionary(Of String, KeyValuePair(Of Integer, Byte()))(StringComparison.InvariantCultureIgnoreCase)

            If (Not bIsPreHashing) Then
                Dim mCachedItems = GetHashCacheItems(True)
                For Each mItem In mCachedItems
                    Dim iPositiveBits As Integer = 0

                    For i = 0 To mItem.Value.Length - 1
                        If (mItem.Value(i) > 0) Then
                            iPositiveBits += 1
                        End If
                    Next

                    mHashCacheItems(mItem.Key) = New KeyValuePair(Of Integer, Byte())(iPositiveBits, mItem.Value)
                Next
            End If

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

                    If (mCancelToken.IsCancellationRequested) Then
                        Throw New ClassThread.ThreadAbortException
                    End If

                    If (bIsPreHashing) Then
                        If (Not IO.File.Exists(sFileA)) Then
                            Continue While
                        End If

                        Dim mHashA As Byte() = m_HashCache(sFileA)
                        If (mHashA.Length > 0) Then
                            Continue While
                        End If

                        Dim mFileAInfo As New IO.FileInfo(sFileA)
                        If (mFileAInfo.Length > MAX_FILE_SIZE) Then
                            Continue While
                        End If

                        Try
                            Select Case (iHashingMethod)
                                Case ENUM_HASHING_METHOD.SKIA
                                    Using i = SkiaSharp.SKBitmap.Decode(sFileA)
                                        If (i Is Nothing) Then
                                            Continue While
                                        End If

                                        ' Success
                                    End Using

                                Case ENUM_HASHING_METHOD.MAGICK
                                    Using mImage As New ImageMagick.MagickImage(sFileA)
                                        ' Success
                                    End Using

                                Case ENUM_HASHING_METHOD.GDI
                                    Using mImage As Image = Image.FromFile(sFileA)
                                        ' Success
                                    End Using

                                Case Else
                                    Continue While
                            End Select
                        Catch ex As ClassThread.ThreadAbortException
                            Throw
                        Catch ex As Exception
                            Continue While
                        End Try

                        Select Case (iHashingMethod)
                            Case ENUM_HASHING_METHOD.SKIA
                                Dim mHasher As New ClassHasherSkia
                                m_HashCache(sFileA) = mHasher.GetHash(sFileA, CUInt(iThumbSize), m_HighQualityHashing)

                            Case ENUM_HASHING_METHOD.MAGICK
                                Dim mHasher As New ClassHasherMagick
                                m_HashCache(sFileA) = mHasher.GetHash(sFileA, CUInt(iThumbSize), m_HighQualityHashing)

                            Case ENUM_HASHING_METHOD.GDI
                                Dim mHasher As New ClassHasherGdi
                                m_HashCache(sFileA) = mHasher.GetHash(sFileA, CUInt(iThumbSize), m_HighQualityHashing)

                            Case Else
                                Continue While
                        End Select
                    Else
                        Dim mHashA As KeyValuePair(Of Integer, Byte()) = Nothing
                        If (Not mHashCacheItems.TryGetValue(sFileA, mHashA)) Then
                            Continue While
                        End If

                        mHashCacheItems.Remove(sFileA)

                        Dim iHashA As Byte() = mHashA.Value
                        If (iHashA.Length = 0) Then
                            Continue While
                        End If

                        Dim iMaxThreshold As Double = (iMaxImageDiff / 100)

                        For Each mFileB In mHashCacheItems
                            Try
                                Dim mHashB = mFileB.Value
                                Dim sFileB As String = mFileB.Key

                                If (mCancelToken.IsCancellationRequested) Then
                                    Throw New ClassThread.ThreadAbortException
                                End If

                                Dim iHashB As Byte() = mHashB.Value
                                If (iHashB.Length = 0) Then
                                    Continue For
                                End If

                                ' Check possible thresholds
                                If (True) Then
                                    Dim iHashADiff As Double = mHashA.Key / iHashA.Length
                                    Dim iHashBDiff As Double = mHashB.Key / iHashB.Length

                                    If (iHashADiff > 0 AndAlso iHashADiff > iHashBDiff) Then
                                        If ((iHashBDiff / iHashADiff) < iMaxThreshold) Then
                                            Continue For
                                        End If
                                    End If

                                    If (iHashBDiff > 0 AndAlso iHashBDiff > iHashADiff) Then
                                        If ((iHashADiff / iHashBDiff) < iMaxThreshold) Then
                                            Continue For
                                        End If
                                    End If
                                End If

                                Dim iAvgDiff As Double = 0.0

                                Select Case (iHashingMethod)
                                    Case ENUM_HASHING_METHOD.SKIA
                                        Dim mHasher As New ClassHasherSkia
                                        iAvgDiff = mHasher.GetSimilarity(iHashA, iHashB, iMaxThreshold)

                                    Case ENUM_HASHING_METHOD.MAGICK
                                        Dim mHasher As New ClassHasherMagick
                                        iAvgDiff = mHasher.GetSimilarity(iHashA, iHashB, iMaxThreshold)

                                    Case ENUM_HASHING_METHOD.GDI
                                        Dim mHasher As New ClassHasherGdi
                                        iAvgDiff = mHasher.GetSimilarity(iHashA, iHashB, iMaxThreshold)

                                    Case Else
                                        Continue For
                                End Select

                                If (iAvgDiff < iMaxThreshold) Then
                                    Continue For
                                End If

                                SyncLock g_mThreadLock
                                    ' Skip if the pair already exists
                                    If (mImageInfo.ContainsKey(sFileB) AndAlso mImageInfo(sFileB).ContainsKey(sFileA)) Then
                                        Continue For
                                    End If

                                    ' Skip if the pair already exists
                                    If (mImageInfo.ContainsKey(sFileA) AndAlso mImageInfo(sFileA).ContainsKey(sFileB)) Then
                                        Continue For
                                    End If

                                    If (Not mImageInfo.ContainsKey(sFileA)) Then
                                        mImageInfo(sFileA) = New Dictionary(Of String, STRUC_IMAGE_INFO)
                                        mImageInfo(sFileA)(sFileA) = New STRUC_IMAGE_INFO(sFileA, 1.0, New IO.FileInfo(sFileA).Length, iHashA)
                                    End If

                                    mImageInfo(sFileA)(sFileB) = New STRUC_IMAGE_INFO(sFileB, iAvgDiff, New IO.FileInfo(sFileB).Length, iHashB)
                                End SyncLock

                            Catch ex As ClassThread.ThreadAbortException
                                Throw
                            Catch ex As Exception
                                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            End Try
                        Next
                    End If
                Catch ex As ClassThread.ThreadAbortException
                    Throw
                Catch ex As Exception
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End While
        End Sub

        Interface IImageHasher(Of T)
            Function GetHash(sFile As String, iThumbSize As UInteger, bHighQuality As Boolean) As Byte()
            Function GetHash(mImage As T, iThumbSize As UInteger, bHighQuality As Boolean) As Byte()
            Function GetHash(mImage As Byte(), iThumbSize As UInteger, bHighQuality As Boolean) As Byte()

            Function GetSimilarity(iHashA As Byte(), iHashB As Byte()) As Double
            Function GetSimilarity(iHashA As Byte(), iHashB As Byte(), iThreshold As Double) As Double
        End Interface

        Class ClassHasherSHA256
            Implements IImageHasher(Of Object)

            Public Function GetHash(sFile As String, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of Object).GetHash
                Using mStream As New IO.FileStream(sFile, IO.FileMode.Open, IO.FileAccess.Read)
                    Using mHash As New Security.Cryptography.SHA256Managed()
                        Return HashToBit(mHash.ComputeHash(mStream))
                    End Using
                End Using
            End Function

            Public Function GetHash(mImage As Object, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of Object).GetHash
                Throw New ArgumentException("Operation not supported")
            End Function

            Public Function GetHash(mImage As Byte(), iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of Object).GetHash
                Using mStream As New IO.MemoryStream(mImage)
                    Using mHash As New Security.Cryptography.SHA256Managed()
                        Return HashToBit(mHash.ComputeHash(mStream))
                    End Using
                End Using
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte()) As Double Implements IImageHasher(Of Object).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, 1.0)
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte(), iThreshold As Double) As Double Implements IImageHasher(Of Object).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, iThreshold)
            End Function

            Public Function GetSimilarityInternal(iHashA As Byte(), iHashB As Byte(), iThreshhold As Double) As Double
                If (iHashA.Length <> iHashB.Length) Then
                    Return 0.0
                End If

                For i = 0 To iHashA.Length - 1
                    If (iHashA(i) <> iHashB(i)) Then
                        Return 0.0
                    End If
                Next

                Return 1.0
            End Function

            Private Function HashToBit(iHash As Byte()) As Byte()
                Dim iHashBits(iHash.Length * 8 - 1) As Byte

                Dim l As Integer = 0

                For i = 0 To iHash.Length - 1
                    For j = 0 To 8 - 1
                        If ((iHash(i) And (1 << j)) = (1 << j)) Then
                            iHashBits(l) = 1
                        Else
                            iHashBits(l) = 0
                        End If

                        l += 1
                    Next
                Next

                Return iHashBits
            End Function
        End Class

        Class ClassHasherSkia
            Implements IImageHasher(Of SkiaSharp.SKBitmap)

            Public Function GetHash(sFile As String, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of SkiaSharp.SKBitmap).GetHash
                Using mThumbImage = SkiaSharp.SKBitmap.Decode(sFile)
                    Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                End Using
            End Function

            Public Function GetHash(mImage As SkiaSharp.SKBitmap, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of SkiaSharp.SKBitmap).GetHash
                Return CalculateHashInternal(mImage, iThumbSize, bHighQuality)
            End Function

            Public Function GetHash(mImage As Byte(), iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of SkiaSharp.SKBitmap).GetHash
                Using mThumbImage = SkiaSharp.SKBitmap.Decode(mImage)
                    Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                End Using
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte()) As Double Implements IImageHasher(Of SkiaSharp.SKBitmap).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, 1.0)
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte(), iThreshold As Double) As Double Implements IImageHasher(Of SkiaSharp.SKBitmap).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, iThreshold)
            End Function

            Public Function GetSimilarityInternal(iHashA As Byte(), iHashB As Byte(), iThreshhold As Double) As Double
                If (iHashA.Length <> iHashB.Length) Then
                    Return 0.0
                End If

                Dim iTotalBits As Integer = iHashA.Length
                Dim iMatchingBits As Integer = 0
                Dim iMinRequiredMatches As Integer = CInt(Math.Ceiling(iTotalBits * iThreshhold))
                Dim iMaxAllowedMismatches As Integer = iTotalBits - iMinRequiredMatches

                For i = 0 To iHashA.Length - 1
                    If (iHashA(i) = iHashB(i)) Then
                        iMatchingBits += 1
                    End If

                    Dim iCount As Integer = i + 1
                    Dim iCurrentMatches As Integer = iCount - iMatchingBits
                    Dim iRemainingBits As Integer = iTotalBits - iCount

                    If (iCurrentMatches > iMaxAllowedMismatches) Then
                        Return 0.0
                    End If

                    If (iMatchingBits + iRemainingBits < iMinRequiredMatches) Then
                        Return 0.0
                    End If
                Next

                Return iMatchingBits / iHashA.Length
            End Function

            Private Function CalculateHashInternal(mThumbImage As SkiaSharp.SKBitmap, iThumbSize As UInteger, bHighQuality As Boolean) As Byte()
                Dim mResizedThumb As SkiaSharp.SKBitmap = Nothing
                Try
                    If (bHighQuality) Then
                        mResizedThumb = ResizeImage(mThumbImage, iThumbSize, iThumbSize, bHighQuality)
                    Else
                        mResizedThumb = ResizeImage(mThumbImage, iThumbSize, iThumbSize, bHighQuality)
                    End If

                    Using mGrayImage = ConvertToGrayscale(mResizedThumb, bHighQuality)
                        Dim mPixelVal As New List(Of Byte)()
                        For y As Integer = 0 To mGrayImage.Height - 1
                            For x As Integer = 0 To mGrayImage.Width - 1
                                Dim mPixel = mGrayImage.GetPixel(x, y)
                                mPixelVal.Add(mPixel.Red)
                            Next
                        Next

                        Dim iTotal As ULong = 0
                        For Each i As Byte In mPixelVal
                            iTotal += i
                        Next
                        Dim iAvg As Double = (iTotal / mPixelVal.Count)

                        Dim iHash(mPixelVal.Count - 1) As Byte
                        For i = 0 To mPixelVal.Count - 1
                            If (mPixelVal(i) >= iAvg) Then
                                iHash(i) = 1
                            Else
                                iHash(i) = 0
                            End If
                        Next

                        Return iHash
                    End Using
                Finally
                    If (mResizedThumb IsNot Nothing AndAlso mResizedThumb IsNot mThumbImage) Then
                        mResizedThumb.Dispose()
                    End If
                End Try
            End Function

            Private Function ResizeImage(mSource As SkiaSharp.SKBitmap, iWidth As UInteger, iHeight As UInteger, bHighQuality As Boolean) As SkiaSharp.SKBitmap
                Dim mSampling As SkiaSharp.SKSamplingOptions

                If (bHighQuality) Then
                    mSampling = New SkiaSharp.SKSamplingOptions(SkiaSharp.SKCubicResampler.Mitchell)
                Else
                    mSampling = New SkiaSharp.SKSamplingOptions(SkiaSharp.SKFilterMode.Linear)
                End If

                Dim mDest As New SkiaSharp.SKBitmap(CInt(iWidth), CInt(iHeight))
                Using mSurface As New SkiaSharp.SKCanvas(mDest)
                    mSurface.DrawBitmap(
                        mSource,
                        New SkiaSharp.SKRect(0, 0, iWidth, iHeight),
                        mSampling
                    )
                End Using
                Return mDest
            End Function

            Private Function ConvertToGrayscale(mSource As SkiaSharp.SKBitmap, bHighQuality As Boolean) As SkiaSharp.SKBitmap
                Dim mSampling As SkiaSharp.SKSamplingOptions

                If (bHighQuality) Then
                    mSampling = New SkiaSharp.SKSamplingOptions(SkiaSharp.SKCubicResampler.Mitchell)
                Else
                    mSampling = New SkiaSharp.SKSamplingOptions(SkiaSharp.SKFilterMode.Linear)
                End If

                Dim mDest As New SkiaSharp.SKBitmap(mSource.Width, mSource.Height)
                Using mCanvas As New SkiaSharp.SKCanvas(mDest)
                    Using mColorFilter As SkiaSharp.SKColorFilter = SkiaSharp.SKColorFilter.CreateColorMatrix(New Single() {
                            0.333F, 0.333F, 0.333F, 0, 0,   ' Red channel
                            0.333F, 0.333F, 0.333F, 0, 0,   ' Green channel  
                            0.333F, 0.333F, 0.333F, 0, 0,   ' Blue channel
                            0, 0, 0, 1, 0                    ' Alpha channel
                        })
                        Using paint As New SkiaSharp.SKPaint()
                            paint.ColorFilter = mColorFilter

                            mCanvas.DrawBitmap(mSource, 0, 0, mSampling, paint)
                        End Using
                    End Using
                End Using
                Return mDest
            End Function
        End Class

        Class ClassHasherMagick
            Implements IImageHasher(Of ImageMagick.MagickImage)

            Public Function GetHash(sFile As String, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of ImageMagick.MagickImage).GetHash
                Using mThumbImage As New ImageMagick.MagickImage(sFile)
                    Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                End Using
            End Function

            Public Function GetHash(mImage As ImageMagick.MagickImage, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of ImageMagick.MagickImage).GetHash
                Using mThumbImage As New ImageMagick.MagickImage(mImage)
                    Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                End Using
            End Function

            Public Function GetHash(mImage As Byte(), iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of ImageMagick.MagickImage).GetHash
                Using mThumbImage As New ImageMagick.MagickImage(mImage)
                    Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                End Using
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte()) As Double Implements IImageHasher(Of ImageMagick.MagickImage).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, 1.0)
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte(), iThreshold As Double) As Double Implements IImageHasher(Of ImageMagick.MagickImage).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, iThreshold)
            End Function

            Public Function GetSimilarityInternal(iHashA As Byte(), iHashB As Byte(), iThreshhold As Double) As Double
                If (iHashA.Length <> iHashB.Length) Then
                    Return 0.0
                End If

                Dim iTotalBits As Integer = iHashA.Length
                Dim iMatchingBits As Integer = 0
                Dim iMinRequiredMatches As Integer = CInt(Math.Ceiling(iTotalBits * iThreshhold))
                Dim iMaxAllowedMismatches As Integer = iTotalBits - iMinRequiredMatches

                For i = 0 To iHashA.Length - 1
                    If (iHashA(i) = iHashB(i)) Then
                        iMatchingBits += 1
                    End If

                    Dim iCount As Integer = i + 1
                    Dim iCurrentMatches As Integer = iCount - iMatchingBits
                    Dim iRemainingBits As Integer = iTotalBits - iCount

                    If (iCurrentMatches > iMaxAllowedMismatches) Then
                        Return 0.0
                    End If

                    If (iMatchingBits + iRemainingBits < iMinRequiredMatches) Then
                        Return 0.0
                    End If
                Next

                Return iMatchingBits / iHashA.Length
            End Function

            Private Function CalculateHashInternal(mThumbImage As ImageMagick.MagickImage, iThumbSize As UInteger, bHighQuality As Boolean) As Byte()
                Dim mThumbGeo As New ImageMagick.MagickGeometry(iThumbSize, iThumbSize)
                mThumbGeo.IgnoreAspectRatio = True

                If (bHighQuality) Then
                    mThumbImage.Resize(mThumbGeo, ImageMagick.FilterType.Lanczos)
                Else
                    mThumbImage.Resize(mThumbGeo, ImageMagick.FilterType.Triangle)
                End If

                mThumbImage.Grayscale(ImageMagick.PixelIntensityMethod.Average)

                Dim mPixels = mThumbImage.GetPixels()

                Dim mPixelVal As New List(Of UShort)()

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

                Dim iHashBits(mPixelVal.Count - 1) As Byte
                For i = 0 To mPixelVal.Count - 1
                    If (mPixelVal(i) >= iAverage) Then
                        iHashBits(i) = 1
                    Else
                        iHashBits(i) = 0
                    End If
                Next

                Return iHashBits
            End Function
        End Class

        Class ClassHasherGdi
            Implements IImageHasher(Of Image)

            Private Shared g_mDrawingLock As New Object

            Public Function GetHash(sFile As String, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of Image).GetHash
                Using mThumbImage As Image = Image.FromFile(sFile)
                    Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                End Using
            End Function

            Public Function GetHash(mImage As Image, iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of Image).GetHash
                Return CalculateHashInternal(mImage, iThumbSize, bHighQuality)
            End Function

            Public Function GetHash(mImage As Byte(), iThumbSize As UInteger, bHighQuality As Boolean) As Byte() Implements IImageHasher(Of Image).GetHash
                Using mMemorySteam As New IO.MemoryStream(mImage)
                    Using mThumbImage As Image = Image.FromStream(mMemorySteam)
                        Return CalculateHashInternal(mThumbImage, iThumbSize, bHighQuality)
                    End Using
                End Using
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte()) As Double Implements IImageHasher(Of Image).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, 1.0)
            End Function

            Public Function GetSimilarity(iHashA As Byte(), iHashB As Byte(), iThreshold As Double) As Double Implements IImageHasher(Of Image).GetSimilarity
                Return GetSimilarityInternal(iHashA, iHashB, iThreshold)
            End Function

            Public Function GetSimilarityInternal(iHashA As Byte(), iHashB As Byte(), iThreshhold As Double) As Double
                If (iHashA.Length <> iHashB.Length) Then
                    Return 0.0
                End If

                Dim iTotalBits As Integer = iHashA.Length
                Dim iMatchingBits As Integer = 0
                Dim iMinRequiredMatches As Integer = CInt(Math.Ceiling(iTotalBits * iThreshhold))
                Dim iMaxAllowedMismatches As Integer = iTotalBits - iMinRequiredMatches

                For i = 0 To iHashA.Length - 1
                    If (iHashA(i) = iHashB(i)) Then
                        iMatchingBits += 1
                    End If

                    Dim iCount As Integer = i + 1
                    Dim iCurrentMatches As Integer = iCount - iMatchingBits
                    Dim iRemainingBits As Integer = iTotalBits - iCount

                    If (iCurrentMatches > iMaxAllowedMismatches) Then
                        Return 0.0
                    End If

                    If (iMatchingBits + iRemainingBits < iMinRequiredMatches) Then
                        Return 0.0
                    End If
                Next

                Return iMatchingBits / iHashA.Length
            End Function

            Private Function CalculateHashInternal(mThumbImage As Image, iThumbSize As UInteger, bHighQuality As Boolean) As Byte()
                Using mThumbBitmap As New Bitmap(CInt(iThumbSize), CInt(iThumbSize))
                    Using mG As Graphics = Graphics.FromImage(mThumbBitmap)
                        SyncLock g_mDrawingLock
                            If (bHighQuality) Then
                                mG.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
                            Else
                                mG.InterpolationMode = Drawing.Drawing2D.InterpolationMode.Bilinear
                            End If
                            mG.DrawImage(mThumbImage, 0, 0, iThumbSize, iThumbSize)
                        End SyncLock
                    End Using

                    Dim iAvgBrightness As Double = GetAverageBrightness(mThumbBitmap)

                    Dim iHashBits(CInt(iThumbSize * iThumbSize) - 1) As Byte
                    Dim iHashBitCount As Integer = 0

                    For iX As Integer = 0 To CInt(iThumbSize - 1)
                        For iY As Integer = 0 To CInt(iThumbSize - 1)
                            Dim mPB As Color = mThumbBitmap.GetPixel(iY, iX)
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
        End Class

        Private Sub LoadCache()
            Dim iHashingMethod As Integer = m_HashingMethod
            Dim iHashSize As Integer = m_ThumbSize
            Dim bHighQualityHashing As Boolean = m_HighQualityHashing
            Dim sCacheFile As String = IO.Path.Combine(Application.StartupPath, String.Format("hash_cache_{0}_{1}_{2}.dat", iHashingMethod, iHashSize, If(bHighQualityHashing, 1, 0)))

            If (Not IO.File.Exists(sCacheFile)) Then
                Return
            End If

            SyncLock g_mHashCacheLock
                g_mHashCache.Clear()

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

                g_bHashCacheChanged = False
            End SyncLock
        End Sub

        Private Sub SaveCache()
            Dim iHashingMethod As Integer = m_HashingMethod
            Dim iHashSize As Integer = m_ThumbSize
            Dim bHighQualityHashing As Boolean = m_HighQualityHashing
            Dim sCacheFile As String = IO.Path.Combine(Application.StartupPath, String.Format("hash_cache_{0}_{1}_{2}.dat", iHashingMethod, iHashSize, If(bHighQualityHashing, 1, 0)))

            SyncLock g_mHashCacheLock
                If (Not g_bHashCacheChanged) Then
                    Return
                End If

                Using mStream As New IO.MemoryStream()
                    Using mBinReader As New IO.BinaryWriter(mStream)
                        Using mFileStream As New IO.FileStream(sCacheFile, IO.FileMode.Create, IO.FileAccess.ReadWrite)
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
        Class ClassFormating
            Public Shared Function FormatBytes(lBytes As Double) As String
                Try
                    Dim aPosForm() As String = {"Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"}
                    For i = aPosForm.Length - 1 To 0 Step -1
                        If lBytes > 1024 ^ i Then
                            lBytes = lBytes / (1024 ^ i)
                            Return lBytes.ToString("0.00") & " " & aPosForm(i)
                        End If
                    Next
                Catch ex As Exception
                End Try

                Return lBytes.ToString("N") & " Bytes"
            End Function
        End Class

        Class ClassTreeNodes
            Public Shared Sub CompactNodeCollection(mNodes As TreeNodeCollection)
                If (mNodes.Count < 1) Then
                    Return
                End If

                Dim mAllNodes As New List(Of TreeNode)
                For Each node As TreeNode In mNodes
                    mAllNodes.Add(node)
                Next

                For i = 0 To mAllNodes.Count - 1
                    Dim mCurrentNode As TreeNode = mAllNodes(i)

                    If (mCurrentNode Is Nothing) Then
                        Continue For
                    End If

                    ' If we are the only parents node, ignore
                    If (mCurrentNode.Parent IsNot Nothing AndAlso mCurrentNode.Parent.Nodes.Count = 1) Then
                        Continue For
                    End If

                    Dim mRootNode As TreeNode = GetRootNode(mCurrentNode)
                    If (mRootNode Is Nothing) Then
                        Continue For
                    End If

                    Dim mFoundNodes As New List(Of TreeNode)
                    FindNodesByName(mRootNode, mCurrentNode.Name, mFoundNodes)

                    ' Skep when none or itself is found
                    If (mFoundNodes.Count <= 1) Then
                        Continue For
                    End If

                    Dim mTargetNode As TreeNode = Nothing
                    For Each mFoundNode As TreeNode In mFoundNodes
                        If (mFoundNode Is mCurrentNode) Then
                            Continue For
                        End If

                        ' Avoid circular references
                        If (IsAncestorOf(mCurrentNode, mFoundNode)) Then
                            Continue For
                        End If

                        mTargetNode = mFoundNode
                        Exit For
                    Next

                    If (mTargetNode Is Nothing) Then
                        Continue For
                    End If

                    Dim mNodesToMove As New List(Of TreeNode)
                    For Each mNode As TreeNode In mCurrentNode.Nodes
                        mNodesToMove.Add(mNode)
                    Next

                    For Each mNode As TreeNode In mNodesToMove
                        mNode.Remove()
                        mTargetNode.Nodes.Add(mNode)
                    Next

                    mCurrentNode.Remove()
                Next

                For Each mNode As TreeNode In mNodes
                    If (mNode Is Nothing) Then
                        Continue For
                    End If

                    If (mNode.Nodes.Count < 1) Then
                        Continue For
                    End If

                    CompactNodeCollection(mNode.Nodes)
                Next
            End Sub

            Private Shared Function GetRootNode(mNode As TreeNode) As TreeNode
                If (mNode Is Nothing) Then
                    Return Nothing
                End If

                Dim mCurrentNode As TreeNode = mNode

                While (mCurrentNode.Parent IsNot Nothing)
                    mCurrentNode = mCurrentNode.Parent
                End While

                Return mCurrentNode
            End Function

            Private Shared Function IsAncestorOf(mAncestor As TreeNode, mDescendant As TreeNode) As Boolean
                Dim mCurrentNode As TreeNode = mDescendant.Parent

                While (mCurrentNode IsNot Nothing)
                    If (mCurrentNode Is mAncestor) Then
                        Return True
                    End If

                    mCurrentNode = mCurrentNode.Parent
                End While

                Return False
            End Function

            Private Shared Sub FindNodesByName(mRootNode As TreeNode, sName As String, mFoundNodes As List(Of TreeNode))
                If (mRootNode Is Nothing) Then
                    Return
                End If

                If (mRootNode.Name = sName) Then
                    mFoundNodes.Add(mRootNode)
                End If

                For Each mNode As TreeNode In mRootNode.Nodes
                    FindNodesByName(mNode, sName, mFoundNodes)
                Next
            End Sub

            Public Shared Sub SortNodeCollection(mNodes As TreeNodeCollection)
                If (mNodes.Count < 1) Then
                    Return
                End If

                Dim mSortedNodes As New List(Of ClassImageTreeNode)
                For Each mNode As ClassImageTreeNode In mNodes
                    mSortedNodes.Add(mNode)
                Next

                mSortedNodes.Sort(Function(x As ClassImageTreeNode, y As ClassImageTreeNode)
                                      Return y.m_ImageInfo.iDifference.CompareTo(x.m_ImageInfo.iDifference)
                                  End Function)

                mNodes.Clear()
                For Each mNode As ClassImageTreeNode In mSortedNodes
                    mNodes.Add(mNode)

                    SortNodeCollection(mNode.Nodes)
                Next
            End Sub

        End Class
    End Class

    Private Sub FormMain_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        CleanUp()
    End Sub

    Private Sub CleanUp()
        RemoveHandler ClassTreeViewColumns_Images.m_TreeView.AfterSelect, AddressOf TreeView_AfterNode
        RemoveHandler ClassTreeViewColumns_Images.m_TreeView.NodeMouseDoubleClick, AddressOf TreeView_NodeMouseDoubleClick
        RemoveHandler ClassTreeViewColumns_Images.m_TreeView.NodeMouseClick, AddressOf TreeView_MouseClick

        If (g_ClassScanner IsNot Nothing) Then
            g_ClassScanner.Abort()
            g_ClassScanner.Join()
            g_ClassScanner = Nothing
        End If

        If (PictureBox_ImageAPreview.Image IsNot Nothing) Then
            PictureBox_ImageAPreview.Image.Dispose()

            PictureBox_ImageAPreview.Image = Nothing
            PictureBox_ImageAPreview.Tag = Nothing
        End If

        If (PictureBox_ImageBPreview.Image IsNot Nothing) Then
            PictureBox_ImageBPreview.Image.Dispose()

            PictureBox_ImageBPreview.Image = Nothing
            PictureBox_ImageBPreview.Tag = Nothing
        End If
    End Sub
End Class
