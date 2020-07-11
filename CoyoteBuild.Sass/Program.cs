using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            var startInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            };
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo.FileName = @".\osx\dart";
                startInfo.Arguments = @".\osx\sass.snapshot";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    startInfo.FileName = @".\linux\x64\dart";
                    startInfo.Arguments = @".\linux\x64\sass.snapshot";
                }
                else if(RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    startInfo.FileName = @".\linux\x86\dart";
                    startInfo.Arguments = @".\linux\x86\sass.snapshot";
                }
                else
                {
                    Console.WriteLine($"CB003 : Error : {RuntimeInformation.OSArchitecture} is currently Unsupported. Please report this issue at https://github.com/digitalcoyote/CoyoteBuild/issues to request support");
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                Console.WriteLine($"CB002 : Error : FreeBSD is currently Unsupported. Please report this issue at https://github.com/digitalcoyote/CoyoteBuild/issues to request support");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    startInfo.FileName = @".\win\x64\dart";
                    startInfo.Arguments = @".\win\x64\sass.snapshot";
                }
                else if(RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    startInfo.FileName = @".\win\x86\dart";
                    startInfo.Arguments = @".\win\x86\sass.snapshot";
                }
                else
                {
                    Console.WriteLine($"CB003 : Error : {RuntimeInformation.OSArchitecture} is currently Unsupported. Please report this issue at https://github.com/digitalcoyote/CoyoteBuild/issues to request support");
                }            
            }
            else
            {
                Console.WriteLine($"CB001 : Error : Unable to properly detect OS. Os detected is '{RuntimeInformation.OSDescription}'. Please report this issue at https://github.com/digitalcoyote/CoyoteBuild/issues");
            }
        }
    }
}