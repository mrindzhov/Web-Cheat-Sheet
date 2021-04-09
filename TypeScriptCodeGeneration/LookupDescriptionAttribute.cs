using System;

namespace CodeGeneration
{
    public class LookupDescriptionAttribute : Attribute
    {
        public LookupDescriptionAttribute(string longDescription = null, string shortDescription = null)
        {
            LongDescription = longDescription;
            ShortDescription = shortDescription;
        }

        public string LongDescription { get; set; }
        public string ShortDescription { get; set; }
    }
}
