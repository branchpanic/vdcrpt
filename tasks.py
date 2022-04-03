# TODO: This is a mess

from invoke import task
import xml.etree.ElementTree as ET

import sys
import shutil
from pathlib import Path

FFMPEG_BUILDS = {
    "win-x64": (
        "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z",
        ".7z",
    ),
    "osx-x64": (
        "https://evermeet.cx/ffmpeg/get",
        ".7z",
    ),
    "linux-x64": (
        "https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz",
        ".tar.xz",
    ),
}


DIST_DIR = Path('./dist')
TEMP_DIR = Path('./temp')


def _get_system_runtime():
    platform = sys.platform

    if platform == 'win32':
        return 'win-x64'
    elif platform == 'darwin':
        return 'osx-x64'
    elif platform == 'linux':
        return 'linux-x64'

    return ''


def _ensure_dir():
    if not Path("./Vdcrpt.sln").is_file():
        print("This script must be run from the root of the project.")
        sys.exit(1)


@task
def clean(c):
    _ensure_dir()
    c.run(f"dotnet clean -r {_get_system_runtime()}")

    for script_dir in (DIST_DIR, TEMP_DIR):
        path.rmdir()
        path.mkdir(exist_ok=True)


@task
def fetch_ffmpeg(c, runtime=""):
    _ensure_dir()

    runtime = runtime or _get_system_runtime()

    try:
        (url, ext) = FFMPEG_BUILDS[runtime]
    except KeyError:
        print("Unsupported runtime: {}".format(runtime))
        sys.exit(1)

    Path('temp').mkdir(exist_ok=True)
    archive_path = Path(f"temp/ffmpeg-{runtime}").with_suffix(ext)
    binary_path = Path(f"temp/ffmpeg-{runtime}")
    if runtime == "win-x64":
        binary_path = binary_path.with_suffix(".exe")

    if binary_path.is_file():
        return

    if not archive_path.is_file():
        c.run(f"wget -O {archive_path} {url}")

    if runtime == "win-x64":
        c.run(f"7z e {archive_path} -otemp ffmpeg.exe -r -y")
        Path("temp/ffmpeg.exe").rename(binary_path)
    elif runtime == "osx-x64":
        c.run(f"7z e {archive_path} -otemp ffmpeg -r -y")
        Path("temp/ffmpeg").rename(binary_path)
    elif runtime == "linux-x64":
        with c.cd('temp'):
            c.run(f"tar --strip-components=1 -xvf {archive_path.relative_to('temp')} --wildcards \"*/ffmpeg\"")
        Path("temp/ffmpeg").rename(binary_path)


