# -*- mode: python ; coding: utf-8 -*-

block_cipher = None


a = Analysis(
    ["vdcrpt/gui.py"],
    pathex=[],
    binaries=[],
    datas=[("./vdcrpt/main.qml", "."), ("./vdcrpt/icon.png", ".")],
    hiddenimports=[],
    hookspath=[],
    runtime_hooks=[],
    excludes=['tkinter'],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=block_cipher,
    noarchive=False,
)

pyz = PYZ(a.pure, a.zipped_data, cipher=block_cipher)
exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name="vdcrpt",
    debug=True,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,
    icon="./vdcrpt/icon.ico",
)
