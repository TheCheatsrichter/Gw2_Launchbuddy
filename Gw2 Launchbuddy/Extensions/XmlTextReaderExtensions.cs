using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Gw2_Launchbuddy.Extensions
{
    public static class XmlTextReaderExtensions
    {
        public static string GetValue(this XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                return reader.Value;
            }
            return null;
        }
    }
}
