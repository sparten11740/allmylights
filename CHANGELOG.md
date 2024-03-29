## [v10]

### Sinks
- Enable `Wallpaper` sink to receive objects, where file paths are keyed by 
  screen index, to set individual wallpapers for different screens (supported 
  from Windows 8 upwards)
### Transformations
- Enable `Mapping` transformation to return objects (which are provided as 
  `JObject` to succeeding transformations)

## [v9]

### Sinks
- Fix profile loading for later OpenRGB versions

## [v8]

### Routes
- Introduce the concept of routes to allow for defining explicit data flows 
  (relevant for more complex scenarios where some sinks are meant to receive 
  values from certain sources but not from others)
  

## [v7]

### Sources
- Add `OpenRGB` source to enable use cases in which OpenRGB is the source of 
truth

### Sinks
- Add `Mqtt` sink to enable publishing values to Mqtt
## [v6]

### Sinks
- Add `Chroma` sink to control Chroma enabled devices

## [v5] - 02/14/2020

### General
- Rename cli argument `--list-devices` to `--info` as it evolved into a more 
generic way of showing dynamic information on configured sinks (such as available 
profiles in case of OpenRGB or file names in the `RelativeTo` directory of the 
`Wallpaper` sink)
- Parameter `--config` is no longer required. If not provided, the default config 
file `allmylightsrc.json` will be assumed in the same directory where the 
executable resides.

### OpenRGB sink
- The `OpenRGB` sink now supports loading of profiles. Any string ending in `.orp` 
consumed by the sink will be sent as a load profile request to your OpenRGB instance.

### Sinks
- Add `Wallpaper` sink that applies a desktop background based on the input value 
received (file path)
### Transformations
- Add `Expression` transformation to support use cases that require advanced logic

## [v4] - 01/05/2020

### General
- Add command-line flag to export a schema representation of the configuration 
file (OpenAPI 3 format).
- Add command-line flag to generate a systemd service definition and enable 
autostart (Linux) or create a shortcut in the user's startup folder (Windows)
- Introduce verbose logging for all transformations (log level debug)

### Transformations
- Add `Mapping` transformation to map matching input strings to output value.
Supports regular expressions.

## [v3] - 12/12/2020
### General
- In preparation for future changes, this release aims to achieve a clear 
separation between transport and transformation logic
- Introduce pluggable transformations to make logic such as extracting values 
via JsonPath or color conversions reusable across different source and sink types
## MQTT source
- Remove transformation logic from `Mqtt` source

## [v2] - 12/06/2020
### General
- Introduce the concept of sources where multiple sources can be used as a color 
- Introduce the concept of sinks where all configured sinks will consume a color
### OpenRGB sink
- Add option to ignore a device
- Add option to ignore a zone of a device
- Add option to override the channel layout of a device
- Add option to override the channel layout of a zone

[v10]: https://github.com/sparten11740/allmylights/compare/v9...v10
[v9]: https://github.com/sparten11740/allmylights/compare/v8...v9
[v8]: https://github.com/sparten11740/allmylights/compare/v7...v8
[v7]: https://github.com/sparten11740/allmylights/compare/v6...v7
[v6]: https://github.com/sparten11740/allmylights/compare/v5...v6
[v5]: https://github.com/sparten11740/allmylights/compare/v4...v5
[v4]: https://github.com/sparten11740/allmylights/compare/v3...v4
[v3]: https://github.com/sparten11740/allmylights/compare/v2...v3
[v2]: https://github.com/sparten11740/allmylights/compare/v1...v2
