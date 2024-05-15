using System;
namespace MvcPustok.Helpers {
  public class FileManager {
    public static string Save(IFormFile file, string root, string folder) {
      string newFileName = Guid.NewGuid().ToString() + file.FileName;
      string path = Path.Combine(root, folder, newFileName);

      using FileStream fs = new(path, FileMode.Create);
      file.CopyTo(fs);

      return newFileName;
    }
    public static bool Delete(string root, string folder, string fileName) {
      string path = Path.Combine(root, folder, fileName);

      if (File.Exists(path)) {
        File.Delete(path);
        return true;
      }
      return false;
    }
  }
}

