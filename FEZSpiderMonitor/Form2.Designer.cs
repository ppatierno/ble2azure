namespace FEZSpiderMonitor
{
    partial class Form2
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
            Telerik.WinControls.UI.CartesianArea cartesianArea1 = new Telerik.WinControls.UI.CartesianArea();
            this.radChartViewHeartRate = new Telerik.WinControls.UI.RadChartView();
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewHeartRate)).BeginInit();
            this.SuspendLayout();
            // 
            // radChartViewHeartRate
            // 
            this.radChartViewHeartRate.AreaDesign = cartesianArea1;
            this.radChartViewHeartRate.Location = new System.Drawing.Point(12, 10);
            this.radChartViewHeartRate.Name = "radChartViewHeartRate";
            this.radChartViewHeartRate.ShowGrid = false;
            this.radChartViewHeartRate.Size = new System.Drawing.Size(643, 180);
            this.radChartViewHeartRate.TabIndex = 1;
            this.radChartViewHeartRate.Text = "radChartViewTemp";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 200);
            this.Controls.Add(this.radChartViewHeartRate);
            this.Name = "Form2";
            this.Text = "Form2";
            ((System.ComponentModel.ISupportInitialize)(this.radChartViewHeartRate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Telerik.WinControls.UI.RadChartView radChartViewHeartRate;
    }
}