//-------------------------------------------------------
//
//  These sources are provided by the company SMACCHIA.COM SARL
//  Download the trial edition of our flagship product: http://www.NDepend.com
//  NDepend is a tool for .NET developers.
//  It is a static analyzer that simplifies managing a complex .NET code base
//
//-------------------------------------------------------
using System;
namespace OpenEhr.Utilities.PathHelper
{
   sealed class DirectoryPathRelative : DirectoryPath
   {
      public DirectoryPathRelative(string path) : base(path, false) { }

      //
      //  Absolute/Relative path conversion
      //
      public DirectoryPathAbsolute GetAbsolutePathFrom(DirectoryPathAbsolute path) {
         if (path == null) {
            throw new ArgumentNullException();
         }
         if (PathHelper.IsEmpty(this) || PathHelper.IsEmpty(path)) {
            throw new ArgumentException("Cannot compute an absolute path from an empty path.");
         }
         return new DirectoryPathAbsolute(BasePath.GetAbsolutePathFrom(path, this));
      }

      public bool CanGetAbsolutePathFrom(DirectoryPathAbsolute path) {
         try {
            this.GetAbsolutePathFrom(path);
            return true;
         } catch { }
         return false;
      }




      //
      //  Path Browsing facilities
      //


      public new DirectoryPathRelative ParentDirectoryPath {
         get {
            string parentPath = InternalStringHelper.GetParentDirectory(this.Path);
            return new DirectoryPathRelative(parentPath);
         }
      }

      public FilePathRelative GetBrotherFileWithName(string fileName) {
         if (fileName == null) { throw new ArgumentNullException("filename"); }
         if (fileName.Length == 0) { throw new ArgumentException("Can't get brother of an empty file", "filename"); }
         if (this.IsEmpty) { throw new InvalidOperationException("Can't get brother of an empty file"); }
         return this.ParentDirectoryPath.GetChildFileWithName(fileName);
      }

      public DirectoryPathRelative GetBrotherDirectoryWithName(string fileName) {
         if (fileName == null) { throw new ArgumentNullException("filename"); }
         if (fileName.Length == 0) { throw new ArgumentException("Can't get brother of an empty file", "filename"); }
         if (this.IsEmpty) { throw new InvalidOperationException("Can't get brother of an empty file"); }
         return this.ParentDirectoryPath.GetChildDirectoryWithName(fileName);
      }

      public FilePathRelative GetChildFileWithName(string fileName) {
         if (fileName == null) { throw new ArgumentNullException("filename"); }
         if (fileName.Length == 0) { throw new ArgumentException("Empty filename not accepted", "filename"); }
         if (this.IsEmpty) { throw new InvalidOperationException("Can't get a child file name from an empty path"); }
         return new FilePathRelative(this.Path + System.IO.Path.DirectorySeparatorChar + fileName);
      }

      public DirectoryPathRelative GetChildDirectoryWithName(string directoryName) {
         if (directoryName == null) { throw new ArgumentNullException("directoryName"); }
         if (directoryName.Length == 0) { throw new ArgumentException("Empty directoryName not accepted", "directoryName"); }
         if (this.IsEmpty) { throw new InvalidOperationException("Can't get a child directory name from an empty path"); }
         return new DirectoryPathRelative(this.Path + System.IO.Path.DirectorySeparatorChar + directoryName);
      }


      //
      //  Empty DirectoryPathRelative
      //
      private DirectoryPathRelative() : base() { }
      private static DirectoryPathRelative s_Empty = new DirectoryPathRelative();
      public static DirectoryPathRelative Empty { get { return s_Empty; } }

      public override bool IsAbsolutePath { get { return false; } }
      public override bool IsRelativePath { get { return true; } }
   }
}
