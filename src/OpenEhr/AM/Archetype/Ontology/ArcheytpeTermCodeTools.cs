using System;
using System.Collections.Generic;
using System.Text;
using OpenEhr.Resources;

namespace OpenEhr.AM.Archetype.Ontology
{
    /// <summary>
    ///  Tools for manipulating archetype codes. Term codes take the form of 'atNNNN.N..'.
    ///  At the first level, 'atNNNN' is used; specialised codes are of the 
    ///  form 'atNNNN.N' to whatever depth require; intervening '0's are allowed,
    ///  indicating that the code is a child of a parent more than one level
    ///  up, e.g. 'at0021.0.3' is the 3rd specialisation of the top level code
    ///  'at0021', in the 2nd specialsiation level.
    ///  New codes can also be introduced in specialised archetypes, which are
    ///  not themselves specialisations of any existing code. These have the form 
    ///  'at.NN.NN', e.g. 'at.0.0.2'
    ///  The factory routines in this class are smart - they don't just make new specialised
    ///  codes by adding '.1', but by looking at the current hash of codes known in the 
    /// archetype ontology into which this class is inherited.
    /// </summary>
    internal class ArcheytpeTermCodeTools
    {
        const char specialisationSeparator='.';

        /// <summary>
        /// Infer number of levels of specialisation from `a_code'
        /// </summary>
        /// <param name="aCode"></param>
        /// <returns></returns>
        internal static int SpecialisationDepthFromCode(string aCode)
        {
            DesignByContract.Check.Require(!string.IsNullOrEmpty(aCode),
                string.Format(CommonStrings.XMustNotBeNullOrEmpty, "aCode"));

            string[] array = aCode.Split(new char[] { specialisationSeparator }, 
                StringSplitOptions.RemoveEmptyEntries);

            int depth = 0;

            foreach (string aString in array)
            {
                if (aString != "at0000")
                    depth++;
            }

            return depth;
        }
    }
}