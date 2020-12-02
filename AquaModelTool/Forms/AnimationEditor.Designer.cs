namespace AquaModelTool
{
    partial class AnimationEditor
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
            this.animTreeView = new System.Windows.Forms.TreeView();
            this.dataPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // animTreeView
            // 
            this.animTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.animTreeView.Location = new System.Drawing.Point(0, 0);
            this.animTreeView.Name = "animTreeView";
            this.animTreeView.Size = new System.Drawing.Size(154, 250);
            this.animTreeView.TabIndex = 0;
            this.animTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.animTreeView_MouseDown);
            // 
            // dataPanel
            // 
            this.dataPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPanel.Location = new System.Drawing.Point(155, 0);
            this.dataPanel.Name = "dataPanel";
            this.dataPanel.Size = new System.Drawing.Size(230, 250);
            this.dataPanel.TabIndex = 1;
            // 
            // AnimationEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataPanel);
            this.Controls.Add(this.animTreeView);
            this.Name = "AnimationEditor";
            this.Size = new System.Drawing.Size(384, 250);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView animTreeView;
        private System.Windows.Forms.Panel dataPanel;
    }
}
