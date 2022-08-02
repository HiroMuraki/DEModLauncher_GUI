using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DEModLauncher_GUI
{
    public static class Util
    {
        public static T? FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T t)
                {
                    return t;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
        public static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                {
                    return t;
                }
                var childItem = FindVisualChild<T>(child);
                if (childItem != null)
                {
                    return childItem;
                }
            }
            return null;
        }
        public static IEnumerable<string> TravelFiles(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                yield return file;
            }
            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                foreach (string file in TravelFiles(subDirectory))
                {
                    yield return file;
                }
            }
        }
        public static List<string> FilesCleaner(ICollection<string> preservedFiles, IEnumerable<string> allFiles)
        {
            var removedFiles = new List<string>();
            foreach (string file in allFiles)
            {
                if (!preservedFiles.Contains(file))
                {
                    File.Delete(file);
                    removedFiles.Add(Path.GetFileName(file));
                }
            }
            return removedFiles;
        }
    }
}
