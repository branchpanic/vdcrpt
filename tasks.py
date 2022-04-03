#
# Run `inv dist` to create an archivev on the system you want to deploy for.
# Output will be placed in `dist/`.
#
# Format this file with `black` (`black tasks.py`).
#
# This isn't necessary for development builds. For that, use the dotnet CLI or your
# IDE.
#
# TODO: Maybe msbuild can do more of this.
#

from invoke import task  # pip install invoke
from pathlib import Path
import sys
import xml.etree.ElementTree as ET
import shutil

TEMP = Path("temp")
DIST = Path("dist")
SRC = Path("src")
RESOURCES = Path("resources")

SLN_FILE = Path("Vdcrpt.sln")
PROJECT_DIR = SRC / "Vdcrpt.Desktop"
CSPROJ_FILE = PROJECT_DIR / "Vdcrpt.Desktop.csproj"


def check_dir():
    if not SLN_FILE.is_file():
        print("This script must be run from the root of the project.")
        sys.exit(1)


def get_version():
    try:
        tree = ET.parse(CSPROJ_FILE)
        return tree.findtext(".//Version")
    except:
        print("Warning: could not determine version", file=sys.stderr)
        return "unknown"


def guess_runtime():
    platform = sys.platform

    # TODO: arm64?

    if platform == "win32":
        return "win-x64"
    elif platform == "darwin":
        return "osx-x64"
    elif platform == "linux":
        return "linux-x64"
    else:
        raise NotImplementedError(f"Couldn't determine runtime for platform {platform}")


FFMPEG_BUILDS = {
    "win-x64": ("https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-essentials.7z", "7z"),
    "osx-x64": ("https://evermeet.cx/ffmpeg/get", "7z"),
    "linux-x64": ("https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz", "tar.xz"),
}

def fetch_ffmpeg(c, runtime: str) -> str:
    work_path = TEMP / runtime
    work_path.mkdir(parents=True, exist_ok=True)

    ffmpeg_path = work_path / "ffmpeg"
    if runtime == "win-x64":
        ffmpeg_path = ffmpeg_path.with_suffix(".exe")

    if ffmpeg_path.is_file():
        print('Reusing existing FFmpeg at', ffmpeg_path)
        return ffmpeg_path

    url, ext = FFMPEG_BUILDS[runtime]
    archive_name = f"ffmpeg-{runtime}.{ext}"

    with c.cd(work_path):  # only applies to c.run
        if not (work_path / archive_name).is_file():
            print_step(f"Downloading {archive_name} from {url}")
            c.run(f"wget {url} -O {archive_name}")

        if runtime == "win-x64":
            c.run(f"7z e {archive_name} ffmpeg.exe -r -y")

        elif runtime == "osx-x64":
            c.run(f"7z e {archive_name} ffmpeg -r -y")

        elif runtime == "linux-x64":
            c.run(
                f'tar --strip-components=1 -xvf {archive_name} --wildcards "*/ffmpeg"'
            )

        else:
            raise NotImplementedError(f"Unsupported runtime: {runtime}")

    assert ffmpeg_path.is_file(), "ffmpeg not extracted to expected path"
    return ffmpeg_path


@task
def clean(c):
    c.run("dotnet clean -p:PublishSelfContained=false")

    for script_path in (TEMP, DIST):
        shutil.rmtree(script_path, ignore_errors=True)
        script_path.mkdir(parents=True, exist_ok=True)


def print_step(msg):
    print()
    print(">>>", msg)
    print()


@task
def dist(c, runtime=""):
    check_dir()

    runtime = runtime or guess_runtime()
    version = get_version()

    print(f"Building vdcrpt {version} release archive for {runtime}")

    dist_path = DIST / f"vdcrpt-{version}-{runtime}"
    shutil.rmtree(dist_path, ignore_errors=True)
    dist_path.mkdir(parents=True, exist_ok=True)

    print_step("Fetching FFmpeg")
    ffmpeg_path = fetch_ffmpeg(c, runtime)

    print_step(f"Building for {runtime}")
    if runtime == "win-x64":
        shutil.rmtree(f"dist/vdcrpt-win-x64", ignore_errors=True)
        c.run(
            f"dotnet publish {PROJECT_DIR} "
            "-c Release "
            "-r win-x64 "
            "--self-contained true "
            f"-o {dist_path}"
        )

        shutil.copy(ffmpeg_path, dist_path / "ffmpeg.exe")

    elif runtime == "linux-x64":
        appdir_path = TEMP / "vdcrpt.AppDir"
        shutil.rmtree(appdir_path, ignore_errors=True)

        shutil.copytree(RESOURCES / "vdcrpt.AppDir", appdir_path)

        # FIXME: On a case-insensitive filesystem, "Vdcrpt.Desktop" (the program assembly)
        #   clobbers "vdcrpt.desktop" (the FreeDesktop .desktop file). We'll rename the assembly
        #   eventually. For now, a workaround.
        shutil.move(appdir_path / "vdcrpt.desktop", appdir_path / "vdcrpt.desktop_")

        c.run(
            f"dotnet publish {PROJECT_DIR} "
            "-c Release "
            "-r linux-x64 "
            "-p:PublishSingleFile=false "
            "--self-contained true "
            f"-o {appdir_path}"
        )

        shutil.move(appdir_path / "Vdcrpt.Desktop", appdir_path / "vdcrpt")
        shutil.move(appdir_path / "vdcrpt.desktop_", appdir_path / "vdcrpt.desktop")

        shutil.copy(ffmpeg_path, appdir_path / "ffmpeg")

        print_step('Creating AppImage')

        appimagetool = TEMP / "appimagetool-x86_64.AppImage"
        if not appimagetool.is_file():
            appimagetool_url = "https://github.com/AppImage/AppImageKit/releases/download/13/appimagetool-x86_64.AppImage"
            print('Downloading AppImage tool from', appimagetool_url)
            c.run(f"wget -O {appimagetool} {appimagetool_url}")
            c.run(f"chmod +x {appimagetool}")

        appimage_path = dist_path / f"vdcrpt-{version}-{runtime}.AppImage"
        c.run(f"ARCH=x86_64 {appimagetool} {appdir_path} {appimage_path}")
        c.run(f"chmod +x {appimage_path}")

    elif runtime == "osx-x64":
        c.run(f"dotnet restore {PROJECT_DIR} -r osx-x64")
        c.run(
            f"dotnet msbuild {PROJECT_DIR} "
            "-t:BundleApp "
            "-p:RuntimeIdentifier=osx-x64 "
            "-p:UseAppHost=true "
            "-p:Configuration=Release"
        )

        bundle_path = dist_path / "vdcrpt.app"
        shutil.copytree(
            PROJECT_DIR / "bin/Release/net5.0/osx-x64/publish/vdcrpt.app",
            bundle_path,
        )

        shutil.copy(ffmpeg_path, bundle_path / "Contents/MacOS/ffmpeg")
        shutil.copy(
            RESOURCES / "vdcrpt.icns",
            bundle_path / "Contents/Resources/vdcrpt.icns",
        )

        print_step('Signing .app')

        c.run(f"codesign --remove-signature {bundle_path}")
        c.run(f"codesign --force --deep --sign - {bundle_path}")

    else:
        print(f"Unsupported runtime: {runtime}")
        sys.exit(1)

    print_step("Copying resources")
    shutil.copy("LICENSE", dist_path / "LICENSE.txt")
    shutil.copy("README_USER.md", dist_path / "README.txt")

    print_step("Creating archive")
    shutil.make_archive(dist_path, "zip", dist_path)

    print()
    print("Done! Archive created at", dist_path)
