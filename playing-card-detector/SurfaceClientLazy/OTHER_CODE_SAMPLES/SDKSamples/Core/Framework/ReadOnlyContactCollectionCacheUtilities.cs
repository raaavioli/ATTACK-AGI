using System.Collections.Generic;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    static class ReadOnlyTouchCollectionCacheUtilities
    {
        internal static bool Contains(this List<TouchPoint> touches, int id)
        {
            foreach (TouchPoint touch in touches)
            {
                if (touch.Id == id)
                    return true;
            }

            return false;
        }

        internal static void Remove(this List<TouchPoint> touches, int id)
        {
            for (int i = 0; i < touches.Count; i++)
			{
                TouchPoint touch = touches[i];

                if (touch.Id == id)
                {
                    touches.RemoveAt(i);

                    return;
                }
            }
        }
    }
}
