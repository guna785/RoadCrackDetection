using DirectShowLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alturos.Yolo;
using System.IO;
using System.Drawing.Imaging;
using Alturos.Yolo.Model;

namespace RoadCrackDetection
{
    public partial class Form1 : Form
    {
        VideoCapture capture;
        Thread thread;
        private delegate void SafeCallDelegate(Mat text);

        private const double Thresh = 80;
        private const double ThresholdMaxVal = 255;
        private Mat detectedFace;
        public bool recognise = false, exitRecognise = false;
        private int _CameraIndex = 0;
        public string sub = "", message = "";
        List<KeyValuePair<int, string>> ListCamerasData = new List<KeyValuePair<int, string>>();
        public Form1()
        {
            InitializeComponent();
            DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            comboBox1.DataSource = null;
            comboBox1.Items.Clear();
            int _DeviceIndex = 0;
            foreach (DirectShowLib.DsDevice _Camera in _SystemCamereas)
            {
                ListCamerasData.Add(new KeyValuePair<int, string>(_DeviceIndex, _Camera.Name));
                comboBox1.Items.Add(_Camera.Name);
                _DeviceIndex++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            detectedFace = new Mat();
            //-> Get the selected item in the combobox
            var selected = comboBox1.SelectedItem.ToString();
            var key1 = ListCamerasData.Where(x => x.Value.Equals(selected)).FirstOrDefault();
            _CameraIndex = Convert.ToInt32(key1.Key);
            capture = VideoCapture.FromCamera(_CameraIndex);
            thread = new Thread(new ThreadStart(DoEvents));
            thread.Start();
        }
        private async void UpdatePictureBox(Mat mat)
        {
            // var haarCascade = new CascadeClassifier(Path.Combine(Application.StartupPath, "haarcascade_frontalface_default.xml"));

            pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            pictureBox1.Refresh();
            var configurationDetector = new YoloConfigurationDetector();
            var config = configurationDetector.Detect();
            using (var yolo = new YoloWrapper(config))
            {
                var stream = new MemoryStream();
                pictureBox1.Image.Save(stream, ImageFormat.Png);
                var items = yolo.Detect(stream.ToArray()).ToList();
                AddPictureBoxToRender(pictureBox1, items);
            }
            pictureBox2.Refresh();


        }
        void AddPictureBoxToRender(PictureBox picture, List<YoloItem> items)
        {
            var img = picture.Image;
            var graphics = Graphics.FromImage(img);
            var font = new Font("Arial", 50, FontStyle.Bold);
            var brush = new SolidBrush(Color.Red);
            foreach (var i in items)
            {
                var x = i.X;
                var y = i.Y;
                var width = i.Width;
                var height = i.Height;
                var rect = new Rectangle(x, y, width, height);
                var pen = new Pen(Color.Red, 6);
                var point = new System.Drawing.Point(x+width/2,y+height/2);
                graphics.DrawRectangle(pen, rect);
                graphics.DrawString(i.Type, font, brush,point);

            }
            pictureBox2.Image = img;

        }
        public void DoEvents()
        {
            try
            {
                Mat mat = new Mat();
                while (true)
                {
                    capture.Read(mat);
                    if (pictureBox1.Image != null)
                        pictureBox1.Image.Dispose();
                    SafeCallDelegate s = new SafeCallDelegate(UpdatePictureBox);

                    this.Invoke(s, mat);


                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
