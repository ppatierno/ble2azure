namespace FEZSpiderMonitor
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Telerik.WinControls.UI.CartesianArea cartesianArea4 = new Telerik.WinControls.UI.CartesianArea();
            Telerik.WinControls.UI.CartesianArea cartesianArea1 = new Telerik.WinControls.UI.CartesianArea();
            Telerik.WinControls.UI.CartesianArea cartesianArea2 = new Telerik.WinControls.UI.CartesianArea();
            this.radChartViewTemp = new Telerik.WinControls.UI.RadChartView();
            this.radChartViewHmdt = new Telerik.WinControls.UI.RadChartView();
            this.radChartViewAcc = new Telerik.WinControls.UI.RadChartView();
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewTemp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewHmdt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewAcc)).BeginInit();
            this.SuspendLayout();
            // 
            // radChartViewTemp
            // 
            this.radChartViewTemp.AreaDesign = cartesianArea4;
            this.radChartViewTemp.Location = new System.Drawing.Point(12, 10);
            this.radChartViewTemp.Name = "radChartViewTemp";
            this.radChartViewTemp.ShowGrid = false;
            this.radChartViewTemp.Size = new System.Drawing.Size(643, 180);
            this.radChartViewTemp.TabIndex = 0;
            this.radChartViewTemp.Text = "radChartViewTemp";
            // 
            // radChartViewHmdt
            // 
            this.radChartViewHmdt.AreaDesign = cartesianArea1;
            this.radChartViewHmdt.Location = new System.Drawing.Point(12, 196);
            this.radChartViewHmdt.Name = "radChartViewHmdt";
            this.radChartViewHmdt.ShowGrid = false;
            this.radChartViewHmdt.Size = new System.Drawing.Size(643, 180);
            this.radChartViewHmdt.TabIndex = 1;
            this.radChartViewHmdt.Text = "radChartViewHmdt";
            // 
            // radChartViewAcc
            // 
            this.radChartViewAcc.AreaDesign = cartesianArea2;
            this.radChartViewAcc.Location = new System.Drawing.Point(12, 382);
            this.radChartViewAcc.Name = "radChartViewAcc";
            this.radChartViewAcc.ShowGrid = false;
            this.radChartViewAcc.Size = new System.Drawing.Size(643, 180);
            this.radChartViewAcc.TabIndex = 2;
            this.radChartViewAcc.Text = "radChartViewAcc";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 574);
            this.Controls.Add(this.radChartViewAcc);
            this.Controls.Add(this.radChartViewHmdt);
            this.Controls.Add(this.radChartViewTemp);
            this.Name = "Form1";
            this.Text = "Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewTemp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewHmdt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewAcc)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Telerik.WinControls.UI.RadChartView radChartViewTemp;
        private Telerik.WinControls.UI.RadChartView radChartViewHmdt;
        private Telerik.WinControls.UI.RadChartView radChartViewAcc;
    }
}

