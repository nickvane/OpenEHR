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
using System.Diagnostics;

namespace OpenEhr.Utilities.PathHelper
{
   abstract class FilePath : BasePath {
      protected FilePath() : base() { } // special for empty
      protected FilePath(string path, bool isAbsolute): base(path, isAbsolute) {
         if (!InternalStringHelper.HasParentDir(this.Path)) {
            throw new ArgumentException(this.Path, @"The file path has no parent directory.");
         }
      }

      public override bool IsDirectoryPath { get { return false; } }
      public override bool IsFilePath { get { return true; } }

      //
      //  FileName and extension
      //
      public string FileName { get { return InternalStringHelper.GetLastName(this.Path); } }

      public string FileNameWithoutExtension {
         get {
            string fileName = this.FileName;
            string extension = this.FileExtension;
            if (extension==null || extension.Length==0) {
               return fileName;
            }
            Debug.Assert(fileName.Length - extension.Length >= 0);
            return fileName.Substring(0, fileName.Length - extension.Length);
         }
      }

      public string FileExtension { get { return InternalStringHelper.GetExtension(this.Path); } }
      public bool HasExtension(string extension) {
         if (extension == null || extension.Length < 2 || extension[0] != '.') {
            throw new ArgumentException(@"The input extension string is """+extension+@""".
The extension must be a non-null string that begins with a dot","extension");
         }
         // Ignore case comparison
         return (string.Compare(this.FileExtension, extension, true) == 0);
      }




   }
}
