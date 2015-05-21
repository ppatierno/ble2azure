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
    public partial class Form2 : Form
    {
        EventProcessorHost eventProcessorHost;

        // model for the charts on the UI
        HeartDataModel model;
        // queue for reading data (for the model) from the Event Hub Processor
        Queue<ChartBusinessObject> queue;

        Font font = new Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular);

        // timer to dequeue data (for the UI)
        private Timer timer;

        public Form2()
        {
            InitializeComponent();

            model = Locator.GetInstance().HeartDataModel;
            queue = Locator.GetInstance().Queue;

            this.timer = new Timer();
            this.timer.Interval = 100;
            this.timer.Tick += timer_Tick;

            InitializeChartHeartRate();

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
                        case ChartBusinessObjectType.HeartRate:
                            if (model.HeartRate.Count > 30)
                                model.HeartRate.RemoveAt(0);
                            model.HeartRate.Add(obj);
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

        private void InitializeChartHeartRate()
        {
            CartesianArea area = this.radChartViewHeartRate.GetArea<CartesianArea>();
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
            lineSeries.DataSource = model.HeartRate;
            lineSeries.BorderColor = Color.FromArgb(142, 196, 65);
            lineSeries.BorderWidth = 2;

            this.radChartViewHeartRate.Series.Add(lineSeries);

            this.radChartViewHeartRate.ChartElement.TitlePosition = TitlePosition.Top;
            this.radChartViewHeartRate.Title = "Heart Rate";
            this.radChartViewHeartRate.ChartElement.ShowTitle = true;
            this.radChartViewHeartRate.ChartElement.TitleElement.TextAlignment = ContentAlignment.MiddleLeft;
            this.radChartViewHeartRate.ChartElement.TitleElement.Margin = new Padding(10, 0, 0, 0);
            this.radChartViewHeartRate.ChartElement.TitleElement.Font = font;
            this.radChartViewHeartRate.View.Margin = new Padding(10, 0, 10, 0);

            LinearAxis axeY = radChartViewHeartRate.Axes.Get<LinearAxis>(1);
            axeY.Minimum = 100;
            axeY.Maximum = 180;
            //axeY.MajorStep = 1;

            CategoricalAxis axeX = radChartViewHeartRate.Axes.Get<CategoricalAxis>(0);
            axeX.LabelInterval = 5;
            axeX.LabelFormat = "{0:HH:mm:ss}";
            axeX.LastLabelVisibility = Telerik.Charting.AxisLastLabelVisibility.Visible;
        }
    }
}
