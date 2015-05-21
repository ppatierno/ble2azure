using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace FEZSpiderMonitor
{
    public partial class Form1 : Form
    {
        EventProcessorHost eventProcessorHost;

        // model for the charts on the UI
        LiveDataModel model;
        // queue for reading data (for the model) from the Event Hub Processor
        Queue<ChartBusinessObject> queue;

        Font font = new Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular);

        // timer to dequeue data (for the UI)
        private Timer timer;
        
        public Form1()
        {
            InitializeComponent();

            model = Locator.GetInstance().LiveDataModel;
            queue = Locator.GetInstance().Queue;

            this.timer = new Timer();
            this.timer.Interval = 100;
            this.timer.Tick += timer_Tick;

            InitializeChartTemp();
            InitializeChartHmdt();
            InitializeChartAcc();

            this.timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (queue.Count > 0)
            {
                int count = queue.Count;

                while (count > 0)
                {
                    ChartBusinessObject obj = queue.Dequeue();

                    switch (obj.Type)
                    {
                        case ChartBusinessObjectType.Temperature:
                            if (model.Temperature.Count > 30)
                                model.Temperature.RemoveAt(0);
                            model.Temperature.Add(obj);
                            break;
                        case ChartBusinessObjectType.Humidity:
                            if (model.Humidity.Count > 30)
                                model.Humidity.RemoveAt(0);
                            model.Humidity.Add(obj);
                            break;
                        case ChartBusinessObjectType.AccelerationX:
                            if (model.AccX.Count > 30)
                                model.AccX.RemoveAt(0);
                            model.AccX.Add(obj);
                            break;
                        case ChartBusinessObjectType.AccelerationY:
                            if (model.AccY.Count > 30)
                                model.AccY.RemoveAt(0);
                            model.AccY.Add(obj);
                            break;
                        case ChartBusinessObjectType.AccelerationZ:
                            if (model.AccZ.Count > 30)
                                model.AccZ.RemoveAt(0);
                            model.AccZ.Add(obj);
                            break;
                    }

                    count--;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string eventHubConnectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            string eventHubName = ConfigurationManager.AppSettings["EventHubName"];
            string storageConnectionString = ConfigurationManager.AppSettings["Microsoft.WindowsAzure.Storage.ConnectionString"];

            string eventProcessorHostName = Guid.NewGuid().ToString();
            eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            eventProcessorHost.RegisterEventProcessorAsync<FEZSpiderEventHubProcessor>();
        }

        private void InitializeChartTemp()
        {
            CartesianArea area = this.radChartViewTemp.GetArea<CartesianArea>();
            CartesianGrid grid = area.GetGrid<CartesianGrid>();
            grid.ForeColor = Color.FromArgb(235, 235, 235);
            grid.DrawVerticalStripes = false;
            grid.DrawHorizontalStripes = true;
            grid.DrawHorizontalFills = false;
            grid.DrawVerticalFills = false;
            area.ShowGrid = true;

            LineSeries lineSeries = new LineSeries();
            lineSeries.PointSize = new SizeF(0, 0);
            lineSeries.CategoryMember = "Time";
            lineSeries.ValueMember = "Value";
            lineSeries.DataSource = model.Temperature;
            lineSeries.BorderColor = Color.FromArgb(142, 196, 65);
            lineSeries.BorderWidth = 2;

            this.radChartViewTemp.Series.Add(lineSeries);

            this.radChartViewTemp.ChartElement.TitlePosition = TitlePosition.Top;
            this.radChartViewTemp.Title = "Temperature";
            this.radChartViewTemp.ChartElement.ShowTitle = true;
            this.radChartViewTemp.ChartElement.TitleElement.TextAlignment = ContentAlignment.MiddleLeft;
            this.radChartViewTemp.ChartElement.TitleElement.Margin = new Padding(10, 0, 0, 0);
            this.radChartViewTemp.ChartElement.TitleElement.Font = font;
            this.radChartViewTemp.View.Margin = new Padding(10, 0, 10, 0);

            LinearAxis axeY = radChartViewTemp.Axes.Get <LinearAxis>(1);
            //axeY.Minimum = 18;
            //axeY.Maximum = 25;
            //axeY.MajorStep = 0.5;

            CategoricalAxis axeX = radChartViewTemp.Axes.Get <CategoricalAxis>(0);
            axeX.LabelInterval = 5;
            axeX.LabelFormat = "{0:HH:mm:ss}";
            axeX.LastLabelVisibility = Telerik.Charting.AxisLastLabelVisibility.Visible;
        }

        private void InitializeChartHmdt()
        {
            CartesianArea area = this.radChartViewHmdt.GetArea<CartesianArea>();
            CartesianGrid grid = area.GetGrid<CartesianGrid>();
            grid.ForeColor = Color.FromArgb(235, 235, 235);
            grid.DrawVerticalStripes = false;
            grid.DrawHorizontalStripes = true;
            grid.DrawHorizontalFills = false;
            grid.DrawVerticalFills = false;
            area.ShowGrid = true;

            LineSeries lineSeries = new LineSeries();
            lineSeries.PointSize = new SizeF(0, 0);
            lineSeries.CategoryMember = "Time";
            lineSeries.ValueMember = "Value";
            lineSeries.DataSource = model.Humidity;
            lineSeries.BorderColor = Color.FromArgb(142, 196, 65);
            lineSeries.BorderWidth = 2;

            this.radChartViewHmdt.Series.Add(lineSeries);

            this.radChartViewHmdt.ChartElement.TitlePosition = TitlePosition.Top;
            this.radChartViewHmdt.Title = "Humidity";
            this.radChartViewHmdt.ChartElement.ShowTitle = true;
            this.radChartViewHmdt.ChartElement.TitleElement.TextAlignment = ContentAlignment.MiddleLeft;
            this.radChartViewHmdt.ChartElement.TitleElement.Margin = new Padding(10, 0, 0, 0);
            this.radChartViewHmdt.ChartElement.TitleElement.Font = font;
            this.radChartViewHmdt.View.Margin = new Padding(10, 0, 10, 0);

            LinearAxis axeY = radChartViewHmdt.Axes.Get<LinearAxis>(1);
            //axeY.Minimum = 18;
            //axeY.Maximum = 25;
            //axeY.MajorStep = 0.5;

            CategoricalAxis axeX = radChartViewHmdt.Axes.Get<CategoricalAxis>(0);
            axeX.LabelInterval = 5;
            axeX.LabelFormat = "{0:HH:mm:ss}";
            axeX.LastLabelVisibility = Telerik.Charting.AxisLastLabelVisibility.Visible;
        }

        private void InitializeChartAcc()
        {
            CartesianArea area = this.radChartViewAcc.GetArea<CartesianArea>();
            CartesianGrid grid = area.GetGrid<CartesianGrid>();
            grid.ForeColor = Color.FromArgb(235, 235, 235);
            grid.DrawVerticalStripes = false;
            grid.DrawHorizontalStripes = true;
            grid.DrawHorizontalFills = false;
            grid.DrawVerticalFills = false;
            area.ShowGrid = true;

            LineSeries lineSeriesAccX = new LineSeries();
            lineSeriesAccX.PointSize = new SizeF(0, 0);
            lineSeriesAccX.CategoryMember = "Time";
            lineSeriesAccX.ValueMember = "Value";
            lineSeriesAccX.DataSource = model.AccX;
            lineSeriesAccX.BorderColor = Color.FromArgb(0, 0, 255);
            lineSeriesAccX.BorderWidth = 2;

            LineSeries lineSeriesAccY = new LineSeries();
            lineSeriesAccY.PointSize = new SizeF(0, 0);
            lineSeriesAccY.CategoryMember = "Time";
            lineSeriesAccY.ValueMember = "Value";
            lineSeriesAccY.DataSource = model.AccY;
            lineSeriesAccY.BorderColor = Color.FromArgb(142, 196, 65);
            lineSeriesAccY.BorderWidth = 2;

            LineSeries lineSeriesAccZ = new LineSeries();
            lineSeriesAccZ.PointSize = new SizeF(0, 0);
            lineSeriesAccZ.CategoryMember = "Time";
            lineSeriesAccZ.ValueMember = "Value";
            lineSeriesAccZ.DataSource = model.AccZ;
            lineSeriesAccZ.BorderColor = Color.FromArgb(255, 0, 0);
            lineSeriesAccZ.BorderWidth = 2;

            this.radChartViewAcc.Series.Add(lineSeriesAccX);
            this.radChartViewAcc.Series.Add(lineSeriesAccY);
            this.radChartViewAcc.Series.Add(lineSeriesAccZ);

            this.radChartViewAcc.ChartElement.TitlePosition = TitlePosition.Top;
            this.radChartViewAcc.Title = "Accelerometer";
            this.radChartViewAcc.ChartElement.ShowTitle = true;
            this.radChartViewAcc.ChartElement.TitleElement.TextAlignment = ContentAlignment.MiddleLeft;
            this.radChartViewAcc.ChartElement.TitleElement.Margin = new Padding(10, 0, 0, 0);
            this.radChartViewAcc.ChartElement.TitleElement.Font = font;
            this.radChartViewAcc.View.Margin = new Padding(10, 0, 10, 0);

            LinearAxis axeY = radChartViewAcc.Axes.Get<LinearAxis>(1);
            //axeY.Minimum = 18;
            //axeY.Maximum = 25;
            //axeY.MajorStep = 0.5;

            CategoricalAxis axeX = radChartViewAcc.Axes.Get<CategoricalAxis>(0);
            axeX.LabelInterval = 5;
            axeX.LabelFormat = "{0:HH:mm:ss}";
            axeX.LastLabelVisibility = Telerik.Charting.AxisLastLabelVisibility.Visible;
        }
    }
}
