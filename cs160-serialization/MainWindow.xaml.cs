using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Coding4Fun.Kinect.Wpf;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace cs160_serialization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        KinectSensor sensor;

        private String fakeSongFile = "testFiles/test1.mp3";
        private DanceRoutine routine;
        private DanceSegment segment;
        private int framesToRecord = 29 * 3;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //sign up for the event
            kinectSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser_KinectSensorChanged);
            if (DanceRoutine.saveAlreadyExists(fakeSongFile))
            {
                Debug.WriteLine("save already exists.");
               routine = DanceRoutine.load(DanceRoutine.saveDestinationName(fakeSongFile));
               String h = "hi";
                return;
               // Debug.WriteLine("loaded save.");
            } else {
                Debug.WriteLine("creating new routine");
                routine = new DanceRoutine(fakeSongFile);
                Debug.WriteLine("created new routine");

                segment = routine.addDanceSegment(0);
            }
        }

        void kinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            var oldSensor = (KinectSensor)e.OldValue;

            //stop the old sensor
            if (oldSensor != null)
            {
                StopKinect(oldSensor);
            }

            //get the new sensor
            sensor = (KinectSensor)e.NewValue;
            if (sensor == null)
            {
                return;
            }

            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.2f,
                Prediction = 0.4f,
                JitterRadius = 0.1f,
                MaxDeviationRadius = 0.4f
            };
            parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();
            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);
            
            //turn on features that you need
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.SkeletonStream.Enable();

            try
            {
                sensor.Start();
            }
            catch (System.IO.IOException)
            {
                //this happens if another app is using the Kinect
                kinectSensorChooser.AppConflictOccurred();
            }
        }

        //this event fires when Color/Depth/Skeleton are synchronized
        void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton first = GetFirstSkeleton(e);
            BitmapSource bitmap = GetBitmap(e);
            if (first == null)
            {
                return;
            }
            if (framesToRecord > 0)
            {
                segment.updateImages(bitmap);
                segment.updateSkeletons(first);
                framesToRecord--;
            }
            else
            {
                framesToRecord = -1;
                Debug.WriteLine("recorded the necessary number of frames!");
                Debug.WriteLine("will no longer record");
                Debug.WriteLine("Saving...");
                routine.save();
                Debug.WriteLine("Saved.");
            }
        }

        BitmapSource GetBitmap(AllFramesReadyEventArgs e)
        {
            ColorImageFrame colorFrame = e.OpenColorImageFrame();
            return colorFrame.ToBitmapSource();
        }

        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }


                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();

                return first;

            }
        }


        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            KinectSkeletonViewer.Kinect = null;
            StopKinect(kinectSensorChooser.Kinect);
        }
    }
}
