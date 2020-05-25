import os
import sys

from PyQt5.QtGui import QGuiApplication, QIcon
from PyQt5.QtQml import QQmlApplicationEngine
from PyQt5.QtQuick import QQuickItem
from PyQt5.QtCore import QUrl, QThread, pyqtSignal, QObject

from vdcrpt.corruptor import corrupt_video
from vdcrpt.corruptions import *

if not os.getenv("QML_DISABLE_DISTANCEFIELD", None):
    os.environ["QML_DISABLE_DISTANCEFIELD"] = "1"

os.environ["QT_QUICK_CONTROLS_STYLE"] = "universal"

if getattr(sys, "frozen", False):
    dir_ = os.path.dirname(sys.executable)
else:
    dir_ = os.path.dirname(os.path.realpath(__file__))

app = QGuiApplication(sys.argv)
app.setWindowIcon(QIcon(os.path.join(dir_, "icon.png")))
engine = QQmlApplicationEngine()
engine.load(QUrl.fromLocalFile(os.path.join(dir_, "main.qml")))
window = engine.rootObjects()[0]


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
    input_video, chunk_size, stutter_min, stutter_max, iterations, output_video
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
