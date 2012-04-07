﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using Coding4Fun.Kinect.Wpf;
using System.IO;
using System.Runtime.Serialization;
using System.Drawing;
namespace cs160_serialization
{
    [Serializable]
    class DanceSegment
    {
        [Serializable]
        class SerializableJoint
        {
            public SkeletonPoint Position;
            public SerializableJoint(Joint j)
            {
                Position = j.Position;
            }
        }

        [Serializable]
        class SerializableSkeleton
        {
            public Dictionary<JointType, SerializableJoint> Joints;
            public SerializableSkeleton(Skeleton skeleton)
            {
                Joints = new Dictionary<JointType, SerializableJoint>();
                foreach (JointType jt in Enum.GetValues(typeof(JointType)))
                {
                    Joints.Add(jt, (SerializableJoint) (object) skeleton.Joints[jt]);
                }
            }
        }

        private LinkedList<Skeleton> skeletons;
        private Guid guid;
        private String saveDestinationFolder;

        public DanceSegment(DanceRoutine routine)
        {
            skeletons = new LinkedList<Skeleton>();
            guid = Guid.NewGuid();

            saveDestinationFolder = Path.Combine(routine.saveDestinationFolder, guid.ToString());
            Directory.CreateDirectory(saveDestinationFolder);
        }

        public void updateSkeletons(Skeleton skeleton)
        {
            skeletons.AddLast(skeleton);
        }

        public void updateImages(BitmapSource newFrame)
        {
            newFrame.Save(imageFramePath(skeletons.Count), Coding4Fun.Kinect.Wpf.ImageFormat.Jpeg);
        }

        private String imageFramePath(int frameNumber)
        {
            // note that frameNumbers start at 1
            return @"" + saveDestinationFolder + "\\" + frameNumber + ".jpg";
        }

        // returns the length in number of frames
        public int length
        {
            get { return skeletons.Count; }
        }

        public Bitmap getFirstFrame()
        {
            return new Bitmap(imageFramePath(0));
        }

        public Bitmap getLastFrame()
        {
            return new Bitmap(imageFramePath(skeletons.Count));
        }
    }
}
