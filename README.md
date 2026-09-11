# GZDoom Batch Launcher (a.k.a. Doomer)

![Doomer.](/Doomer/Doomer.ico)</br>

Doomer is a small WinForms app I built while playing some Doom. I always wondered what it would look like to have a simple UI listing all of my <ins>.bat</ins> launchers as clickable icons, instead of digging through a folder — so here it is.

## Requirements

* Windows with the .NET 10 desktop runtime.
* [GZDoom](https://zdoom.org/downloads) installed somewhere on disk.
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
* **Icons**: size and spacing of the buttons in the main window.

## Using the app

The main window lists one button per batch file found in `GZDoom.Batchs.Location`, using the matching image from `GZDoom.Images.Location` (same file name, different extension) as its icon — falling back to plain text if no image is found. Use the search box in the menu to filter the list by name.

Right-click any WAD button to **Edit** or **Delete** its batch file.

## Adding a new batch file

Whenever you download a new WAD and want a launcher for it, use **Add Batch File** from the menu:

![Add Batch File Form](https://i.ibb.co/pjQzFDcG/Add-New-Batch-File.png)

* <ins>File Name</ins>: the name the batch file (and its button) will use. **Note**: it must match the name of the icon image for that WAD.
* <ins>IWAD</ins>: the full path to the IWAD your WAD was made from, **including its `.wad` extension**. For example, if your IWADs are under `D:\GZDoom\wads\doom2.wad`, enter `wads/doom2.wad`.
* <ins>WAD</ins>: the full path to the WAD itself, **without the `.wad` extension** — it's added automatically. For the Ancient Aliens WAD at `D:\GZDoom\wads\Ancient Aliens\aaliens.wad`, enter `wads/Ancient Aliens/aaliens`.
* <ins>Plugins (optional)</ins>: just the file name of an extra file you want loaded alongside the WAD (e.g. SmoothDoom, Corruption Cards, IDClever). It's combined with the `GZDoom.Plugins` folder from your settings, so don't include a path here.

If a batch file with the same name already exists, you'll be asked to confirm before it's overwritten.

The end result is a `.bat` file with a command like this:

```
"D:\gzdoom\gzdoom" -iwad "wads/doom2.wad" -file "wads/Ancient Aliens/aaliens.wad"
```

## Building and testing

```
dotnet build
dotnet test
```

`Doomer.Tests` covers the batch command building, parsing and file name validation logic in `Doomer/Services/BatchFileService.cs`.

## Roadmap

- [ ] Support multiple WAD/plugin files per batch.
