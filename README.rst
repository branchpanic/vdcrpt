video_corruptor
===============

An experimental tool for corrupting your favorite videos.

Usage
-----

.. warning:: By its nature, ``video_corruptor`` can and probably will generate loud audio with flashing imagery.

To use default settings::

    video_corruptor in.mp4 out.mp4 --iterations 15

Increase ``iterations`` to as high of a value as you dare. This value should be roughly proportional to the size of the
input video. Somewhere between 5-25 iterations works well as a starting point for a 10-second video.

.. warning:: Right now, ``video_corruptor`` loads the entire input video into memory.

FFmpeg might spit out some errors towards the end. As long as the program returns 0 -- which it might not always do --
the output was rendered successfully.

Overview
--------

``video_corruptor`` takes a video and converts it to a more "vulnerable" format (see Codecs below). It then relentlessly
trashes all but the first few bytes of header data to produce a strange and hopefully-entertaining result. The program
randomly chooses from a variety of effects: shifting chunks of data around, repeating them, performing arithmetic on
individual bytes, and more. It does this a user-specified number of times and, if the input survived, spits out a
slightly-less-broken MP4 for your viewing pleasure (note that timecodes can still get messed up pretty bad).

Codecs
~~~~~~

For best results, use a container and codec that are capable of handling a lot of abuse. ``video_corruptor`` doesn't
try to protect anything but the file header, so it's always possible for the final render to fail. 

By default, ``video_corruptor`` uses an AVI file with ``vp8`` for video and ``libvorbis`` for audio. Codecs can be set
with ``--temp_vc`` and ``--temp_ac`` respectively. Any encoders supported by your local build of FFmpeg are usable.
