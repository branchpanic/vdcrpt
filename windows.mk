FFMPEG_BUILD=ffmpeg-4.2.2-win64-static-lgpl

all: vdcrpt.zip

vdcrpt.zip: dist/vdcrpt.exe dist/ffmpeg.exe
	7z a vdcrpt.zip ./dist/vdcrpt.exe ./dist/ffmpeg.exe ./README_USER.txt

$(FFMPEG_BUILD).zip:
	wget https://ffmpeg.zeranoe.com/builds/win64/static/$(FFMPEG_BUILD).zip

dist/ffmpeg.exe: $(FFMPEG_BUILD).zip
	7z e ./$(FFMPEG_BUILD).zip $(FFMPEG_BUILD)/bin/ffmpeg.exe -r -o./dist -y

dist/vdcrpt.exe:
	pyinstaller vdcrpt.spec
