using System;
using System.Linq;
using System.Reactive.Linq;
using Doaking.Core.Oak;
using MonikDesktop.Common.Interfaces;
using MonikDesktop.ViewModels.ShowModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class SourcesViewModel : ReactiveObject, ISourcesWindow
	{
		private readonly ISourcesCache _cache;

		private ReactiveList<short> _selectedGroups;
		private ReactiveList<int> _selectedInstances;

		private ShowModel _model;

		public SourcesViewModel(Shell aShell, ISourcesCache aCache)
		{
			_cache = aCache;
			Title = "Sources";

			SourceItems = new ReactiveList<SourceItem>();

			FilteredItems = new ReactiveList<SourceItem>();

			FilteredItems
				.ItemChanged
				.Subscribe(x => OnSourceChanged(x.Sender));

			FillSourcesTree();

			aShell.WhenAnyValue(x => x.SelectedWindow)
				.Where(v => v is IShowWindow)
				.Subscribe(v => OnSelectedWindow(v as IShowWindow));

			this.ObservableForProperty(x => x.SelectedItem)
				.Where(v => v.Value != null)
				.Subscribe(v => SelectedHack = v.Value);

			RefreshCommand = ReactiveCommand.Create(Refresh);

			this.ObservableForProperty(x => x.FilterText)
				.Throttle(TimeSpan.FromSeconds(0.7), RxApp.MainThreadScheduler)
				.Subscribe(v => Filter(v.Value));
		}

		[Reactive]
		public SourceItem SelectedHack { get; set; }

		[Reactive]
		public SourceItem SelectedItem { get; set; }

		[Reactive]
		public ReactiveList<SourceItem> FilteredItems { get; private set; }

		[Reactive]
		public ReactiveList<SourceItem> SourceItems { get; private set; }

		[Reactive]
		public string FilterText { get; set; }

		[Reactive]
		public string Title { get; set; }

		[Reactive]
		public bool CanClose { get; set; } = false;

		[Reactive]
		public ReactiveCommand CloseCommand { get; set; } = null;

		[Reactive]
		public ReactiveCommand SelectNoneCommand { get; set; }

		[Reactive]
		public ReactiveCommand SelectGroupCommand { get; set; }

		[Reactive]
		public ReactiveCommand RefreshCommand { get; set; }

		private void Filter(string aFilter)
		{
			aFilter = aFilter.ToLower();
			FilteredItems.Clear();

		    if (string.IsNullOrWhiteSpace(aFilter))
		        FilteredItems.AddRange(SourceItems);
		    else
		        FilteredItems.AddRange(SourceItems
		            .Where(x =>
		                x.SourceName.ToLower().Contains(aFilter) ||
		                x.InstanceName.ToLower().Contains(aFilter)));
		}

		private void SelectNone()
		{
			_selectedGroups.Clear();
			_selectedInstances.Clear();

			foreach (var it in SourceItems)
				it.Checked = false;
		}

		private void SelectGroup()
		{
		    if (SelectedHack == null || _selectedGroups.Contains(SelectedHack.GroupID))
				return;

		    foreach (var x in SourceItems.Where(x => x.GroupID == SelectedHack.GroupID).ToList())
		        x.Checked = true;
		}

		private void Refresh()
		{
			_cache.Reload();

			FillSourcesTree();
			SyncCheckStatuses();

			this.RaisePropertyChanged("SourceItems");
		}

		private void FillSourcesTree()
		{
			SourceItems.Clear();
			FilteredItems.Clear();

			var groups = _cache.Groups;
			var instances = _cache.Instances;

            SourceItems.AddRange(groups.SelectMany(gr => gr.Instances,
                (gr, inst) => new SourceItem
                {
                    GroupID = gr.ID,
                    GroupName = gr.Name,
                    SourceName = inst.Source.Name,
                    InstanceName = inst.Name,
                    InstanceID = inst.ID
                }));
            
		    SourceItems.AddRange(instances.Where(inst => SourceItems.All(x => x.InstanceID != inst.ID))
		        .Select(inst => new SourceItem
		        {
		            GroupID = 0,
		            GroupName = "[NOGROUP]",
		            SourceName = inst.Source.Name,
		            InstanceName = inst.Name,
		            InstanceID = inst.ID
		        }));
            
            FilterText = "";

			FilteredItems.ChangeTrackingEnabled = false;
            
            FilteredItems.AddRange(SourceItems);

			FilteredItems.ChangeTrackingEnabled = true;
		}

		private void SyncCheckStatuses()
		{
			SourceItems.ChangeTrackingEnabled = false;

			foreach (var it in SourceItems)
				it.Checked = false;

			// fill from IShowWindow
			foreach (var it in SourceItems.Where(it=>_selectedGroups.Contains(it.GroupID) || _selectedInstances.Contains(it.InstanceID)).ToList())
					it.Checked = true;

			SourceItems.ChangeTrackingEnabled = true;
		}

		private void OnSelectedWindow(IShowWindow aWindow)
		{
			if (aWindow.Model == _model)
				return;

			_model = aWindow.Model;

			SelectNoneCommand = null;
			SelectGroupCommand = null;

			_selectedGroups = aWindow.Model.Groups;
			_selectedInstances = aWindow.Model.Instances;

			SyncCheckStatuses();

			// update view
			this.RaisePropertyChanged("SourceItems");

			// Select None Command
			var canSelectNone = _model.WhenAny(
				x => x.Instances.Count,
				x => x.Groups.Count,
				(ins, gr) => ins.Value > 0 || gr.Value > 0);

			SelectNoneCommand = ReactiveCommand.Create(SelectNone, canSelectNone);

			// Select Group Command
			var canSelectGroup = this.WhenAny(x => x.SelectedHack, v => v.Value)
			    .Merge(SourceItems.ItemChanged.Select(v => v.Sender))
				.Select(si => si != null && !_selectedGroups.Contains(si.GroupID));

			SelectGroupCommand = ReactiveCommand.Create(SelectGroup, canSelectGroup);
		}

		/// <summary>
		///     Manage instance and group lists. Join to groups if need.
		///     aItem have new Checked state.
		/// </summary>
		private void OnSourceChanged(SourceItem aItem)
		{
			if (aItem.Checked)
				_selectedInstances.Add(aItem.InstanceID);
			else
				_selectedInstances.Remove(aItem.InstanceID);

			if (!aItem.Checked && (aItem.GroupID > 0) && _selectedGroups.Contains(aItem.GroupID))
			{
				var checkedItems = SourceItems
					.Where(x => (x.GroupID == aItem.GroupID) && x.Checked)
					.Select(x => x.InstanceID);

				_selectedInstances.AddRange(checkedItems);
				_selectedGroups.Remove(aItem.GroupID);
			}

			if (aItem.Checked && (aItem.GroupID > 0) && !_selectedGroups.Contains(aItem.GroupID))
			{
				var allItems = SourceItems
					.Where(x => x.GroupID == aItem.GroupID)
					.ToList();

				var allCheckedItems = SourceItems
					.Where(x => (x.GroupID == aItem.GroupID) && x.Checked)
					.ToList();

				if (allItems.Count == allCheckedItems.Count)
				{
					_selectedGroups.Add(aItem.GroupID);
					_selectedInstances.RemoveAll(allItems.Select(x => x.InstanceID));
				}
			}
		}
	} //end of class
}