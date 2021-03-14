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
            this.components = new System.ComponentModel.Container();
            this.animTreeView = new System.Windows.Forms.TreeView();
            this.dataPanel = new System.Windows.Forms.Panel();
            this.nodeMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transformGroupMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertTransformGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTransformGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyframeMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertKeyframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateKeyframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeKeyframeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animIDLabel = new System.Windows.Forms.Label();
            this.animIDCB = new System.Windows.Forms.ComboBox();
            this.nodeMenuStrip.SuspendLayout();
            this.transformGroupMenuStrip.SuspendLayout();
            this.keyframeMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // animTreeView
            // 
            this.animTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animTreeView.Location = new System.Drawing.Point(0, 46);
            this.animTreeView.Name = "animTreeView";
            this.animTreeView.Size = new System.Drawing.Size(154, 207);
            this.animTreeView.TabIndex = 0;
            this.animTreeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.animTreeView_MouseDown);
            // 
            // dataPanel
            // 
            this.dataPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPanel.Location = new System.Drawing.Point(160, 3);
            this.dataPanel.Name = "dataPanel";
            this.dataPanel.Size = new System.Drawing.Size(225, 253);
            this.dataPanel.TabIndex = 1;
            // 
            // nodeMenuStrip
            // 
            this.nodeMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertNodeToolStripMenuItem,
            this.duplicateNodeToolStripMenuItem,
            this.renameNodeToolStripMenuItem,
            this.removeNodeToolStripMenuItem});
            this.nodeMenuStrip.Name = "nodeMenuStrip";
            this.nodeMenuStrip.Size = new System.Drawing.Size(157, 92);
            // 
            // insertNodeToolStripMenuItem
            // 
            this.insertNodeToolStripMenuItem.Name = "insertNodeToolStripMenuItem";
            this.insertNodeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.insertNodeToolStripMenuItem.Text = "Insert Node";
            // 
            // duplicateNodeToolStripMenuItem
            // 
            this.duplicateNodeToolStripMenuItem.Name = "duplicateNodeToolStripMenuItem";
            this.duplicateNodeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.duplicateNodeToolStripMenuItem.Text = "Duplicate Node";
            // 
            // renameNodeToolStripMenuItem
            // 
            this.renameNodeToolStripMenuItem.Name = "renameNodeToolStripMenuItem";
            this.renameNodeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.renameNodeToolStripMenuItem.Text = "Rename Node";
            // 
            // removeNodeToolStripMenuItem
            // 
            this.removeNodeToolStripMenuItem.Name = "removeNodeToolStripMenuItem";
            this.removeNodeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.removeNodeToolStripMenuItem.Text = "Remove Node";
            // 
            // transformGroupMenuStrip
            // 
            this.transformGroupMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertTransformGroupToolStripMenuItem,
            this.removeTransformGroupToolStripMenuItem});
            this.transformGroupMenuStrip.Name = "transformGroupMenuStrip";
            this.transformGroupMenuStrip.Size = new System.Drawing.Size(210, 48);
            // 
            // insertTransformGroupToolStripMenuItem
            // 
            this.insertTransformGroupToolStripMenuItem.Name = "insertTransformGroupToolStripMenuItem";
            this.insertTransformGroupToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.insertTransformGroupToolStripMenuItem.Text = "Insert Transform Group";
            // 
            // removeTransformGroupToolStripMenuItem
            // 
            this.removeTransformGroupToolStripMenuItem.Name = "removeTransformGroupToolStripMenuItem";
            this.removeTransformGroupToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.removeTransformGroupToolStripMenuItem.Text = "Remove Transform Group";
            // 
            // keyframeMenuStrip
            // 
            this.keyframeMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertKeyframeToolStripMenuItem,
            this.duplicateKeyframeToolStripMenuItem,
            this.removeKeyframeToolStripMenuItem});
            this.keyframeMenuStrip.Name = "keyframeMenuStrip";
            this.keyframeMenuStrip.Size = new System.Drawing.Size(178, 70);
            // 
            // insertKeyframeToolStripMenuItem
            // 
            this.insertKeyframeToolStripMenuItem.Name = "insertKeyframeToolStripMenuItem";
            this.insertKeyframeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.insertKeyframeToolStripMenuItem.Text = "Insert Keyframe";
            // 
            // duplicateKeyframeToolStripMenuItem
            // 
            this.duplicateKeyframeToolStripMenuItem.Name = "duplicateKeyframeToolStripMenuItem";
            this.duplicateKeyframeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.duplicateKeyframeToolStripMenuItem.Text = "Duplicate Keyframe";
            // 
            // removeKeyframeToolStripMenuItem
            // 
            this.removeKeyframeToolStripMenuItem.Name = "removeKeyframeToolStripMenuItem";
            this.removeKeyframeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.removeKeyframeToolStripMenuItem.Text = "Remove Keyframe";
            // 
            // animIDLabel
            // 
            this.animIDLabel.AutoSize = true;
            this.animIDLabel.Location = new System.Drawing.Point(3, 3);
            this.animIDLabel.Name = "animIDLabel";
            this.animIDLabel.Size = new System.Drawing.Size(53, 13);
            this.animIDLabel.TabIndex = 4;
            this.animIDLabel.Text = "Motion ID";
            // 
            // animIDCB
            // 
            this.animIDCB.FormattingEnabled = true;
            this.animIDCB.Location = new System.Drawing.Point(13, 19);
            this.animIDCB.Name = "animIDCB";
            this.animIDCB.Size = new System.Drawing.Size(43, 21);
            this.animIDCB.TabIndex = 3;
            this.animIDCB.SelectedIndexChanged += new System.EventHandler(this.animIDCB_SelectedIndexChanged);
            // 
            // AnimationEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.animIDLabel);
            this.Controls.Add(this.animIDCB);
            this.Controls.Add(this.dataPanel);
            this.Controls.Add(this.animTreeView);
            this.Name = "AnimationEditor";
            this.Size = new System.Drawing.Size(384, 256);
            this.nodeMenuStrip.ResumeLayout(false);
            this.transformGroupMenuStrip.ResumeLayout(false);
            this.keyframeMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView animTreeView;
        private System.Windows.Forms.Panel dataPanel;
        private System.Windows.Forms.ContextMenuStrip nodeMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem insertNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeNodeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip transformGroupMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem insertTransformGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeTransformGroupToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip keyframeMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem insertKeyframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateKeyframeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeKeyframeToolStripMenuItem;
        private System.Windows.Forms.Label animIDLabel;
        private System.Windows.Forms.ComboBox animIDCB;
    }
}
