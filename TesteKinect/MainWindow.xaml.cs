﻿using System;
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
using AuxiliarKinect2.FuncoesBasicas;

namespace TesteKinect
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;


        /**
         * Regras da programação
         */
        bool MaoDireitaAcimaCabeca, MaoEsquerdaAcimaCabeca;




        /**
         * Rastreamento do Esqueleto
         */
        byte[] info_cores_sensor_kinect = null;
        WriteableBitmap bmp_rgb_cores = null;


        public MainWindow()
        {
            InitializeComponent();

            InicializarSensor();
        }

        private void InicializarSensor()
        {
            // Inicializando o kinect no angulo 0.
            kinect = InicializadorKinect.InicializarPrimeiroSensor(0);
            kinect.SkeletonStream.Enable();

            
            // Vinculo dos eventos que queremos reconhecer.
            kinect.SkeletonFrameReady += KinectEvent;


            // Vinculando aos eventos para exeibir o esqueleto do usuário na tela de "espelho" canvas
            kinect.ColorStream.Enable();
            kinect.SkeletonFrameReady += etecnect_SkeletonFrameReady;
            kinect.ColorFrameReady += etecnect_ColorFrameReady;

        }


        /**
         * Função para Ajustar o angulo do Kinect
         */ 
        private void AtualizarValores()
        {
            kinect.ElevationAngle = Convert.ToInt32(slider.Value);
            label.Content = kinect.ElevationAngle;
        }

        private void slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            AtualizarValores();
        }

        /**
         * Vinculando  os eventos para que o kinect dispare as mensagens.
         */ 
        private void KinectEvent(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame quadroAtual = e.OpenSkeletonFrame()) {
                if (quadroAtual != null) { 
                    ExecutarRegraMaoDireitaAcimaDaCabeca(quadroAtual);
                    ExecutarRegraMaoEsquerdaAcimaDaCabeca(quadroAtual);
                } 
            }
        }


        private void ExecutarRegraMaoDireitaAcimaDaCabeca(SkeletonFrame quadroAtual)
        {
            Skeleton[] esqueletos = new Skeleton[6];

            quadroAtual.CopySkeletonDataTo(esqueletos);
            Skeleton usuario = esqueletos.FirstOrDefault(esqueleto => esqueleto.TrackingState == SkeletonTrackingState.Tracked);

            if (usuario != null)
            {
                Joint maoDireita = usuario.Joints[JointType.HandRight];
                Joint cabeca = usuario.Joints[JointType.Head];

                bool novoTesteMaoDireitaAcimaCabeca = maoDireita.Position.Y > cabeca.Position.Y;

                if (MaoDireitaAcimaCabeca != novoTesteMaoDireitaAcimaCabeca)
                {
                    MaoDireitaAcimaCabeca = novoTesteMaoDireitaAcimaCabeca;
                    if (MaoDireitaAcimaCabeca) MessageBox.Show("Rá! Levantou a mão DIREITA!");
                }
            }
        }

        private void ExecutarRegraMaoEsquerdaAcimaDaCabeca(SkeletonFrame quadroAtual)
        {
            Skeleton[] esqueletos = new Skeleton[6];

            quadroAtual.CopySkeletonDataTo(esqueletos);
            Skeleton usuario = esqueletos.FirstOrDefault(esqueleto => esqueleto.TrackingState == SkeletonTrackingState.Tracked);

            if (usuario != null)
            {
                Joint maoEsquerda = usuario.Joints[JointType.HandLeft];
                Joint cabeca = usuario.Joints[JointType.Head];

                bool novoTesteMaoEsquerdaAcimaCabeca = maoEsquerda.Position.Y > cabeca.Position.Y;

                if (MaoEsquerdaAcimaCabeca != novoTesteMaoEsquerdaAcimaCabeca)
                {
                    MaoEsquerdaAcimaCabeca = novoTesteMaoEsquerdaAcimaCabeca;
                    
                    if (MaoEsquerdaAcimaCabeca) MessageBox.Show("PARABÉNS!!! Detectei que você levantou a mão esquerda!");
                }
            }
        }




        /**
         * Desenhar o esqueleto do usuário.
         */
        void etecnect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame visualizacao = e.OpenColorImageFrame())
            {
                if (visualizacao == null) return;

                if (info_cores_sensor_kinect == null)
                    info_cores_sensor_kinect = new byte[visualizacao.PixelDataLength];

                visualizacao.CopyPixelDataTo(info_cores_sensor_kinect);

                if (bmp_rgb_cores == null)
                {
                    this.bmp_rgb_cores = new WriteableBitmap(
                        visualizacao.Width,
                        visualizacao.Height,
                        96,
                        96,
                        PixelFormats.Bgr32,
                        null);
                }

                this.bmp_rgb_cores.WritePixels(new Int32Rect(0, 0, visualizacao.Width, visualizacao.Height), info_cores_sensor_kinect, visualizacao.Width * visualizacao.BytesPerPixel, 0);
                esqueletobugado.Background = new ImageBrush(bmp_rgb_cores);
            }
        }



        void etecnect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            esqueletobugado.Children.Clear();
            Skeleton[] esqueletos = null;

            using (SkeletonFrame esqueletos_quadros_ms = e.OpenSkeletonFrame())
            {
                if (esqueletos_quadros_ms != null)
                {
                    esqueletos = new Skeleton[esqueletos_quadros_ms.SkeletonArrayLength];
                    esqueletos_quadros_ms.CopySkeletonDataTo(esqueletos);
                }
            }

            if (esqueletos == null) return;

            foreach (Skeleton esqueleto in esqueletos)
            {
                if (esqueleto.TrackingState == SkeletonTrackingState.Tracked)
                {
                    //Oh, I'll never kill myself to save my soul
                    Joint handJoint = esqueleto.Joints[JointType.HandRight];
                    Joint elbowJoint = esqueleto.Joints[JointType.ElbowRight];
                    bones_esqueleto(esqueleto.Joints[JointType.Head], esqueleto.Joints[JointType.ShoulderCenter]);
                    bones_esqueleto(esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.Spine]);
                    bones_esqueleto(esqueleto.Joints[JointType.Spine], esqueleto.Joints[JointType.HipCenter]);
                    bones_esqueleto(esqueleto.Joints[JointType.HipCenter], esqueleto.Joints[JointType.HipLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.HipLeft], esqueleto.Joints[JointType.KneeLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.KneeLeft], esqueleto.Joints[JointType.AnkleLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.AnkleLeft], esqueleto.Joints[JointType.FootLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.HipCenter], esqueleto.Joints[JointType.HipRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.HipRight], esqueleto.Joints[JointType.KneeRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.KneeRight], esqueleto.Joints[JointType.AnkleRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.AnkleRight], esqueleto.Joints[JointType.FootRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.ShoulderLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.ShoulderLeft], esqueleto.Joints[JointType.ElbowLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.ElbowLeft], esqueleto.Joints[JointType.WristLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.WristLeft], esqueleto.Joints[JointType.HandLeft]);
                    bones_esqueleto(esqueleto.Joints[JointType.ShoulderCenter], esqueleto.Joints[JointType.ShoulderRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.ShoulderRight], esqueleto.Joints[JointType.ElbowRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.ElbowRight], esqueleto.Joints[JointType.WristRight]);
                    bones_esqueleto(esqueleto.Joints[JointType.WristRight], esqueleto.Joints[JointType.HandRight]);
                    //I was gone, but how was I to know?
                }
            }
        }



        void bones_esqueleto(Joint j1, Joint j2)
        {
            Line esp_bones_esqueleto = new Line();
            esp_bones_esqueleto.Stroke = new SolidColorBrush(Colors.Purple);
            esp_bones_esqueleto.StrokeThickness = 5;
            ColorImagePoint j1P = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            esp_bones_esqueleto.X1 = j1P.X;
            esp_bones_esqueleto.Y1 = j1P.Y;
            ColorImagePoint j2P = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            esp_bones_esqueleto.X2 = j2P.X;
            esp_bones_esqueleto.Y2 = j2P.Y;
            esqueletobugado.Children.Add(esp_bones_esqueleto);
        }



    }
}
