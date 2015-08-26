//-------------------------------------------------------
//
//  These sources are provided by the company SMACCHIA.COM SARL
//  Download the trial edition of our flagship product: http://www.NDepend.com
//  NDepend is a tool for .NET developers.
//  It is a static analyzer that simplifies managing a complex .NET code base
//
//-------------------------------------------------------
using System.IO;
using System;
namespace OpenEhr.Utilities.PathHelper
{
   sealed class FilePathAbsolute : FilePath {
      

      public FilePathAbsolute(string path) : base(path, true) {
      }



      //
      //  Absolute/Relative path conversion
      //
      public FilePathRelative GetPathRelativeFrom(DirectoryPathAbsolute path) {
         if (path == null) {
            throw new ArgumentNullException();
         }
         if (PathHelper.IsEmpty(this) || PathHelper.IsEmpty(path)) {
            throw new ArgumentException("Cannot compute a relative path from an empty path.");
         }
         string pathRelative = BasePath.GetPathRelative(path, this);
         return new FilePathRelative(pathRelative + System.IO.Path.DirectorySeparatorChar + this.FileName);
      }

      public bool CanGetPathRelativeFrom(DirectoryPathAbsolute path) {
         try {
            this.GetPathRelativeFrom(path);
            return true;
         } catch { }
         return false;
      }




      //
      //  Path Browsing facilities
      //
      public new DirectoryPathAbsolute ParentDirectoryPath {
         get {
            string parentPath = InternalStringHelper.GetParentDirectory(this.Path);
            return new DirectoryPathAbsolute(parentPath);
         }
      }

      public FilePathAbsolute GetBrotherFileWithName(string fileName) {
         if (fileName == null) { throw new ArgumentNullException("filename"); }
         if (fileName.Length == 0) { throw new ArgumentException("Can't get brother of an empty file", "filename"); }
         if (this.IsEmpty) { throw new InvalidOperationException("Can't get brother of an empty file"); }
         return this.ParentDirectoryPath.GetChildFileWithName(fileName);
      }

      public DirectoryPathAbsolute GetBrotherDirectoryWithName(string fileName) {
         if (fileName == null) { throw new ArgumentNullException("filename"); }
         if (fileName.Length == 0) { throw new ArgumentException("Can't get brother of an empty file", "filename"); }
         if (this.IsEmpty) { throw new InvalidOperationException("Can't get brother of an empty file"); }
         return this.ParentDirectoryPath.GetChildDirectoryWithName(fileName);
      }


      public FilePathAbsolute ChangeExtension(string newExtension) {
         if (newExtension == null) {
            throw new ArgumentNullException(newExtension);
         }
         if (newExtension.Length > 0 && newExtension[0] != '.') {
            throw new ArgumentException("A file extension must begin with a dot", newExtension);
         }
         if (this.IsEmpty) {
            throw new InvalidOperationException("Cannot change the extension on an empty file");
         }
         return new FilePathAbsolute(
            this.ParentDirectoryPath.GetChildFileWithName(
               this.FileNameWithoutExtension + newExtension).Path);
      }


      //
      //  Operations that requires physical access
      //
      public string Drive { get { return this.DriveProtected; } }

      public bool Exists {
         get {
            return File.Exists(this.Path);
         }
      }

      public FileInfo FileInfo {
         get {
            if (!this.Exists) {
               throw new FileNotFoundException(this.Path);
            }
            return new FileInfo(this.Path);
         }
      }


      //
      //  Empty FilePathAbsolute
      //
      private FilePathAbsolute() { }  // Special for empty Path
      private static FilePathAbsolute s_Empty = new FilePathAbsolute();
      public static FilePathAbsolute Empty { get { return s_Empty; } }

      public override bool IsAbsolutePath { get { return true; } }
      public override bool IsRelativePath { get { return false; } }
   }
}
