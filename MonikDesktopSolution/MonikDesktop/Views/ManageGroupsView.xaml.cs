using MonikDesktop.Common.Interfaces;
using MonikDesktop.Common.ModelsApp;
using MonikDesktop.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MonikDesktop.Views
{
    public partial class ManageGroupsView : ViewUserControl
    {
        public ManageGroupsView(ManageGroupsViewModel vm)
            : base(vm)
        {
            InitializeComponent();
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
                var grid = (DataGrid) sender;
                var gridRow = FindAnchestor<DataGridRow>((DependencyObject) e.OriginalSource);

                if (gridRow == null)
                    return;

                var instance = (Instance) grid.ItemContainerGenerator.ItemFromContainer(gridRow);

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
                var view = (ManageGroupsViewModel) ViewModel;
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

        private void GridDragEnter(object sender, DragEventArgs e)
        {
            ValidateDrag(sender, e);
        }

        private void GridDragOver(object sender, DragEventArgs e)
        {
            ValidateDrag(sender, e);
        }

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