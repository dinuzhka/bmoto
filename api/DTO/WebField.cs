using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bmoto.DTO
{
    public class WebField
    {
        public string Section { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public float Max { get; set; }
        public bool IsPoints { get; set; }
        public bool Mandatory { get; set; }

        public WebField() { }
        public WebField(string section, string name, string type, string format = null, float max = 0)
        {
            this.Section = section;
            this.Name = name;
            this.Type = type;
            this.Format = !string.IsNullOrEmpty(format) ? format : type;
            this.Max = max;
        }
    }
}
