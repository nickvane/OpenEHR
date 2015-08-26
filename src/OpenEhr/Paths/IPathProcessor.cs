using System;
using OpenEhr.AssumedTypes;

namespace OpenEhr.Paths
{
    public interface IPathProcessor
    {
        object ItemAtPath(string path);
        List<object> ItemsAtPath(string path);
        bool PathExists(string path);
        bool PathUnique(string path);
    }
}
