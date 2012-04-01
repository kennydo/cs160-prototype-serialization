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
        private String name;
        private String saveName;
        private Dictionary<DanceSegment, DateTime> segments;
        private Dictionary<String, DateTime> comments;
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
                segments = new Dictionary<DanceSegment, DateTime>();
                comments = new Dictionary<string, DateTime>();
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
    }
}
