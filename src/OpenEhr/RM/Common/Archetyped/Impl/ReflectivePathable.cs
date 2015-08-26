using System;

using OpenEhr.DesignByContract;
using OpenEhr.AssumedTypes;
using OpenEhr.Paths;

namespace OpenEhr.RM.Common.Archetyped.Impl
{
    public abstract class ReflectivePathable : Pathable
    {
        public override bool PathExists(string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool PathUnique(string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override object ItemAtPath(string path)
        {
            Check.Require(!string.IsNullOrEmpty(path), "Path must not be null or empty.");

            object itemInDictionary = null;
            Path pathObject = new Path(path);


            if (itemInDictionary == null)
                throw new PathNotExistException(path);

            if (itemInDictionary is AssumedTypes.IList)
            {
                AssumedTypes.IList list = itemInDictionary as AssumedTypes.IList;

                if (list.Count > 1)
                    throw new PathNotUniqueException(path);
                else
                    return list[0];
            }
            else
                return itemInDictionary;
        }

        public override List<object> ItemsAtPath(string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string PathOfItem(Pathable item)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
