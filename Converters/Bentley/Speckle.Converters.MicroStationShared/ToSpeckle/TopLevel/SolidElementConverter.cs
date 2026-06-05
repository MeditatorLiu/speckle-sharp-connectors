using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Speckle.Converter.MicroStation.Settings;
using Speckle.Converters.Common;
using Speckle.Objects.Geometry;
using Speckle.Sdk.Models;
using MgdSurfaceOrSolid = Bentley.DgnPlatformNET.Elements.SurfaceOrSolidElement;

namespace Speckle.Converter.MicroStation.ToSpeckle.TopLevel;

/// <summary>
/// Converts a managed <see cref="MgdSurfaceOrSolid"/> (3D solid, swept surface, etc.) into a
/// bounding-box Speckle <see cref="Base"/>. Real B-rep tessellation through
/// <c>SolidPrimitiveQuery</c> + <c>PolyfaceConstruction</c> is the follow-up — that path was
/// historically CSE-prone in this codebase, so we deliberately ship a placeholder until the
/// tessellation pipeline is hardened.
/// </summary>
public class SolidElementConverter(
  ToMeshProcessor processor,
  IConverterSettingsStore<MicroStationConversionSettings> settingsStore,
  FallbackElementMeshConverter fallbackConverter
)
{
  public Mesh Convert(MgdElement mgdSolid)
  {
    var applicationId = ((ulong)mgdSolid.ElementId).ToString();

    ElementGraphicsOutput.Process(mgdSolid, processor);

    if (processor.Meshes.Count == 0)
    {
      return fallbackConverter.Convert(mgdSolid);
    }

    var vertices = new List<double>();
    var faces = new List<int>();
    var vertexOffset = 0;

    foreach (var polyface in processor.Meshes)
    {
      AppendPolyface(polyface, vertices, faces, ref vertexOffset);
    }

    if (vertices.Count == 0 || faces.Count == 0)
    {
      return fallbackConverter.Convert(mgdSolid);
    }

    return new Mesh
    {
      vertices = vertices,
      faces = faces,
      units = settingsStore.Current.SpeckleUnits,
      applicationId = applicationId,
    };
  }

  private static void AppendPolyface(
    PolyfaceHeader polyface,
    List<double> vertices,
    List<int> faces,
    ref int vertexOffset
  )
  {
    var pointList = polyface.Point.ToArray();
    foreach (var pt in pointList)
    {
      vertices.Add(pt.X);
      vertices.Add(pt.Y);
      vertices.Add(pt.Z);
    }

    var indices = polyface.PointIndex.ToArray();
    int i = 0;
    while (i < indices.Length)
    {
      int facetStart = i;
      while (i < indices.Length && indices[i] != 0)
      {
        i++;
      }

      int facetSize = i - facetStart;
      if (facetSize >= 3)
      {
        faces.Add(facetSize);
        for (int j = facetStart; j < i; j++)
        {
          faces.Add(vertexOffset + Math.Abs(indices[j]) - 1);
        }
      }

      i++;
    }

    vertexOffset += pointList.Length;
  }
}
