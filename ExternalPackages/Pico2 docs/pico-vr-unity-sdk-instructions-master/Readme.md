# Pico Unity SDK Instructions

> These instructions are for v2.7.8 of the Pico Unity SDK. If you are looking for the instructions for v2.3.0, you can find them [here](https://github.com/wearvr/pico-vr-unity-sdk-instructions/tree/v2.3.0).

Instructions for how to create new Unity virtual reality experiences for the Pico Goblin & Pico Neo headsets (or port existing ones).

<p align="center">
  <img alt="Pico Goblin" width="500px" src="/docs/assets/Pico.svg">
</p>

## The Pico Goblin & Pico Neo

#### Support & Revenue share

WEARVR.com, the world's largest independent VR app store, has partnered with Pico Interactive to provide development kits and assistance with promotion, technical support and advice to help get your content into Pico's global marketplace (including China) - at no cost to you. You get the same high revenue share of 70%.

| Region | Developer Revenue Share |
| :---: | :----: |
| Worldwide | 70% |

#### Specifications

View the [full headset specifications](https://www.wearvr.com/developer-center/devices/pico).

#### Requesting a development kit

You can [request a Pico Goblin](/docs/pico-development-kit.md) to help get your VR experiences Pico-compatible.

## Prerequisites

You will require the following in order to develop a Pico app:

A compatible version of Unity:

* For Unity2017, all versions above and including Unity2017.4.0 are supported. 
* Unity2018 is currently supported up until to Unity2018.4.0.

* Android API Level 24 (Android 7.0) or above.
* A Pico Goblin or Pico Neo headset of any kind.
* JDK v1.8 or later.

### User accounts and adding in-app purchases

To access user information on the Pico Goblin headset, or add in-app purchases to your VR content, your app will normally need to already be [registered through WEARVR](https://users.wearvr.com/apps) to generate the necessary access credentials.

If this is a problem, you can get in touch via `devs@wearvr.com` to discuss your needs.

## Overview

You can easily create a new Unity VR app, but the fastest way to get up and running on Pico Goblin is to convert an existing Google Cardboard, Google Daydream or Samsung Gear VR experience.

* [Installing and configuring the Pico VR Unity SDK](/docs/pico-vr-unity-sdk-installation.md)
* [Camera & input module setup](/docs/pico-vr-camera-setup.md)
* [Controller and headset inputs](/docs/pico-goblin-and-neo-controllers.md)
* [Enabling USB debugging](/docs/pico-goblin-developer-mode-usb-debugging.md)
* [Building to the device](/docs/building-to-pico-goblin.md)

Optional:

* [Working with the current user](/docs/pico-payment-sdk-user-management.md)
* [Adding in-app purchases](/docs/pico-payment-sdk-in-app-purchases.md)
* [Testing in-app purchases](/docs/testing-in-app-purchases.md)
* [Troubleshooting](/docs/troubleshooting.md)
* [Optimizing Pico experiences](/docs/optimizing-pico-experiences.md)
* [Localization](/docs/pico-unity-localization.md)
* [Getting device information at runtime](/docs/getting-device-information-at-runtime.md)
* [Overview of API](/docs/api-overview.md)
* [Using Unity splash screens](/docs/using-unity-splash-screens.md)
* [Using Pico transmission layers](/docs/compositor-layers.md)

Device:

* [Connecting to a Wifi network](/docs/connecting-to-a-wifi-network.md)
* [Upgrading the Pico Goblin OS & Firmware](/docs/upgrading-pico-goblin-operating-system-firmware.md)
* [Changing the Pico Goblin's language](/docs/changing-pico-goblins-language-setting.md)

There is an [example project](examples/PicoUnityVRSDKExample/Readme.md) to use as a reference as you follow this guide.

There is also an [example project](examples/input%20test/Readme.md) of input, and how to make UI elements work for VR platforms.

## Upgrade Guides

[Upgrading from v2.3.0](/docs/upgrading/upgrading-from-v2-3-0-to-v2-7-4.md)

## Unable to port yourself?

Depending on availability and need, WEARVR may be able to provide addition support to help port your existing VR experiences to be compatible with the Pico Goblin and Pico Neo, all the way up to completing the port for you and localizing it for the Chinese market.

Please get in touch on `devs@wearvr.com` to discuss your needs and how WEARVR may be able to help.

## Uploading and selling your experiences

### What build variants do I need?

If you have used v2.7.5 or higher of the Pico Unity SDK and you have not used the [Pico User API](/docs/pico-payment-sdk-user-management.md) or [Payment API](/docs/pico-payment-sdk-in-app-purchases.md), then you will only need one build that can be sold inside and outside of China, for both the Pico Goblin and the Pico Neo.

If you have used the [Pico User API](/docs/pico-payment-sdk-user-management.md), you will need two build variants - one that includes the [domestic credential fields in your AndroidManifest.xml](/docs/pico-payment-sdk-user-management.md#credentials#domestic-values-inside-china) for the China store and one that includes the [foreign credential fields](/docs/pico-payment-sdk-user-management.md#credentials#foreign-values-outside-china) for stores outside of China (these are the same builds and same credential values in the AndroidManifest.xml - just with different attribute names).

### Submission process

When you are ready, it's time to release your Pico VR experiences to the global and Chinese Pico stores [through WEARVR.com](https://users.wearvr.com/apps).

## Copyright & Trademarks

These instructions and example project are maintained by WEARVR LLC, the largest independent virtual reality app store. WEARVR is interested in connecting VR content creators and consumers, globally. We love working with the VR community and would be delighted to hear from you at `devs@wearvr.com`.

You can find more information about WEARVR at www.wearvr.com

The Pico Interactive Inc trademark, Pico virtual reality headsets and Pico VR Unity SDK are all owned by [Pico Interactive Inc](https://www.pico-interactive.com/).

