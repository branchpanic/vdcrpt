import os
import random
import tempfile
import hashlib

import ffmpeg
from subprocess import call


def _hexdigest(filename: str):
    with open(filename, "rb") as f:
        file_hash = hashlib.blake2b()
        while chunk := f.read(8192):
            file_hash.update(chunk)
    return file_hash.hexdigest()


def corrupt_video(
    in_file: str,
    out_file: str,
    effect_pool: list,
    iterations: int = 10,
    temp_vc: str = "mpeg4",
    temp_ac: str = "libvorbis",
    cache_avi: bool = True,
):
    in_fn = os.path.splitext(os.path.basename(in_file))[0]
    avi_fn = os.path.join(
        tempfile.gettempdir(), f"{_hexdigest(in_file)}_{temp_vc}_{temp_ac}.avi"
    )
    temp_fn = os.path.join(tempfile.gettempdir(), f"{in_fn}_vdcrpt_temp.avi")

    if not os.path.isfile(avi_fn):
        ffmpeg.input(in_file).output(
            avi_fn, vcodec=temp_vc, acodec=temp_ac
        ).overwrite_output().run()

    b = bytearray()
    with open(avi_fn, "rb") as fp:
        b.extend(fp.read())

    for i in range(iterations):
        random.choice(effect_pool)(b)

    with open(temp_fn, "wb") as fp:
        fp.write(b)

    ffmpeg.input(temp_fn).output(out_file).overwrite_output().run()

    if not cache_avi:
        os.remove(avi_fn)
    os.remove(temp_fn)
