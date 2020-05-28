import os
import sys

from PyQt5.QtGui import QGuiApplication, QIcon
from PyQt5.QtQml import QQmlApplicationEngine
from PyQt5.QtQuick import QQuickItem
from PyQt5.QtCore import QUrl, QThread, pyqtSignal, QObject

import vdcrpt
from vdcrpt.corruptor import corrupt_video
from vdcrpt.corruptions import stutter

if not os.getenv("QML_DISABLE_DISTANCEFIELD", None):
    os.environ["QML_DISABLE_DISTANCEFIELD"] = "1"

os.environ["QT_QUICK_CONTROLS_STYLE"] = "universal"

icon_path = "icon.png"
qml_path = "main.qml"

if getattr(sys, "frozen", False):
    icon_path = os.path.join(sys._MEIPASS, icon_path)
    qml_path = os.path.join(sys._MEIPASS, qml_path)

app = QGuiApplication(sys.argv)
app.setWindowIcon(QIcon(icon_path))
engine = QQmlApplicationEngine()
engine.load(QUrl.fromLocalFile(qml_path))
window = engine.rootObjects()[0]


window.setProperty("version", vdcrpt.__version__)


class RenderThread(QThread):
    render_succeeded = pyqtSignal(str)
    render_failed = pyqtSignal()

    def __init__(self):
        super().__init__()
        self.input_video = None
        self.chunk_size = 0
        self.stutter_min = 0
        self.stutter_max = 0
        self.iterations = 0
        self.output_video = None

    def run(self):
        try:
            corrupt_video(
                self.input_video,
                self.output_video,
                [stutter(self.chunk_size, (self.stutter_min, self.stutter_max))],
                iterations=self.iterations,
            )
            self.render_succeeded.emit(self.output_video)
        except:
            self.render_failed.emit()


def on_render_succeeded(output_video):
    window.setProperty("rendering", False)
    window.setProperty("statusText", f"Success! Wrote to {output_video}")


def on_render_failed():
    window.setProperty("rendering", False)
    window.setProperty("statusText", f"Failed, try again :(")


rt = RenderThread()
rt.render_succeeded.connect(on_render_succeeded)
rt.render_failed.connect(on_render_failed)


def start_corrupting(
    input_video: str,
    chunk_size: int,
    stutter_min: int,
    stutter_max: int,
    iterations: int,
    output_video: str,
):
    rt.input_video = input_video.toLocalFile()
    rt.chunk_size = chunk_size
    rt.stutter_min = stutter_min
    rt.stutter_max = stutter_max
    rt.iterations = iterations
    rt.output_video = output_video.toLocalFile()

    window.setProperty("statusText", f"Working...")
    rt.start()


window.startCorrupting.connect(start_corrupting)


def main():
    sys.exit(app.exec_())


if __name__ == "__main__":
    main()
