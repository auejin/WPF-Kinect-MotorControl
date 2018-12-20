//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Input;
    using Microsoft.Kinect;
    using System.Windows.Controls;
    using System.Windows.Media.Media3D;
    using System.IO.Ports; // for serial port connection with arduino

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// serial port to connect with motor controller (arduino)
        /// </summary>
        private SerialPort motorCotrollerPort;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            motorCotrollerPort = new SerialPort("COM7", 9600); // com7 : 우측 위에 포트.
            try
            {
                motorCotrollerPort.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot Open Port!!");
            }
            
        }

        private double motor_angle;

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                
                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

                // 모터가 돌아가 있는 초기 x 각도값을 0으로 정의함.
                this.motor_angle = 0;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        ///     누른 이미지 내 좌표로부터 depth 값(단위:mm)을 반환
        ///     
        ///     문제 : 화면에는 colorframe의 결과가 떠야 한다.
        ///     문제 : 화각이 너무 좁다(60도)는거다. 카메라 밖으로 화면 가려질 가능성 농후. tilt는 up and down만 가능하다.
        ///     문제 : depth가 좌우 반전됨. -> 구한 alpha값에 -1 곱함
        ///     스펙 참고 : https://www.slideshare.net/MatteoValoriani/introduction-to-kinect-update-v-18
        /// </summary>
        private void ImageMouseDown(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(sender as Image);
                int x = (int)Math.Floor(position.X);
                int y = (int)Math.Floor(position.Y);
                int width = this.colorBitmap.PixelWidth;
                int height = this.colorBitmap.PixelHeight;

                short depth = depthPixels[width*y+x].Depth; // this.sensor.DepthStream.FramePixelDataLength == 640 * 480
                double new_motor_angle = (180 / Math.PI) * AngleFromMousePosition(position, depth) * -1; // depth camera 좌우가 반전되어 -1를 곱함.

                double angle_diff_deg = new_motor_angle - motor_angle; // 얘(angle_diff)를 모터로 전송해야 한다. angle_diff>0면 x+방향이므로 CW방향

                //SendPortString(motorCotrollerPort, TranslateGara(position));
                SendPortString(motorCotrollerPort, TranslateAngleToString(angle_diff_deg));
                motor_angle = new_motor_angle;

                string print_string = "point : (" + x + ", " + y + ")\n" +
                    "depth : " + depth.ToString() + "mm" + "\n" +
                    "angle_diff  : " + angle_diff_deg.ToString() + "\n" +   /// 모터가 돌려야 하는 각도
                    "motor_angle : " + motor_angle.ToString() + "\n";       /// 모터가 위치해야 하는 각도
                MessageBox.Show(print_string);
            }
        }

        private string TranslateGara(Point k)
        {
            if(k.X >= this.colorBitmap.PixelWidth/2)
            {
                return "4 3 50";
            }
            else
            {
                return "4 2 50";
            }
        }

        private string TranslateAngleToString(double degree)
        {
            int tic = (int) Math.Floor(Math.Abs(degree / 1.8))*50/4;
            string direction = (degree >= 0)  ? "3" : "2";
            
            return "4 "+direction+" "+tic.ToString();
        }

        /// <summary>
        ///     send string to designated port
        /// </summary>.
        private void SendPortString(SerialPort port, String str)
        {
            MessageBox.Show(str);
            port.WriteLine(str);
        }

        /// <summary>
        ///     중심점으로 부터 x방향의 각도차이(radian)값을 반환
        /// </summary>.
        private double AngleFromMousePosition(Point position, short depth)
        {
            const double MAX_ALPHA = 57  * (Math.PI / 180);    // kinect v1 기준 최대 width angle 
            const double MAX_BETA = 43.5 * (Math.PI / 180); // kinect v1 기준 최대 height angle 

            position.X = position.X - this.colorBitmap.PixelWidth / 2;
            position.Y = position.Y - this.colorBitmap.PixelHeight / 2;

            double alpha = Math.Atan(position.X * Math.Tan(MAX_ALPHA / 2) / this.colorBitmap.PixelWidth / 2);
            double beta = Math.Atan(position.Y * Math.Tan(MAX_BETA / 2) / this.colorBitmap.PixelHeight / 2);
            Vector3D absolute_location = new Vector3D(depth * Math.Tan(alpha), depth * Math.Tan(beta), depth);
            return alpha;
        }
        

        /// <summary>
        ///     Vector3D 사이의 두 각도를 double로 반환
        ///     Vector3D sample = new Vector3D(0, 0, 0);
        /// </summary>
        private double AngleBetweenTwoVectors(Vector3D vectorA, Vector3D vectorB)
        {
            double dotProduct = 0.0;
            vectorA.Normalize();
            vectorB.Normalize();
            dotProduct = Vector3D.DotProduct(vectorA, vectorB);

            return (double)Math.Acos(dotProduct) / Math.PI * 180;
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthPixels[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Write out blue byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out green byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.colorPixels[colorPixelIndex++] = intensity;
                                                
                        // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                        // If we were outputting BGRA, we would write alpha here.
                        ++colorPixelIndex;
                    }

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private void Window_closing()
        {
            motorCotrollerPort.Close();
            MessageBox.Show("Serial Port Closed");
        }
    }
}