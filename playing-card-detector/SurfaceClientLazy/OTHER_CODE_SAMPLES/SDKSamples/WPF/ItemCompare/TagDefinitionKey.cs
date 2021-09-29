using System;
using Microsoft.Surface.Presentation.Input;

namespace ItemCompare
{
    /// <summary>
    /// Encapsulates a definition of a tag. This is used to uniquely identify a specific type of tag.
    /// </summary>
    internal class TagDefinitionKey
    {
        private readonly TagValue schema;
        private readonly TagValue series;
        private readonly TagValue extendedValue;
        private readonly TagValue value;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TagDefinitionKey(TagValue schema, TagValue series, TagValue extendedValue, TagValue value)
        {
            this.schema = schema;
            this.series = series;
            this.extendedValue = extendedValue;
            this.value = value;
        }

        /// <summary>
        /// The tag schema.
        /// </summary>
        public TagValue Schema
        {
            get { return schema; }
        }

        /// <summary>
        /// The tag series.
        /// </summary>
        public TagValue Series
        {
            get { return series; }
        }

        /// <summary>
        /// The tag extended value.
        /// </summary>
        public TagValue ExtendedValue
        {
            get { return extendedValue; }
        }

        /// <summary>
        /// The tag value.
        /// </summary>
        public TagValue Value
        {
            get { return value; }
        }

        /// <summary>
        /// Check if the given tag data matches this definition or not.
        /// </summary>
        /// <param name="tagData">A TagData to compare.</param>
        /// <returns>True if it matches, otherwise false.</returns>
        public bool Matches(TagData tagData)
        {
            // If all the values match the corresponding patterns, then it's a match.
            if ((Schema.IsAny || (Schema.IsSpecific && Schema == tagData.Schema)) &&
                (Series.IsAny || (Series.IsSpecific && Series == tagData.Series)) &&
                (ExtendedValue.IsAny || (ExtendedValue.IsSpecific && ExtendedValue == tagData.ExtendedValue)) &&
                (Value.IsAny || (Value.IsSpecific && Value == tagData.Value)))
            {
                return true;
            }

            // Something didn't match.
            return false;
        }
    }
}
