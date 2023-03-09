# Upgrading Pico Goblin operating system firmware

Currently the latest version of the Pico Goblin OS is 2.3.0. This is the lowest version of the OS your app will need to support.

## Prerequisites

To upgrade a Pico Goblin device's operating system and firmware, you will need:

* A Pico Goblin device that is *fully charged*
* A Micro-SD card 1GB or larger, formatted as FAT32
* A computer to download the file and place it on the Micro-SD card
* A WEARVR developer account

## Checking the current Pico OS version

On the Pico Goblin device's home screen, select the **Settings** option on the right of the screen:

<p align="center">
  <img alt="Select the Settings option" width="500px" src="assets/SettingsMenuOption.png">
</p>

Select **About** from the settings options:

<p align="center">
  <img alt="Select the About option" width="500px" src="assets/AboutMenuOption.png">
</p>

View the **PUI Version**:

<p align="center">
  <img alt="View the PUIVersion" width="500px" src="assets/PUIVersion.png">
</p>

Compare the PUI Version to [latest version of the Pico Goblin operation system available](https://users.wearvr.com/developers/devices/pico-goblin/resources/firmware).

## Upgrade instructions

If you have not already, download the [latest version of the Pico Goblin operation system and firmware](https://users.wearvr.com/developers/devices/pico-goblin/resources/firmware).

**Do not** extract the .zip file and do not rename it.

Check that your Micro-SD has been formatted using FAT32. If not, re-format it using your tool of choice.

Create a folder at the root of your Micro-SD card called `dload`.

Move the .zip file you just downloaded into this folder.

Eject the Micro-SD card and push it into the Micro-SD slot at the bottom of the Pico Goblin headset until you hear a click. Be careful to correctly orient the SD card: it should not stick out at all once it has been fully inserted.

Select the **Libary** menu option:

<p align="center">
  <img alt="Select Library menu option" width="500px" src="assets/LibraryMenuOption.png">
</p>

Open the **System Update** app:

<p align="center">
  <img alt="Open the System Update app" width="500px" src="assets/SystemUpdateApp.png">
</p>

> Do **not** press the power button or remove the Micro-SD until the upgrade is complete or you may damage the device.

Select the **Offline Update** option and follow the instructions.

<p align="center">
  <img alt="Select Offline Update" width="500px" src="assets/OfflineUpdateOption.png">
</p>

Once the device has updated and restarted, you now have the latest version of the Pico VR operating system and firmware installed.

You can now remove the Micro-SD card from the headset.

## Troubleshooting

If the device does not detect the updated operating system file on the Micro-SD, check that the file has its original name and is a directory called `dload`, at the root of the Micro-SD card's file system.

If that has no effect, try the following steps:

Open settings:

<p align="center">
  <img alt="Select the Settings option" width="500px" src="assets/SettingsMenuOption.png">
</p>

Select the **Developer** or **Advanced Options**:

<p align="center">
  <img alt="Select the Developer option" width="500px" src="assets/PicoDeveloperSettings.png">
</p>

Select the **Storage & USB**:

<p align="center">
  <img alt="Select Storage & USB" width="500px" src="assets/SelectStorageAndUSB.png">
</p>

Select the Micro-SD card:

<p align="center">
  <img alt="Select th Micro SD card" width="500px" src="assets/SelectSDCard.png">
</p>

Navigate to the `dload` directory. If there are two files that appear in that directory with the same name, delete the one with the smaller size (usually around 4kb).

Re-attempt the instructions above.
