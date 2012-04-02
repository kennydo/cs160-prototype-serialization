using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
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
        private LinkedList<BitmapSource> images;

        public DanceSegment()
        {
            skeletons = new LinkedList<Skeleton>();
            images = new LinkedList<BitmapSource>();
        }

        public void updateSkeletons(Skeleton skeleton)
        {
            skeletons.AddLast(skeleton);
        }

        public void updateImages(BitmapSource newFrame)
        {
            images.AddLast(newFrame);
        }

        // returns the length in number of frames
        public int length
        {
            get { return skeletons.Count; }
        }

        public BitmapSource getFirstFrame()
        {
            return images.First.Value;
        }

        public BitmapSource getLastFrame()
        {
            return images.Last.Value;
        }
    }
}
