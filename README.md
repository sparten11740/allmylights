# AllMyLights

- [AllMyLights](#allmylights)
  - [What am I?](#what-am-i)
  - [Prerequisites](#prerequisites)
    - [OpenRGB](#openrgb)

## What am I?
I am a little command-line utility that is meant to synchronize your lighting across a home automation bus (HAB) and proprietary RGB peripherals of a computer. I serve as a broker that consumes colors via MQTT and passes them on to an OpenRGB instance on your target machine.

## Prerequisites
### OpenRGB
On the system that has the RGB peripherals you want to synchronize with your HAB, you have to setup OpenRGB.

> Open source RGB lighting control that doesn't depend on manufacturer software. Supports Windows and Linux.

You can download the OpenRGB binaries in the [release section of the project's gitlab](https://gitlab.com/CalcProgrammer1/OpenRGB/-/releases)

Follow [these instructions](https://gitlab.com/CalcProgrammer1/OpenRGB/-/wikis/Frequently-Asked-Questions#can-i-have-openrgb-start-automatically-when-i-log-in) to run a minimized OpenRGB server when logging in to your machine.

For questions around the detection of your devices, please refer to the OpenRGB community. 