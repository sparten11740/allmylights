## [0.4.0] - TBD

### General
- Add command-line flag to export a schema representation of the configuration file (OpenAPI 3 format).
- Add command-line flag to generate a systemd service definition and enable autostart (Linux)
- Introduce verbose logging for all transformations (log level debug)

### Transformations
- Add `Mapping` transformation to map matching input strings to output value. Supports regular expressions.

## [0.3.0] - 12/12/2020
### General
- In preparation for future changes, this release aims to achieve a clear separation between transport and transformation logic
- Introduce pluggable transformations to make logic such as extracting values via JsonPath or color conversions reusable across different source and sink types
## MqttSource
- Remove transformation logic from MqttSource

## [0.2.0] - 12/06/2020
### General
- Introduce the concept of sources where multiple sources can be used as a color signal
- Introduce the concept of sinks where all configured sinks will consume a color signal
### OpenRGBSink
- Add option to ignore a device
- Add option to ignore a zone of a device
- Add option to override the channel layout of a device
- Add option to override the channel layout of a zone

[0.4.0]: https://github.com/sparten11740/allmylights/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/sparten11740/allmylights/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/sparten11740/allmylights/compare/v0.1.0...v0.2.0
