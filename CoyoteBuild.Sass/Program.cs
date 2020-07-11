using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CoyoteBuild.Sass
{
    class Program
    {
        private static void Main(string[] args)
        {
            var projectDirectory = Path.GetDirectoryName(args[0]);
            var Includes = XElement.Load(args[0], LoadOptions.SetLineInfo).Descendants()
                .Where(element => element.Attributes("Include").Any())
                .Select(element => Path.GetFullPath(element.Attribute("Include")?.Value ?? string.Empty)).ToList();
            var SassFiles = Includes.Concat(Directory.GetFiles(projectDirectory, "*.scss || *.sass",
                SearchOption.AllDirectories)).Distinct();
        }
    }
}