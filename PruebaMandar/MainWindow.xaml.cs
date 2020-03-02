using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using System.IO;

using WindowsInput;

namespace PruebaMandar
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int iNum = 0;
        KinectSensor Kinect;
        SkeletonPoint skAux, sKLH, sKRH;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("Ningun kinect detectado", "Visor de Posicion de Articulasion");
                Application.Current.Shutdown();
                return;
            }
            iNum = 0;
            Kinect = KinectSensor.KinectSensors[0];
            skAux = new SkeletonPoint();
            sKLH = new SkeletonPoint();
            sKRH = new SkeletonPoint();
        }
        void Kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs eventK)
        {
            using (ColorImageFrame frameImage = eventK.OpenColorImageFrame())
            {
                if (frameImage == null)
                    return;
                byte[] dataColor = new byte[frameImage.PixelDataLength];

                frameImage.CopyPixelDataTo(dataColor);
                Video.Source = BitmapSource.Create(
                    frameImage.Width,
                    frameImage.Height,
                    128,
                    128,
                    PixelFormats.Bgr32,
                    null,
                    dataColor,
                    frameImage.Width * frameImage.BytesPerPixel
                );
            }
        }
        void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            string sMensaje = "No hay datos de esqueleto";
            string sMensajeCaptura = "";
            Skeleton[] Skeletons = null;

            using (SkeletonFrame framesEsqueleto = e.OpenSkeletonFrame()) {
                if (framesEsqueleto != null) {
                    Skeletons = new Skeleton[framesEsqueleto.SkeletonArrayLength];
                    framesEsqueleto.CopySkeletonDataTo(Skeletons);
                }
            }

            if (Skeletons == null) return;

            foreach(Skeleton esqueleto in Skeletons){
                if (esqueleto.TrackingState == SkeletonTrackingState.Tracked) {

                    if (esqueleto.ClippedEdges == 0)
                    {
                        sMensajeCaptura = "Colocado Perfectamente";
                    }
                    else {
                        if ((esqueleto.ClippedEdges & FrameEdges.Bottom) != 0)
                            sMensajeCaptura += "Moverse mas arriba";
                        if ((esqueleto.ClippedEdges & FrameEdges.Top) != 0)
                            sMensajeCaptura += "Moverse mas abajo";
                        if ((esqueleto.ClippedEdges & FrameEdges.Right) != 0)
                            sMensajeCaptura += "Moverse mas a la izquierda";
                        if ((esqueleto.ClippedEdges & FrameEdges.Left) != 0)
                            sMensajeCaptura += "Moverse mas a la derecha";
                    }

                    Joint jointRH = esqueleto.Joints[JointType.HandRight];
                    Joint jointLH = esqueleto.Joints[JointType.HandLeft];
                    Joint jointRW = esqueleto.Joints[JointType.WristRight];
                    SkeletonPoint posRH = jointRH.Position;
                    SkeletonPoint posLH = jointLH.Position;
                    SkeletonPoint posRW = jointRW.Position;
                    sMensaje = string.Format("Right Hand: X:{0:0.0} Y:{1:0.0} Z:{2:0.0} \n"
                        + "Left Hand: X:{3:0.0} Y:{4:0.0} Z:{5:0.0} \n"
                        + "Right Wrist: X:{6:0.0.0} Y:{7:0.0.0} Z:{8:0.0.0}",
                        posRH.X, posRH.Y, posRH.Z,
                        posLH.X, posLH.Y, posLH.Z,
                        posRW.X, posRW.Y, posRW.Z
                        );
                    sKLH = posLH;
                    sKRH = posRH;
                }
            }
            ControlerLeft(sKLH);
            if(sKRH.Y > 0 && iNum > 10)
                InputSimulator.SimulateKeyDown(VirtualKeyCode.SPACE);
            skAux = sKLH;
            iNum++;
            SkeletonEstatus.Text = sMensaje;
            SkeletonCapture.Text = sMensajeCaptura;
        }
        private void ControlerLeft(SkeletonPoint posLH)
        {
            if (!(posLH.X > -0.11 && posLH.X < 0.11) && (posLH.Y < 0.11 && posLH.Y > -0.2) && iNum > 10)
            {
                
                if ((posLH.X - skAux.X) > 0 || posLH.X > 0.1)
                {
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.RIGHT);
                    //posLH.X = 0;
                }
                /*else*/ if ((posLH.X - skAux.X) < 0 || posLH.X < 0)
                {
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.LEFT);
                    //posLH.X = 0;
                }
                if (posLH.Y > 0)
                {
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.UP);
                    posLH.Y = 0;
                }
                /*else*/ if (posLH.Y < -0.2)
                {
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.DOWN);
                    posLH.Y = 0;
                }
                iNum=0;
            }
            
               // InputSimulator.SimulateKeyDown(VirtualKeyCode.CLEAR);    
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Kinect.SkeletonStream.Enable();
                Kinect.Start();
                Kinect.ColorStream.Enable();
            }
            catch
            {
                MessageBox.Show("Fallo en la Iniciacion de Kinect", "Visor de Posicion de Articulacion");
                Application.Current.Shutdown();
            }
            Kinect.ColorFrameReady += Kinect_ColorFrameReady;
            Kinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Kinect.Stop();
            Kinect.ColorStream.Disable();
            Kinect.SkeletonStream.Disable();
        }
    
    }

    
}
