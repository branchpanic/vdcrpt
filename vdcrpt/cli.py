import fire

from .corruptor import corrupt_video
from .corruptions import stutter


def main(
    in_file: str,
    out_file: str,
    chunk_size: int = 1000,
    stutter_min: int = 10,
    stutter_max: int = 90,
    iterations: int = 10,
):
    corrupt_video(
        in_file, out_file, [stutter(chunk_size, (stutter_min, stutter_max))], iterations
    )


if __name__ == "__main__":
    fire.Fire(main)
