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
using System.Collections.Generic;

namespace OpenEhr.Utilities.PathHelper
{
   abstract class DirectoryPath : BasePath {
      protected DirectoryPath() { }  // Special for empty Path
      protected DirectoryPath(string path, bool isAbsolute) : base(path, isAbsolute){
      }

      public override bool IsDirectoryPath { get { return true; } }
      public override bool IsFilePath { get { return false; } }



      //
      //  DirectoryName
      //
      public string DirectoryName { get { return InternalStringHelper.GetLastName(this.Path); } }
      public bool HasParentDir { get { return InternalStringHelper.HasParentDir(this.Path); } }
   }
}
