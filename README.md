# GZDoom Batch Launcher (a.k.a. Doomer)

![Doomer.](/Doomer/Doomer.ico)</br>

Doomer is a small WinForms app I built while playing some Doom. I always wondered what it would look like to have a simple UI listing all of my <ins>.bat</ins> launchers as clickable icons, instead of digging through a folder — so here it is.

## Requirements

* Windows (x64). The installer bundles the .NET 10 runtime, so no separate .NET install is needed.
* [GZDoom](https://zdoom.org/downloads) and/or [DSDA Doom](https://github.com/kraflab/dsda-doom) installed somewhere on disk.
* Your IWADs, WADs and batch files organized in folders (see Configuration below).

## Configuration

The app reads its settings from <ins>appsettings.json</ins> on startup. You can edit that file directly, or open **Settings** from the app's menu to change everything (including browsing for the GZDoom executable and folders) without touching JSON.

```json
{
  "GZDoom": {
    "Location": "D:\\gzdoom\\gzdoom",
    "Plugins": "plugins",
    "Batchs": {
      "Location": "D:\\GZDoom\\batchs",
      "Extension": ".bat"
    },
    "Images": {
      "Location": "D:\\GZDoom\\batchs\\Pics",
      "Extension": ".png"
    }
  },
  "DSDADoom": {
    "Location": "E:\\DSDA\\dsda-doom"
  },
  "Icons": {
    "Width": 120,
    "Height": 100,
    "Padding": 5
  }
}
```

* **GZDoom.Location**: full path to your `gzdoom.exe`.
* **GZDoom.Plugins**: the folder (relative to your GZDoom install) where optional extra files live, e.g. SmoothDoom, Corruption Cards.
* **GZDoom.Batchs**: where your `.bat` launchers live, and their extension.
* **GZDoom.Images**: where the button icons live, and their extension.
* **DSDADoom.Location**: full path to your `dsda-doom.exe` (optional — only needed for DSDA Doom batches). At least one of the two executables must be set.
* **Icons**: size and spacing of the buttons in the main window.

## Using the app

The main window lists one button per batch file found in `GZDoom.Batchs.Location`, using the matching image from `GZDoom.Images.Location` (same file name, different extension) as its icon — falling back to plain text if no image is found. Use the search box in the menu to filter the list by name.

Right-click any WAD button to **Edit** or **Delete** its batch file. Deleting a batch also removes its icon image, if one exists.

## Adding a new batch file

Whenever you download a new WAD and want a launcher for it, use **Add Batch File** from the menu:

![Add Batch File Form](https://i.ibb.co/pjQzFDcG/Add-New-Batch-File.png)

* <ins>File Name</ins>: the name the batch file (and its button) will use. **Note**: it must match the name of the icon image for that WAD.
* <ins>Source Port</ins>: **GZDoom** or **DSDA Doom**. It picks which executable from your settings the batch launches, and how the command is written.
* <ins>IWAD</ins>: the path to the IWAD your WAD was made from. The `.wad` extension is optional — it's added automatically if you leave it out. For example, if your IWADs are under `D:\GZDoom\wads\doom2.wad`, enter `wads/doom2`.
* <ins>WAD</ins>: the path to the WAD itself, same rule — with or without `.wad`. Other extensions (e.g. `.pk3`) are kept as-is. For the Ancient Aliens WAD at `D:\GZDoom\wads\Ancient Aliens\aaliens.wad`, enter `wads/Ancient Aliens/aaliens`.
* <ins>Plugins (optional, GZDoom only)</ins>: just the file name of an extra file you want loaded alongside the WAD (e.g. SmoothDoom, Corruption Cards, IDClever). It's combined with the `GZDoom.Plugins` folder from your settings, so don't include a path here.

If a batch file with the same name already exists, you'll be asked to confirm before it's overwritten. If you enter a full (rooted) path for IWAD or WAD that doesn't exist on disk, you'll also get a confirmation before the batch is created — relative paths aren't checked, since they're resolved by GZDoom at launch time, not by Doomer.

Both IWAD and WAD have a **Browse...** button to pick the file instead of typing the path.

The end result is a `.bat` file with a command like this:

```
GZDoom:    "D:\gzdoom\gzdoom" -iwad "wads/doom2.wad" -file "wads/Ancient Aliens/aaliens.wad"
DSDA Doom: "E:\DSDA\dsda-doom" "WadSmoosh/source_wads/tnt.wad" "wads/D.O.O.M\DrakeRC2.wad"
```

## Building and testing

```
dotnet build
dotnet test
```

`Doomer.Tests` covers the batch command building, parsing and file name validation logic in `Doomer/Services/BatchFileService.cs`.

## Creating an installer

To build a distributable installer that doesn't require .NET or Visual Studio on the target machine:

1. Install [Inno Setup](https://jrsoftware.org/isinfo.php) (one-time, on the machine building the installer).
2. Run:
   ```powershell
   .\installer\publish.ps1 -Version 1.0.0
   ```

This publishes a self-contained, single-file build of the app (bundling the .NET runtime) and, if Inno Setup is found, compiles it into `installer\Output\DoomerSetup-<version>.exe`. That single file is all you need to hand out — it installs the app, creates Start Menu/desktop shortcuts, and registers an uninstaller.

## Releasing an update

On startup, Doomer checks the [latest GitHub release](https://github.com/JPDKR/Doomer/releases/latest). If its tag is newer than the installed version, the app offers to update: it downloads the `DoomerSetup-<version>.exe` attached to the release, installs it silently and relaunches. Your settings live in `%AppData%\Doomer`, so updating doesn't touch them.

To publish a new version, push a version tag:

```
git tag v1.1.0
git push origin v1.1.0
```

The `Release` workflow builds the installer for that version and attaches it to a new GitHub release.

## Roadmap

- [ ] Support multiple WAD/plugin files per batch.
