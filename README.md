# Animated Jacket

Play a looping MP4 in place of a song jacket on the music select screen.

Author: Mayu

## Requirements

- maimai DX (Sinmai) with [MelonLoader](https://melonwiki.xyz/) installed
- The mod file `AnimatedJacket.dll` in the game's `Mods` folder

The game executable folder looks like this:

```text
Package\
  Sinmai.exe
  Mods\
    AnimatedJacket.dll
  Sinmai_Data\
    StreamingAssets\
      A001\
        AssetBundleImages\
          jacket_animated\
            ui_jacket_001885_animated.mp4
```

## Install

1. Copy `AnimatedJacket.dll` into `Package\Mods`.
2. Start the game. MelonLoader loads the mod together with your other mods.
3. You do not need to edit `Assembly-CSharp.dll`.

## Add a video

Put the MP4 here:

```text
Sinmai_Data\StreamingAssets\A001\AssetBundleImages\jacket_animated\
```

`A000\AssetBundleImages\jacket_animated\` is also checked. If the same file exists in more than one option folder, the folder with the higher name is used (`A001` overrides `A000`).

Name the file after the jacket number, with six digits:

```text
ui_jacket_001885_animated.mp4
```

The jacket number is the number in the normal jacket file, `UI_Jacket_001885`. Song IDs of 10000 or higher use the last four digits of that ID. Song `11885` uses `ui_jacket_001885_animated.mp4`.

Restart the game after adding or replacing a video.

## What you will see

On the music select screen, select a song that has a matching MP4. The jacket on the enlarged center card, and the jacket in the song information panel, play that video on a loop.

The video has no sound, so the normal music preview is unchanged.

A song with no matching MP4 keeps its normal still jacket. Cards that are not the current selection also stay still.

## Example

| Song | File |
| --- | --- |
| False Amber (song ID 11885) | `ui_jacket_001885_animated.mp4` |
| チューリングの跡 (jacket 001388) | `ui_jacket_001388_animated.mp4` |
