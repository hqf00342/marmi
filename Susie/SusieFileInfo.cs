using System;

namespace Marmi
{
    public struct SusieFileInfo
    {
        public string method;
        public uint position;
        public uint compsize;
        public uint filesize;
        public DateTime timestamp;
        public string path;
        public string filename;
        public uint crc;
    }
}