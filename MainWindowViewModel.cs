using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WebM_Converter
{
	public enum SubtitlesMode
	{
		None,
		Internal,
		External
	}

	public class MainWindowViewModel : ViewModelBase
	{
		readonly Func<string, IVideoFile> _videoFileFactory;

		IVideoFile _videoFile;

		private string _sourceVideoFileName = string.Empty;
		private string _subtitlesFileName = string.Empty;
		private string _destinationVideoFileName = string.Empty;
		private TimeSpan _startTime = TimeSpan.Zero;
		private TimeSpan _endTime = TimeSpan.Zero;
		private decimal _targetSizeInMegabytes = 20m;
		private bool _enableAudio = true;
		private int _audioBitrate = 96;
		private int _outputVideoWidth = -1;
		private int _outputVideoHeight = -1;
		private string _crop = "in_w:in_h:0:0";
		private double _previewRelativePosition = 0.0;
		private BitmapImage _previewImage = null;
		private bool _isBusy = false;

		public MainWindowViewModel(Func<string, IVideoFile> videoFileFactory)
		{
			_videoFileFactory = videoFileFactory;
		}

		public string SourceFileName
		{
			get { return _sourceVideoFileName; }
			private set
			{
				Set(() => SourceFileName, ref _sourceVideoFileName, value);
			}
		}

		public string SubtitlesFileName
		{
			get { return _subtitlesFileName; }
			private set
			{
				Set(() => SubtitlesFileName, ref _subtitlesFileName, value);
			}
		}

		public string DestinationFileName
		{
			get { return _destinationVideoFileName; }
			private set
			{
				Set(() => DestinationFileName, ref _destinationVideoFileName, value);
			}
		}

		public TimeSpan StartTime
		{
			get { return _startTime; }

			set
			{
				Set(() => StartTime, ref _startTime, value);
			}
		}

		public TimeSpan EndTime
		{
			get { return _endTime; }

			set
			{
				Set(() => EndTime, ref _endTime, value);
			}
		}

		public decimal TargetSizeInMegabytes
		{
			get { return _targetSizeInMegabytes; }

			set
			{
				Set(() => TargetSizeInMegabytes, ref _targetSizeInMegabytes, value);
			}
		}


		public bool EnableAudio
		{
			get { return _enableAudio; }

			set
			{
				Set(() => EnableAudio, ref _enableAudio, value);
			}
		}

		public int AudioBitrate
		{
			get { return _audioBitrate; }

			set
			{
				Set(() => AudioBitrate, ref _audioBitrate, value);
			}
		}

		public int OutputVideoWidth
		{
			get { return _outputVideoWidth; }

			set
			{
				Set(() => OutputVideoWidth, ref _outputVideoWidth, value);
				UpdatePreview();
			}
		}

		public int OutputVideoHeight
		{
			get { return _outputVideoHeight; }

			set
			{
				Set(() => OutputVideoHeight, ref _outputVideoHeight, value);
				UpdatePreview();
			}
		}

		public string Crop
		{
			get { return _crop; }

			set
			{
				Set(() => Crop, ref _crop, value);
				UpdatePreview();
			}
		}

		public double PreviewRelativePosition
		{
			get { return _previewRelativePosition; }

			set
			{
				if (_previewRelativePosition == value)
					return;

				_previewRelativePosition = value;
				RaisePropertyChanged(() => PreviewRelativePosition);
				RaisePropertyChanged(() => PreviewTimeStamp);
				UpdatePreview();
			}
		}

		public TimeSpan PreviewTimeStamp
		{
			get
			{
				return _videoFile != null
				  ? TimeSpan.FromSeconds(_videoFile.Duration.TotalSeconds * _previewRelativePosition)
				  : TimeSpan.Zero;
			}
		}

		public BitmapImage PreviewImage
		{
			get { return _previewImage; }

			private set
			{
				Set(() => PreviewImage, ref _previewImage, value);
			}
		}

		private RelayCommand _selectInputFileCommand;
		public RelayCommand SelectInputFileCommand
		{
			get
			{
				return _selectInputFileCommand != null
					? _selectInputFileCommand
					: _selectInputFileCommand = new RelayCommand(
						() =>
						{
							var dialog = new OpenFileDialog();
							if (dialog.ShowDialog() != true)
								return;
							SetSourceFile(dialog.FileName);
						}
						)
				;
			}
		}

		private RelayCommand _selectOutputFileCommand;
		public RelayCommand SelectOutputFileCommand
		{
			get
			{
				return _selectOutputFileCommand != null
					? _selectOutputFileCommand
					: _selectOutputFileCommand = new RelayCommand(
						() =>
						{
							var dialog = new SaveFileDialog();
							if (dialog.ShowDialog() != true)
								return;

							DestinationFileName = dialog.FileName;
						}
						)
				;
			}
		}


		private RelayCommand _selectSubtitlesFileCommand;
		public RelayCommand SelectSubtitlesFileCommand
		{
			get
			{
				return _selectSubtitlesFileCommand != null
					? _selectSubtitlesFileCommand
					: _selectSubtitlesFileCommand = new RelayCommand(
						() =>
						{
							var dialog = new OpenFileDialog();
							if (dialog.ShowDialog() != true)
								return;

							SubtitlesFileName = dialog.FileName;
						}
						)
				;
			}
		}

		private RelayCommand _convertCommand;
		public RelayCommand ConvertCommand
		{
			get
			{
				return _convertCommand != null
					? _convertCommand
					: _convertCommand = new RelayCommand(
						() =>
						{
							IsBusy = true;
							Task.Run(async () =>
								await _videoFile.Convert(_destinationVideoFileName, _startTime, _endTime - _startTime, _outputVideoWidth, _outputVideoHeight, _crop, (long)(_targetSizeInMegabytes * 1024 * 1024), _enableAudio, _audioBitrate * 1000)
									.ContinueWith(task => IsBusy = false)
							);
						},
						() => !IsBusy && _videoFile != null
						)
				;
			}
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				Set(() => IsBusy, ref _isBusy, value);
				_convertCommand.RaiseCanExecuteChanged();
			}
		}

		private void SetSourceFile(string fileName)
		{
			SourceFileName = fileName;
			_videoFile = _videoFileFactory(SourceFileName);

			StartTime = TimeSpan.Zero;
			EndTime = _videoFile.Duration;
			PreviewRelativePosition = 0.0;
			_convertCommand.RaiseCanExecuteChanged();
			UpdatePreview();
		}


		private void UpdatePreview()
		{
			if (_videoFile == null)
				return;

			PreviewImage = _videoFile.GetFrame(PreviewTimeStamp, _outputVideoWidth, _outputVideoHeight, _crop);
		}
	}
}
