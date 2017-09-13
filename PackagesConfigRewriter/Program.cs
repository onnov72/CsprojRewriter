using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PackagesConfigRewriter
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = args[0];//Path.Combine(Directory.GetCurrentDirectory(), "packages.config");
            if (File.Exists(fileName))
            {
                XDocument document = XDocument.Load(fileName);
                var elements = document.Root.Elements("package").ToList();
                elements.Reverse();
                elements.ForEach(x => Change(x));
                document.Root.Name = "ItemGroup";
                document.Save(fileName);
            }
        }

        private static void Change(XElement element)
        {
            element.Name = "PackageReference";
            foreach (XAttribute attribute in element.Attributes().Reverse().ToList())
            {
                if (attribute.Name == "id")
                {
                    element.SetAttributeValue("Include", attribute.Value);
                }
                else
                {
                    if(attribute.Name == "version")
                    {
                        element.SetAttributeValue("Version", attribute.Value);
                    }
                }
                attribute.Remove();
            }
        }
    }
}
