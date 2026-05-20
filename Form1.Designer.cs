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
            picCurFrame = new PictureBox();
            trbFrame = new TrackBar();
            btnPageUp = new Button();
            btnFileOpen = new Button();
            lstFiles = new ListBox();
            btnPageDown = new Button();
            lblTitle = new Label();
            lblFrameNum = new Label();
            lblAcceleration = new Label();
            lblSteeringAngle = new Label();
            btnFileDelete = new Button();
            txbFrame = new TextBox();
            btnFrameMove = new Button();
            lblCurFilePage = new Label();
            txbFileNum = new TextBox();
            btnExpandUi = new Button();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)picCurFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbFrame).BeginInit();
            SuspendLayout();
            // 
            // picCurFrame
            // 
            picCurFrame.Location = new Point(234, 12);
            picCurFrame.Name = "picCurFrame";
            picCurFrame.Size = new Size(668, 317);
            picCurFrame.TabIndex = 0;
            picCurFrame.TabStop = false;
            // 
            // trbFrame
            // 
            trbFrame.Location = new Point(234, 335);
            trbFrame.Name = "trbFrame";
            trbFrame.Size = new Size(668, 45);
            trbFrame.TabIndex = 1;
            // 
            // btnPageUp
            // 
            btnPageUp.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnPageUp.Location = new Point(12, 385);
            btnPageUp.Name = "btnPageUp";
            btnPageUp.Size = new Size(68, 116);
            btnPageUp.TabIndex = 2;
            btnPageUp.Text = "▲";
            btnPageUp.UseVisualStyleBackColor = true;
            // 
            // btnFileOpen
            // 
            btnFileOpen.Font = new Font("맑은 고딕", 12F);
            btnFileOpen.Location = new Point(908, 256);
            btnFileOpen.Name = "btnFileOpen";
            btnFileOpen.Size = new Size(211, 51);
            btnFileOpen.TabIndex = 4;
            btnFileOpen.Text = "파일 열기";
            btnFileOpen.UseVisualStyleBackColor = true;
            // 
            // lstFiles
            // 
            lstFiles.FormattingEnabled = true;
            lstFiles.Location = new Point(86, 385);
            lstFiles.Name = "lstFiles";
            lstFiles.Size = new Size(1033, 274);
            lstFiles.TabIndex = 6;
            // 
            // btnPageDown
            // 
            btnPageDown.Font = new Font("맑은 고딕", 26.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnPageDown.Location = new Point(12, 539);
            btnPageDown.Name = "btnPageDown";
            btnPageDown.Size = new Size(68, 116);
            btnPageDown.TabIndex = 7;
            btnPageDown.Text = "▼";
            btnPageDown.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Italic, GraphicsUnit.Point, 129);
            lblTitle.Location = new Point(12, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(159, 20);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "Donkey Car Manager";
            lblTitle.Click += label1_Click;
            // 
            // lblFrameNum
            // 
            lblFrameNum.AutoSize = true;
            lblFrameNum.Font = new Font("맑은 고딕", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lblFrameNum.Location = new Point(235, 14);
            lblFrameNum.Name = "lblFrameNum";
            lblFrameNum.Size = new Size(97, 20);
            lblFrameNum.TabIndex = 9;
            lblFrameNum.Text = "프레임 번호 :";
            // 
            // lblAcceleration
            // 
            lblAcceleration.AutoSize = true;
            lblAcceleration.Font = new Font("맑은 고딕", 14.25F);
            lblAcceleration.Location = new Point(12, 43);
            lblAcceleration.Name = "lblAcceleration";
            lblAcceleration.Size = new Size(50, 25);
            lblAcceleration.TabIndex = 10;
            lblAcceleration.Text = "속도";
            // 
            // lblSteeringAngle
            // 
            lblSteeringAngle.AutoSize = true;
            lblSteeringAngle.Font = new Font("맑은 고딕", 14.25F);
            lblSteeringAngle.Location = new Point(12, 68);
            lblSteeringAngle.Name = "lblSteeringAngle";
            lblSteeringAngle.Size = new Size(69, 25);
            lblSteeringAngle.TabIndex = 11;
            lblSteeringAngle.Text = "조향각";
            // 
            // btnFileDelete
            // 
            btnFileDelete.Font = new Font("맑은 고딕", 12F);
            btnFileDelete.Location = new Point(908, 313);
            btnFileDelete.Name = "btnFileDelete";
            btnFileDelete.Size = new Size(211, 51);
            btnFileDelete.TabIndex = 12;
            btnFileDelete.Text = "파일 삭제";
            btnFileDelete.UseVisualStyleBackColor = true;
            // 
            // txbFrame
            // 
            txbFrame.Font = new Font("맑은 고딕", 12F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txbFrame.Location = new Point(908, 12);
            txbFrame.Name = "txbFrame";
            txbFrame.Size = new Size(123, 29);
            txbFrame.TabIndex = 13;
            // 
            // btnFrameMove
            // 
            btnFrameMove.Font = new Font("맑은 고딕", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            btnFrameMove.Location = new Point(1037, 12);
            btnFrameMove.Name = "btnFrameMove";
            btnFrameMove.Size = new Size(87, 29);
            btnFrameMove.TabIndex = 14;
            btnFrameMove.Text = "프레임 이동";
            btnFrameMove.UseVisualStyleBackColor = true;
            // 
            // lblCurFilePage
            // 
            lblCurFilePage.AutoSize = true;
            lblCurFilePage.Location = new Point(86, 365);
            lblCurFilePage.Name = "lblCurFilePage";
            lblCurFilePage.Size = new Size(78, 15);
            lblCurFilePage.TabIndex = 15;
            lblCurFilePage.Text = "현재 페이지 :";
            // 
            // txbFileNum
            // 
            txbFileNum.Location = new Point(12, 507);
            txbFileNum.Name = "txbFileNum";
            txbFileNum.Size = new Size(68, 23);
            txbFileNum.TabIndex = 16;
            // 
            // btnExpandUi
            // 
            btnExpandUi.Font = new Font("맑은 고딕", 12F);
            btnExpandUi.Location = new Point(1037, 63);
            btnExpandUi.Name = "btnExpandUi";
            btnExpandUi.Size = new Size(87, 37);
            btnExpandUi.TabIndex = 17;
            btnExpandUi.Text = "->";
            btnExpandUi.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Font = new Font("맑은 고딕", 12F);
            button1.Location = new Point(30, 140);
            button1.Name = "button1";
            button1.Size = new Size(87, 37);
            button1.TabIndex = 18;
            button1.Text = "->";
            button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1131, 667);
            Controls.Add(button1);
            Controls.Add(btnExpandUi);
            Controls.Add(txbFileNum);
            Controls.Add(lblCurFilePage);
            Controls.Add(btnFrameMove);
            Controls.Add(txbFrame);
            Controls.Add(btnFileDelete);
            Controls.Add(lblSteeringAngle);
            Controls.Add(lblAcceleration);
            Controls.Add(lblFrameNum);
            Controls.Add(lblTitle);
            Controls.Add(btnPageDown);
            Controls.Add(lstFiles);
            Controls.Add(btnFileOpen);
            Controls.Add(btnPageUp);
            Controls.Add(trbFrame);
            Controls.Add(picCurFrame);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)picCurFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)trbFrame).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picCurFrame;
        private TrackBar trbFrame;
        private Button btnPageUp;
        private Button btnFileOpen;
        private ListBox lstFiles;
        private Button btnPageDown;
        private Label lblTitle;
        private Label lblFrameNum;
        private Label lblAcceleration;
        private Label lblSteeringAngle;
        private Button btnFileDelete;
        private TextBox txbFrame;
        private Button btnFrameMove;
        private Label lblCurFilePage;
        private TextBox txbFileNum;
        private Button btnExpandUi;
        private Button button1;
    }
}
