using MonikDesktop.Common.Enums;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels.ShowModels;
using MonikDesktop.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Ui.Wpf.Common;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.ViewModels
{
    public class PropertiesViewModel : ViewModelBase
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

        [Reactive] public ShowModel Model { get; private set; }
        [Reactive] public IShowViewModel ShowWindow { get; private set; }

        public IList<TopType> TopTypes => Enum.GetValues(typeof(TopType)).Cast<TopType>().ToList();

        public IList<SeverityCutoffType> SeverityCutoffTypes =>
            Enum.GetValues(typeof(SeverityCutoffType)).Cast<SeverityCutoffType>().ToList();

        public IList<MetricTerminalMode> MetricTerminalModes =>
            Enum.GetValues(typeof(MetricTerminalMode)).Cast<MetricTerminalMode>().ToList();

        public IList<LevelType> LevelTypes => Enum.GetValues(typeof(LevelType)).Cast<LevelType>().ToList();

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

        [Reactive] public bool LogsVisible { get; set; }
        [Reactive] public bool KeepAlivesVisible { get; set; }
        [Reactive] public bool MetricsVisible { get; set; }

        private void OnSelectedShowView(IShowView aWindow)
        {
            switch (aWindow)
            {
                case LogsView _:
                    LogsVisible = true;
                    KeepAlivesVisible = false;
                    MetricsVisible = false;
                    break;
                case KeepAliveView _:
                    LogsVisible = false;
                    KeepAlivesVisible = true;
                    MetricsVisible = false;
                    break;
                case MetricsView _:
                    LogsVisible = false;
                    KeepAlivesVisible = false;
                    MetricsVisible = true;
                    break;
            }

            Model = aWindow.ShowViewModel.Model;
            ShowWindow = aWindow.ShowViewModel;
        }
    }
}