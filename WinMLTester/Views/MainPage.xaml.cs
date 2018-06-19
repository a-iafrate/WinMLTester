using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using WinMLTester.Models;

namespace WinMLTester.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private CustomVisionModel _model;
        private MediaPlayer mediaPlayer;

        private ObservableCollection<ResultModel> resultsList=new ObservableCollection<ResultModel>();
        public MainPage()
        {
            InitializeComponent();
            ListViewResults.ItemsSource = resultsList;
            //initModel();
            //initCamera();

        }

        private async void initCamera()
        {
            if (_model == null)
            {
                await initModel();
            }
            resultsList.Clear();
            CameraPreviewControl.Visibility = Visibility.Visible;
            ImagePreview.Visibility = Visibility.Collapsed;
            StopAll();
            await CameraPreviewControl.StartAsync();
            CameraPreviewControl.CameraHelper.FrameArrived += CameraPreviewControl_FrameArrived;
            
        }

        private void StopAll()
        {

        }

        public async Task initModel()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".onnx");
            StorageFile selectedStorageFile = await fileOpenPicker.PickSingleFileAsync();

            //TemporaryFix for debug
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sf2 = await selectedStorageFile.CopyAsync(storageFolder, selectedStorageFile.Name, NameCollisionOption.ReplaceExisting);
            // Load the model
            //StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///CustomVision.onnx"));
            try
            {
                _model = await CustomVisionModel.CreateCustomVisionModel(sf2);
                StackButtons.Visibility = Visibility.Visible;
            }catch(Exception ex)
            {
                StackButtons.Visibility = Visibility.Collapsed;
                new MessageDialog(ex.StackTrace,ex.Message).ShowAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_model == null)
            {
                await initModel();
            }
            resultsList.Clear();
            CameraPreviewControl.Visibility = Visibility.Collapsed;
            ImagePreview.Visibility = Visibility.Visible;
            StopAll();
            //ButtonRun.IsEnabled = false;

            //UIPreviewImage.Source = null;

            try

            {

                //if (_model == null)

                //{

                //    // Load the model

                //    await Task.Run(async () => await LoadModelAsync());

                //}



                // Trigger file picker to select an image file

                FileOpenPicker fileOpenPicker = new FileOpenPicker();

                fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

                fileOpenPicker.FileTypeFilter.Add(".bmp");

                fileOpenPicker.FileTypeFilter.Add(".jpg");

                fileOpenPicker.FileTypeFilter.Add(".png");

                fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

                StorageFile selectedStorageFile = await fileOpenPicker.PickSingleFileAsync();



                SoftwareBitmap softwareBitmap;

                using (IRandomAccessStream stream = await selectedStorageFile.OpenAsync(FileAccessMode.Read))

                {

                    // Create the decoder from the stream 

                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);



                    // Get the SoftwareBitmap representation of the file in BGRA8 format

                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                }



                // Display the image

                SoftwareBitmapSource imageSource = new SoftwareBitmapSource();

                await imageSource.SetBitmapAsync(softwareBitmap);

                ImagePreview.Source = imageSource;



                // Encapsulate the image in the WinML image type (VideoFrame) to be bound and evaluated

                VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);



                await Task.Run(async () =>

                {

                    // Evaluate the image

                    await EvaluateVideoFrameAsync(inputImage);

                });

            }

            catch (Exception ex)

            {

                Debug.WriteLine($"error: {ex.Message}");

               // await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusBlock.Text = $"error: {ex.Message}");

            }

            finally

            {

                //ButtonRun.IsEnabled = true;

            }
        }

        private async void CameraPreviewControl_FrameArrived(object sender, FrameEventArgs e)
        {
            var videoFrame = e.VideoFrame;
            var softwareBitmap = e.VideoFrame.SoftwareBitmap;
            var targetSoftwareBitmap = softwareBitmap;

            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    targetSoftwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(targetSoftwareBitmap);
                EvaluateVideoFrameAsync(inputImage);
                //await softwareBitmapSource.SetBitmapAsync(targetSoftwareBitmap);
            }
        }


        private async Task EvaluateVideoFrameAsync(VideoFrame frame)

        {
            

            if (frame != null)

            {

                try

                {
                    
                    //_stopwatch.Restart();

                    CustomVisionModelInput inputData = new CustomVisionModelInput();

                    inputData.data = frame;

                    var results = await _model.EvaluateAsync(inputData);

                    var loss = results.loss.Where(x=>x.Value>0.8).ToList();

                    if (loss.Count > 0)
                    {

                        var labels = results.classLabel;

                        //_stopwatch.Stop();

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                            () => {
                                //Get current image
                                Image m = new Image();
                        var source = new SoftwareBitmapSource();
                        source.SetBitmapAsync(frame.SoftwareBitmap);
                        //m.Source = source;

                        var lossStr =
                            loss.Select(l => new ResultModel()
                            {
                               Name = l.Key,
                                Percent =  l.Value * 100.0f,
                                Image = source
                            }).FirstOrDefault();

                        resultsList.Add(lossStr); });
                        //string message = $"Evaluation took {_stopwatch.ElapsedMilliseconds}ms to execute, Predictions: {lossStr}.";

                        //Debug.WriteLine(lossStr);
                    }
                    // await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusBlock.Text = message);

                }

                catch (Exception ex)

                {

                    Debug.WriteLine($"error: {ex.Message}");

                    //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => StatusBlock.Text = $"error: {ex.Message}");

                }



                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ButtonRun.IsEnabled = true);

            }

        }

        private async void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_model == null)
            {
                await initModel();
            }
            resultsList.Clear();
            CameraPreviewControl.Visibility = Visibility.Collapsed;
            ImagePreview.Visibility = Visibility.Visible;
            StopAll();

            FileOpenPicker fileOpenPicker = new FileOpenPicker();

            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            fileOpenPicker.FileTypeFilter.Add(".avi");

            fileOpenPicker.FileTypeFilter.Add(".mkv");
            fileOpenPicker.FileTypeFilter.Add(".mp4");


            StorageFile selectedStorageFile = await fileOpenPicker.PickSingleFileAsync();


            mediaPlayer = new MediaPlayer();
            mediaPlayer.Source = MediaSource.CreateFromStorageFile(selectedStorageFile);
            //mediaPlayer.VideoFrameAvailable += mediaPlayer_VideoFrameAvailable;
            mediaPlayer.IsVideoFrameServerEnabled = true;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            //mediaPlayer.Play();
            
        }
        bool mediaEnded = false;
        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            mediaEnded = true;
        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            mediaEnded = false;
            while (!mediaEnded)
            {
                await NextFrame();
            }
        }

        private async Task NextFrame()
        {
            mediaPlayer.StepForwardOneFrame();
            SoftwareBitmap frameServerDest = null;
            SoftwareBitmap sb = null;
           await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() =>
            {
                CanvasDevice canvasDevice = CanvasDevice.GetSharedDevice();
                frameServerDest = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)1200, (int)1200, BitmapAlphaMode.Premultiplied);
                var canvasImageSource = new CanvasImageSource(canvasDevice, (int)1200, (int)1200, DisplayInformation.GetForCurrentView().LogicalDpi);//96);


               
                using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameServerDest))
                using (CanvasDrawingSession ds = canvasImageSource.CreateDrawingSession(Windows.UI.Colors.Transparent))
                {

                    mediaPlayer.CopyFrameToVideoSurface(inputBitmap);
                    ds.DrawImage(inputBitmap);



                    frameServerDest = SoftwareBitmap.CreateCopyFromSurfaceAsync(inputBitmap, BitmapAlphaMode.Premultiplied).AsTask().Result;
                }
                ImagePreview.Source = canvasImageSource;


                
            });


            VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(frameServerDest);
            // Evaluate the image

            await EvaluateVideoFrameAsync(inputImage);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            initCamera();
        }

        private void ButtonLoadOnnx_Click(object sender, RoutedEventArgs e)
        {
            initModel();
        }
    }
}
