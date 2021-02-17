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

[v6]: https://github.com/sparten11740/allmylights/compare/v5...v6
[v5]: https://github.com/sparten11740/allmylights/compare/v4...v5
[v4]: https://github.com/sparten11740/allmylights/compare/v3...v4
[v3]: https://github.com/sparten11740/allmylights/compare/v2...v3
[v2]: https://github.com/sparten11740/allmylights/compare/v1...v2
