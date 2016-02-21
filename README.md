WebM Converter
=========
![Screenshot](http://s22.postimg.org/nv82aq15t/yawmc.png)
Downloads
=========
The [release page](https://github.com/kosumosu/YetAnotherWebMConverter/releases) should have a build of the most recent version.

You're going to need [ffmpeg](http://ffmpeg.zeranoe.com/builds/).

You also need [.NET Framework 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49981)

Place Yawmc.exe and .dll files in the same directory as ffmpeg.exe

Documentation
=========

Start Time

Enter the start time in format hh:mm:ss.

	00:43:54.901341

Stop Time

Enter the stop time in hh:mm:ss.

	01:34:12.123

Target size

The desired size for the output in megabytes.

	2
	3.14

Audio

Enter the bitrate for audio in kilobits per second. Uncheck if you don't want sound.

	32
	192

Resolution

The resolution of the output file. Width and height. -1 scales the other size to keep the same aspect ratio, leave both as -1 to keep the input resolution.

	-1:720
	-1:1080
	1280:-1

Crop

This lets you crop a video\. The command looks like this, Width:Height:X:Y. Width is the width of the rectangle being cropped, height is the height, x and y are the coordinates of the rectangle being cropped. in_w and in_h grab the videos width and height respectively.

	500:500:10:10

Image Preview

This lets you preview the output video, very useful for checking cropping.
