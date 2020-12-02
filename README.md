# AllMyLights

- [AllMyLights](#allmylights)
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
  - [Autostart](#autostart)
    - [Windows](#windows)

## What am I?
I am a little command-line utility that is meant to synchronize your lighting across a home automation bus (HAB) and proprietary RGB peripherals of a computer. I serve as a broker that consumes colors via MQTT and passes them on to an OpenRGB instance on your target machine. 

In other words, I let your Razer Mouse, Gigabyte Aorus RTX 2080TI, MSI Tomahawk X570 Wifi, Corsair H150i RGB Pro, or whatever other device is supported by OpenRGB, shine in the same bright light as the ambient lighting in your appartment.

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
> :grey_exclamation: An OpenRGB Binding for OpenHAB does not exist. However, it is something that would be easy to implement and would make myself as a companion obsolete.

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
Download my latest binaries for your target platform in the releases section of this repository and place them in your desired target location. You can also clone this repository and build the project yourself.

### Building the project yourself
First you need to install the Visual [Studio Community Edition 2019](https://visualstudio.microsoft.com/vs/) on your machine.

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
  // ip address & port of machine that runs openrgb
  "OpenRgb": {
    "Server": "127.0.0.1", 
    "Port": 6742
  },
  "Mqtt": {
    "Server": "192.168.178.20",
    "Port": 1883,
    "Topics": {
      // optional command topic that is used to request the current color on startup
      "Command": "cmnd/sonoff-1144-dimmer-5/color",
      "Result": {
        // topic to grab the color from
        "Path": "stat/sonoff-1144-dimmer-5/RESULT",
        // JsonPath expression pointing to the property that holds the color value
        "ValuePath": "$.Color"
      }
    }
  }
}
```

For further information on how to extract a value from JSON using `JsonPath` expressions, please refer to [this documentation](https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html). Supported are hex strings such as the following `#f2d`, `#ed20ff`, `#2020ffed` and color names where the name can be any [known color](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.knowncolor?view=net-5.0).

## Run me
You can run me as simple as follows, only the path to a valid config file is required:

```powershell
.\AllMyLights.exe --config allmylightsrc.json
```

You can also change the log level to one of the following: `info`, `debug`, `warn` (default), `none`.

```powershell
.\AllMyLights.exe --config allmylightsrc.json --log-level debug
```

## Autostart
### Windows
Create a shortcut to your `AllMyLights.exe`, open its properties and change the target to something along the lines:

```
"D:\Program Files\AllMyLights\AllMyLights.exe" --config allmylightsrc.json
```

Move the shortcut to `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`

Make also sure that your OpenRGB server is run on startup as described in the [OpenRGB Section](#openrgb)