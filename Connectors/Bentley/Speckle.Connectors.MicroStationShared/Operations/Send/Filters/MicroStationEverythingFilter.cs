using Speckle.Connectors.DUI.Models.Card.SendFilter;
using Speckle.Connectors.DUI.Utils;
using Speckle.Connectors.MicroStation.Plugin;

namespace Speckle.Connectors.MicroStation.Operations.Send.Filters;

/// <summary>
/// Collects all graphic elements in the active MicroStation model.
/// This is the default send filter.
/// </summary>
public class MicroStationEverythingFilter : DiscriminatedObject, ISendFilter
{
  public string Id { get; set; } = "everything";
  public string Type { get; set; } = "Everything";
  public string Name { get; set; } = "Everything";
  public string? Summary { get; set; } = "All elements in the active model";
  public bool IsDefault { get; set; }
  public List<string> SelectedObjectIds { get; set; } = [];
  public Dictionary<string, string>? IdMap { get; set; }

  public List<string> RefreshObjectIds()
  {
    var model = MsApp.Model;
    if (model == null)
    {
      return [];
    }

    var ids = new List<string>();
    var elems = model.GetGraphicElements();
    foreach (MgdElement elem in elems)
    {
      if (elem != null)
      {
        ids.Add(elem.ElementId.ToString());
      }
    }

    return ids;
  }
}
