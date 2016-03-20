using NUnit.Framework;
using Northwind.Data.NHibernateMappings;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Tests.Northwind.Data.NHibernateMaps
{
    [TestFixture]
    public class CategoryMapTests
    {
        [Test]
        public void CanGenerateXmlMapping() {
            CategoryMap categoryMap = new CategoryMap();

            XmlDocument xml = categoryMap.Generate();
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xml.WriteTo(xw);
           
            Debug.WriteLine(sw.ToString());
        }
    }
}
