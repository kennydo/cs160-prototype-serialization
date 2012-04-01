using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
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
                foreach (JointType jt in Enum.GetValues(typeof(JointType)))
                {
                    Joints.Add(jt, (SerializableJoint) (object) skeleton.Joints[jt]);
                }
            }
        }

        private LinkedList<Skeleton> skeletons;
        private LinkedList<ImageFrame> images;

        public DanceSegment()
        {
            skeletons = new LinkedList<Skeleton>();
            images = new LinkedList<ImageFrame>();
        }

        public void updateSkeletons(Skeleton skeleton)
        {
            skeletons.AddLast(skeleton);
        }

        public void updateImageFrames(ImageFrame newFrame)
        {
            images.AddLast(newFrame);
        }

        // returns the length in number of frames
        public int length
        {
            get { return skeletons.Count; }
        }

        public ImageFrame getFirstFrame()
        {
            return images.First.Value;
        }

        public ImageFrame getLastFrame()
        {
            return images.Last.Value;
        }
    }
}
