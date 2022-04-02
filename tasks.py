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


def _ensure_dir():
    if not Path("./Vdcrpt.sln").is_file():
        print("This script must be run from the root of the project.")
        sys.exit(1)


@task
def clean(c):
    _ensure_dir()
    c.run("dotnet clean")

    for script_dir in ("temp", "dist"):
        path = Path(script_dir)
        path.rmdir()
        path.mkdir(exist_ok=True)


@task
def fetch_ffmpeg(c, runtime="win-x64"):
    _ensure_dir()

    try:
        (url, ext) = FFMPEG_BUILDS[runtime]
    except KeyError:
        print("Unsupported runtime: {}".format(runtime))
        sys.exit(1)

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
        # xattr -r -d com.apple.quarantine <something>
        raise NotImplementedError(runtime)
    elif runtime == "linux-x64":
        raise NotImplementedError(runtime)


@task(fetch_ffmpeg)
def dist(c, runtime="win-x64"):
    _ensure_dir()

    try:
        tree = ET.parse("./src/Vdcrpt.Desktop/Vdcrpt.Desktop.csproj")
        version = tree.findtext(".//AssemblyVersion")
    except:
        print("Warning: could not determine version", file=sys.stderr)
        version = "UNKNOWN"

    print()
    print(f"Building vdcrpt {version} release archive for {runtime}")
    print()

    c.run(f"dotnet clean")

    if runtime == "win-x64":
        shutil.rmtree(f"dist/vdcrpt-win-x64", ignore_errors=True)
        c.run(
            f"dotnet publish ./src/Vdcrpt.Desktop -c Release -r win-x64 --self-contained true -o ./dist/vdcrpt-win-x64"
        )
        shutil.move(
            "dist/vdcrpt-win-x64/Vdcrpt.Desktop.exe", f"dist/vdcrpt-win-x64/vdcrpt.exe"
        )

        # Copy resources
        shutil.copy(f"./temp/ffmpeg-win-x64.exe", f"dist/vdcrpt-win-x64/ffmpeg.exe")
        shutil.copy(f"./LICENSE", f"dist/vdcrpt-win-x64/LICENSE.txt")
        shutil.copy(f"./README_USER.md", f"dist/vdcrpt-win-x64/README.txt")

        shutil.make_archive(f"dist/vdcrpt-win-x64", "zip", f"dist/vdcrpt-win-x64")

    elif runtime == "linux-x64":
        # c.run(f'dotnet clean')
        # c.run(f'dotnet publish ./src/Vdcrpt.Desktop -c Release -r linux-x64')
        # shutil.rmtree(f'dist/{runtime}', ignore_errors=True)
        raise NotImplementedError(runtime)

    elif runtime == "osx-x64":
        if sys.platform != "darwin":
            print(f"Builds for {runtime} are only supported on macOS.")
            sys.exit(1)

        # Use dotnet-bundle to generate an app bundle on macOS
        raise NotImplementedError(runtime)

    else:
        print(f"Unsupported runtime: {runtime}")
        sys.exit(1)
