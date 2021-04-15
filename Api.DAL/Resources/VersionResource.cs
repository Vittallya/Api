using System;

namespace Api.DAL.Resources
{
    public class VersionResource
    {
        public int Id { get; set; }
        public long Size { get; set; }
        public DateTime UploadDateTime { get; set; }
        public int Release { get; set; }
        public string Path { get; set; }
        public int DocId { get; set; }
    }
}
