using BetterLoadMenu.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterLoadMenu.Cache
{
    public class ConstructCacheData
    {

        public long Modified { get; private set; }
        public long FileSize { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string Hash { get; private set; }

        public ConstructCacheData(string fileName, string filePath, string hash, long modified, long fileSize)
        {
            this.FileName = fileName;
            this.FilePath = filePath;
            this.Hash = hash;
            this.Modified = modified;
            this.FileSize = fileSize;
        }

        public ConfigNode ConfigNode
        {
            get { 
                /*ConfigNode c = new ConfigNode();
                c.AddNode(this.FileName.Replace(" ", "_"));*/
                ConfigNode n = new ConfigNode(Utils.createNodeNameFromCraftFile(this.FileName));
                n.AddValue("FileName", this.FileName);
                n.AddValue("FilePath", this.FilePath);
                n.AddValue("Modified", this.Modified);
                n.AddValue("FileSize", this.FileSize);
                n.AddValue("FileHash", this.Hash);
                return n;
            }
        }

    }
}
