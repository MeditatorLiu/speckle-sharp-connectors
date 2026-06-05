using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;

namespace Speckle.Converter.MicroStation.Helpers;

public class MicroStationContext
{
  public Session Session => Session.Instance;
  public DgnFile ActiveFile => Session.GetActiveDgnFile();
  public DgnModel ActiveModel => Session.GetActiveDgnModel();

  public double UOR =>ActiveModel.GetModelInfo().UorPerMaster;
}
