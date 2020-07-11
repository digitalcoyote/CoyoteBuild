using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace BuildTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            var cancellationToken = new CancellationTokenSource();
            var client = new HttpClient();
            var request = client.GetAsync("https://github.com/sass/dart-sass/releases");
            cancellationToken.Token.ThrowIfCancellationRequested();

            var response = request.Result.Content.ReadAsStringAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();
          var links = Regex.Matches(response.Result, "<a.*?href=(\"|')(.+?)(\"|').*?>", RegexOptions.Singleline | RegexOptions.IgnoreCase)
              .Select(m =>
              {
                  var hrefUrl= m.Value.Substring(m.Value.IndexOf("href=\"", StringComparison.Ordinal) + 6);
                  hrefUrl =    hrefUrl.Substring(0, hrefUrl.IndexOf("\"", StringComparison.Ordinal));
                  return hrefUrl;
              });
          var version = links.First(l =>
              Regex.IsMatch(l,@"\/sass\/dart-sass\/tree\/[0-9]{1,4}\.[0-9]{1,4}\.{0,1}[0-9]{0,4}\.{0,1}[0-9]{0,4}")).Split('/').Last();
          
          Parallel.ForEach(links.Where(l => l.Contains($"dart-sass-{version}-")), (link) =>
          {
              using var client = new WebClient();
              var filename = link.Split('/').Last();
              client.DownloadFile($"https://github.com/{link}", filename);
              var subfolder = "osx";

              string getTempFolder() => Path.Combine(args[0], subfolder);
              if (filename.EndsWith(".tar.gz"))
              {
                  using var inStream = File.OpenRead(filename);
                  using var gzipStream = new GZipInputStream(inStream);

                  using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                  if (link.Contains("linux"))
                  {
                      subfolder = link.Contains("x64") ? "linux/x64" : "linux/x86";
                  }
                  tarArchive.ExtractContents(getTempFolder());
              }
              else if (filename.EndsWith(".zip"))
              {
                  subfolder = link.Contains("x64") ? "win/x64" : "win/x86";
                  using Stream fsInput = File.OpenRead(filename);
                  using var zf = new ZipFile(fsInput);

                  foreach (ZipEntry zipEntry in zf) {
                      if (!zipEntry.IsFile) {
                          // Ignore directories
                          continue;
                      }
                      var entryFileName = Path.GetFileName(zipEntry.Name);
                      if (entryFileName.ToLowerInvariant() != "sass.snapshot" 
                          && entryFileName.ToLowerInvariant() != "dart.exe" 
                          && entryFileName.ToLowerInvariant() != "license") 
                      {
                          // Ignore other files
                          continue;
                      }

                      var fullZipToPath = Path.Combine(getTempFolder(), entryFileName);
                      var directoryName = Path.GetDirectoryName(fullZipToPath);
                      if (directoryName.Length > 0) {
                          Directory.CreateDirectory(directoryName);
                      }

                      // 4K is optimum
                      var buffer = new byte[4096];

                      // Unzip file in buffered chunks. This is just as fast as unpacking
                      // to a buffer the full size of the file, but does not waste memory.
                      // The "using" will close the stream even if an exception occurs.
                      using var zipStream = zf.GetInputStream(zipEntry);
                      using Stream fsOutput = File.Create(fullZipToPath);
                      StreamUtils.Copy(zipStream, fsOutput , buffer);
                  }
              }
              else
              {
                  Console.WriteLine($"CB00A : Error : Unexpected File Type for file: {filename}");
              }
          });
        }
    }
}