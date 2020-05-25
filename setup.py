from cx_Freeze import setup, Executable
from os.path import dirname, join
import PyQt5

pyqt_dir = dirname(PyQt5.__file__)


# Dependencies are automatically detected, but it might need
# fine tuning.
buildOptions = dict(
    packages=[],
    excludes=["fire", "tkinter"],
    include_files=["vdcrpt/main.qml", "vdcrpt/icon.png"],
    includes=["PyQt5", "ffmpeg"],
)

base = "Win32GUI"

executables = [
    Executable("vdcrpt/gui.py", base=base, targetName="vdcrpt", icon="vdcrpt/icon.ico")
]

setup(
    name="vdcrpt",
    version="0.1.0",
    description="",
    options=dict(build_exe=buildOptions),
    executables=executables,
)
