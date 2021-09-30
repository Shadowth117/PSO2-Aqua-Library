namespace AquaModelTool
{
    partial class BTIEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btiIEntryCB = new System.Windows.Forms.ComboBox();
            this.modelIDLabel = new System.Windows.Forms.Label();
            this.modelPanel = new System.Windows.Forms.Panel();
            this.startTimeLabel = new System.Windows.Forms.Label();
            this.startFrameUD = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.startFrameUD)).BeginInit();
            this.SuspendLayout();
            // 
            // btiIEntryCB
            // 
            this.btiIEntryCB.FormattingEnabled = true;
            this.btiIEntryCB.Location = new System.Drawing.Point(10, 15);
            this.btiIEntryCB.Name = "btiIEntryCB";
            this.btiIEntryCB.Size = new System.Drawing.Size(43, 21);
            this.btiIEntryCB.TabIndex = 1;
            this.btiIEntryCB.SelectedIndexChanged += new System.EventHandler(this.modelIDCB_SelectedIndexChanged);
            // 
            // modelIDLabel
            // 
            this.modelIDLabel.AutoSize = true;
            this.modelIDLabel.Location = new System.Drawing.Point(3, 0);
            this.modelIDLabel.Name = "modelIDLabel";
            this.modelIDLabel.Size = new System.Drawing.Size(65, 13);
            this.modelIDLabel.TabIndex = 2;
            this.modelIDLabel.Text = "BTI Entry ID";
            // 
            // modelPanel
            // 
            this.modelPanel.Location = new System.Drawing.Point(4, 43);
            this.modelPanel.Name = "modelPanel";
            this.modelPanel.Size = new System.Drawing.Size(542, 318);
            this.modelPanel.TabIndex = 7;
            // 
            // startTimeLabel
            // 
            this.startTimeLabel.AutoSize = true;
            this.startTimeLabel.Location = new System.Drawing.Point(87, 0);
            this.startTimeLabel.Name = "startTimeLabel";
            this.startTimeLabel.Size = new System.Drawing.Size(64, 13);
            this.startTimeLabel.TabIndex = 29;
            this.startTimeLabel.Text = "End frame? ";
            // 
            // startFrameUD
            // 
            this.startFrameUD.DecimalPlaces = 6;
            this.startFrameUD.Location = new System.Drawing.Point(90, 17);
            this.startFrameUD.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.startFrameUD.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.startFrameUD.Name = "startFrameUD";
            this.startFrameUD.Size = new System.Drawing.Size(69, 20);
            this.startFrameUD.TabIndex = 30;
            this.startFrameUD.ValueChanged += new System.EventHandler(this.startFrameUD_ValueChanged);
            // 
            // BTIEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.startTimeLabel);
            this.Controls.Add(this.startFrameUD);
            this.Controls.Add(this.modelPanel);
            this.Controls.Add(this.modelIDLabel);
            this.Controls.Add(this.btiIEntryCB);
            this.Name = "BTIEditor";
            this.Size = new System.Drawing.Size(546, 361);
            ((System.ComponentModel.ISupportInitialize)(this.startFrameUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox btiIEntryCB;
        private System.Windows.Forms.Label modelIDLabel;
        private System.Windows.Forms.Panel modelPanel;
        private System.Windows.Forms.Label startTimeLabel;
        private System.Windows.Forms.NumericUpDown startFrameUD;
    }
}
