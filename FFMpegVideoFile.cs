using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace WebM_Converter
{
	class FFMpegVideoFile : IVideoFile
	{
		readonly string _fileName;

		public TimeSpan Duration { get; private set; }

		public FFMpegVideoFile(string fileName)
		{
			_fileName = fileName;
			FillVideoInfo();
		}

		private void FillVideoInfo()
		{
			var probe = CreateInvisibleXmlFormattedFFProbeProcess($@"-show_format -i ""{_fileName}""");
			probe.Start();
			probe.WaitForExit(5000);
			string output = probe.StandardOutput.ReadToEnd();
			var doc = XDocument.Parse(output);
			Duration = TimeSpan.FromTicks((long)(decimal.Parse(doc.Element("ffprobe").Element("format").Attribute("duration").Value, CultureInfo.InvariantCulture) * TimeSpan.TicksPerSecond));
		}

		public BitmapImage GetFrame(TimeSpan time, int width, int height, string crop)
		{
			var ffmpeg = CreateInvisibleFFMpegProcess($@"-y -ss {time} -i ""{_fileName}"" -vframes 1 -vf crop={crop},scale={width}:{height} temp/preview.png");
			ffmpeg.StartInfo.RedirectStandardError = true;
			ffmpeg.Start();
			ffmpeg.WaitForExit();
			var a = ffmpeg.StandardError.ReadToEnd();

			BitmapImage preview = new BitmapImage();
			using (var fileStream = File.OpenRead("temp/preview.png"))
			{
				var memoryStream = new MemoryStream();
				fileStream.CopyTo(memoryStream);
				preview.BeginInit();
				preview.StreamSource = memoryStream;
				preview.EndInit();
			}
			return preview;
		}

		public Task Convert(string outputFileName, TimeSpan startTime, TimeSpan duration, int width, int height, string crop, long desiredFileSizeInBytes, bool enableAudio, long averageAudioBitrate)
		{
			return Task.Run(() =>
			{
				long averageTotalBitrate = (long)(desiredFileSizeInBytes * 8 / duration.TotalSeconds);
				long averageVideoBitrate = averageTotalBitrate - averageAudioBitrate;

				RunPass(outputFileName, startTime, duration, averageVideoBitrate, enableAudio, averageAudioBitrate, width, height, crop, 1, false);
				RunPass(outputFileName, startTime, duration, averageVideoBitrate, enableAudio, averageAudioBitrate, width, height, crop, 2, true);
			});
		}

		private void RunPass(string outputFileName, TimeSpan startTime, TimeSpan duration, long averageVideoBitrate, bool enableAudio, long averageAudioBitrate, int width, int height, string crop, int passNumber, bool isFinalPass)
		{
			var ffmpeg = isFinalPass
				? CreateVisibleFFMpegProcess($@"-y -ss {startTime} -t {duration} -i ""{_fileName}"" -pass {passNumber} -c:v libvpx-vp9 -b:v {averageVideoBitrate / 1000}k -threads {Environment.ProcessorCount}"
					+ $@" -speed 1 -tile-columns 6 -frame-parallel 1 -vf crop={crop},scale={width}:{height} -auto-alt-ref 1 -lag-in-frames 25 " + (enableAudio ? $"-b:a {averageAudioBitrate / 1000}k -c:a libvorbis" : "-an") + $@" ""{outputFileName}""")
				: CreateVisibleFFMpegProcess($@"-y -ss {startTime} -t {duration} -i ""{_fileName}"" -pass {passNumber} -c:v libvpx-vp9 -b:v {averageVideoBitrate / 1000}k -threads {Environment.ProcessorCount}"
					+ $@" -speed 4 -tile-columns 6 -frame-parallel 1 -vf crop={crop},scale={width}:{height} -an ""{outputFileName}""");
			ffmpeg.Start();
			ffmpeg.WaitForExit();
			//var a = ffmpeg.StandardError.ReadToEnd();
		}

		private Process CreateInvisibleFFMpegProcess(string additionalArguments)
		{
			var process = new Process();
			process.StartInfo.FileName = "ffmpeg";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.Arguments = "-v quiet -hide_banner " + additionalArguments;
			return process;
		}

		private Process CreateVisibleFFMpegProcess(string additionalArguments)
		{
			var process = new Process();
			process.StartInfo.FileName = "ffmpeg";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.Arguments = "-hide_banner " + additionalArguments;
			//process.StartInfo.RedirectStandardError = true;
			return process;
		}

		private Process CreateInvisibleXmlFormattedFFProbeProcess(string additionalArguments)
		{
			var process = new Process();
			process.StartInfo.FileName = "ffprobe.exe";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.Arguments = "-v quiet -hide_banner -of xml " + additionalArguments;
			return process;
		}
	}
}
