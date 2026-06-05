using Speckle.Converter.MicroStation.Helpers;
using Speckle.Converter.MicroStation.Services;

namespace Speckle.Converter.MicroStation.Settings;

public interface IMicroStationConversionSettingsFactory
{
  MicroStationConversionSettings Create(bool includeInvisibleElements = false);
}

/// <summary>
/// Creates <see cref="MicroStationConversionSettings"/> from the currently active MicroStation model.
/// The <see cref="MicroStationContext"/> MicroStationContext object is injected by the connector's DI registration so the
/// converter project does not need to reference the connector project.
/// </summary>
public class MicroStationConversionSettingsFactory(
  MicroStationToSpeckleUnitConverter unitConverter,
  MicroStationContext context
) : IMicroStationConversionSettingsFactory
{
  public MicroStationConversionSettings Create(bool includeInvisibleElements = false)
  {
    if (null == context.ActiveModel)
    {
      return new MicroStationConversionSettings(SSC.Units.Meters, includeInvisibleElements);
    }

    var model = context.ActiveModel;
    var speckleUnits = unitConverter.ConvertOrThrow(model.GetModelInfo().GetMasterUnit());

    return new MicroStationConversionSettings(speckleUnits, includeInvisibleElements);
  }
}
