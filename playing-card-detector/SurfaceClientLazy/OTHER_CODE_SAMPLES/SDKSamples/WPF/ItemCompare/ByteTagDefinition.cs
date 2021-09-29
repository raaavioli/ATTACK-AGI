using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Microsoft.Surface.Presentation.Input;

namespace ItemCompare
{
    /// <summary>
    /// Exposes tag definition information.
    /// </summary>
    /// <remarks>
    /// The Surface shell's object-routing feature reads tag definition
    /// information from the XML. We hard-code the same information here
    /// to drive our UI at run time.
    /// </remarks>
    internal class ByteTagDefinition
    {
        private static readonly Dictionary<TagDefinitionKey, ByteTagDefinition> registeredTagDefinitions = new Dictionary<TagDefinitionKey, ByteTagDefinition>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ByteTagDefinition()
        {
            // When you update ItemCompare.ObjectSets.xml file, you need to update the following definitions to be in synch with the XML.
            registeredTagDefinitions.Add(new TagDefinitionKey("*", "*", "*", "0xC1"), new ByteTagDefinition(new Vector(2.5, -0.5), -90));
            registeredTagDefinitions.Add(new TagDefinitionKey("*", "*", "*", "0xC2"), new ByteTagDefinition(new Vector(2.5, -0.5), -90));
            registeredTagDefinitions.Add(new TagDefinitionKey("*", "*", "*", "0xC3"), new ByteTagDefinition(new Vector(2.5, -0.5), -90));
        }

        private readonly Vector physicalCenterOffsetFromTag;
        private readonly double orientationOffsetFromTag;

        /// <summary>
        /// Gets the physical center offset from tag.
        /// </summary>
        public Vector PhysicalCenterOffsetFromTag
        {
            get { return physicalCenterOffsetFromTag; }
        }

        /// <summary>
        /// Gets the orientation offset from tag.
        /// </summary>
        public double OrientationOffsetFromTag
        {
            get { return orientationOffsetFromTag; }
        }

        /// <summary>
        /// Looks for a byte tag definition with the specified tag value.
        /// Returns null if not found.
        /// </summary>
        /// <param name="tagValue"></param>
        /// <returns></returns>
        public static ByteTagDefinition Find(TagData tagValue)
        {
            TagDefinitionKey foundKey = registeredTagDefinitions.Keys.FirstOrDefault(definition => definition.Matches(tagValue));
            return (foundKey != null) ? registeredTagDefinitions[foundKey] : null;
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="physicalCenterOffsetFromTag"></param>
        /// <param name="orientationOffsetFromTag"></param>
        private ByteTagDefinition(
            Vector physicalCenterOffsetFromTag,
            double orientationOffsetFromTag)
        {
            this.physicalCenterOffsetFromTag = physicalCenterOffsetFromTag;
            this.orientationOffsetFromTag = orientationOffsetFromTag;
        }
    }
}
