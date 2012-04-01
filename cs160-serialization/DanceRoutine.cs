using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace cs160_serialization
{
    class DanceRoutine
    {
        // time is kept by the integer number of frames since the beginning

        public String name;
        private String saveName;
        public Dictionary<DanceSegment, int> segments;
        public Dictionary<String, int> comments;
        // private Soundtrack soundtrack;

        public DanceRoutine(String filename){
            name = filename;
            saveName = saveDestinationName(filename);

            if (saveAlreadyExists(filename))
            {
                throw new Exception("Should be loading. Save already exists.");
            }
            else
            {
                segments = new Dictionary<DanceSegment, int>();
                comments = new Dictionary<string, int>();
            }
        }

        static public String saveDestinationName(String songFilename){
            return Path.ChangeExtension(songFilename, ".dat");
        }

        static public Boolean saveAlreadyExists(String songFilename)
        {
            String saveName = saveDestinationName(songFilename);
            return File.Exists(saveName);
        }

        public Boolean save()
        {
            var formatter = new BinaryFormatter();
            var fs = new FileStream(saveName, FileMode.Create);
            try{
                formatter.Serialize(fs, this);
            }
            catch (Exception e)
            {
                Debugger.Log(0, "Serialization", e.ToString());
                return false;
            }
            return true;
        }

        static public DanceRoutine load(String saveFilename)
        {
            var formatter = new BinaryFormatter();
            var fs = new FileStream(saveFilename, FileMode.Open);
            try
            {
                return (DanceRoutine)formatter.Deserialize(fs);
            }
            catch (Exception e)
            {
                Debugger.Log(0, "Serialization", e.ToString());
            }
            return null;
        }

        public DanceSegment addDanceSegment(int startFrame)
        {
            var segment = new DanceSegment();
            segments.Add(segment, startFrame);
            return segment;
        }

        public String addComment(int frame, String comment)
        {
            comments.Add(comment, frame);
            return comment;
        }
    }
}
