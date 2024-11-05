namespace MyGameName;
using CLSS;
using Godot;
using MyGameName.ValueObjects.FileSystem;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

public static class PathUtility {
  private static readonly MemoizedFunc<GodotPath, GlobalizedPath> _getGlobalizedPathSafe = MemoizedFunc<GodotPath, GlobalizedPath>.From(GetGlobalizedPathSafe_Private);

  #region methods

  public static async Task<GlobalizedPath> CompressZipToBrotliAsync(FileInfo fileToCompress) {
    var path = ReplaceExtension(fileToCompress, (FileExtension)".br");
    using (var originalFileStream = fileToCompress.OpenRead()) {
      using (var compressedFileStream = File.Create(path.Value)) {
        using (var compressionStream = new BrotliStream(compressedFileStream, CompressionLevel.Optimal)) {
          await originalFileStream.CopyToAsync(compressionStream);
        }
      }
    }
    return path;
  }

  public static async Task<GlobalizedPath> DecompressBrotliToZipAsync(FileInfo fileToDecompress) {
    var path = ReplaceExtension(fileToDecompress, (FileExtension)".zip");
    using (var originalFileStream = fileToDecompress.OpenRead()) {
      using (var decompressedFileStream = File.Create(path.Value)) {
        using (var decompressionStream = new BrotliStream(originalFileStream, CompressionMode.Decompress)) {
          await decompressionStream.CopyToAsync(decompressedFileStream);
        }
      }
    }
    return path;
  }

  public static IEnumerable<(FileName, FileExtension)> GetFileNamesAndExtensionsInDirectory(GlobalizedPath directoryPath) {
    var files = Directory.GetFiles(directoryPath.Value);
    return files.Select(path => {
      var name = (FileName)Path.GetFileNameWithoutExtension(path);
      var ext = (FileExtension)Path.GetExtension(path);
      return (name, ext);
    });
  }

  public static IEnumerable<(FileName, FileExtension)> GetFileNamesAndExtensionsInSubDirectories(GlobalizedPath directoryPath) {
    var directories = Directory.GetDirectories(directoryPath.Value);
    return directories.SelectMany(directory => GetFileNamesAndExtensionsInDirectory((GlobalizedPath)directory));
  }

  public static GlobalizedPath GetGlobalizedPathSafe(GodotPath path) => _getGlobalizedPathSafe.Invoke(path);

  public static string JoinAndReplaceWhitespace(params string[] args) {
    var firstPass = string.Join('-', args);
    return firstPass.Replace(' ', '_');
  }

  public static GlobalizedPath ReplaceExtension(FileInfo fileInfo, FileExtension newExtension) {
    var currentFileName = (GlobalizedPath)fileInfo.FullName;
    var newFileName = currentFileName.Value.Remove(currentFileName.Value.Length - fileInfo.Extension.Length);
    newFileName = $"{newFileName}{newExtension}";
    return (GlobalizedPath)newFileName;
  }

  private static GlobalizedPath GetGlobalizedPathSafe_Private(GodotPath path) {
    GlobalizedPath resultPath;
    if (!path.Value.StartsWith(GodotPath.ResPathPrefix.Value) || OS.HasFeature("editor") || path.Value.StartsWith(GodotPath.UserPathPrefix.Value)) {
      resultPath = (GlobalizedPath)ProjectSettings.GlobalizePath(path.Value);
      return resultPath;
    }
    var afterRes = path.Value[6..];
    resultPath = (GlobalizedPath)OS.GetExecutablePath().GetBaseDir().PathJoin(afterRes);
    return resultPath;
  }

  #endregion
}
