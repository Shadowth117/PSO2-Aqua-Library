
namespace AquaModelTool.Forms.ModelSubpanels
{
    partial class MeshStructEditor
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
            this.meshIDCB = new System.Windows.Forms.ComboBox();
            this.meshLabel = new System.Windows.Forms.Label();
            this.flagsUD = new System.Windows.Forms.NumericUpDown();
            this.flagsLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.matIDLabel = new System.Windows.Forms.Label();
            this.matIDCB = new System.Windows.Forms.ComboBox();
            this.renderIDLabel = new System.Windows.Forms.Label();
            this.renderIDCB = new System.Windows.Forms.ComboBox();
            this.shaderLabel = new System.Windows.Forms.Label();
            this.shadIDCB = new System.Windows.Forms.ComboBox();
            this.texSetIDLabel = new System.Windows.Forms.Label();
            this.tsetIDCB = new System.Windows.Forms.ComboBox();
            this.vsetIDLabel = new System.Windows.Forms.Label();
            this.vsetIDCB = new System.Windows.Forms.ComboBox();
            this.psetIDLabel = new System.Windows.Forms.Label();
            this.faceSetIDCB = new System.Windows.Forms.ComboBox();
            this.warningLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.flagsUD)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // meshIDCB
            // 
            this.meshIDCB.FormattingEnabled = true;
            this.meshIDCB.Location = new System.Drawing.Point(4, 21);
            this.meshIDCB.Name = "meshIDCB";
            this.meshIDCB.Size = new System.Drawing.Size(49, 21);
            this.meshIDCB.TabIndex = 3;
            // 
            // meshLabel
            // 
            this.meshLabel.AutoSize = true;
            this.meshLabel.Location = new System.Drawing.Point(4, 4);
            this.meshLabel.Name = "meshLabel";
            this.meshLabel.Size = new System.Drawing.Size(89, 13);
            this.meshLabel.TabIndex = 2;
            this.meshLabel.Text = "Mesh Parameters";
            // 
            // flagsUD
            // 
            this.flagsUD.Location = new System.Drawing.Point(111, 22);
            this.flagsUD.Maximum = new decimal(new int[] {
            1569325056,
            23283064,
            0,
            0});
            this.flagsUD.Minimum = new decimal(new int[] {
            1569325056,
            23283064,
            0,
            -2147483648});
            this.flagsUD.Name = "flagsUD";
            this.flagsUD.Size = new System.Drawing.Size(76, 20);
            this.flagsUD.TabIndex = 9;
            // 
            // flagsLabel
            // 
            this.flagsLabel.AutoSize = true;
            this.flagsLabel.Location = new System.Drawing.Point(108, 5);
            this.flagsLabel.Name = "flagsLabel";
            this.flagsLabel.Size = new System.Drawing.Size(32, 13);
            this.flagsLabel.TabIndex = 8;
            this.flagsLabel.Text = "Flags";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.meshIDCB);
            this.panel1.Controls.Add(this.meshLabel);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(105, 51);
            this.panel1.TabIndex = 10;
            // 
            // matIDLabel
            // 
            this.matIDLabel.AutoSize = true;
            this.matIDLabel.Location = new System.Drawing.Point(7, 163);
            this.matIDLabel.Name = "matIDLabel";
            this.matIDLabel.Size = new System.Drawing.Size(58, 13);
            this.matIDLabel.TabIndex = 2;
            this.matIDLabel.Text = "Material ID";
            // 
            // matIDCB
            // 
            this.matIDCB.FormattingEnabled = true;
            this.matIDCB.Location = new System.Drawing.Point(7, 180);
            this.matIDCB.Name = "matIDCB";
            this.matIDCB.Size = new System.Drawing.Size(49, 21);
            this.matIDCB.TabIndex = 3;
            // 
            // renderIDLabel
            // 
            this.renderIDLabel.AutoSize = true;
            this.renderIDLabel.Location = new System.Drawing.Point(62, 163);
            this.renderIDLabel.Name = "renderIDLabel";
            this.renderIDLabel.Size = new System.Drawing.Size(56, 13);
            this.renderIDLabel.TabIndex = 2;
            this.renderIDLabel.Text = "Render ID";
            // 
            // renderIDCB
            // 
            this.renderIDCB.FormattingEnabled = true;
            this.renderIDCB.Location = new System.Drawing.Point(62, 180);
            this.renderIDCB.Name = "renderIDCB";
            this.renderIDCB.Size = new System.Drawing.Size(49, 21);
            this.renderIDCB.TabIndex = 3;
            // 
            // shaderLabel
            // 
            this.shaderLabel.AutoSize = true;
            this.shaderLabel.Location = new System.Drawing.Point(117, 163);
            this.shaderLabel.Name = "shaderLabel";
            this.shaderLabel.Size = new System.Drawing.Size(55, 13);
            this.shaderLabel.TabIndex = 2;
            this.shaderLabel.Text = "Shader ID";
            // 
            // shadIDCB
            // 
            this.shadIDCB.FormattingEnabled = true;
            this.shadIDCB.Location = new System.Drawing.Point(117, 180);
            this.shadIDCB.Name = "shadIDCB";
            this.shadIDCB.Size = new System.Drawing.Size(49, 21);
            this.shadIDCB.TabIndex = 3;
            // 
            // texSetIDLabel
            // 
            this.texSetIDLabel.AutoSize = true;
            this.texSetIDLabel.Location = new System.Drawing.Point(172, 163);
            this.texSetIDLabel.Name = "texSetIDLabel";
            this.texSetIDLabel.Size = new System.Drawing.Size(58, 13);
            this.texSetIDLabel.TabIndex = 2;
            this.texSetIDLabel.Text = "Tex Set ID";
            // 
            // tsetIDCB
            // 
            this.tsetIDCB.FormattingEnabled = true;
            this.tsetIDCB.Location = new System.Drawing.Point(172, 180);
            this.tsetIDCB.Name = "tsetIDCB";
            this.tsetIDCB.Size = new System.Drawing.Size(49, 21);
            this.tsetIDCB.TabIndex = 3;
            // 
            // vsetIDLabel
            // 
            this.vsetIDLabel.AutoSize = true;
            this.vsetIDLabel.Location = new System.Drawing.Point(242, 163);
            this.vsetIDLabel.Name = "vsetIDLabel";
            this.vsetIDLabel.Size = new System.Drawing.Size(70, 13);
            this.vsetIDLabel.TabIndex = 2;
            this.vsetIDLabel.Text = "Vertex Set ID";
            // 
            // vsetIDCB
            // 
            this.vsetIDCB.FormattingEnabled = true;
            this.vsetIDCB.Location = new System.Drawing.Point(242, 180);
            this.vsetIDCB.Name = "vsetIDCB";
            this.vsetIDCB.Size = new System.Drawing.Size(49, 21);
            this.vsetIDCB.TabIndex = 3;
            // 
            // psetIDLabel
            // 
            this.psetIDLabel.AutoSize = true;
            this.psetIDLabel.Location = new System.Drawing.Point(308, 163);
            this.psetIDLabel.Name = "psetIDLabel";
            this.psetIDLabel.Size = new System.Drawing.Size(64, 13);
            this.psetIDLabel.TabIndex = 2;
            this.psetIDLabel.Text = "Face Set ID";
            // 
            // faceSetIDCB
            // 
            this.faceSetIDCB.FormattingEnabled = true;
            this.faceSetIDCB.Location = new System.Drawing.Point(308, 180);
            this.faceSetIDCB.Name = "faceSetIDCB";
            this.faceSetIDCB.Size = new System.Drawing.Size(49, 21);
            this.faceSetIDCB.TabIndex = 3;
            // 
            // warningLabel
            // 
            this.warningLabel.AutoSize = true;
            this.warningLabel.Location = new System.Drawing.Point(64, 131);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(227, 13);
            this.warningLabel.TabIndex = 11;
            this.warningLabel.Text = "Warning, high risk of issues from editing below!";
            this.warningLabel.Click += new System.EventHandler(this.warningLabel_Click);
            // 
            // MeshStructEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.faceSetIDCB);
            this.Controls.Add(this.psetIDLabel);
            this.Controls.Add(this.vsetIDCB);
            this.Controls.Add(this.vsetIDLabel);
            this.Controls.Add(this.tsetIDCB);
            this.Controls.Add(this.texSetIDLabel);
            this.Controls.Add(this.shadIDCB);
            this.Controls.Add(this.shaderLabel);
            this.Controls.Add(this.renderIDCB);
            this.Controls.Add(this.renderIDLabel);
            this.Controls.Add(this.matIDCB);
            this.Controls.Add(this.matIDLabel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flagsUD);
            this.Controls.Add(this.flagsLabel);
            this.Name = "MeshStructEditor";
            this.Size = new System.Drawing.Size(378, 204);
            ((System.ComponentModel.ISupportInitialize)(this.flagsUD)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox meshIDCB;
        private System.Windows.Forms.Label meshLabel;
        private System.Windows.Forms.NumericUpDown flagsUD;
        private System.Windows.Forms.Label flagsLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label matIDLabel;
        private System.Windows.Forms.ComboBox matIDCB;
        private System.Windows.Forms.Label renderIDLabel;
        private System.Windows.Forms.ComboBox renderIDCB;
        private System.Windows.Forms.Label shaderLabel;
        private System.Windows.Forms.ComboBox shadIDCB;
        private System.Windows.Forms.Label texSetIDLabel;
        private System.Windows.Forms.ComboBox tsetIDCB;
        private System.Windows.Forms.Label vsetIDLabel;
        private System.Windows.Forms.ComboBox vsetIDCB;
        private System.Windows.Forms.Label psetIDLabel;
        private System.Windows.Forms.ComboBox faceSetIDCB;
        private System.Windows.Forms.Label warningLabel;
    }
}
