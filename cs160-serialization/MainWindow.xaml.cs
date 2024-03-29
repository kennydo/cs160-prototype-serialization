﻿using System;
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
using System.IO;
using System.Windows.Threading;


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

        private String fakeSongFile = "test1.mp3";
        private DanceRoutine routine;
        private DanceSegment segment;
        private int framesToRecord = 29 * 3;
        public int videoCounter;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            audioPlayerPlayForDuration(new TimeSpan(0, 1, 0), new TimeSpan(0, 0, 10));

           //sign up for the event
            kinectSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser_KinectSensorChanged);
            if (DanceRoutine.saveAlreadyExists(fakeSongFile))
            {
              Debug.WriteLine("save already exists.");
              routine = DanceRoutine.load(DanceRoutine.getSaveDestinationName(fakeSongFile));
              segment = routine.segments[0];
              Debug.WriteLine("Loaded segment " + segment);
              videoCounter = 0;
              kinectSkeletonViewerCanvas.Visibility = Visibility.Hidden;
              var dispatcherTimer = new DispatcherTimer();
              dispatcherTimer.Tick += new EventHandler(videoPlayerTick);
              dispatcherTimer.Interval = new TimeSpan(0, 0, 1/29);
              dispatcherTimer.Start();

              framesToRecord = -2;
            } else {
                Debug.WriteLine("creating new routine");
                routine = new DanceRoutine(fakeSongFile);
                Debug.WriteLine("created new routine");
                segment = routine.addDanceSegment(0);
            }
        }

        private void audioPlayerPlayForDuration(TimeSpan startPosition, TimeSpan duration)
        {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                songMediaElement.Stop();
                (sender as DispatcherTimer).Stop();
            });
            dispatcherTimer.Interval = duration;
            songMediaElement.Position = startPosition;
            songMediaElement.Play();
            dispatcherTimer.Start();
        }

        private void videoPlayerTick(object sender, EventArgs e)
        {
            for (int elementIndex = videoPlaybackCanvas.Children.Count - 1; elementIndex >= 0; elementIndex--)
            {
                var child = videoPlaybackCanvas.Children[elementIndex];
                videoPlaybackCanvas.Children.Remove(child);
            }
            if (videoCounter >= segment.length)
            {
                (sender as DispatcherTimer).Stop();
                return;
            }

            var img = new System.Windows.Controls.Image();
            img.Source = segment.getFrame(videoCounter);
            videoPlaybackCanvas.Children.Add(img);

            Canvas.SetTop(img, 0);
            Canvas.SetLeft(img, 0);

            videoCounter++;
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
            BitmapSource bitmap = GetBitmap();
            if (first == null)
            {
                return;
            }
            if (framesToRecord > 0)
            {
                Debug.WriteLine("Recording frame..." + " with " + framesToRecord.ToString() + " to go!");
                segment.updateImages(bitmap);
                segment.updateSkeletons(first);
                framesToRecord--;
            }
            else if (framesToRecord == 0){
                Debug.WriteLine("recorded the necessary number of frames!");
                Debug.WriteLine("will no longer record");
                Debug.WriteLine("Saving...");
                routine.save();
                Debug.WriteLine("Saved.");
                framesToRecord = -2;
            }
            else if (framesToRecord == -2)
            {
                sensor.AllFramesReady -= newSensor_AllFramesReady;
            }
        }

        BitmapSource GetBitmap()
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)kinectSkeletonViewerCanvas.ActualWidth, (int)kinectSkeletonViewerCanvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(kinectSkeletonViewerCanvas);
            return renderBitmap;
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
