using Bentley.DgnPlatformNET;
using Speckle.Connectors.DUI.Models.Card.SendFilter;
using Speckle.Connectors.MicroStation.Plugin;

namespace Speckle.Connectors.MicroStation.Operations.Send.Filters;

/// <summary>
/// Sends only the elements currently selected (highlighted) in the MicroStation selection set.
/// The selection is captured at the time the model card is created or refreshed.
/// </summary>
public class MicroStationSelectionFilter : DirectSelectionSendFilter
{
  public MicroStationSelectionFilter()
  {
    IsDefault = false;
  }

  public override List<string> RefreshObjectIds()
  {
    var model = MsApp.Model;

    ElementAgenda agenda = new ElementAgenda();//声明元素容器
    SelectionSetManager.BuildAgenda(ref agenda);

    if (model == null || agenda.IsEmpty)
    {
      return SelectedObjectIds;
    }

    // Collect elements flagged as IsHighlighted (= currently selected)
    var ids = new List<string>();

    for (uint i = 0; i < agenda.GetCount(); i++)
    {
      ids.Add(agenda.GetEntry(i).ElementId.ToString());
    }

    SelectedObjectIds = ids;
    return SelectedObjectIds;
  }
}
