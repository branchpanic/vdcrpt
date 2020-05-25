import math
import itertools
import random


def iterbytes(b: bytes):
    for i in range(len(b)):
        yield int(b[i])


def random_slice(b: bytearray):
    clip_start = random.randint(32, len(b))
    clip_length = random.randint(0, len(b) - clip_start)
    return (clip_start, clip_length, b[clip_start:clip_length])


def each_byte(f):
    """
    Creates a corruption that applies an arbitrary function to every byte of a random chunk.
    """

    def g(b):
        start, length, _ = random_slice(b)
        for i in range(start, start + length):
            if random.choice([True, False]):
                b[i] = int(f(b[i])) % 256

    return g


def mod(mod: int):
    """
    Creates a corruption that replaces a random chunk with each byte % a given value.
    """
    if mod <= 0 or mod > 256:
        raise ValueError("Modulo corruption must be in range (0, 256]")

    return each_byte(lambda b: b % mod)


def add(i: int):
    """
    Creates a corruption that replaces a random chunk with each byte + a given value (mod 256).
    """

    return each_byte(lambda b: b + i)


# A few more math effects, because why not?
log = each_byte(math.log)
sin = each_byte(math.sin)
tan = each_byte(math.tan)


def reverse(b):
    """
    A corruption that reverses a random chunk.
    """
    start, length, clip = random_slice(b)
    del b[start : start + length]
    b[start:start] = bytearray(reversed(clip))


def shift(length: int):
    """
    A corruption that shifts a random chunk from one place to another.
    """

    def f(b):
        start = random.randint(32, len(b))
        insert = random.randint(0, len(b) - length)

        clip = b[start : start + length]
        del b[start : start + length]
        b[insert:insert] = clip

    return f


def duplicate(length: int):
    """
    A corruption that duplicates a random chunk in another place.
    """

    def f(b):
        start = random.randint(32, len(b))
        insert = random.randint(0, len(b) - length)

        clip = b[start : start + length]
        b[insert:insert] = clip

    return f


def stutter(length: int, time_range: tuple):
    """
    Creates a corruption that stutters a random chunk a random amount of times (within time_range).
    """

    def f(b):
        start = random.randint(32, len(b))
        times = random.randint(*time_range)

        clip = b[start : start + length]
        for _ in range(times):
            b[start:start] = clip

    return f


def dilate(factor: int):
    """
    Creates a corruption that repeats each byte in a random chunk a given number of times.
    """

    if factor <= 0:
        raise ValueError("Dilation factor must be > 0")

    def f(b):
        start, length, clip = random_slice(b)
        del b[start : start + length]
        b[start:start] = bytearray(
            itertools.chain.from_iterable(
                itertools.repeat(i, factor) for i in iterbytes(clip)
            )
        )

    return f
