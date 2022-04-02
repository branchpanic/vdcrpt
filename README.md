# vdcrpt

Video corruptor. Note: can produce loud audio and flashing imagery.

Download releases at https://branchpanic.itch.io/vdcrpt.

## Release Instructions

*For development builds, use your IDE or the dotnet CLI. These steps are only
 necessary for producing self-contained archives.*

After installing the requirements below, run `inv clean dist`.

### Requirements

- dotnet 5.0
- python
	- invoke (`pip install invoke`)
- wget
- 7z (Windows, macOS)
- tar (Linux)

### Notes and Caveats

- macOS builds can only be built on macOS (no Apple Silicon support yet)
- Linux AppImage testing is minimal
	- More support will be given if there's sizeable demand
- Static FFmpeg builds are downloaded from:
	- Windows: https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z
	- macOS: https://evermeet.cx/ffmpeg/get
	- Linux: https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz
