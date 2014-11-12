using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using MergingPhonesBase.Config;

namespace MergingPhonesBase.XmlWorker
{
    internal static class DataXmlWorker
    {
        //private const string XmlFilePath = @"Full.xml";

        internal static IList<string> GetTels(string fileName)
        {
            var doc = XDocument.Load(fileName);
            var att =
                (IEnumerable)
                    doc.XPathSelectElements("//tels/item").Select(x => x.Value);

            return att as IList<string> ?? att.Cast<string>().ToList();
        }

        internal static void SetTels(IEnumerable<InfoHolder> values, string fileName)
        {
            var doc = XDocument.Load(fileName);

            doc.XPathSelectElement("//tels")
                .Add(values
                    .Select(value => new XElement("item",
                        new XAttribute("site", value.Site),
                        new XAttribute("direction", value.Direction),
                        new XAttribute("city", value.City),
                        new XAttribute("name", value.Name),
                        value.Phone)));

            doc.Save(fileName);
        }
    }
}