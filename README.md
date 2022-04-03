# vdcrpt

Video corruptor. Note: can produce loud audio and flashing imagery.

Download releases at https://branchpanic.itch.io/vdcrpt.

## Release Instructions

*For development builds, use your IDE or the dotnet CLI. These steps are only
 necessary for producing self-contained archives.*

After installing the requirements below, run `inv clean dist` on the platform
you want to deploy for.

### Requirements

- dotnet 5.0
- python
	- invoke (`pip install invoke`)
- wget
- 7z (Windows, macOS)
- tar (Linux)

Other dependencies (ffmpeg, AppImage tool) will be downloaded to the temp/
directory.

Static FFmpeg builds are downloaded from:
- Windows: https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z
- macOS: https://evermeet.cx/ffmpeg/get
- Linux: https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz
