using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudImageStorage.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public string FileName { get; set; }
        public string[] Tags { get; set; }
    }
}
