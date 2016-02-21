using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WebM_Converter
{
	public interface IVideoFile
	{
		TimeSpan Duration { get; }

		BitmapImage GetFrame(TimeSpan time, int width, int height, string crop);

		Task Convert(string outputFileName, TimeSpan startTime, TimeSpan duration, int width, int height, string crop, long desiredFileSizeInBytes, bool enableAudio, long averageAudioBitrate);
	}
}