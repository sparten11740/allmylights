#  allmylights 


![test](https://github.com/sparten11740/allmylights/workflows/test/badge.svg) ![build | windows](https://github.com/sparten11740/allmylights/workflows/build%20%7C%20windows/badge.svg) ![build | unix](https://github.com/sparten11740/allmylights/workflows/build%20%7C%20unix/badge.svg)

- [What am I?](#what-am-i)
- [Dependencies](#dependencies)
  - [OpenRGB](#openrgb)
  - [MQTT Server](#mqtt-server)
    - [OpenHAB Integration](#openhab-integration)
- [Installation](#installation)
  - [Using the binaries](#using-the-binaries)
  - [Building the project yourself](#building-the-project-yourself)
- [Configuration](#configuration)
  - [Sources](#sources)
    - [MQTT](#mqtt)
    - [OpenRGB](#openrgb-1)
  - [Sinks](#sinks)
    - [OpenRGB](#openrgb-2)
    - [MQTT](#mqtt-1)
    - [Wallpaper](#wallpaper)
    - [Chroma](#chroma)
  - [Transformations](#transformations)
    - [JsonPath](#jsonpath)
    - [Color](#color)
    - [Mapping](#mapping)
    - [Expression](#expression)
- [Run me](#run-me)
  - [Windows](#windows)
  - [Linux](#linux)
  - [Command Line Arguments](#command-line-arguments)
- [Autostart](#autostart)
  - [Windows](#windows-1)
    - [CLI](#cli)
    - [Manually](#manually)
  - [Linux](#linux-1)
    - [CLI](#cli-1)
    - [Manually](#manually-1)
- [Attribution](#attribution)

## What am I?
I am a little command line utility that is meant to synchronize your ambient 
lighting and RGB peripherals. I serve as a broker that consumes input values via 
MQTT and applies those to a number of configurable sinks: 

Those can be an OpenRGB instance, Chroma enabled devices, or your Desktop 
wallpaper.


[View demo on YouTube](https://www.youtube.com/watch?v=1Y9CBZFACro&ab_channel=JanWendland)

## Dependencies
### OpenRGB
OpenRGB is one of the supported options to control the RGB peripherals of your
host system. 

> Open source RGB lighting control that doesn't depend on manufacturer software. 
> Supports Windows and Linux.

You can download the OpenRGB binaries in the 
[release section of the project's gitlab](https://gitlab.com/CalcProgrammer1/OpenRGB/-/releases)

Follow [these instructions](https://gitlab.com/CalcProgrammer1/OpenRGB/-/wikis/Frequently-Asked-Questions#can-i-have-openrgb-start-automatically-when-i-log-in) 
to run a minimized OpenRGB server when logging in to your machine.

For questions around the detection of your devices, please refer to the OpenRGB 
community. 

### MQTT Server
Inputs such as profile names, or color strings are received through subscription 
of an [MQTT](https://mqtt.org/) topic. 

> MQTT is an OASIS standard messaging protocol for the Internet of Things (IoT).
> It is designed as an extremely lightweight publish/subscribe messaging 
> transport that is ideal for connecting remote devices with a small code 
> footprint and minimal network bandwidth

Given that you are looking at this page, you probably have a smart home 
framework in place already and want to integrate with it. Chances are that 
you are using MQTT as part of that setup. In that case you can go ahead and 
skip the rest of this section. If you're using a smart home framework without 
MQTT, please refer to the following resources for getting your MQTT server 
started and integrated.

#### OpenHAB Integration
> An OpenRGB Binding for OpenHAB does not exist. However, it is something 
> that would be easy to implement.

OpenHAB has a binding that connects with an MQTT broker. Install the broker 
[Mosquitto](https://mosquitto.org/) on your device running OpenHAB and afterwards 
proceed installing the [MQTT Binding](https://www.openhab.org/addons/bindings/mqtt/).

You could then create a rule along the lines (using the rules DSL):

```kotlin
"Set RGB value"
    when
        Item My_Color_Item received command
    then
    
    val actions = getActions("mqtt","mqtt:broker:mosquitto")
    
    val HSBType hsb = receivedCommand as HSBType
    val Color color = Color::getHSBColor(hsb.hue.floatValue / 360, hsb.saturation.floatValue / 100, hsb.brightness.floatValue / 100)
    val String rgb = String::format("%1$02x%2$02x%3$02xFF" , color.red, color.green, color.blue)

    actions.publishMQTT("stat/open-rgb/RESULT", rgb)
end
```

##  Installation
### Using the binaries 
Download the binaries for your target platform in the 
[releases section](https://github.com/sparten11740/allmylights/releases) (stable) 
or from the uploaded artifacts of the 
[most recent workflow runs](https://github.com/sparten11740/allmylights/actions). 
Place them in your desired target location. You can also clone this repository 
and build the project yourself.

### Building the project yourself
> In order to build the binaries for Windows, you have to use a Windows machine. 
> This is because of a framework dependency on `Microsoft.WindowsDesktop.App` 
> that is only available on Windows. However, MacOS and Linux binaries can be 
> built and published from any host system.
 
First you need to install the 
[Visual Studio Community Edition 2019](https://visualstudio.microsoft.com/vs/) 
on your machine.

Make sure `dotnet` is available in your path and run the following command from 
the project root to build a standalone `.exe` (Windows):

```sh
dotnet publish --runtime win-x64 --configuration Release -p:PublishSingleFile=true --self-contained false
```

Run the following if you want the application to work on a target without the 
.NET runtime installed:

```sh
dotnet publish --runtime win-x64 --configuration Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
```

## Configuration
The required parameters such as connection details are provided in a 
configuration file called `allmylightsrc.json` (default). Hereinafter, I will 
adivse on what this file should look like to satisfy your requirements. 

A number of usage examples can also be found in 
[the wiki](https://github.com/sparten11740/allmylights/wiki).

The configuration distinguishes between 
- sources, that provide values to the app
- transformations, that alter those values
- sinks, that consume and apply those values

Any value emitted by a source is routed to all sinks.

The structure of the `allmylightsrc.json` is the following:

```json5
// allmylightsrc.json

{
  "Sources":  [
    // ... see available options below
  ],
  "Sinks": [
    // ... see available options below
  ]
}
```
Available source, sink, and transformation types are:

| Type           | Options                                      |
| ---------------| ---------------------------------------------|
| Source         | `Mqtt`, `OpenRGB`                            |
| Sink           | `OpenRGB`, `Wallpaper`, `Chroma`, `Mqtt`     | 
| Transformation | `JsonPath`, `Color`, `Mapping`, `Expression` |


### Sources
Sources produce values that are eventually consumed by sinks. All sources have a 
`Transformations` property in common. Therein you can define transformations 
that alter the value emitted by the source to suit your requirements.

#### MQTT
The MQTT source subscribes to a topic and emits all values that are published to 
that topic.

```json5
{
  "Type" : "Mqtt",
  "Server": "192.168.1.20",
  "Port": 1883,
  "Topics": {
    // optional command topic that is used to request the current color on startup
    "Command": "cmnd/sonoff-1144-dimmer-5/color",
    "Result": "stat/sonoff-1144-dimmer-5/RESULT"
  },
  // transformations are applied in order on any received message 
  "Transformations": [
    // ... see section transformations for options
  ]
}
```


#### OpenRGB
The OpenRGB source continueously polls your configured OpenRGB server and emits 
an object that contains the colors per device whenever a device state changes.

```json5
{
  "Type" : "OpenRGB",
  "Server": "127.0.0.1",
  "Port": 6742,
  // controls how often OpenRGB is asked for changes, default is 1000ms
  "PollingInterval": 1000,
  // transformations are applied in order on any received message 
  "Transformations": [
    // ... see section transformations for options
  ]
}
```

The produced value is of type `Dictionary<string, DeviceState>` where the
key is the name of your OpenRGB device and `DeviceState` is a struct that
has the following properties:

```csharp
public struct DeviceState
{
    public IEnumerable<Color> Colors;
}
```

To extract values from it, use the [`Expression`](#expression) transformation such as 

```json5
{
  "Type": "Expression",
  "Expression": "value[\"Corsair H150i PRO RGB\"].Colors.Cast().First().ToCommaSeparatedRgbString()"
}
```

### Sinks
Sinks conclude the transformation process and consume values. A sink can define 
transformations which are applied on the value **before** it is consumed by the sink.

#### OpenRGB
The OpenRGB sink can receive values of type `System.Drawing.Color` or `string`.

Colors are applied to all devices connected to your OpenRGB instance 
unless specified otherwise in the sink's `Overrides` property. A color type can 
be converted from a string (such as `#FF0022` or `Red`) by adding a `Color` 
transformation to the sink.

String values received by the sink have to be valid OpenRGB profile names such 
as `MyProfile.orp`. They end in `.orp` and have to exist on your OpenRGB host. 
Note that a working version of the profile management API was only added in 
commit `f63aec11` to OpenRGB. Please make sure that you have an up-to-date 
version on your machine.

```json5
{
  // configure one or more target OpenRGB instances
  "Type": "OpenRGB",
  "Server": "127.0.0.1", 
  "Port": 6742,
  // if you want to override certain OpenRGB controlled devices you can do so here
  "Overrides": {
    // ignore an entire device
    "Razer Copperhead": {
      "Ignore": true,
    },
    "MSI Mystic Light MS_7C84": {
      "Zones": {
        // configure what color is passed to what channel of a zone
        "JRGB2": {
          "ChannelLayout": "GRB"
        },
        // ignore a single zone of a device
        "JRAINBOW1": {
          "Ignore": true
        }
      }
    }
  },
  // transformations can also be applied before a sink consumes a value
  "Transformations" : []
}
```

#### MQTT
The MQTT sink publishes any value it consumes to all configured topics.

```json5
{
    "Type" : "Mqtt",
    "Server": "192.168.1.20",
    "Port": 1883,
    // Optional username, remove if not required
    "Username": "",
    // Optional password, remove if not required
    "Password": "",
    "Topics": [ "some/topic", "another/topic" ],
    "Transformations": [
      // ... see section transformations for options
    ]
  }
```

#### Wallpaper
The Wallpaper sink currently only works on Windows machines and only if you 
run the app on the host machine itself. It can receive values of type `string`, 
which have to be valid file paths. If `RelativeTo` is specified, the command 
line flag `--info` can be used to print available files under that directory.

```json5
{
  "Type": "Wallpaper",
  // if the input value is a relative path or file name and RelativeTo is specified, it will be prepended to the input value
  "RelativeTo": "C:\\Users\\brucewayne\\Pictures\\Wallpaper",
  "Transformations": [
    // one could for example map from a color to a filename here, see transformation options down below
  ]
}
```

#### Chroma
The Chroma sink controls the RGB of your Chroma enabled devices. You can provide 
values of type `System.Drawing.Color` (returned by the `Color` transformation) 
or `string`.

It depends on the Razer Synapse software on your machine with the Chroma Connect 
module added.

Any `System.Drawing.Color` received will be applied to all devices listed in 
`SupportedDevices` as static effect.

Any `string` received must be valid JSON and conform to the expected Chroma SDK 
structure. Examples of valid inputs can be found in the 
[Razer Chroma SDK REST Documentation](https://assets.razerzone.com/dev_portal/REST/html/md__r_e_s_t_external_04_8mouse.html)


```json5
{
  "Type": "Chroma",
  "SupportedDevices": [
    // can be any combination of the following
    "keyboard",
    "mouse",
    "headset",
    "mousepad",
    "keypad",
    "chromalink"
  ]
}
```

### Transformations
#### JsonPath
This transformation can be used for extracting values from a JSON input.

```json
  { "Type": "JsonPath", "Expression": "$.data[0].color" }
```

For further information on how to extract a value from JSON using `JsonPath` 
expressions, please refer to 
[this documentation](https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html). 


#### Color
This transformation is used to deal with the type conversion from an input 
string to a Color type that can be consumed by a sink that expects such.

```json
  { "Type": "Color", "ChannelLayout": "RGBA" }
```

Supported values are hex strings such as the following `#f2d`, `#ed20ff`, 
`#2020ffed` and color names where the name can be any 
[known color](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.knowncolor?view=net-5.0).

It also supports the optional property `ChannelLayout` to deal with cases where
the channels of a color in the hex string are mixed up for some reason such as 
f.i. when the first hex number does not correspond with red but with green. 
Possible values are any string of up to 4 characters containing `R`, `G`, `B`, 
or `A` for alpha. Also `_` can be used to ignore values, for instance if you had
an RGBA hex string, you could use the channel layout `RGB_` to ignore the alpha 
value which then will default to `Oxff` (255). The default for ignored color 
channels is `0x00` (0);

#### Mapping
This transformation is used to map an input value to an output value. It can be 
provided a number of mappings where the first matching one is used to map the 
input value. Per default `FailOnMiss` is false so that input values that are not
matched by any mapping are simply passed through.

The `From` property of a mapping expects a regular expression. Please make sure 
that you escape any control characters if your goal is a simple string match.

```json
  {
    "Type": "Mapping",
    "FailOnMiss":  false,
    "Mappings": [
      {
        "From": "#?FFFFFF.+",
        "To": "#ff0000"
      }
    ]
  }
```

#### Expression
This transformation is used to transform a value by applying advanced logic. Any 
expression covered in the 
[simple expression section](https://github.com/codingseb/ExpressionEvaluator/wiki/Getting-Started#simple-expressions) of the ExpressionEvaluator repo 
can be evaluated by this transformation. The input value provided to this 
transformation is made available to the context of the expression as `value`.

Non primitive types are also supported. For instance by prepending a color 
transformation, any method or property defined in the `System.Drawing.Color` 
([documentation](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-5.0)) 
becomes available in the expression context on value.


```json
  {
    "Type": "Expression",
    "Expression":  "value.B > value.R && value.B > value.G ? \"Blueish\" : \"Some other color\""
  }
```

## Run me
### Windows
Before you start, download and install the .NET core runtime 
[here](https://dotnet.microsoft.com/download). 

You can run the app as simple as follows (assuming the 
`allmylightsrc.json` resides in the same directory as the executable)

```powershell
.\AllMyLights.exe
```

You can also change the log level to one of the following: 
`info`, `debug`, `warn`, `error`, `off`.

```powershell
.\AllMyLights.exe --config /config/elsewhere/allmylightsrc.json --log-level off
```

### Linux
As a prerequisite follow 
[Microsoft's instructions](https://docs.microsoft.com/en-us/dotnet/core/install/linux) 
to install the .NET Core runtime or alternatively build the project yourself 
with the `--self-contained` flag set to `true`. The latter results in a 
framework independant binary.

You can run the app by simply calling (assuming the default config file 
`allmylightsrc.json` in the same folder as the binary)

```sh
./AllMyLights
```

### Command Line Arguments

| Argument                                 | Description                                                                                    |
| -----------------------------------------| ---------------------------------------------------------------------------------------------- |
| &#x2011;&#x2011;config                   | Path to the config file that contains the sink & source settings. Default `allmylightsrc.json` |
| &#x2011;&#x2011;fail-on-unknown-property | Fails if an unknown property is encountered in the provided config file                        |
| &#x2011;&#x2011;export-config-schema-to  | Writes the configuration schema (Open API v3) to the provided filepath.                        |
| &#x2011;&#x2011;log-level                | Change the log level to either debug, info (default), warn, error, or off.                     |
| &#x2011;&#x2011;log-file                 | If provided, log output will additionally be captured in the provided file.                    |
| &#x2011;&#x2011;minimized                | Minimize to tray after startup (Windows only)                                                  |
| &#x2011;&#x2011;info                     | List dynamic information of your sinks such as available options                               |
| &#x2011;&#x2011;enable-autostart         | Generates an autostart entry                                                                   |

## Autostart
### Windows
#### CLI
A shortcut can be generated in the user startup folder by runningthe following 
(the config file `allmylightsrc.json` is expected to reside in the same folder 
as the executable, but can be customized by providing the `--config` parameter)
```ps
.\AllMyLights.exe --enable-autostart
```
#### Manually
Create a shortcut to your `AllMyLights.exe`, open its properties and change the 
target to something along the lines:

```powershell
"D:\Program Files\AllMyLights\AllMyLights.exe" --config allmylightsrc.json --minimized true
```

Move the shortcut to `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`

Make also sure that your OpenRGB server is run on startup as described in the 
[OpenRGB Section](#openrgb)

### Linux

#### CLI
I can generate a service definition and enable startup on boot (in distributions 
using systemd). Simply call me with elevated privileges using the 
`--enable-autostart` flag. I assume that a config file named 
`allmylightsrc.json` resides in the same folder as my executable. This can be 
customized using the `--config` parameter. Also the log level can be changed. 
Before executing the following lines, please make sure that the environment 
variable DOTNET_ROOT is set (f.i. in `/etc/environment`)

```bash
sudo su
./AllMyLights --enable-autostart --log-level debug

# double check that I am up and running
service allmylights status
```
#### Manually
> The following instructions work for Raspbian or any other distribution that 
> uses systemd. 

Copy the binary for your platform and the `allmylightsrc.json` to a target 
directory (f.i. `$HOME/allmylights`) on your machine. Create a service 
definition using `sudo vi /etc/systemd/system/allmylights.service` and copy the 
following configuration:

```ini
[Unit]
Description=AllMyLights service to sync colors via MQTT to an OpenRGB instance

[Service]
WorkingDirectory=/home/pi/allmylights
ExecStart=/home/pi/allmylights/AllMyLights --config allmylightsrc.json --log-level info
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=allmylights
User=pi
Environment=DOTNET_ROOT=/home/pi/dotnet-arm32

[Install]
WantedBy=multi-user.target
```
Customize the `WorkingDirectory` and `ExecStart` to use the folder created in 
the previous step if required. Change the value of `User` to the user you want 
the service to be run as. Also note the `DOTNET_ROOT` environment variable. For 
framework dependant binaries, you have to change the path to the directory where 
your .NET Core runtime resides.

Start the service with `sudo service allmylights start` and check that 
everything is running smoothly using `sudo service allmylights status`. 
Afterwards use `sudo systemctl enable allmylights` to make systemd automatically 
start your service during boot up.

Make also sure that the OpenRGB server on the machine you want to control is run 
on startup as described in the [OpenRGB Section](#openrgb)

## Attribution
"[RGB Icon](https://icon-icons.com/icon/rgb/23694)" by [Elegantthemes](http://www.elegantthemes.com/) licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)
