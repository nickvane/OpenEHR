//-------------------------------------------------------
//
//  These sources are provided by the company SMACCHIA.COM SARL
//  Download the trial edition of our flagship product: http://www.NDepend.com
//  NDepend is a tool for .NET developers.
//  It is a static analyzer that simplifies managing a complex .NET code base
//
//-------------------------------------------------------
using System.IO;
using System.Collections.Generic;
using System;
namespace OpenEhr.Utilities.PathHelper
{
   sealed class DirectoryPathAbsolute : DirectoryPath {
      public DirectoryPathAbsolute(string path) : base(path,true) {
      }


      //
      //  Absolute/Relative path conversion
      //
      public DirectoryPathRelative GetPathRelativeFrom(DirectoryPathAbsolute path) {
         if (path == null) {
            throw new ArgumentNullException();
         }
         if (PathHelper.IsEmpty(this) || PathHelper.IsEmpty(path)) {
            throw new ArgumentException("Cannot compute a relative path from an empty path.");
         }
         return new DirectoryPathRelative(BasePath.GetPathRelative(path, this));
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

      public FilePathAbsolute GetChildFileWithName(string fileName) {
         if (fileName == null) { throw new ArgumentNullException("filename"); }
         if (fileName.Length == 0) { throw new ArgumentException("Empty filename not accepted", "filename"); }
         if (this.IsEmpty) {throw new InvalidOperationException("Can't get a child file name from an empty path");}
         return new FilePathAbsolute(this.Path + System.IO.Path.DirectorySeparatorChar + fileName);
      }

      public DirectoryPathAbsolute GetChildDirectoryWithName(string directoryName) {
         if (directoryName == null) { throw new ArgumentNullException("directoryName"); }
         if (directoryName.Length == 0) { throw new ArgumentException("Empty directoryName not accepted", "directoryName"); }
         if (this.IsEmpty) {throw new InvalidOperationException("Can't get a child directory name from an empty path");}
         return new DirectoryPathAbsolute(this.Path + System.IO.Path.DirectorySeparatorChar + directoryName);
      }

      public bool IsChildDirectoryOf(DirectoryPathAbsolute parentDir) {
         if (parentDir == null) { throw new ArgumentNullException("parentDir"); }
         if (parentDir.IsEmpty) { throw new ArgumentException("Empty parentDir not accepted", "parentDir"); }
         string parentPathUpperCase = parentDir.Path.ToUpper();
         string thisPathUpperCase = this.Path.ToUpper();
         return thisPathUpperCase.Contains(parentPathUpperCase);
      }

      

      //
      //  Operations that requires physical access
      //
      public string Drive { get { return this.DriveProtected; } }

      public bool Exists {
         get {
            return Directory.Exists(this.Path);
         }
      }

      public DirectoryInfo DirectoryInfo {
         get {
            if (!this.Exists) {
               throw new FileNotFoundException(this.Path);
            }
            return new DirectoryInfo(this.Path);
         }
      }

      public List<FilePathAbsolute> ChildrenFilesPath {
         get {
            DirectoryInfo directoryInfo = this.DirectoryInfo;
            FileInfo[] filesInfos = directoryInfo.GetFiles();
            List<FilePathAbsolute> childrenFilesPath = new List<FilePathAbsolute>();
            foreach (FileInfo fileInfo in filesInfos) {
               childrenFilesPath.Add(new FilePathAbsolute(fileInfo.FullName));
            }
            return childrenFilesPath;
         }
      }
      public List<DirectoryPathAbsolute> ChildrenDirectoriesPath {
         get {
            DirectoryInfo directoryInfo = this.DirectoryInfo;
            DirectoryInfo[] directoriesInfos = directoryInfo.GetDirectories();
            List<DirectoryPathAbsolute> childrenDirectoriesPath = new List<DirectoryPathAbsolute>();
            foreach (DirectoryInfo childDirectoryInfo in directoriesInfos) {
               childrenDirectoriesPath.Add(new DirectoryPathAbsolute(childDirectoryInfo.FullName));
            }
            return childrenDirectoriesPath;
         }
      }


      //
      //  Empty DirectoryPathAbsolute
      //
      private DirectoryPathAbsolute() : base (){ }
      private static DirectoryPathAbsolute s_Empty = new DirectoryPathAbsolute();
      public static DirectoryPathAbsolute Empty { get { return s_Empty; } }


      public override bool IsAbsolutePath { get { return true; } }
      public override bool IsRelativePath { get { return false; } }
   }
}
