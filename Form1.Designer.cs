namespace Donkey_car_manager
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            trbFrame = new TrackBar();
            btnFileOpen = new Button();
            lblTitle = new Label();
            lblAcceleration = new Label();
            lblSteeringAngle = new Label();
            btnFileDelete = new Button();
            txbFrame = new TextBox();
            btnFrameMove = new Button();
            lblCurFilePage = new Label();
            txbFileNum = new TextBox();
            picCurFrame = new PictureBox();
            lblFrameNum = new Label();
            btnFileMultiDel = new Button();
            dgvDebug = new DataGridView();
            btnAutoPic = new Button();
            txbFPS = new TextBox();
            timerPlay = new System.Windows.Forms.Timer(components);
            label1 = new Label();
            lstFiles = new ListView();
            colNum = new ColumnHeader();
            colName = new ColumnHeader();
            colTime = new ColumnHeader();
            btnStartCollection = new Button();
            btnStartLearning = new Button();
            ((System.ComponentModel.ISupportInitialize)trbFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCurFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvDebug).BeginInit();
            SuspendLayout();
            // 
            // trbFrame
            // 
            trbFrame.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trbFrame.LargeChange = 1;
            trbFrame.Location = new Point(229, 404);
            trbFrame.Maximum = 10000;
            trbFrame.Name = "trbFrame";
            trbFrame.Size = new Size(668, 45);
            trbFrame.TabIndex = 5;
            trbFrame.Scroll += trbFrame_Scroll;
            trbFrame.MouseDown += trbFrame_MouseDown;
            // 
            // btnFileOpen
            // 
            btnFileOpen.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFileOpen.BackColor = Color.FromArgb(255, 255, 128);
            btnFileOpen.FlatStyle = FlatStyle.Flat;
            btnFileOpen.Font = new Font("맑은 고딕", 15.75F);
            btnFileOpen.ForeColor = Color.FromArgb(0, 0, 64);
            btnFileOpen.Location = new Point(908, 269);
            btnFileOpen.Name = "btnFileOpen";
            btnFileOpen.Size = new Size(211, 51);
            btnFileOpen.TabIndex = 9;
            btnFileOpen.Text = "파일 열기";
            btnFileOpen.UseVisualStyleBackColor = false;
            btnFileOpen.Click += btnFileOpen_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Yellow;
            lblTitle.Location = new Point(-6, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(234, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Donkey Car Manager";
            lblTitle.Click += label1_Click;
            // 
            // lblAcceleration
            // 
            lblAcceleration.AutoSize = true;
            lblAcceleration.Font = new Font("맑은 고딕", 14.25F);
            lblAcceleration.ForeColor = Color.Silver;
            lblAcceleration.Location = new Point(11, 76);
            lblAcceleration.Name = "lblAcceleration";
            lblAcceleration.Size = new Size(50, 25);
            lblAcceleration.TabIndex = 10;
            lblAcceleration.Text = "속도";
            // 
            // lblSteeringAngle
            // 
            lblSteeringAngle.AutoSize = true;
            lblSteeringAngle.Font = new Font("맑은 고딕", 14.25F);
            lblSteeringAngle.ForeColor = Color.Silver;
            lblSteeringAngle.Location = new Point(11, 101);
            lblSteeringAngle.Name = "lblSteeringAngle";
            lblSteeringAngle.Size = new Size(69, 25);
            lblSteeringAngle.TabIndex = 11;
            lblSteeringAngle.Text = "조향각";
            // 
            // btnFileDelete
            // 
            btnFileDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFileDelete.BackColor = Color.IndianRed;
            btnFileDelete.FlatStyle = FlatStyle.Flat;
            btnFileDelete.Font = new Font("맑은 고딕", 15.75F);
            btnFileDelete.ForeColor = Color.White;
            btnFileDelete.Location = new Point(908, 326);
            btnFileDelete.Name = "btnFileDelete";
            btnFileDelete.Size = new Size(211, 51);
            btnFileDelete.TabIndex = 10;
            btnFileDelete.Text = "파일 삭제";
            btnFileDelete.UseVisualStyleBackColor = false;
            btnFileDelete.Click += btnFileDelete_Click;
            // 
            // txbFrame
            // 
            txbFrame.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txbFrame.BackColor = Color.LightSteelBlue;
            txbFrame.BorderStyle = BorderStyle.FixedSingle;
            txbFrame.Font = new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txbFrame.Location = new Point(908, 12);
            txbFrame.Name = "txbFrame";
            txbFrame.Size = new Size(123, 29);
            txbFrame.TabIndex = 6;
            txbFrame.KeyDown += txbFrame_KeyDown;
            txbFrame.KeyPress += txbFrame_KeyPress;
            // 
            // btnFrameMove
            // 
            btnFrameMove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFrameMove.BackColor = Color.LightSteelBlue;
            btnFrameMove.FlatStyle = FlatStyle.Flat;
            btnFrameMove.Font = new Font("맑은 고딕", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnFrameMove.ForeColor = Color.FromArgb(0, 0, 64);
            btnFrameMove.Location = new Point(1037, 12);
            btnFrameMove.Name = "btnFrameMove";
            btnFrameMove.Size = new Size(87, 29);
            btnFrameMove.TabIndex = 7;
            btnFrameMove.Text = "프레임 이동";
            btnFrameMove.UseVisualStyleBackColor = false;
            btnFrameMove.Click += btnFrameMove_Click;
            // 
            // lblCurFilePage
            // 
            lblCurFilePage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCurFilePage.AutoSize = true;
            lblCurFilePage.BackColor = Color.LightSteelBlue;
            lblCurFilePage.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblCurFilePage.ForeColor = Color.FromArgb(0, 0, 64);
            lblCurFilePage.Location = new Point(11, 402);
            lblCurFilePage.Name = "lblCurFilePage";
            lblCurFilePage.Size = new Size(114, 25);
            lblCurFilePage.TabIndex = 15;
            lblCurFilePage.Text = "현재 페이지";
            // 
            // txbFileNum
            // 
            txbFileNum.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txbFileNum.BackColor = Color.LightSteelBlue;
            txbFileNum.BorderStyle = BorderStyle.FixedSingle;
            txbFileNum.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txbFileNum.Location = new Point(133, 399);
            txbFileNum.Name = "txbFileNum";
            txbFileNum.Size = new Size(90, 33);
            txbFileNum.TabIndex = 4;
            txbFileNum.TextChanged += txbFileNum_TextChanged;
            txbFileNum.KeyDown += txbFileNum_KeyDown;
            // 
            // picCurFrame
            // 
            picCurFrame.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picCurFrame.Location = new Point(234, 12);
            picCurFrame.Name = "picCurFrame";
            picCurFrame.Size = new Size(668, 386);
            picCurFrame.SizeMode = PictureBoxSizeMode.Zoom;
            picCurFrame.TabIndex = 0;
            picCurFrame.TabStop = false;
            picCurFrame.Click += picCurFrame_Click;
            // 
            // lblFrameNum
            // 
            lblFrameNum.AutoSize = true;
            lblFrameNum.BackColor = Color.Transparent;
            lblFrameNum.Font = new Font("맑은 고딕", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblFrameNum.ForeColor = Color.Silver;
            lblFrameNum.Location = new Point(12, 47);
            lblFrameNum.Name = "lblFrameNum";
            lblFrameNum.Size = new Size(97, 20);
            lblFrameNum.TabIndex = 9;
            lblFrameNum.Text = "프레임 번호 :";
            // 
            // btnFileMultiDel
            // 
            btnFileMultiDel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFileMultiDel.BackColor = Color.IndianRed;
            btnFileMultiDel.FlatStyle = FlatStyle.Flat;
            btnFileMultiDel.Font = new Font("맑은 고딕", 15.75F);
            btnFileMultiDel.ForeColor = Color.White;
            btnFileMultiDel.Location = new Point(908, 383);
            btnFileMultiDel.Name = "btnFileMultiDel";
            btnFileMultiDel.Size = new Size(211, 51);
            btnFileMultiDel.TabIndex = 11;
            btnFileMultiDel.Text = "파일 다중 삭제";
            btnFileMultiDel.UseVisualStyleBackColor = false;
            btnFileMultiDel.Click += btnFileMultiDel_Click;
            // 
            // dgvDebug
            // 
            dgvDebug.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDebug.BackgroundColor = Color.LightSteelBlue;
            dgvDebug.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDebug.Location = new Point(577, 455);
            dgvDebug.Name = "dgvDebug";
            dgvDebug.Size = new Size(542, 441);
            dgvDebug.TabIndex = 13;
            // 
            // btnAutoPic
            // 
            btnAutoPic.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAutoPic.BackColor = SystemColors.ControlLight;
            btnAutoPic.FlatStyle = FlatStyle.Flat;
            btnAutoPic.Font = new Font("맑은 고딕", 15.75F);
            btnAutoPic.ForeColor = Color.FromArgb(0, 0, 64);
            btnAutoPic.Location = new Point(11, 298);
            btnAutoPic.Name = "btnAutoPic";
            btnAutoPic.Size = new Size(211, 51);
            btnAutoPic.TabIndex = 3;
            btnAutoPic.Text = "자동 넘기기";
            btnAutoPic.UseVisualStyleBackColor = false;
            btnAutoPic.Click += btnAutoPic_Click;
            // 
            // txbFPS
            // 
            txbFPS.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txbFPS.BackColor = Color.LightSteelBlue;
            txbFPS.BorderStyle = BorderStyle.FixedSingle;
            txbFPS.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txbFPS.Location = new Point(59, 360);
            txbFPS.Name = "txbFPS";
            txbFPS.Size = new Size(164, 33);
            txbFPS.TabIndex = 2;
            // 
            // timerPlay
            // 
            timerPlay.Interval = 20;
            timerPlay.Tick += timerPlay_Tick;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.BackColor = Color.LightSteelBlue;
            label1.Font = new Font("맑은 고딕", 14.25F);
            label1.ForeColor = Color.FromArgb(0, 0, 64);
            label1.Location = new Point(11, 363);
            label1.Name = "label1";
            label1.Size = new Size(42, 25);
            label1.TabIndex = 22;
            label1.Text = "FPS";
            // 
            // lstFiles
            // 
            lstFiles.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstFiles.BackColor = Color.LightSteelBlue;
            lstFiles.Columns.AddRange(new ColumnHeader[] { colNum, colName, colTime });
            lstFiles.ForeColor = SystemColors.WindowText;
            lstFiles.FullRowSelect = true;
            lstFiles.Location = new Point(12, 455);
            lstFiles.Name = "lstFiles";
            lstFiles.Size = new Size(559, 441);
            lstFiles.TabIndex = 12;
            lstFiles.UseCompatibleStateImageBehavior = false;
            lstFiles.View = View.Details;
            lstFiles.ColumnClick += lstFiles_ColumnClick;
            lstFiles.ItemSelectionChanged += lstFiles_ItemSelectionChanged;
            lstFiles.SelectedIndexChanged += lstFiles_SelectedIndexChanged;
            lstFiles.KeyDown += lstFiles_KeyDown;
            // 
            // colNum
            // 
            colNum.Text = "번호";
            colNum.Width = 100;
            // 
            // colName
            // 
            colName.Text = "파일명";
            colName.Width = 200;
            // 
            // colTime
            // 
            colTime.Text = "수정한 날짜";
            colTime.Width = 300;
            // 
            // btnStartCollection
            // 
            btnStartCollection.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnStartCollection.BackColor = Color.Lime;
            btnStartCollection.FlatStyle = FlatStyle.Flat;
            btnStartCollection.Font = new Font("맑은 고딕", 15.75F);
            btnStartCollection.ForeColor = Color.FromArgb(0, 0, 64);
            btnStartCollection.Location = new Point(11, 241);
            btnStartCollection.Name = "btnStartCollection";
            btnStartCollection.Size = new Size(211, 51);
            btnStartCollection.TabIndex = 1;
            btnStartCollection.Text = "시뮬레이터 실행";
            btnStartCollection.UseVisualStyleBackColor = false;
            btnStartCollection.Click += btnStartCollection_Click;
            // 
            // btnStartLearning
            // 
            btnStartLearning.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnStartLearning.BackColor = Color.FromArgb(255, 255, 128);
            btnStartLearning.FlatStyle = FlatStyle.Flat;
            btnStartLearning.Font = new Font("맑은 고딕", 15.75F);
            btnStartLearning.ForeColor = Color.FromArgb(0, 0, 64);
            btnStartLearning.Location = new Point(908, 212);
            btnStartLearning.Name = "btnStartLearning";
            btnStartLearning.Size = new Size(211, 51);
            btnStartLearning.TabIndex = 8;
            btnStartLearning.Text = "학습 시작";
            btnStartLearning.UseVisualStyleBackColor = false;
            btnStartLearning.Click += btnStartLearning_Click_1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(0, 0, 64);
            ClientSize = new Size(1129, 905);
            Controls.Add(btnStartLearning);
            Controls.Add(btnStartCollection);
            Controls.Add(lstFiles);
            Controls.Add(label1);
            Controls.Add(txbFPS);
            Controls.Add(btnAutoPic);
            Controls.Add(dgvDebug);
            Controls.Add(btnFileMultiDel);
            Controls.Add(lblSteeringAngle);
            Controls.Add(lblFrameNum);
            Controls.Add(lblAcceleration);
            Controls.Add(picCurFrame);
            Controls.Add(lblTitle);
            Controls.Add(txbFileNum);
            Controls.Add(lblCurFilePage);
            Controls.Add(btnFrameMove);
            Controls.Add(txbFrame);
            Controls.Add(btnFileDelete);
            Controls.Add(btnFileOpen);
            Controls.Add(trbFrame);
            ForeColor = Color.Silver;
            Name = "Form1";
            Text = "Donkey Car Manager v1.0";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)trbFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCurFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvDebug).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TrackBar trbFrame;
        private Button btnFileOpen;
        private Label lblTitle;
        private Label lblAcceleration;
        private Label lblSteeringAngle;
        private Button btnFileDelete;
        private TextBox txbFrame;
        private Button btnFrameMove;
        private Label lblCurFilePage;
        private TextBox txbFileNum;
        private PictureBox picCurFrame;
        private Label lblFrameNum;
        private Button btnFileMultiDel;
        private DataGridView dgvDebug;
        private Button btnAutoPic;
        private TextBox txbFPS;
        private System.Windows.Forms.Timer timerPlay;
        private Label label1;
        private ListView lstFiles;
        private ColumnHeader colName;
        private ColumnHeader colTime;
        private ColumnHeader colNum;
        private Button btnStartCollection;
        private Button btnStartLearning;
    }
}
