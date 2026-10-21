# GZDoom Batch Launcher (a.k.a. Doomer)

![Doomer.](/Doomer/Doomer.ico)</br>

Doomer is a small WinForms app I built while playing some Doom. I always wondered what it would look like to have a simple UI listing all of my <ins>.bat</ins> launchers as clickable icons, instead of digging through a folder — so here it is.

## Requirements

* Windows (x64). The installer bundles the .NET 10 runtime, so no separate .NET install is needed.
* [GZDoom](https://zdoom.org/downloads) and/or [DSDA Doom](https://github.com/kraflab/dsda-doom) installed somewhere on disk.
* Your IWADs, WADs and batch files organized in folders (see Configuration below).

## Configuration

The app reads its settings from <ins>appsettings.json</ins> on startup. You can edit that file directly, or open **Settings** with the gear (⚙) button in the app's menu to change everything (including browsing for the GZDoom executable and folders) without touching JSON.

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
  },
  "General": {
    "Language": ""
  }
}
```

* **GZDoom.Location**: full path to your `gzdoom.exe`.
* **GZDoom.Plugins**: the folder (relative to your GZDoom install) where optional extra files live, e.g. SmoothDoom, Corruption Cards.
* **GZDoom.Batchs**: where your `.bat` launchers live, and their extension.
* **GZDoom.Images**: where the button icons live, and their extension.
* **DSDADoom.Location**: full path to your `dsda-doom.exe` (optional — only needed for DSDA Doom batches). At least one of the two executables must be set.
* **Icons**: size and spacing of the buttons in the main window.
* **General.Language**: the app's language — `en` (English) or `es` (Español). Leave it empty to follow Windows' language (English if Windows uses one the app isn't translated to).

### Translations

UI texts live in `Doomer/Strings.resx` (English) and one `Strings.<language>.resx` per translation, e.g. `Strings.es.resx`. To add a language, copy `Strings.es.resx` with the new language code, translate its values, and add it to `Localization.Languages` in `Doomer/Localization.cs` so it shows up in Settings. A test checks that every translation has all the strings and the same `{0}` placeholders as English.

## Using the app

The main window lists one button per batch file found in `GZDoom.Batchs.Location`, using the matching image from `GZDoom.Images.Location` (same file name, different extension) as its icon — falling back to plain text if no image is found. Use the search box in the menu to filter the list by name.

Click a button to run its batch file. It runs without a console window; if it fails (e.g. the executable was moved), Doomer shows the error instead.

Right-click any WAD button to **Edit** or **Delete** its batch file. Deleting a batch also removes its icon image, if one exists.

## Adding a new batch file

Whenever you download a new WAD and want a launcher for it, use **Add Batch File** from the menu:

![Add Batch File Form](https://i.ibb.co/pjQzFDcG/Add-New-Batch-File.png)

* <ins>File Name</ins>: the name the batch file (and its button) will use. **Note**: it must match the name of the icon image for that WAD.
* <ins>Source Port</ins>: **GZDoom** or **DSDA Doom**. It picks which executable from your settings the batch launches, and how the command is written.
* <ins>IWAD</ins>: the path to the IWAD your WAD was made from. The `.wad` extension is optional — it's added automatically if you leave it out. For example, if your IWADs are under `D:\GZDoom\wads\doom2.wad`, enter `wads/doom2`. The list offers the IWADs your other batches already use, with the same path they have (e.g. `D:\DSDA\doom.wad`), plus any IWAD found in the WADs folder — or type a path.
* <ins>WAD</ins>: the path to the WAD itself, same rule — with or without `.wad`. Other extensions (e.g. `.pk3`) are kept as-is. For the Ancient Aliens WAD at `D:\GZDoom\wads\Ancient Aliens\aaliens.wad`, enter `wads/Ancient Aliens/aaliens`.
* <ins>Plugins (optional, GZDoom only)</ins>: just the file name of an extra file you want loaded alongside the WAD (e.g. SmoothDoom, Corruption Cards, IDClever). Pick it from the list, which shows the `.wad`/`.pk3`/`.pk7`/`.zip`/`.deh`/`.bex` files in the `GZDoom.Plugins` folder (relative to your GZDoom install), or type a name. It's combined with that folder, so don't include a path here. If the file isn't in the folder, you'll be asked to confirm before saving.
* <ins>Image (optional)</ins>: the icon for the WAD's button. Pick a file with **Browse...**, paste the image's URL (right-click the image in your browser > Copy image address; it's downloaded when you save), or right-click the image > Copy image and press **Ctrl+V** in the Image box. Either way the image is stored in `GZDoom.Images.Location` under the batch's name. Some sites, like Doom Wiki, block downloads from apps — use Copy image + Ctrl+V for those.

If a batch file with the same name already exists, you'll be asked to confirm before it's overwritten. If you enter a full (rooted) path for IWAD or WAD that doesn't exist on disk, you'll also get a confirmation before the batch is created — relative paths aren't checked, since they're resolved by GZDoom at launch time, not by Doomer.

Both IWAD and WAD have a **Browse...** button to pick the file instead of typing the path.

If you just downloaded a WAD as an archive (`.zip`, `.rar` or `.7z`), use **From archive...** at the top of the form. It extracts the archive into its own folder inside the WADs directory named after it with each word capitalized (e.g. `D.O.O.M..rar` → `<WADs>\D.O.O.M\`, `ancient aliens.zip` → `<WADs>\Ancient Aliens\`) — if everything in the archive is inside a single folder, that folder's contents go straight into it — fills in WAD with the `.wad`/`.pk3`/`.pk7` it finds there (asking you to pick one if there are several), and, for a new batch, uses the folder name as the File Name. Before extracting, it shows the suggested name so you can keep it or type a different one.

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
