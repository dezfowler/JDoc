using System;
using Newtonsoft.Json.Linq;

namespace JDoc
{
    public static class JObjectExtensions
    {
        /// <summary>
        /// Merge the property values defined in the source into the target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static JObject Merge(this JObject target, JObject source, MergeOptions mergeOptions)
        {
            var working = (JObject)target.DeepClone();
            source = (JObject)source.DeepClone();

            foreach (var thing in source)
            {
                //if ((mergeOptions & MergeOptions.OnlyInTarget) != 0 && ((IDictionary<string, JToken>)working).ContainsKey(thing.Key))
                //{
                //    if ((mergeOptions & MergeOptions.DeepMerge) != 0)
                //    working[thing.Key] = thing.Value;
                //}
            }

            return working;
        }
    }

    [Flags]
    public enum MergeOptions
    {
        Default = 0,

        /// <summary>
        /// Only merges properties already defined in target.
        /// </summary>
        OnlyInTarget = 1,

        /// <summary>
        /// If the value of any property is a JObject - will apply merge to this also.
        /// </summary>
        DeepMerge = 2,
    }
}