using MonikDesktop.Common.Enums;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class PropertiesViewModel : ViewModelBase, IPropertiesViewModel
    {
        public PropertiesViewModel(IShell shell)
        {
            Title = "Properties";

            shell.WhenAnyValue(x => x.SelectedView)
               .Where(v => !(v is IToolView))
               .Subscribe(v =>
                {
                    if (v is IShowView showView)
                    {
                        OnSelectedShowView(showView);
                        IsEnabled = true;
                    }
                    else
                        IsEnabled = false;
                });
        }

        [Reactive] public ShowModel   Model      { get; private set; }
        [Reactive] public IShowViewModel ShowWindow { get; private set; }

        public IList<TopType>            TopTypes            => Enum.GetValues(typeof(TopType)).Cast<TopType>().ToList();
        public IList<SeverityCutoffType> SeverityCutoffTypes => Enum.GetValues(typeof(SeverityCutoffType)).Cast<SeverityCutoffType>().ToList();
        public IList<MetricTerminalMode> MetricTerminalModes => Enum.GetValues(typeof(MetricTerminalMode)).Cast<MetricTerminalMode>().ToList();
        public IList<LevelType>          LevelTypes          => Enum.GetValues(typeof(LevelType)).Cast<LevelType>().ToList();

        public IList<string> DatetimeFormats =>
            new[]
            {
                "HH:mm:ss",
                "HH:mm:**",
                "dd.MM.yyyy HH:mm:ss",
                "dd.MM.yyyy HH:mm:**",
                "dd.MM HH:mm:ss",
                "dd.MM HH:mm:**"
            };

        [Reactive] public Visibility LogsVisibility       { get; set; } = Visibility.Collapsed;
        [Reactive] public Visibility KeepAlivesVisibility { get; set; } = Visibility.Collapsed;
        [Reactive] public Visibility MetricsVisibility    { get; set; } = Visibility.Collapsed;

        private void OnSelectedShowView(IShowView aWindow)
        {
            switch (aWindow)
            {
                case ILogsView _:
                    LogsVisibility       = Visibility.Visible;
                    KeepAlivesVisibility = Visibility.Collapsed;
                    MetricsVisibility    = Visibility.Collapsed;
                    break;
                case IKeepAliveView _:
                    LogsVisibility       = Visibility.Collapsed;
                    KeepAlivesVisibility = Visibility.Visible;
                    MetricsVisibility    = Visibility.Collapsed;
                    break;
                case IMetricsView _:
                    LogsVisibility       = Visibility.Collapsed;
                    KeepAlivesVisibility = Visibility.Collapsed;
                    MetricsVisibility    = Visibility.Visible;
                    break;
            }

            Model      = aWindow.ShowViewModel.Model;
            ShowWindow = aWindow.ShowViewModel;
        }
    }
}