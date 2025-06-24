# GZDoom Batch Launcher (a.k.a. Doomer)
![Doomer.](/Doomer/Doomer.ico)</br>
This is a small proyect I've made while playing some Doom. I've always wandered whay would happen if I had a simple UI with all of my <ins>.bat</ins> listed and can choose everything I wanted, so this.
The main configuration starts with the <ins>appsettings.json</ins>.

<pre csharp>{
  "GZDoom": {
    "Location": "D:\\gzdoom\\gzdoom",
    "Plugins":  "plugins",
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
} </pre>

You can customize the icons, and everything related to GZDoom, starting with the GZDoom location, the plugins you have (just some extra files you wanted to use, SmoothDoom, Corruption Cards, for example) and also the Batchs and Images location and extension.

## Add new Batch File

Another thing I got accross was whenever I downloaded a new wad and wanted to make the new batch file for that file. I came across with this solution:

![Add Batch File Form](https://i.ibb.co/pjQzFDcG/Add-New-Batch-File.png)

Those four text boxes can be completed as follow:
* <ins>File Name</ins>: The name of the batch file you want your WAD to be presented. NOTE: Has to be the same as your image file for the button.
* <ins>IWad</ins>: The full path for the IWAD your WAD was made from. For example, my wads are on D:\GZDoom\wads\doom2.wad, so the result should be wads/doom2.wad.
* <ins>Wad</ins>: The WAD itself, like the IWads has to have the same full path. For the Ancient Aliens, the result should be wads/Ancient Aliens/aaliens.wad.
  - [ ] Add option for multiple files.
* <ins>Plugins (OPTIONAL)</ins>: I've put this one just in cases you needed extra files, for example SmoothDoom, Corruption Cards or IDClever.
  - [ ] Add option for multiple files.

 The end result should be something like this:
 <pre>D:\gzdoom\gzdoom -iwad "wads/doom2" -file "wads/Ancient Aliens/aaliens.wad"</pre>
