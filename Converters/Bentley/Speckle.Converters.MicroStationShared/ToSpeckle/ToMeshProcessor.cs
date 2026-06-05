using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Speckle.Converter.MicroStation.Helpers;

namespace Speckle.Converter.MicroStation.ToSpeckle;

public class ToMeshProcessor(
  MicroStationContext context
) : ElementGraphicsProcessor
{
  private DTransform3d _transform = DTransform3d.Identity;

  public List<PolyfaceHeader> Meshes { get; set; } = new();

  public override void AnnounceTransform(DTransform3d trans)
  {
    _transform = trans != null ? trans : DTransform3d.Identity;
  }

  public override bool ProcessAsBody(bool isCurved) => false;
  public override bool ProcessAsFacets(bool isPolyface) => true;

  public override BentleyStatus ProcessFacets(PolyfaceHeader meshData, bool filled)
  {
    _ = filled;

    var meshCopy = new PolyfaceHeader();
    meshCopy.CopyFrom(meshData);
    meshCopy.Transform(ref _transform, false);
    // The mesh vertices are in internal uor units, we need to scale them down to match the master units of the model, which is what Speckle expects.
    var scaleTrans = DTransform3d.Scale(1 / context.UOR);
    meshCopy.Transform(ref scaleTrans, false);
    Meshes.Add(meshCopy);

    return BentleyStatus.Success;
  }

  public override FacetOptions GetFacetOptions()
  {
    var facetOptions = FacetOptions.New();

    facetOptions.SetDefaults();

    facetOptions.NormalsRequired = true;
    facetOptions.EdgeHiding = false;
    facetOptions.ChordTolerance = 0.1;
    facetOptions.AngleTolerance = 0.1;

    return facetOptions;
  }
}
