using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;

namespace MonikDesktop.Views
{
    public partial class MetricsChartTooltipView : IChartTooltip
    {
        private TooltipData _data;

        public MetricsChartTooltipView()
        {
            InitializeComponent();

            //LiveCharts will inject the tooltip data in the Data property
            //your job is only to display this data as required

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipData Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}