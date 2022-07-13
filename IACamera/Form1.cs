using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IACamera
{
    public partial class Form1 : Form
    {
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, byte> frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> Users = new List<string>();
        int Count, NumLabels, t;
        string name, names = null;

        private bool mouseDown;
        private Point lastLocation;

        public Form1()
        {
            InitializeComponent();

            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                Start();
                string Labelsinf = "";
                if (File.Exists(Application.StartupPath + "/Faces/Faces.txt")) 
                {
                    Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                }
              
                string[] Labels = Labelsinf.Split(',');
                NumLabels = Convert.ToInt16(Labels[0]);
                Count = NumLabels;

                string FacesLoad;
                for (int i = 1; i <= NumLabels; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    //trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/Faces.txt"));
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/" + FacesLoad);
                    labels.Add(Labels[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nothing in the Database");
                Console.WriteLine(ex.Message);
            }
        }

        private void Configure() 
        {
            try
            {
                int negative_counter = 1;
                int positive_counter = 1;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {


                    foreach (var item in openFileDialog1.FileNames)
                    {
                        string path = item.Substring(0, item.LastIndexOf("\\"));
                        System.IO.File.Move(item, string.Format("{0}\\negativo{1}.jpg", path, negative_counter));

                        negative_counter++;
                    }
                }

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {

                    foreach (var item in openFileDialog1.FileNames)
                    {
                        string path = item.Substring(0, item.LastIndexOf("\\"));
                        System.IO.File.Move(item, string.Format("{0}\\positivo{1}.jpg", path, positive_counter));

                        positive_counter++;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void txt_nome_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue == 13)
                {
                    btn_salvarFace.PerformClick();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                mouseDown = true;
                lastLocation = e.Location;
            }
            catch (Exception)
            {
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                mouseDown = false;
            }
            catch (Exception)
            {
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (mouseDown)
                {
                    this.Location = new Point(
                        (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                    this.Update();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Start() 
        {
            try
            {
                camera = new Capture(1);
                camera.QueryFrame();
                Application.Idle += Application_Idle;
            }
            catch (Exception ex)
            {
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                Users.Add("");
                frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                grayFace = frame.Convert<Gray, byte>();
                MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2,10,Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20,20));

                foreach (MCvAvgComp f in facesDetectedNow[0])
                {
                    result = frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100,100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    frame.Draw(f.rect, new Bgr(Color.Green), 2);

                    if (trainingImages.ToArray().Length != 0 )
                    {
                        MCvTermCriteria termCriteria = new MCvTermCriteria(Count, 0.001);
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriteria);
                        name = recognizer.Recognize(result);
                        frame.Draw(name, ref font, new Point(f.rect.X - 1 , f.rect.Y -1), new Bgr(Color.Red));
                        Console.WriteLine(name);
                    }

                    //Users[t - 1] = name;
                    Users.Add("");
                }

                CameraBox.Image = frame;
                names = "";
                Users.Clear();
            }
            catch (Exception)
            {

            }
        }

        private void btnSaveCLick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txt_name.Text)) 
                {
                    return;
                }
                Count = Count + 1;
                grayFace = camera.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                MCvAvgComp[][] detectionFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
                foreach (MCvAvgComp f in detectionFaces[0])
                {
                    TrainedFace = frame.Copy(f.rect).Convert<Gray, byte>();
                     break;
                }
                if (TrainedFace != null)
                {
                    TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    trainingImages.Add(TrainedFace);
                    labels.Add(txt_name.Text);
                    File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");
                    
                    for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                    {
                        trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                        File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", labels.ToArray()[i - 1] + ",");
                    }
                    
                    MessageBox.Show($"{txt_name.Text} Adicionado com sucesso.");
                }
                else 
                {
                    MessageBox.Show("Nenhum rosto detectado.");
                }

                TrainedFace = null;
            }
            catch (Exception)
            {
            }
        }

    }
}
