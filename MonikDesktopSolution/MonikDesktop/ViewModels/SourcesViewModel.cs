using Gemini.Framework;
using Gemini.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

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
    public bool Checked { get; set; }
  }

  public class SourcesViewModel : Tool, ISourcesWindow
  {
    private IMonikService FService;
    public ReactiveList<SourceItem> SourceItems { get; private set; } = null;

    private ReactiveList<short> FGroups = null;
    private ReactiveList<int> FInstances = null;

    public SourcesViewModel(MApp aApp, IMonikService aService)
    {
      FService = aService;
      DisplayName = "Sources";

      aApp.WhenAnyValue(x => x.SelectedWindow).Subscribe(w => OnSelectedWindow(w));
    }

    private void OnSelectedWindow(IShowWindow aWindow)
    {
      if (aWindow == null)
      {
        SourceItems = null;
        FGroups = null;
        FInstances = null;

        return;
      }

      SourceItems = new ReactiveList<SourceItem>() { ChangeTrackingEnabled = true };
      FGroups = aWindow.Model.Groups;
      FInstances = aWindow.Model.Instances;

      var _groups = FService.GetGroups();
      var _sources = FService.GetSources();
      var _instances = FService.GetInstances();

      foreach (var gr in _groups)
        foreach (var inst in gr.Instances)
        {
          EInstance _instance = _instances.First(x => x.ID == inst);
          ESource _source = _sources.First(x => x.ID == _instance.SourceID);

          SourceItem _si = new SourceItem()
          {
            GroupID = gr.ID,
            GroupName = gr.Name,
            SourceName = _source.Name,
            InstanceName = _instance.Name,
            InstanceID = inst
          };

          SourceItems.Add(_si);
        }

      foreach (var inst in _instances)
        if (SourceItems.Count(x => x.InstanceID == inst.ID) == 0)
        {
          ESource _source = _sources.First(x => x.ID == inst.SourceID);

          SourceItem _si = new SourceItem()
          {
            GroupID = 0,
            GroupName = "[NOGROUP]",
            SourceName = _source.Name,
            InstanceName = inst.Name,
            InstanceID = inst.ID
          };

          SourceItems.Add(_si);
        }

      // TODO: fill from IShowWindow

      SourceItems.ItemChanged.Subscribe(x => OnSourceChanged(x.Sender));
    }

    /// <summary>
    /// Manage instance and group lists. Join to groups if need.
    /// aItem have new Checked state.
    /// </summary>
    private void OnSourceChanged(SourceItem aItem)
    {
      if (aItem.Checked)
        FInstances.Add(aItem.InstanceID);
      else
        FInstances.Remove(aItem.InstanceID);

      if (!aItem.Checked && aItem.GroupID > 0 && FGroups.Contains(aItem.GroupID))
      {
        var _checkedItems = SourceItems.Where(x => x.GroupID == aItem.GroupID && x.Checked).Select(x => x.InstanceID);
        FInstances.AddRange(_checkedItems);
        FGroups.Remove(aItem.GroupID);
      }

      if (aItem.Checked && aItem.GroupID > 0 && !FGroups.Contains(aItem.GroupID))
      {
        var _allItems = SourceItems.Where(x => x.GroupID == aItem.GroupID);
        var _checkedItems = _allItems.Where(x => x.Checked);

        if (_allItems.Count() == _checkedItems.Count())
        {
          FGroups.Add(aItem.GroupID);
          FInstances.RemoveAll(_allItems.Select(x => x.InstanceID));
        }
      }
    }

    public override PaneLocation PreferredLocation
    {
      get { return PaneLocation.Left; }
    }
  }
}
