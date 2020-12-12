#  AllMyLights 


![test](https://github.com/sparten11740/allmylights/workflows/test/badge.svg) ![build | windows](https://github.com/sparten11740/allmylights/workflows/build%20%7C%20windows/badge.svg) ![build | unix](https://github.com/sparten11740/allmylights/workflows/build%20%7C%20unix/badge.svg)

- [What am I?](#what-am-i)
- [Prerequisites](#prerequisites)
  - [OpenRGB](#openrgb)
  - [MQTT Server](#mqtt-server)
    - [OpenHAB Integration](#openhab-integration)
- [Installation](#installation)
  - [Using the binaries](#using-the-binaries)
  - [Building the project yourself](#building-the-project-yourself)
- [Configuration](#configuration)
- [Run me](#run-me)
  - [Windows](#windows)
  - [Linux](#linux)
- [Autostart](#autostart)
  - [Windows](#windows-1)
  - [Linux](#linux-1)
- [Attribution](#attribution)

## What am I?
I am a little command-line utility that is meant to synchronize your lighting across a home automation bus (HAB) and proprietary RGB peripherals of a computer. I serve as a broker that consumes colors via MQTT and passes them on to an OpenRGB instance on your target machine. 

In other words, I let your Razer devices, Gigabyte graphics card, MSI MysticLight powered mainboard, Corsair Hydro liquid cooler, or whatever other device is supported by OpenRGB, shine in the same bright light as the ambient lighting in your appartment.

[View demo on YouTube](https://www.youtube.com/watch?v=1Y9CBZFACro&ab_channel=JanWendland)

## Prerequisites
### OpenRGB
On the system that has the RGB peripherals you want to synchronize with your HAB, you have to setup OpenRGB.

> Open source RGB lighting control that doesn't depend on manufacturer software. Supports Windows and Linux.

You can download the OpenRGB binaries in the [release section of the project's gitlab](https://gitlab.com/CalcProgrammer1/OpenRGB/-/releases)

Follow [these instructions](https://gitlab.com/CalcProgrammer1/OpenRGB/-/wikis/Frequently-Asked-Questions#can-i-have-openrgb-start-automatically-when-i-log-in) to run a minimized OpenRGB server when logging in to your machine.

For questions around the detection of your devices, please refer to the OpenRGB community. 

### MQTT Server
As of now I can only consume color updates via [MQTT](https://mqtt.org/). 

> MQTT is an OASIS standard messaging protocol for the Internet of Things (IoT). It is designed as an extremely lightweight publish/subscribe messaging transport that is ideal for connecting remote devices with a small code footprint and minimal network bandwidth

Given that you are looking at this page, you probably have a smart home framework in place already and want to integrate with it. Chances are that you are using MQTT as part of that setup. In that case you can go ahead and skip the rest of this section. If you're using a smart home framework without MQTT, please refer to the following resources for getting your MQTT server started and integrated.

#### OpenHAB Integration
> An OpenRGB Binding for OpenHAB does not exist. However, it is something that would be easy to implement and would make myself as a companion obsolete.

OpenHAB has a binding that connects with an MQTT broker. Install the broker [Mosquitto](https://mosquitto.org/) on your device running OpenHAB and afterwards proceed installing the [MQTT Binding](https://www.openhab.org/addons/bindings/mqtt/).

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
Download my binaries for your target platform in the [releases section](https://github.com/sparten11740/allmylights/releases) (stable) or from the uploaded artifacts of the [most recent workflow runs](https://github.com/sparten11740/allmylights/actions) (bleeding edge). Place them in your desired target location. You can also clone this repository and build the project yourself.

### Building the project yourself
> In order to build the binaries for Windows, you have to use a Windows machine. This is because of a framework dependency on `Microsoft.WindowsDesktop.App` that is only available on Windows. However, MacOS and Linux binaries can be built and published from any host system.
 
First you need to install the [Visual Studio Community Edition 2019](https://visualstudio.microsoft.com/vs/) on your machine.

Make sure `dotnet` is available in your path and run the following command from the project root to build a standalone `.exe` (Windows):

```sh
dotnet publish --runtime win-x64 --configuration Release -p:PublishSingleFile=true --self-contained false
```

Run the following if you want the application to work on a target without the .NET runtime installed:

```sh
dotnet publish --runtime win-x64 --configuration Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
```

## Configuration
I read the required server ip addresses, ports, topics etc. from a configuration file. This repository and the release section contains a sample configuration called `allmylightsrc.json`. Hereinafter, I will adivse on what this file should look like to satisfy your needs.

```json5
// allmylightsrc.json

{
  "Sources":  [{
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
      // JsonPath expression transformation to extract the value that holds the color 
      {
        "Type": "JsonPath",
        "Expression": "$.Color"
      },
      // decodes Color from string value (required type f.i. for the OpenRGB sink)
      { "Type": "Color" }
    ]
  }],
  
  // ip address & port of machine that runs openrgb
  "Sinks": [{
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
  }]
}
```
Available source, sink, and transformations types as of this version are:

| Type           | Options                    |
| ---------------| -------------------------- |
| Source         | `Mqtt`                     |
| Sink           | `OpenRGB`                  | 
| Transformation | `JsonPath`, `Color`        |


### JsonPath Transformation
This transformation can be used for extracting values from a json string.

```json
  { "Type": "JsonPath", "Expression": "$.data[0].color" }
```

For further information on how to extract a value from JSON using `JsonPath` expressions, please refer to [this documentation](https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html). 


### Color Transformation
This transformation is used to deal with the type conversion from an input string to a Color type that can be consumed by a sink that expects such.

```json
  { "Type": "Color", "ChannelLayout": "RGBA" }
```

Supported values are hex strings such as the following `#f2d`, `#ed20ff`, `#2020ffed` and color names where the name can be any [known color](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.knowncolor?view=net-5.0).

It also supports the optional property `ChannelLayout` to deal with cases where the channels of a color in the hex string are mixed up for some reason such as f.i. when the first hex number does not correspond with red but with green. Possible values are any string of up to 4 characters containing `R`, `G`, `B`, or `A` for alpha. Also `_` can be used to ignore values, for instance if you had an RGBA hex string, you could use the channel layout `RGB_` to ignore the alpha value (will default to `Oxff`);

## Run me
### Windows
Before you start, download and install the .NET core runtime [here](https://dotnet.microsoft.com/download). 

You can run me as simple as follows, only the path to a valid config file is required:

```powershell
.\AllMyLights.exe --config allmylightsrc.json
```

You can also change the log level to one of the following: `info`, `debug`, `warn` (default), `error`, `off`.

```powershell
.\AllMyLights.exe --config allmylightsrc.json --log-level debug
```

### Linux
As a prerequisite follow [Microsoft's instructions](https://docs.microsoft.com/en-us/dotnet/core/install/linux) to install the .NET Core runtime or alternatively build the project yourself with the `--self-contained` flag set to `true`. The latter results in a framework independant binary.

You can run me as simple as follows, only the path to a valid config file is required:

```sh
./AllMyLights --config allmylightsrc.json
```



### Command Line Arguments

| Argument                   | Description                                                                 |
| -------------------------- | --------------------------------------------------------------------------- |
| **&#x2011;&#x2011;config** | Path to the config file that contains the MQTT and OpenRGB settings         |
| &#x2011;&#x2011;log-level  | Change the log level to either debug, info, warn, error, or off.            |
| &#x2011;&#x2011;log-file   | If provided, log output will additionally be captured in the provided file. |
| &#x2011;&#x2011;minimized  | Minimize to tray after startup (Windows only)                               |

## Autostart
### Windows
Create a shortcut to your `AllMyLights.exe`, open its properties and change the target to something along the lines:

```
"D:\Program Files\AllMyLights\AllMyLights.exe" --config allmylightsrc.json --minimized true
```

Move the shortcut to `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`

Make also sure that your OpenRGB server is run on startup as described in the [OpenRGB Section](#openrgb)

### Linux
The following instructions work for Raspbian or any other distribution that uses systemd. 

Copy the binary for your platform and the `allmylightsrc.json` to a target directory (f.i. `$HOME/allmylights`) on your machine. Create a service definition using `sudo vi /etc/systemd/system/allmylights.service` and copy the following configuration:

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
Customize the `WorkingDirectory` and `ExecStart` to use the folder created in the previous step if required. Change the value of `User` to the user you want the service to be run as. Also note the `DOTNET_ROOT` environment variable. For framework dependant binaries, you have to change the path to the directory where your .NET Core runtime resides.

Start the service with `sudo service allmylights start` and check that everything is running smoothly using `sudo service allmylights status`. Afterwards use `sudo systemctl enable allmylights` to make systemd automatically start your service during boot up.

Make also sure that the OpenRGB server on the machine you want to control is run on startup as described in the [OpenRGB Section](#openrgb)

## Attribution
"[RGB Icon](https://icon-icons.com/icon/rgb/23694)" by [Elegantthemes](http://www.elegantthemes.com/) licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)
