using System;
using System.Linq;
using System.Reactive.Linq;
using Doaking.Core.Oak;
using MonikDesktop.Common.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MonikDesktop.ViewModels
{
	public class SourcesViewModel : ReactiveObject, ISourcesWindow
	{
		private readonly ISourcesCache _cache;

		private ReactiveList<short> _selectedGroups;
		private ReactiveList<int> _selectedInstances;

		public SourcesViewModel(Shell aShell, ISourcesCache aCache)
		{
			_cache = aCache;
			Title = "Sources";

			FillSourcesTree();

			aShell.WhenAnyValue(x => x.SelectedWindow)
				.Where(v => v is IShowWindow)
				.Subscribe(v => OnSelectedWindow(v as IShowWindow));
		}

		[Reactive]
		public ReactiveList<SourceItem> SourceItems { get; private set; }

		[Reactive]
		public string Title { get; set; }

		[Reactive]
		public bool CanClose { get; set; } = false;

		[Reactive]
		public ReactiveCommand CloseCommand { get; set; } = null;

		private void FillSourcesTree()
		{
			SourceItems = new ReactiveList<SourceItem>();

			var groups = _cache.Groups;
			var instances = _cache.Instances;

			foreach (var gr in groups)
				foreach (var inst in gr.Instances)
				{
					var sitem = new SourceItem
					{
						GroupID = gr.ID,
						GroupName = gr.Name,
						SourceName = inst.Source.Name,
						InstanceName = inst.Name,
						InstanceID = inst.ID
					};

					SourceItems.Add(sitem);
				}

			foreach (var inst in instances)
				if (SourceItems.Count(x => x.InstanceID == inst.ID) == 0)
				{
					var sitem = new SourceItem
					{
						GroupID = 0,
						GroupName = "[NOGROUP]",
						SourceName = inst.Source.Name,
						InstanceName = inst.Name,
						InstanceID = inst.ID
					};

					SourceItems.Add(sitem);
				}

			SourceItems.ChangeTrackingEnabled = true;

			SourceItems.ItemChanged.Subscribe(x => OnSourceChanged(x.Sender));
		}

		private void OnSelectedWindow(IShowWindow aWindow)
		{
			SourceItems.ChangeTrackingEnabled = false;

			foreach (var it in SourceItems)
				it.Checked = false;

			_selectedGroups = aWindow.Model.Groups;
			_selectedInstances = aWindow.Model.Instances;

			// fill from IShowWindow
			foreach (var it in SourceItems)
				if (_selectedGroups.Contains(it.GroupID) || _selectedInstances.Contains(it.InstanceID))
					it.Checked = true;

			SourceItems.ChangeTrackingEnabled = true;

			// update view
			this.RaisePropertyChanged("SourceItems");
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
				var checkedItems = SourceItems.Where(x => (x.GroupID == aItem.GroupID) && x.Checked).Select(x => x.InstanceID);
				_selectedInstances.AddRange(checkedItems);
				_selectedGroups.Remove(aItem.GroupID);
			}

			if (aItem.Checked && (aItem.GroupID > 0) && !_selectedGroups.Contains(aItem.GroupID))
			{
				var allItems = SourceItems.Where(x => x.GroupID == aItem.GroupID).ToArray();
				var checkedItems = allItems.Where(x => x.Checked).ToArray();

				if (allItems.Count() == checkedItems.Count())
				{
					_selectedGroups.Add(aItem.GroupID);
					_selectedInstances.RemoveAll(allItems.Select(x => x.InstanceID));
				}
			}
		}
	} //end of class
}