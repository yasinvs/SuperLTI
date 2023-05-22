using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace SuperLTI
{
    public class ZipProgress
    {
        public ZipProgress(int total, int processed, string currentItem)
        {
            Total = total;
            Processed = processed;
            CurrentItem = currentItem;
        }
        public int Total { get; }
        public int Processed { get; }
        public string CurrentItem { get; }
    }

    public static class MyZipFileExtensions
    {
        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, IProgress<ZipProgress> progress)
        {
            ExtractToDirectory(source, destinationDirectoryName, progress, overwrite: true);
        }

        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, IProgress<ZipProgress> progress, bool overwrite)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destinationDirectoryName == null)
                throw new ArgumentNullException(nameof(destinationDirectoryName));


            // Rely on Directory.CreateDirectory for validation of destinationDirectoryName.

            // Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            int count = 0;
            foreach (ZipArchiveEntry entry in source.Entries)
            {
                count++;
                string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, entry.FullName));

                if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("File is extracting to outside of the folder specified.");

                var zipProgress = new ZipProgress(source.Entries.Count, count, entry.FullName);
                progress.Report(zipProgress);

                if (Path.GetFileName(fileDestinationPath).Length == 0)
                {
                    // If it is a directory:

                    if (entry.Length != 0)
                        throw new IOException("Directory entry with data.");

                    Directory.CreateDirectory(fileDestinationPath);
                }
                else
                {
                    // If it is a file:
                    // Create containing directory:
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath));
                    entry.ExtractToFile(fileDestinationPath, overwrite: overwrite);
                }
            }
        }

        public static void ExtractToDirectory(this Ionic.Zip.ZipFile source, string destinationDirectoryName, IProgress<ZipProgress> progress)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destinationDirectoryName == null)
                throw new ArgumentNullException(nameof(destinationDirectoryName));

            // Rely on Directory.CreateDirectory for validation of destinationDirectoryName.

            // Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            di.Attributes = FileAttributes.System | FileAttributes.Hidden;
            string destinationDirectoryFullPath = di.FullName;

            int count = 0;

            foreach (Ionic.Zip.ZipEntry entry in source.Entries)
            {
                count++;
                string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, entry.FileName));

                if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("File is extracting to outside of the folder specified.");

                var zipProgress = new ZipProgress(source.Entries.Count, count, entry.FileName);
                progress.Report(zipProgress);

                //entry.Extract(destinationDirectoryName, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);

                try
                {
                    entry.ExtractWithPassword(baseDirectory: destinationDirectoryFullPath, password: "Sence@2020", extractExistingFile: Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);

                    if (!File.Exists(destinationDirectoryFullPath + "\\packageSettings.xml"))
                    {
                        if (File.Exists(Application.StartupPath + "\\packageSettings.xml"))
                        {
                            File.Copy(Application.StartupPath + "\\packageSettings.xml", destinationDirectoryFullPath + "\\packageSettings.xml", true);
                        }
                    }

                    if (!File.Exists(destinationDirectoryFullPath + "\\packageSettings.json"))
                    {
                        if (File.Exists(Application.StartupPath + "\\packageSettings.json"))
                        {
                            File.Copy(Application.StartupPath + "\\packageSettings.json", destinationDirectoryFullPath + "\\packageSettings.json", true);
                        }
                    }
                }
                catch (Ionic.Zip.BadPasswordException)
                {
                    throw new IOException("The password did not match.");
                }
            }
        }
    }
}
