
namespace AquaModelTool.Forms.ModelSubpanels
{
    partial class TextureListEditor
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
            this.texListCB = new System.Windows.Forms.ComboBox();
            this.texListLabel = new System.Windows.Forms.Label();
            this.texListSelectPanel = new System.Windows.Forms.Panel();
            this.texSlotCB = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.addSlotButton = new System.Windows.Forms.Button();
            this.removeSlotButton = new System.Windows.Forms.Button();
            this.insertSlotButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.insertListButton = new System.Windows.Forms.Button();
            this.removeListButton = new System.Windows.Forms.Button();
            this.addListButton = new System.Windows.Forms.Button();
            this.texListSelectPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // texListCB
            // 
            this.texListCB.FormattingEnabled = true;
            this.texListCB.Location = new System.Drawing.Point(4, 21);
            this.texListCB.Name = "texListCB";
            this.texListCB.Size = new System.Drawing.Size(49, 21);
            this.texListCB.TabIndex = 3;
            this.texListCB.SelectedIndexChanged += new System.EventHandler(this.texListCB_SelectedIndexChanged);
            // 
            // texListLabel
            // 
            this.texListLabel.AutoSize = true;
            this.texListLabel.Location = new System.Drawing.Point(3, 5);
            this.texListLabel.Name = "texListLabel";
            this.texListLabel.Size = new System.Drawing.Size(62, 13);
            this.texListLabel.TabIndex = 2;
            this.texListLabel.Text = "Texture List";
            // 
            // texListSelectPanel
            // 
            this.texListSelectPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.texListSelectPanel.Controls.Add(this.texListCB);
            this.texListSelectPanel.Controls.Add(this.texListLabel);
            this.texListSelectPanel.Location = new System.Drawing.Point(-1, -1);
            this.texListSelectPanel.Name = "texListSelectPanel";
            this.texListSelectPanel.Size = new System.Drawing.Size(77, 51);
            this.texListSelectPanel.TabIndex = 11;
            // 
            // texSlotCB
            // 
            this.texSlotCB.FormattingEnabled = true;
            this.texSlotCB.Location = new System.Drawing.Point(176, 29);
            this.texSlotCB.Name = "texSlotCB";
            this.texSlotCB.Size = new System.Drawing.Size(113, 21);
            this.texSlotCB.TabIndex = 12;
            this.texSlotCB.SelectedIndexChanged += new System.EventHandler(this.texSlotCB_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Texture Slot";
            // 
            // addButton
            // 
            this.addSlotButton.Location = new System.Drawing.Point(295, 3);
            this.addSlotButton.Name = "addButton";
            this.addSlotButton.Size = new System.Drawing.Size(83, 23);
            this.addSlotButton.TabIndex = 16;
            this.addSlotButton.Text = "Add Slot";
            this.addSlotButton.UseVisualStyleBackColor = true;
            this.addSlotButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // removeButton
            // 
            this.removeSlotButton.Location = new System.Drawing.Point(295, 53);
            this.removeSlotButton.Name = "removeButton";
            this.removeSlotButton.Size = new System.Drawing.Size(83, 23);
            this.removeSlotButton.TabIndex = 17;
            this.removeSlotButton.Text = "Remove Slot";
            this.removeSlotButton.UseVisualStyleBackColor = true;
            this.removeSlotButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // insertButton
            // 
            this.insertSlotButton.Location = new System.Drawing.Point(295, 28);
            this.insertSlotButton.Name = "insertButton";
            this.insertSlotButton.Size = new System.Drawing.Size(83, 23);
            this.insertSlotButton.TabIndex = 18;
            this.insertSlotButton.Text = "Insert Slot";
            this.insertSlotButton.UseVisualStyleBackColor = true;
            this.insertSlotButton.Click += new System.EventHandler(this.insertButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(-1, 82);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(379, 171);
            this.panel1.TabIndex = 19;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "label2";
            // 
            // insertListButton
            // 
            this.insertListButton.Location = new System.Drawing.Point(82, 28);
            this.insertListButton.Name = "insertListButton";
            this.insertListButton.Size = new System.Drawing.Size(83, 23);
            this.insertListButton.TabIndex = 22;
            this.insertListButton.Text = "Insert List";
            this.insertListButton.UseVisualStyleBackColor = true;
            // 
            // removeListButton
            // 
            this.removeListButton.Location = new System.Drawing.Point(82, 53);
            this.removeListButton.Name = "removeListButton";
            this.removeListButton.Size = new System.Drawing.Size(83, 23);
            this.removeListButton.TabIndex = 21;
            this.removeListButton.Text = "Remove List";
            this.removeListButton.UseVisualStyleBackColor = true;
            // 
            // addListButton
            // 
            this.addListButton.Location = new System.Drawing.Point(82, 3);
            this.addListButton.Name = "addListButton";
            this.addListButton.Size = new System.Drawing.Size(83, 23);
            this.addListButton.TabIndex = 20;
            this.addListButton.Text = "Add List";
            this.addListButton.UseVisualStyleBackColor = true;
            // 
            // TextureListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.insertListButton);
            this.Controls.Add(this.removeListButton);
            this.Controls.Add(this.addListButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.insertSlotButton);
            this.Controls.Add(this.removeSlotButton);
            this.Controls.Add(this.addSlotButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.texSlotCB);
            this.Controls.Add(this.texListSelectPanel);
            this.Name = "TextureListEditor";
            this.Size = new System.Drawing.Size(378, 253);
            this.texListSelectPanel.ResumeLayout(false);
            this.texListSelectPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox texListCB;
        private System.Windows.Forms.Label texListLabel;
        private System.Windows.Forms.Panel texListSelectPanel;
        private System.Windows.Forms.ComboBox texSlotCB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button addSlotButton;
        private System.Windows.Forms.Button removeSlotButton;
        private System.Windows.Forms.Button insertSlotButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button insertListButton;
        private System.Windows.Forms.Button removeListButton;
        private System.Windows.Forms.Button addListButton;
    }
}
