using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
namespace cs160_serialization
{
    class DanceSegment
    {
        private LinkedList<Skeleton> skeletons;
        private LinkedList<ImageFrame> images;

        public DanceSegment()
        {
        }

        public void updateSkeletonFrames(Skeleton newFrame){
            skeletons.AddLast(newFrame);
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
