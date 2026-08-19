<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If (disposing) Then
                CleanUp()
            End If

            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormMain))
        Me.Button_Select = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.LinkLabel_CacheClear = New System.Windows.Forms.LinkLabel()
        Me.CheckBox_Caching = New System.Windows.Forms.CheckBox()
        Me.ComboBox_HashingQuality = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ComboBox_HashingMethod = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.ComboBox_HashingSize = New System.Windows.Forms.ComboBox()
        Me.CheckBox_CheckSubDirectorys = New System.Windows.Forms.CheckBox()
        Me.NumericUpDown_Threads = New System.Windows.Forms.NumericUpDown()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.NumericUpDown_MaxImageDiff = New System.Windows.Forms.NumericUpDown()
        Me.TextBox_Path = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ContextMenuStrip_Images = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem_Open = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem_OpenExplorer = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripComboBox_ShowDiffPreview = New System.Windows.Forms.ToolStripComboBox()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripMenuItem_Remove = New System.Windows.Forms.ToolStripMenuItem()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar_Progress = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel_Progress = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader7 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ToolTip_Info = New System.Windows.Forms.ToolTip(Me.components)
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.PictureBox_ImageAPreview = New System.Windows.Forms.PictureBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.PictureBox_ImageBPreview = New System.Windows.Forms.PictureBox()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.ClassTreeViewColumns_Images = New ImageDiff.ClassTreeViewColumns()
        Me.ClassListViewEx_FailedFiles = New ImageDiff.ClassListViewEx()
        Me.ColumnHeader11 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.GroupBox1.SuspendLayout()
        CType(Me.NumericUpDown_Threads, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumericUpDown_MaxImageDiff, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip_Images.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        CType(Me.PictureBox_ImageAPreview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        CType(Me.PictureBox_ImageBPreview, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'Button_Select
        '
        Me.Button_Select.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button_Select.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.Button_Select.Location = New System.Drawing.Point(668, 21)
        Me.Button_Select.Name = "Button_Select"
        Me.Button_Select.Size = New System.Drawing.Size(86, 23)
        Me.Button_Select.TabIndex = 0
        Me.Button_Select.Text = "Select"
        Me.Button_Select.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.LinkLabel_CacheClear)
        Me.GroupBox1.Controls.Add(Me.CheckBox_Caching)
        Me.GroupBox1.Controls.Add(Me.ComboBox_HashingQuality)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.ComboBox_HashingMethod)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.ComboBox_HashingSize)
        Me.GroupBox1.Controls.Add(Me.CheckBox_CheckSubDirectorys)
        Me.GroupBox1.Controls.Add(Me.NumericUpDown_Threads)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.NumericUpDown_MaxImageDiff)
        Me.GroupBox1.Controls.Add(Me.TextBox_Path)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.Button_Select)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(760, 132)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Image directory and settings"
        '
        'LinkLabel_CacheClear
        '
        Me.LinkLabel_CacheClear.ActiveLinkColor = System.Drawing.SystemColors.HotTrack
        Me.LinkLabel_CacheClear.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LinkLabel_CacheClear.AutoSize = True
        Me.LinkLabel_CacheClear.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.LinkLabel_CacheClear.LinkColor = System.Drawing.SystemColors.HotTrack
        Me.LinkLabel_CacheClear.Location = New System.Drawing.Point(576, 105)
        Me.LinkLabel_CacheClear.Name = "LinkLabel_CacheClear"
        Me.LinkLabel_CacheClear.Size = New System.Drawing.Size(67, 13)
        Me.LinkLabel_CacheClear.TabIndex = 12
        Me.LinkLabel_CacheClear.TabStop = True
        Me.LinkLabel_CacheClear.Text = "Clear Cache"
        Me.LinkLabel_CacheClear.VisitedLinkColor = System.Drawing.SystemColors.HotTrack
        '
        'CheckBox_Caching
        '
        Me.CheckBox_Caching.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CheckBox_Caching.AutoSize = True
        Me.CheckBox_Caching.Location = New System.Drawing.Point(649, 104)
        Me.CheckBox_Caching.Name = "CheckBox_Caching"
        Me.CheckBox_Caching.Size = New System.Drawing.Size(105, 17)
        Me.CheckBox_Caching.TabIndex = 11
        Me.CheckBox_Caching.Text = "Use hash cache"
        Me.ToolTip_Info.SetToolTip(Me.CheckBox_Caching, "When enabled, hashes will be saved and loaded when needed to speed up the hasing " &
        "process.")
        Me.CheckBox_Caching.UseVisualStyleBackColor = True
        '
        'ComboBox_HashingQuality
        '
        Me.ComboBox_HashingQuality.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBox_HashingQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_HashingQuality.FormattingEnabled = True
        Me.ComboBox_HashingQuality.Location = New System.Drawing.Point(626, 75)
        Me.ComboBox_HashingQuality.Name = "ComboBox_HashingQuality"
        Me.ComboBox_HashingQuality.Size = New System.Drawing.Size(128, 21)
        Me.ComboBox_HashingQuality.TabIndex = 10
        Me.ToolTip_Info.SetToolTip(Me.ComboBox_HashingQuality, "Higher quality image processing will slow down processing speed.")
        '
        'Label5
        '
        Me.Label5.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(541, 78)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(79, 13)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Quality/Speed"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 105)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(93, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Hashing method"
        '
        'ComboBox_HashingMethod
        '
        Me.ComboBox_HashingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_HashingMethod.FormattingEnabled = True
        Me.ComboBox_HashingMethod.Location = New System.Drawing.Point(130, 102)
        Me.ComboBox_HashingMethod.Name = "ComboBox_HashingMethod"
        Me.ComboBox_HashingMethod.Size = New System.Drawing.Size(128, 21)
        Me.ComboBox_HashingMethod.TabIndex = 7
        Me.ToolTip_Info.SetToolTip(Me.ComboBox_HashingMethod, resources.GetString("ComboBox_HashingMethod.ToolTip"))
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(6, 78)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(72, 13)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Hashing size"
        '
        'ComboBox_HashingSize
        '
        Me.ComboBox_HashingSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox_HashingSize.FormattingEnabled = True
        Me.ComboBox_HashingSize.Location = New System.Drawing.Point(130, 75)
        Me.ComboBox_HashingSize.Name = "ComboBox_HashingSize"
        Me.ComboBox_HashingSize.Size = New System.Drawing.Size(128, 21)
        Me.ComboBox_HashingSize.TabIndex = 5
        Me.ToolTip_Info.SetToolTip(Me.ComboBox_HashingSize, "Image comparing size in pixels." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Smaller image sizes can speed up processing but " &
        "does not detect smaller details in the image and could also result in false posi" &
        "tives.")
        '
        'CheckBox_CheckSubDirectorys
        '
        Me.CheckBox_CheckSubDirectorys.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CheckBox_CheckSubDirectorys.AutoSize = True
        Me.CheckBox_CheckSubDirectorys.Location = New System.Drawing.Point(575, 25)
        Me.CheckBox_CheckSubDirectorys.Name = "CheckBox_CheckSubDirectorys"
        Me.CheckBox_CheckSubDirectorys.Size = New System.Drawing.Size(87, 17)
        Me.CheckBox_CheckSubDirectorys.TabIndex = 4
        Me.CheckBox_CheckSubDirectorys.Text = "Sub Folders"
        Me.ToolTip_Info.SetToolTip(Me.CheckBox_CheckSubDirectorys, "Search images in sub folders when enabled.")
        Me.CheckBox_CheckSubDirectorys.UseVisualStyleBackColor = True
        '
        'NumericUpDown_Threads
        '
        Me.NumericUpDown_Threads.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.NumericUpDown_Threads.Location = New System.Drawing.Point(688, 47)
        Me.NumericUpDown_Threads.Name = "NumericUpDown_Threads"
        Me.NumericUpDown_Threads.Size = New System.Drawing.Size(66, 22)
        Me.NumericUpDown_Threads.TabIndex = 3
        Me.ToolTip_Info.SetToolTip(Me.NumericUpDown_Threads, "How many processing threads will be used.")
        Me.NumericUpDown_Threads.Value = New Decimal(New Integer() {4, 0, 0, 0})
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(572, 49)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(48, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Threads"
        '
        'NumericUpDown_MaxImageDiff
        '
        Me.NumericUpDown_MaxImageDiff.Location = New System.Drawing.Point(130, 47)
        Me.NumericUpDown_MaxImageDiff.Name = "NumericUpDown_MaxImageDiff"
        Me.NumericUpDown_MaxImageDiff.Size = New System.Drawing.Size(66, 22)
        Me.NumericUpDown_MaxImageDiff.TabIndex = 1
        Me.ToolTip_Info.SetToolTip(Me.NumericUpDown_MaxImageDiff, "Average difference between images in percent.")
        Me.NumericUpDown_MaxImageDiff.Value = New Decimal(New Integer() {95, 0, 0, 0})
        '
        'TextBox_Path
        '
        Me.TextBox_Path.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBox_Path.BackColor = System.Drawing.SystemColors.Window
        Me.TextBox_Path.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TextBox_Path.Location = New System.Drawing.Point(6, 26)
        Me.TextBox_Path.Name = "TextBox_Path"
        Me.TextBox_Path.ReadOnly = True
        Me.TextBox_Path.Size = New System.Drawing.Size(563, 15)
        Me.TextBox_Path.TabIndex = 1
        Me.TextBox_Path.Text = "Select directory"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 49)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(60, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Difference"
        '
        'ContextMenuStrip_Images
        '
        Me.ContextMenuStrip_Images.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem_Open, Me.ToolStripMenuItem_OpenExplorer, Me.ToolStripSeparator1, Me.ToolStripMenuItem1, Me.ToolStripComboBox_ShowDiffPreview, Me.ToolStripSeparator2, Me.ToolStripMenuItem_Remove})
        Me.ContextMenuStrip_Images.Name = "ContextMenuStrip_Images"
        Me.ContextMenuStrip_Images.Size = New System.Drawing.Size(220, 131)
        '
        'ToolStripMenuItem_Open
        '
        Me.ToolStripMenuItem_Open.Image = CType(resources.GetObject("ToolStripMenuItem_Open.Image"), System.Drawing.Image)
        Me.ToolStripMenuItem_Open.Name = "ToolStripMenuItem_Open"
        Me.ToolStripMenuItem_Open.Size = New System.Drawing.Size(219, 22)
        Me.ToolStripMenuItem_Open.Text = "Open"
        '
        'ToolStripMenuItem_OpenExplorer
        '
        Me.ToolStripMenuItem_OpenExplorer.Name = "ToolStripMenuItem_OpenExplorer"
        Me.ToolStripMenuItem_OpenExplorer.Size = New System.Drawing.Size(219, 22)
        Me.ToolStripMenuItem_OpenExplorer.Text = "Open in explorer"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(216, 6)
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Enabled = False
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        Me.ToolStripMenuItem1.Size = New System.Drawing.Size(219, 22)
        Me.ToolStripMenuItem1.Text = "Show difference in preview:"
        '
        'ToolStripComboBox_ShowDiffPreview
        '
        Me.ToolStripComboBox_ShowDiffPreview.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ToolStripComboBox_ShowDiffPreview.DropDownWidth = 150
        Me.ToolStripComboBox_ShowDiffPreview.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.ToolStripComboBox_ShowDiffPreview.Name = "ToolStripComboBox_ShowDiffPreview"
        Me.ToolStripComboBox_ShowDiffPreview.Size = New System.Drawing.Size(121, 23)
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(216, 6)
        '
        'ToolStripMenuItem_Remove
        '
        Me.ToolStripMenuItem_Remove.Image = CType(resources.GetObject("ToolStripMenuItem_Remove.Image"), System.Drawing.Image)
        Me.ToolStripMenuItem_Remove.Name = "ToolStripMenuItem_Remove"
        Me.ToolStripMenuItem_Remove.Size = New System.Drawing.Size(219, 22)
        Me.ToolStripMenuItem_Remove.Text = "Remove"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.BackColor = System.Drawing.SystemColors.Control
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar_Progress, Me.ToolStripStatusLabel_Progress})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 639)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(784, 22)
        Me.StatusStrip1.TabIndex = 5
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripProgressBar_Progress
        '
        Me.ToolStripProgressBar_Progress.Name = "ToolStripProgressBar_Progress"
        Me.ToolStripProgressBar_Progress.Size = New System.Drawing.Size(100, 16)
        Me.ToolStripProgressBar_Progress.Visible = False
        '
        'ToolStripStatusLabel_Progress
        '
        Me.ToolStripStatusLabel_Progress.Name = "ToolStripStatusLabel_Progress"
        Me.ToolStripStatusLabel_Progress.Size = New System.Drawing.Size(119, 17)
        Me.ToolStripStatusLabel_Progress.Text = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel_Progress.Visible = False
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "File A"
        Me.ColumnHeader2.Width = 250
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "File B"
        Me.ColumnHeader6.Width = 250
        '
        'ColumnHeader7
        '
        Me.ColumnHeader7.Text = "Difference Ratio"
        Me.ColumnHeader7.Width = 75
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Size A"
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Size B"
        '
        'ToolTip_Info
        '
        Me.ToolTip_Info.AutoPopDelay = 30000
        Me.ToolTip_Info.InitialDelay = 500
        Me.ToolTip_Info.ReshowDelay = 100
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Location = New System.Drawing.Point(12, 150)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(760, 486)
        Me.TabControl1.TabIndex = 1
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.SplitContainer1)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Size = New System.Drawing.Size(752, 460)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Image compare"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.ClassTreeViewColumns_Images)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.SplitContainer2)
        Me.SplitContainer1.Size = New System.Drawing.Size(752, 460)
        Me.SplitContainer1.SplitterDistance = 486
        Me.SplitContainer1.TabIndex = 4
        '
        'SplitContainer2
        '
        Me.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.SplitContainer2.Location = New System.Drawing.Point(0, 0)
        Me.SplitContainer2.Name = "SplitContainer2"
        Me.SplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.GroupBox3)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.GroupBox2)
        Me.SplitContainer2.Size = New System.Drawing.Size(262, 460)
        Me.SplitContainer2.SplitterDistance = 228
        Me.SplitContainer2.TabIndex = 4
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.PictureBox_ImageAPreview)
        Me.GroupBox3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox3.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(262, 228)
        Me.GroupBox3.TabIndex = 3
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Selected Image"
        '
        'PictureBox_ImageAPreview
        '
        Me.PictureBox_ImageAPreview.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PictureBox_ImageAPreview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox_ImageAPreview.Location = New System.Drawing.Point(3, 18)
        Me.PictureBox_ImageAPreview.Name = "PictureBox_ImageAPreview"
        Me.PictureBox_ImageAPreview.Size = New System.Drawing.Size(256, 207)
        Me.PictureBox_ImageAPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox_ImageAPreview.TabIndex = 0
        Me.PictureBox_ImageAPreview.TabStop = False
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.PictureBox_ImageBPreview)
        Me.GroupBox2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBox2.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(262, 228)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Parent Image"
        '
        'PictureBox_ImageBPreview
        '
        Me.PictureBox_ImageBPreview.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PictureBox_ImageBPreview.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox_ImageBPreview.Location = New System.Drawing.Point(3, 18)
        Me.PictureBox_ImageBPreview.Name = "PictureBox_ImageBPreview"
        Me.PictureBox_ImageBPreview.Size = New System.Drawing.Size(256, 207)
        Me.PictureBox_ImageBPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox_ImageBPreview.TabIndex = 0
        Me.PictureBox_ImageBPreview.TabStop = False
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.ClassListViewEx_FailedFiles)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Size = New System.Drawing.Size(752, 460)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Unsupported images"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'ClassTreeViewColumns_Images
        '
        Me.ClassTreeViewColumns_Images.AutoScroll = True
        Me.ClassTreeViewColumns_Images.BackColor = System.Drawing.SystemColors.Window
        Me.ClassTreeViewColumns_Images.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ClassTreeViewColumns_Images.Location = New System.Drawing.Point(0, 0)
        Me.ClassTreeViewColumns_Images.m_GridView = True
        Me.ClassTreeViewColumns_Images.Name = "ClassTreeViewColumns_Images"
        Me.ClassTreeViewColumns_Images.Size = New System.Drawing.Size(486, 460)
        Me.ClassTreeViewColumns_Images.TabIndex = 0
        '
        'ClassListViewEx_FailedFiles
        '
        Me.ClassListViewEx_FailedFiles.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.ClassListViewEx_FailedFiles.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader11})
        Me.ClassListViewEx_FailedFiles.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ClassListViewEx_FailedFiles.HideSelection = False
        Me.ClassListViewEx_FailedFiles.Location = New System.Drawing.Point(0, 0)
        Me.ClassListViewEx_FailedFiles.m_SetSortingColumn = True
        Me.ClassListViewEx_FailedFiles.Name = "ClassListViewEx_FailedFiles"
        Me.ClassListViewEx_FailedFiles.Size = New System.Drawing.Size(752, 460)
        Me.ClassListViewEx_FailedFiles.TabIndex = 0
        Me.ClassListViewEx_FailedFiles.UseCompatibleStateImageBehavior = False
        Me.ClassListViewEx_FailedFiles.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader11
        '
        Me.ColumnHeader11.Text = "File"
        Me.ColumnHeader11.Width = 700
        '
        'FormMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.ClientSize = New System.Drawing.Size(784, 661)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.GroupBox1)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "FormMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Image Difference"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.NumericUpDown_Threads, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumericUpDown_MaxImageDiff, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip_Images.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        CType(Me.PictureBox_ImageAPreview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.PictureBox_ImageBPreview, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Button_Select As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents TextBox_Path As TextBox
    Friend WithEvents NumericUpDown_MaxImageDiff As NumericUpDown
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents PictureBox_ImageAPreview As PictureBox
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStripStatusLabel_Progress As ToolStripStatusLabel
    Friend WithEvents ToolStripProgressBar_Progress As ToolStripProgressBar
    Friend WithEvents ContextMenuStrip_Images As ContextMenuStrip
    Friend WithEvents ToolStripMenuItem_Open As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem_OpenExplorer As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ToolStripMenuItem_Remove As ToolStripMenuItem
    Friend WithEvents NumericUpDown_Threads As NumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents CheckBox_CheckSubDirectorys As CheckBox
    Friend WithEvents ColumnHeader6 As ColumnHeader
    Friend WithEvents ColumnHeader7 As ColumnHeader
    Friend WithEvents SplitContainer2 As SplitContainer
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents PictureBox_ImageBPreview As PictureBox
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader3 As ColumnHeader
    Friend WithEvents Label4 As Label
    Friend WithEvents ComboBox_HashingMethod As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents ComboBox_HashingSize As ComboBox
    Friend WithEvents ComboBox_HashingQuality As ComboBox
    Friend WithEvents Label5 As Label
    Friend WithEvents CheckBox_Caching As CheckBox
    Friend WithEvents ToolTip_Info As ToolTip
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents ClassListViewEx_FailedFiles As ClassListViewEx
    Friend WithEvents ColumnHeader11 As ColumnHeader
    Friend WithEvents ClassTreeViewColumns_Images As ClassTreeViewColumns
    Friend WithEvents LinkLabel_CacheClear As LinkLabel
    Friend WithEvents ToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ToolStripComboBox_ShowDiffPreview As ToolStripComboBox
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
End Class
