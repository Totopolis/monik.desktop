using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MonikDesktop.Oak;
using System.Reactive.Linq;

namespace MonikDesktop.ViewModels
{
  public class SourceItem : ReactiveObject
  {
    public short GroupID { get; set; }
    public string GroupName { get; set; }
    public string SourceName { get; set; }
    public string InstanceName { get; set; }
    public int InstanceID { get; set; }

    [Reactive]
    public bool Checked { get; set; } = false;
  }

  public class SourcesViewModel : ReactiveObject, ISourcesWindow
  {
    private ISourcesCache FCache;

    [Reactive]
    public ReactiveList<SourceItem> SourceItems { get; private set; } = null;

    [Reactive]
    public string Title { get; set; }
    [Reactive]
    public bool CanClose { get; set; } = true;
    [Reactive]
    public ReactiveCommand CloseCommand { get; set; } = null;

    private ReactiveList<short> FSelectedGroups = null;
    private ReactiveList<int> FSelectedInstances = null;

    public SourcesViewModel(Shell aShell, ISourcesCache aCache)
    {
      FCache = aCache;
      Title = "Sources";

      FillSourcesTree();

      aShell.WhenAnyValue(x => x.SelectedWindow)
        .Where(v => v is IShowWindow)
        .Subscribe(v => OnSelectedWindow(v as IShowWindow));
    }

    private void FillSourcesTree()
    {
      SourceItems = new ReactiveList<SourceItem>();

      var _groups = FCache.Groups;
      var _sources = FCache.Sources;
      var _instances = FCache.Instances;

      foreach (var gr in _groups)
        foreach (var inst in gr.Instances)
        {
          SourceItem _si = new SourceItem()
          {
            GroupID = gr.ID,
            GroupName = gr.Name,
            SourceName = inst.Source.Name,
            InstanceName = inst.Name,
            InstanceID = inst.ID
          };

          SourceItems.Add(_si);
        }

      foreach (var inst in _instances)
        if (SourceItems.Count(x => x.InstanceID == inst.ID) == 0)
        {
          SourceItem _si = new SourceItem()
          {
            GroupID = 0,
            GroupName = "[NOGROUP]",
            SourceName = inst.Source.Name,
            InstanceName = inst.Name,
            InstanceID = inst.ID
          };

          SourceItems.Add(_si);
        }

      SourceItems.ChangeTrackingEnabled = true;

      SourceItems.ItemChanged.Subscribe(x => OnSourceChanged(x.Sender));
    }

    private void OnSelectedWindow(IShowWindow aWindow)
    {
      SourceItems.ChangeTrackingEnabled = false;

      foreach (var it in SourceItems)
        it.Checked = false;

      FSelectedGroups = aWindow.Model.Groups;
      FSelectedInstances = aWindow.Model.Instances;

      // fill from IShowWindow
      foreach (var it in SourceItems)
        if (FSelectedGroups.Contains(it.GroupID) || FSelectedInstances.Contains(it.InstanceID))
          it.Checked = true;

      SourceItems.ChangeTrackingEnabled = true;

      // update view
      this.RaisePropertyChanged("SourceItems");
    }

    /// <summary>
    /// Manage instance and group lists. Join to groups if need.
    /// aItem have new Checked state.
    /// </summary>
    private void OnSourceChanged(SourceItem aItem)
    {
      if (aItem.Checked)
        FSelectedInstances.Add(aItem.InstanceID);
      else
        FSelectedInstances.Remove(aItem.InstanceID);

      if (!aItem.Checked && aItem.GroupID > 0 && FSelectedGroups.Contains(aItem.GroupID))
      {
        var _checkedItems = SourceItems.Where(x => x.GroupID == aItem.GroupID && x.Checked).Select(x => x.InstanceID);
        FSelectedInstances.AddRange(_checkedItems);
        FSelectedGroups.Remove(aItem.GroupID);
      }

      if (aItem.Checked && aItem.GroupID > 0 && !FSelectedGroups.Contains(aItem.GroupID))
      {
        var _allItems = SourceItems.Where(x => x.GroupID == aItem.GroupID);
        var _checkedItems = _allItems.Where(x => x.Checked);

        if (_allItems.Count() == _checkedItems.Count())
        {
          FSelectedGroups.Add(aItem.GroupID);
          FSelectedInstances.RemoveAll(_allItems.Select(x => x.InstanceID));
        }
      }
    }
  }//end of class
}
