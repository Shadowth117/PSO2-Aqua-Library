
namespace AquaModelTool
{
    partial class EffectEditor
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
            this.dataPanel = new System.Windows.Forms.Panel();
            this.animTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // dataPanel
            // 
            this.dataPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPanel.AutoSize = true;
            this.dataPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dataPanel.Location = new System.Drawing.Point(385, 252);
            this.dataPanel.Name = "dataPanel";
            this.dataPanel.Size = new System.Drawing.Size(0, 0);
            this.dataPanel.TabIndex = 3;
            // 
            // animTreeView
            // 
            this.animTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animTreeView.Location = new System.Drawing.Point(0, 42);
            this.animTreeView.Name = "animTreeView";
            this.animTreeView.Size = new System.Drawing.Size(154, 207);
            this.animTreeView.TabIndex = 2;
            // 
            // EffectEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.dataPanel);
            this.Controls.Add(this.animTreeView);
            this.Name = "EffectEditor";
            this.Size = new System.Drawing.Size(384, 250);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel dataPanel;
        private System.Windows.Forms.TreeView animTreeView;
    }
}
