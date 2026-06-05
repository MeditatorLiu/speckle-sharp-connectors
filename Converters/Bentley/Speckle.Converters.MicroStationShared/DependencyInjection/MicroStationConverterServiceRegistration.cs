using Bentley.DgnPlatformNET;
using Microsoft.Extensions.DependencyInjection;
using Speckle.Converter.MicroStation.Helpers;
using Speckle.Converter.MicroStation.Services;
using Speckle.Converter.MicroStation.Settings;
using Speckle.Converter.MicroStation.ToSpeckle;
using Speckle.Converter.MicroStation.ToSpeckle.TopLevel;
using Speckle.Converters.Common;

namespace Speckle.Converter.MicroStation.DependencyInjection;

/// <summary>
/// Wires up the managed-API conversion pipeline. Each top-level converter is a plain class with
/// a typed <c>Convert(MgdXxxElement)</c> method — no <c>IToSpeckleTopLevelConverter</c> auto-
/// discovery, no <c>NameAndRankValue</c> attribute, no <c>IConverterManager</c> resolution. The
/// root dispatcher pattern-matches managed element types and dispatches directly to the right
/// converter via constructor injection, which keeps the dispatch table compile-time-checked.
/// </summary>
public static class MicroStationConverterServiceRegistration
{
  public static IServiceCollection AddMicroStationConverters(this IServiceCollection serviceCollection)
  {
    serviceCollection.AddSingleton(new MicroStationContext());

    // Root dispatcher — single entry point that the SendBinding / RootObjectBuilder calls.
    // No leaf converter takes a back-reference to this; container elements (CellHeader) get
    // their children walked by the dispatcher itself via private recursion, so there's no
    // DI cycle and no need for Lazy<> / IServiceProvider service-locator workarounds.
    // Scoped: must match the lifetime of IConverterSettingsStore<> which is per-scope.
    serviceCollection.AddScoped<IRootToSpeckleConverter, MicroStationRootToSpeckleConverter>();

    // Per-type managed converters (scoped — they capture IConverterSettingsStore<> which is scoped).
    serviceCollection.AddScoped<LineElementConverter>();
    serviceCollection.AddScoped<ArcElementConverter>();
    serviceCollection.AddScoped<EllipseElementConverter>();
    serviceCollection.AddScoped<LineStringElementConverter>();
    serviceCollection.AddScoped<PointStringElementConverter>();
    serviceCollection.AddScoped<ShapeElementConverter>();
    serviceCollection.AddScoped<ComplexShapeElementConverter>();
    serviceCollection.AddScoped<ComplexStringElementConverter>();
    serviceCollection.AddScoped<BsplineCurveElementConverter>();
    serviceCollection.AddScoped<BSplineSurfaceElementConverter>();
    serviceCollection.AddScoped<CellHeaderElementConverter>();
    serviceCollection.AddScoped<SharedCellElementConverter>();
    serviceCollection.AddScoped<TextElementConverter>();
    serviceCollection.AddScoped<SolidElementConverter>();
    serviceCollection.AddScoped<MeshHeaderElementConverter>();

    // Bounding-box fallback for any managed element that doesn't match the dispatch table or
    // whose dedicated converter throws.
    serviceCollection.AddScoped<FallbackElementMeshConverter>();

    // Mesh processor for real B-rep tessellation of solids, used by SolidElementConverter, CellHeaderElementConverter and etc.
    serviceCollection.AddScoped<ToMeshProcessor>();

    // Unit converter
    serviceCollection.AddSingleton<MicroStationToSpeckleUnitConverter>();
    serviceCollection.AddSingleton<IHostToSpeckleUnitConverter<UnitDefinition>>(sp =>
      sp.GetRequiredService<MicroStationToSpeckleUnitConverter>()
    );

    // Conversion settings factory + per-scope store
    serviceCollection.AddSingleton<IMicroStationConversionSettingsFactory, MicroStationConversionSettingsFactory>();
    serviceCollection.AddScoped<
      IConverterSettingsStore<MicroStationConversionSettings>,
      ConverterSettingsStore<MicroStationConversionSettings>
    >();

    return serviceCollection;
  }
}
