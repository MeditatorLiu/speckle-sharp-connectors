using System.Runtime.InteropServices;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;

namespace Speckle.Connectors.MicroStation.Plugin;

/// <summary>
/// Provides cached access to the running MicroStation 2026 COM Application object.
/// MicroStation registers itself as a COM server (ProgID "MicroStationDGN.Application")
/// when it starts; we obtain the running instance via <see cref="Marshal.GetActiveObject"/>.
/// </summary>
internal static class MsApp
{
  private static Application? s_instance;

  public static Session Instance => Session.Instance;

  /// <summary>Returns the application instance or null if MicroStation is not available.</summary>
  public static Application? TryGetInstance()
  {
    if (s_instance != null)
    {
      return s_instance;
    }

    try
    {
      //s_instance = (Application)Marshal.GetActiveObject("MicroStationDGN.Application");
      s_instance = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
      return s_instance;
    }
    catch (COMException)
    {
      return null;
    }
  }

  public static DgnFile File => Instance.GetActiveDgnFile();
  public static DgnModel Model => Instance.GetActiveDgnModel();
}