@task(fetch_ffmpeg)
def dist(c, runtime=""):
    _ensure_dir()

    runtime = runtime or _get_system_runtime()

    try:
        tree = ET.parse("./src/Vdcrpt.Desktop/Vdcrpt.Desktop.csproj")
        version = tree.findtext(".//Version")
    except:
        print("Warning: could not determine version", file=sys.stderr)
        version = "UNKNOWN"

    print()
    print(f"Building vdcrpt {version} release archive for {runtime}")
    print()

    c.run(f"dotnet clean -r {runtime}")

    if runtime == "win-x64":
        shutil.rmtree(f"dist/vdcrpt-win-x64", ignore_errors=True)
        c.run(
            f"dotnet publish ./src/Vdcrpt.Desktop -c Release -r win-x64 --self-contained true -o ./dist/vdcrpt-win-x64"
        )
        shutil.move(
            "dist/vdcrpt-win-x64/Vdcrpt.Desktop.exe", f"dist/vdcrpt-win-x64/vdcrpt.exe"
        )

        # Copy resources
        shutil.copy(f"./temp/ffmpeg-win-x64.exe",
                    f"dist/vdcrpt-win-x64/ffmpeg.exe")
        shutil.copy(f"./LICENSE", f"dist/vdcrpt-win-x64/LICENSE.txt")
        shutil.copy(f"./README_USER.md", f"dist/vdcrpt-win-x64/README.txt")

        shutil.make_archive(f"dist/vdcrpt-win-x64",
                            "zip", f"dist/vdcrpt-win-x64")

    elif runtime == "linux-x64":
        shutil.rmtree(f"dist/vdcrpt-linux-x64/vdcrpt.AppDir", ignore_errors=True)
        shutil.copytree("resources/vdcrpt.AppDir", "dist/vdcrpt-linux-x64/vdcrpt.AppDir")

        # Single file is probably unnecessary if we're building an AppImage
        c.run(
            f"dotnet publish ./src/Vdcrpt.Desktop -c Release -r linux-x64 -p:PublishSingleFile=false --self-contained true -o ./dist/vdcrpt-linux-x64/vdcrpt.AppDir"
        )

        shutil.copy(f"./temp/ffmpeg-linux-x64",
                    f"dist/vdcrpt-linux-x64/vdcrpt.AppDir/ffmpeg")
        shutil.copy(f"./LICENSE", f"dist/vdcrpt-linux-x64/LICENSE.txt")
        shutil.copy(f"./README_USER.md", f"dist/vdcrpt-linux-x64/README.txt")
        Path('dist/vdcrpt-linux-x64/vdcrpt.AppDir/Vdcrpt.Desktop').rename('dist/vdcrpt-linux-x64/vdcrpt.AppDir/vdcrpt')

        with c.cd('dist/vdcrpt-linux-x64'):
            # TODO: Get appimagetool
            c.run(f"~/appimagetool-x86_64.AppImage vdcrpt.AppDir")


    elif runtime == "osx-x64":
        shutil.rmtree(f"dist/vdcrpt-osx-x64", ignore_errors=True)
        if sys.platform != "darwin":
            print(f"Builds for {runtime} are only supported on macOS.")
            sys.exit(1)

        # Use dotnet-bundle to generate an app bundle on macOS
        c.run("dotnet restore ./src/Vdcrpt.Desktop -r osx-x64")

        dist_abs_path = DIST_DIR / 'vdcrpt-osx-x64'
        dist_abs_path.mkdir(parents=True, exist_ok=True)

        c.run(
            f"dotnet msbuild ./src/Vdcrpt.Desktop -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:UseAppHost=true -p:Configuration=Release")
        shutil.copytree(
            "./src/Vdcrpt.Desktop/bin/Release/net5.0/osx-x64/publish/vdcrpt.app", dist_abs_path / "vdcrpt.app")
        shutil.copy(f"./temp/ffmpeg-osx-x64",
                    f"dist/vdcrpt-osx-x64/vdcrpt.app/Contents/MacOS/ffmpeg")
        shutil.copy(f"./LICENSE", f"dist/vdcrpt-osx-x64/LICENSE.txt")
        shutil.copy(f"./resources/vdcrpt.icns",
                    f"dist/vdcrpt-osx-x64/vdcrpt.app/Contents/Resources/vdcrpt.icns")
        shutil.copy(f"./README_USER.md", f"dist/vdcrpt-osx-x64/README.txt")

        # Some kind of signature thing, not sure if this is right but Apple
        # won't let us run otherwise and I don't want to buy a developer
        # license. Copied from alacritty:
        # https://github.com/alacritty/alacritty/blob/49d64fbeecbdde2293ca0e7c7346941791685c3e/Makefile#L55
        c.run('codesign --remove-signature dist/vdcrpt-osx-x64/vdcrpt.app')
        c.run('codesign --force --deep --sign - dist/vdcrpt-osx-x64/vdcrpt.app')

        shutil.make_archive(f"dist/vdcrpt-osx-x64",
                            "zip", f"dist/vdcrpt-osx-x64")

    else:
        print(f"Unsupported runtime: {runtime}")
        sys.exit(1)
