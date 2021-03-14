namespace AquaModelTool
{
    partial class MaterialEditor
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
            this.materialsLabel = new System.Windows.Forms.Label();
            this.matIDCB = new System.Windows.Forms.ComboBox();
            this.opaqueRB = new System.Windows.Forms.RadioButton();
            this.blendAlphaRB = new System.Windows.Forms.RadioButton();
            this.hollowRB = new System.Windows.Forms.RadioButton();
            this.addRB = new System.Windows.Forms.RadioButton();
            this.blendTypeLabel = new System.Windows.Forms.Label();
            this.matNameTextBox = new System.Windows.Forms.TextBox();
            this.materialNameLabel = new System.Windows.Forms.Label();
            this.diffuseRGBButton = new System.Windows.Forms.Button();
            this.diffuseRGBALabel = new System.Windows.Forms.Label();
            this.diffuseUD = new System.Windows.Forms.NumericUpDown();
            this.tex2UD = new System.Windows.Forms.NumericUpDown();
            this.tex2RGBALabel = new System.Windows.Forms.Label();
            this.tex2RGBAButton = new System.Windows.Forms.Button();
            this.tex3SpecUD = new System.Windows.Forms.NumericUpDown();
            this.tex3RGBALabel = new System.Windows.Forms.Label();
            this.tex3RGBAButton = new System.Windows.Forms.Button();
            this.tex4UD = new System.Windows.Forms.NumericUpDown();
            this.tex4RGBALabel = new System.Windows.Forms.Label();
            this.tex4RGBAButton = new System.Windows.Forms.Button();
            this.specLevelLabel = new System.Windows.Forms.Label();
            this.specLevelUD = new System.Windows.Forms.NumericUpDown();
            this.unkF32UD = new System.Windows.Forms.NumericUpDown();
            this.unkF32Label = new System.Windows.Forms.Label();
            this.unkInt0UD = new System.Windows.Forms.NumericUpDown();
            this.unkInt0Label = new System.Windows.Forms.Label();
            this.unkInt1UD = new System.Windows.Forms.NumericUpDown();
            this.unkInt1Label = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.diffuseUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tex2UD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tex3SpecUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tex4UD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.specLevelUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.unkF32UD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.unkInt0UD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.unkInt1UD)).BeginInit();
            this.SuspendLayout();
            // 
            // materialsLabel
            // 
            this.materialsLabel.AutoSize = true;
            this.materialsLabel.Location = new System.Drawing.Point(4, 4);
            this.materialsLabel.Name = "materialsLabel";
            this.materialsLabel.Size = new System.Drawing.Size(49, 13);
            this.materialsLabel.TabIndex = 0;
            this.materialsLabel.Text = "Materials";
            // 
            // matIDCB
            // 
            this.matIDCB.FormattingEnabled = true;
            this.matIDCB.Location = new System.Drawing.Point(4, 21);
            this.matIDCB.Name = "matIDCB";
            this.matIDCB.Size = new System.Drawing.Size(49, 21);
            this.matIDCB.TabIndex = 1;
            this.matIDCB.SelectedIndexChanged += new System.EventHandler(this.matIDCB_SelectedIndexChanged);
            // 
            // opaqueRB
            // 
            this.opaqueRB.AutoSize = true;
            this.opaqueRB.Location = new System.Drawing.Point(145, 21);
            this.opaqueRB.Name = "opaqueRB";
            this.opaqueRB.Size = new System.Drawing.Size(63, 17);
            this.opaqueRB.TabIndex = 2;
            this.opaqueRB.TabStop = true;
            this.opaqueRB.Text = "Opaque";
            this.opaqueRB.UseVisualStyleBackColor = true;
            this.opaqueRB.CheckedChanged += new System.EventHandler(this.opaqueRB_CheckedChanged);
            // 
            // blendAlphaRB
            // 
            this.blendAlphaRB.AutoSize = true;
            this.blendAlphaRB.Location = new System.Drawing.Point(204, 21);
            this.blendAlphaRB.Name = "blendAlphaRB";
            this.blendAlphaRB.Size = new System.Drawing.Size(78, 17);
            this.blendAlphaRB.TabIndex = 3;
            this.blendAlphaRB.TabStop = true;
            this.blendAlphaRB.Text = "Blendalpha";
            this.blendAlphaRB.UseVisualStyleBackColor = true;
            this.blendAlphaRB.CheckedChanged += new System.EventHandler(this.blendAlphaRB_CheckedChanged);
            // 
            // hollowRB
            // 
            this.hollowRB.AutoSize = true;
            this.hollowRB.Location = new System.Drawing.Point(278, 21);
            this.hollowRB.Name = "hollowRB";
            this.hollowRB.Size = new System.Drawing.Size(57, 17);
            this.hollowRB.TabIndex = 4;
            this.hollowRB.TabStop = true;
            this.hollowRB.Text = "Hollow";
            this.hollowRB.UseVisualStyleBackColor = true;
            this.hollowRB.CheckedChanged += new System.EventHandler(this.hollowRB_CheckedChanged);
            // 
            // addRB
            // 
            this.addRB.AutoSize = true;
            this.addRB.Location = new System.Drawing.Point(331, 21);
            this.addRB.Name = "addRB";
            this.addRB.Size = new System.Drawing.Size(44, 17);
            this.addRB.TabIndex = 5;
            this.addRB.TabStop = true;
            this.addRB.Text = "Add";
            this.addRB.UseVisualStyleBackColor = true;
            this.addRB.CheckedChanged += new System.EventHandler(this.addRB_CheckedChanged);
            // 
            // blendTypeLabel
            // 
            this.blendTypeLabel.AutoSize = true;
            this.blendTypeLabel.Location = new System.Drawing.Point(145, 4);
            this.blendTypeLabel.Name = "blendTypeLabel";
            this.blendTypeLabel.Size = new System.Drawing.Size(61, 13);
            this.blendTypeLabel.TabIndex = 6;
            this.blendTypeLabel.Text = "Blend Type";
            // 
            // matNameTextBox
            // 
            this.matNameTextBox.Location = new System.Drawing.Point(88, 41);
            this.matNameTextBox.MaxLength = 32;
            this.matNameTextBox.Name = "matNameTextBox";
            this.matNameTextBox.Size = new System.Drawing.Size(262, 20);
            this.matNameTextBox.TabIndex = 8;
            this.matNameTextBox.TextChanged += new System.EventHandler(this.matNameTextBox_TextChanged);
            // 
            // materialNameLabel
            // 
            this.materialNameLabel.AutoSize = true;
            this.materialNameLabel.Location = new System.Drawing.Point(4, 44);
            this.materialNameLabel.Name = "materialNameLabel";
            this.materialNameLabel.Size = new System.Drawing.Size(78, 13);
            this.materialNameLabel.TabIndex = 9;
            this.materialNameLabel.Text = "Material Name:";
            // 
            // diffuseRGBButton
            // 
            this.diffuseRGBButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.diffuseRGBButton.Location = new System.Drawing.Point(7, 82);
            this.diffuseRGBButton.Name = "diffuseRGBButton";
            this.diffuseRGBButton.Size = new System.Drawing.Size(31, 23);
            this.diffuseRGBButton.TabIndex = 10;
            this.diffuseRGBButton.UseVisualStyleBackColor = false;
            this.diffuseRGBButton.Click += new System.EventHandler(this.diffuseRGBButton_Click);
            // 
            // diffuseRGBALabel
            // 
            this.diffuseRGBALabel.AutoSize = true;
            this.diffuseRGBALabel.Location = new System.Drawing.Point(5, 67);
            this.diffuseRGBALabel.Name = "diffuseRGBALabel";
            this.diffuseRGBALabel.Size = new System.Drawing.Size(73, 13);
            this.diffuseRGBALabel.TabIndex = 11;
            this.diffuseRGBALabel.Text = "Diffuse RGBA";
            // 
            // diffuseUD
            // 
            this.diffuseUD.DecimalPlaces = 2;
            this.diffuseUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.diffuseUD.Location = new System.Drawing.Point(45, 84);
            this.diffuseUD.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.diffuseUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147418112});
            this.diffuseUD.Name = "diffuseUD";
            this.diffuseUD.Size = new System.Drawing.Size(45, 20);
            this.diffuseUD.TabIndex = 12;
            this.diffuseUD.ValueChanged += new System.EventHandler(this.diffuseUD_ValueChanged);
            // 
            // tex2UD
            // 
            this.tex2UD.DecimalPlaces = 2;
            this.tex2UD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.tex2UD.Location = new System.Drawing.Point(136, 84);
            this.tex2UD.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tex2UD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147418112});
            this.tex2UD.Name = "tex2UD";
            this.tex2UD.Size = new System.Drawing.Size(45, 20);
            this.tex2UD.TabIndex = 15;
            this.tex2UD.ValueChanged += new System.EventHandler(this.tex2UD_ValueChanged);
            // 
            // tex2RGBALabel
            // 
            this.tex2RGBALabel.AutoSize = true;
            this.tex2RGBALabel.Location = new System.Drawing.Point(96, 67);
            this.tex2RGBALabel.Name = "tex2RGBALabel";
            this.tex2RGBALabel.Size = new System.Drawing.Size(64, 13);
            this.tex2RGBALabel.TabIndex = 14;
            this.tex2RGBALabel.Text = "Tex2 RGBA";
            // 
            // tex2RGBAButton
            // 
            this.tex2RGBAButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tex2RGBAButton.Location = new System.Drawing.Point(98, 82);
            this.tex2RGBAButton.Name = "tex2RGBAButton";
            this.tex2RGBAButton.Size = new System.Drawing.Size(31, 23);
            this.tex2RGBAButton.TabIndex = 13;
            this.tex2RGBAButton.UseVisualStyleBackColor = false;
            this.tex2RGBAButton.Click += new System.EventHandler(this.tex2RGBAButton_Click);
            // 
            // tex3SpecUD
            // 
            this.tex3SpecUD.DecimalPlaces = 2;
            this.tex3SpecUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.tex3SpecUD.Location = new System.Drawing.Point(225, 84);
            this.tex3SpecUD.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tex3SpecUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147418112});
            this.tex3SpecUD.Name = "tex3SpecUD";
            this.tex3SpecUD.Size = new System.Drawing.Size(45, 20);
            this.tex3SpecUD.TabIndex = 18;
            this.tex3SpecUD.ValueChanged += new System.EventHandler(this.tex3SpecUD_ValueChanged);
            // 
            // tex3RGBALabel
            // 
            this.tex3RGBALabel.AutoSize = true;
            this.tex3RGBALabel.Location = new System.Drawing.Point(185, 67);
            this.tex3RGBALabel.Name = "tex3RGBALabel";
            this.tex3RGBALabel.Size = new System.Drawing.Size(94, 13);
            this.tex3RGBALabel.TabIndex = 17;
            this.tex3RGBALabel.Text = "Tex3/Spec RGBA";
            // 
            // tex3RGBAButton
            // 
            this.tex3RGBAButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tex3RGBAButton.Location = new System.Drawing.Point(187, 82);
            this.tex3RGBAButton.Name = "tex3RGBAButton";
            this.tex3RGBAButton.Size = new System.Drawing.Size(31, 23);
            this.tex3RGBAButton.TabIndex = 16;
            this.tex3RGBAButton.UseVisualStyleBackColor = false;
            this.tex3RGBAButton.Click += new System.EventHandler(this.tex3RGBAButton_Click);
            // 
            // tex4UD
            // 
            this.tex4UD.DecimalPlaces = 2;
            this.tex4UD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.tex4UD.Location = new System.Drawing.Point(315, 84);
            this.tex4UD.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tex4UD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147418112});
            this.tex4UD.Name = "tex4UD";
            this.tex4UD.Size = new System.Drawing.Size(45, 20);
            this.tex4UD.TabIndex = 21;
            this.tex4UD.ValueChanged += new System.EventHandler(this.tex4UD_ValueChanged);
            // 
            // tex4RGBALabel
            // 
            this.tex4RGBALabel.AutoSize = true;
            this.tex4RGBALabel.Location = new System.Drawing.Point(275, 67);
            this.tex4RGBALabel.Name = "tex4RGBALabel";
            this.tex4RGBALabel.Size = new System.Drawing.Size(64, 13);
            this.tex4RGBALabel.TabIndex = 20;
            this.tex4RGBALabel.Text = "Tex4 RGBA";
            // 
            // tex4RGBAButton
            // 
            this.tex4RGBAButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tex4RGBAButton.Location = new System.Drawing.Point(277, 82);
            this.tex4RGBAButton.Name = "tex4RGBAButton";
            this.tex4RGBAButton.Size = new System.Drawing.Size(31, 23);
            this.tex4RGBAButton.TabIndex = 19;
            this.tex4RGBAButton.UseVisualStyleBackColor = false;
            this.tex4RGBAButton.Click += new System.EventHandler(this.tex4RGBAButton_Click);
            // 
            // specLevelLabel
            // 
            this.specLevelLabel.AutoSize = true;
            this.specLevelLabel.Location = new System.Drawing.Point(4, 112);
            this.specLevelLabel.Name = "specLevelLabel";
            this.specLevelLabel.Size = new System.Drawing.Size(84, 13);
            this.specLevelLabel.TabIndex = 22;
            this.specLevelLabel.Text = "Specular Level?";
            // 
            // specLevelUD
            // 
            this.specLevelUD.Location = new System.Drawing.Point(4, 129);
            this.specLevelUD.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.specLevelUD.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.specLevelUD.Name = "specLevelUD";
            this.specLevelUD.Size = new System.Drawing.Size(74, 20);
            this.specLevelUD.TabIndex = 23;
            this.specLevelUD.ValueChanged += new System.EventHandler(this.specLevelUD_ValueChanged);
            // 
            // unkF32UD
            // 
            this.unkF32UD.Location = new System.Drawing.Point(5, 169);
            this.unkF32UD.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.unkF32UD.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.unkF32UD.Name = "unkF32UD";
            this.unkF32UD.Size = new System.Drawing.Size(74, 20);
            this.unkF32UD.TabIndex = 25;
            this.unkF32UD.ValueChanged += new System.EventHandler(this.unkF32UD_ValueChanged);
            // 
            // unkF32Label
            // 
            this.unkF32Label.AutoSize = true;
            this.unkF32Label.Location = new System.Drawing.Point(5, 152);
            this.unkF32Label.Name = "unkF32Label";
            this.unkF32Label.Size = new System.Drawing.Size(74, 13);
            this.unkF32Label.TabIndex = 24;
            this.unkF32Label.Text = "Unknown F32";
            // 
            // unkInt0UD
            // 
            this.unkInt0UD.Location = new System.Drawing.Point(88, 129);
            this.unkInt0UD.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.unkInt0UD.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.unkInt0UD.Name = "unkInt0UD";
            this.unkInt0UD.Size = new System.Drawing.Size(74, 20);
            this.unkInt0UD.TabIndex = 27;
            this.unkInt0UD.ValueChanged += new System.EventHandler(this.unkInt0UD_ValueChanged);
            // 
            // unkInt0Label
            // 
            this.unkInt0Label.AutoSize = true;
            this.unkInt0Label.Location = new System.Drawing.Point(88, 112);
            this.unkInt0Label.Name = "unkInt0Label";
            this.unkInt0Label.Size = new System.Drawing.Size(74, 13);
            this.unkInt0Label.TabIndex = 26;
            this.unkInt0Label.Text = "Unknown Int0";
            // 
            // unkInt1UD
            // 
            this.unkInt1UD.Location = new System.Drawing.Point(88, 169);
            this.unkInt1UD.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.unkInt1UD.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.unkInt1UD.Name = "unkInt1UD";
            this.unkInt1UD.Size = new System.Drawing.Size(74, 20);
            this.unkInt1UD.TabIndex = 29;
            this.unkInt1UD.ValueChanged += new System.EventHandler(this.unkInt1UD_ValueChanged);
            // 
            // unkInt1Label
            // 
            this.unkInt1Label.AutoSize = true;
            this.unkInt1Label.Location = new System.Drawing.Point(88, 152);
            this.unkInt1Label.Name = "unkInt1Label";
            this.unkInt1Label.Size = new System.Drawing.Size(74, 13);
            this.unkInt1Label.TabIndex = 28;
            this.unkInt1Label.Text = "Unknown Int1";
            // 
            // MaterialEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.unkInt1UD);
            this.Controls.Add(this.unkInt1Label);
            this.Controls.Add(this.unkInt0UD);
            this.Controls.Add(this.unkInt0Label);
            this.Controls.Add(this.unkF32UD);
            this.Controls.Add(this.unkF32Label);
            this.Controls.Add(this.specLevelUD);
            this.Controls.Add(this.specLevelLabel);
            this.Controls.Add(this.tex4UD);
            this.Controls.Add(this.tex4RGBALabel);
            this.Controls.Add(this.tex4RGBAButton);
            this.Controls.Add(this.tex3SpecUD);
            this.Controls.Add(this.tex3RGBALabel);
            this.Controls.Add(this.tex3RGBAButton);
            this.Controls.Add(this.tex2UD);
            this.Controls.Add(this.tex2RGBALabel);
            this.Controls.Add(this.tex2RGBAButton);
            this.Controls.Add(this.diffuseUD);
            this.Controls.Add(this.diffuseRGBALabel);
            this.Controls.Add(this.diffuseRGBButton);
            this.Controls.Add(this.materialNameLabel);
            this.Controls.Add(this.matNameTextBox);
            this.Controls.Add(this.blendTypeLabel);
            this.Controls.Add(this.addRB);
            this.Controls.Add(this.hollowRB);
            this.Controls.Add(this.blendAlphaRB);
            this.Controls.Add(this.opaqueRB);
            this.Controls.Add(this.matIDCB);
            this.Controls.Add(this.materialsLabel);
            this.Name = "MaterialEditor";
            this.Size = new System.Drawing.Size(378, 204);
            ((System.ComponentModel.ISupportInitialize)(this.diffuseUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tex2UD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tex3SpecUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tex4UD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.specLevelUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.unkF32UD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.unkInt0UD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.unkInt1UD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label materialsLabel;
        private System.Windows.Forms.ComboBox matIDCB;
        private System.Windows.Forms.RadioButton opaqueRB;
        private System.Windows.Forms.RadioButton blendAlphaRB;
        private System.Windows.Forms.RadioButton hollowRB;
        private System.Windows.Forms.RadioButton addRB;
        private System.Windows.Forms.Label blendTypeLabel;
        private System.Windows.Forms.TextBox matNameTextBox;
        private System.Windows.Forms.Label materialNameLabel;
        private System.Windows.Forms.Button diffuseRGBButton;
        private System.Windows.Forms.Label diffuseRGBALabel;
        private System.Windows.Forms.NumericUpDown diffuseUD;
        private System.Windows.Forms.NumericUpDown tex2UD;
        private System.Windows.Forms.Label tex2RGBALabel;
        private System.Windows.Forms.Button tex2RGBAButton;
        private System.Windows.Forms.NumericUpDown tex3SpecUD;
        private System.Windows.Forms.Label tex3RGBALabel;
        private System.Windows.Forms.Button tex3RGBAButton;
        private System.Windows.Forms.NumericUpDown tex4UD;
        private System.Windows.Forms.Label tex4RGBALabel;
        private System.Windows.Forms.Button tex4RGBAButton;
        private System.Windows.Forms.Label specLevelLabel;
        private System.Windows.Forms.NumericUpDown specLevelUD;
        private System.Windows.Forms.NumericUpDown unkF32UD;
        private System.Windows.Forms.Label unkF32Label;
        private System.Windows.Forms.NumericUpDown unkInt0UD;
        private System.Windows.Forms.Label unkInt0Label;
        private System.Windows.Forms.NumericUpDown unkInt1UD;
        private System.Windows.Forms.Label unkInt1Label;
    }
}
