using Bentley.DgnPlatformNET;
using Speckle.Connectors.DUI.Models.Card.SendFilter;
using Speckle.Connectors.DUI.Utils;
using Speckle.Connectors.MicroStation.Plugin;

namespace Speckle.Connectors.MicroStation.Operations.Send.Filters;

/// <summary>
/// Sends elements that belong to specific DGN levels (layers).
/// The UI populates <see cref="SelectedLevelNames"/> from <see cref="GetAvailableLevelNames"/>.
/// </summary>
public class MicroStationLevelFilter : DiscriminatedObject, ISendFilter
{
  public string Id { get; set; } = "byLevel";
  public string Type { get; set; } = "Level";
  public string Name { get; set; } = "By Level";
  public string? Summary { get; set; }
  public bool IsDefault { get; set; }
  public List<string> SelectedObjectIds { get; set; } = [];
  public Dictionary<string, string>? IdMap { get; set; }

  /// <summary>Level names chosen by the user in the DUI3 panel.</summary>
  public List<string> SelectedLevelNames { get; set; } = [];

  public List<string> RefreshObjectIds()
  {
    var model = MsApp.Model;
    if (model == null || SelectedLevelNames.Count == 0)
    {
      return [];
    }

    // Build a set of level names to include
    var selectedNames = new HashSet<string>(SelectedLevelNames, StringComparer.OrdinalIgnoreCase);

    var ids = new List<string>();
    var elems = model.GetGraphicElements();
    foreach (MgdElement elem in elems)
    {
      if (elem == null)
      {
        continue;
      }

      using var elePropGetter = new ElementPropertiesGetter(elem);
      var levelId = elePropGetter.Level;

      // CellHeaderElement do not have a level and we will get LevelId = 0, but its children can be on levels. We will skip the CellHeaderElement for now, and evaluate its children instead later.
      if (levelId == 0)
      {
        continue;
      }

      var levelCache = model.GetFileLevelCache();
      var levelHandle = levelCache.GetLevel(levelId, true);

      if (selectedNames.Contains(levelHandle.Name))
      {
        ids.Add(elem.ElementId.ToString());
      }
    }

    return ids;
  }

  /// <summary>
  /// Returns the names of all levels available in the active DGN model.
  /// Called by the DUI3 panel to populate the level picker.
  /// </summary>
  public List<string> GetAvailableLevelNames()
  {
    var model = MsApp.Model;
    if (model == null)
    {
      return [];
    }

    var names = new List<string>();

    var levelCache = model.GetFileLevelCache();

    foreach (var levelHandle in levelCache.GetHandles())
    {
      if (!string.IsNullOrEmpty(levelHandle.Name))
      {
        names.Add(levelHandle.Name);
      }
    }

    return names;
  }
}
