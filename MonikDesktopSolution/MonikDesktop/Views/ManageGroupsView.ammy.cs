using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ui.Wpf.Common.ShowOptions;
using Ui.Wpf.Common.ViewModels;

namespace MonikDesktop.Views
{
    public partial class ManageGroupsView : IManageGroupsView
    {
        public ManageGroupsView(IManageGroupsViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        public IViewModel ViewModel { get; set; }

        public void Configure(UiShowOptions options)
        {
            ViewModel.Title = options.Title;
        }

        private Point DragStartPoint { get; set; }

        private void GridMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            // Store the mouse position
            DragStartPoint = e.GetPosition(null);
        }

        private void GridMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            var mousePos = e.GetPosition(null);
            var diff = DragStartPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                var grid = (DataGrid)sender;
                var gridRow = FindAnchestor<DataGridRow>((DependencyObject)e.OriginalSource);

                if (gridRow == null)
                    return;

                // Find the data behind the ListViewItem
                var instance = (Instance)grid.ItemContainerGenerator.ItemFromContainer(gridRow);

                // Initialize the drag & drop operation
                var dragData = new DataObject();
                dragData.SetData("Instance", instance);
                dragData.SetData("Source", grid);
                DragDrop.DoDragDrop(gridRow, dragData, DragDropEffects.Move);
            }
        }

        private void GridDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Instance"))
            {
                var instance = (Instance) e.Data.GetData("Instance");
                var grid = (DataGrid) sender;
                var view = (IManageGroupsViewModel) ViewModel;
                switch (grid.Tag)
                {
                    case "InGroup":
                        view.AddInstanceToCurrentGroup(instance);
                        break;
                    case "WithoutGroup":
                        view.RemoveInstanceFromCurrentGroup(instance);
                        break;
                }
            }
        }

        private void GridDragEnter(object sender, DragEventArgs e) { ValidateDrag(sender, e); }
        private void GridDragOver(object sender, DragEventArgs e) { ValidateDrag(sender, e); }

        private static void ValidateDrag(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Instance") ||
                !e.Data.GetDataPresent("Source") ||
                sender == e.Data.GetData("Source"))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T variable)
                    return variable;

                current = VisualTreeHelper.GetParent(current);
            } while (current != null);

            return null;
        }
    }
}
