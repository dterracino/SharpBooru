using System;
using System.Collections.Generic;

namespace TA.SharpBooru.Client
{
    public class BooruPostBuffer
    {
        private Dictionary<ulong, WeakReference> _bufferDict = new Dictionary<ulong, WeakReference>();

        public void Add(BooruPost Post)
        {
            WeakReference weakRef = new WeakReference(Post);
            _bufferDict.Add(Post.ID, weakRef);
        }

        public BooruPost this[ulong ID]
        {
            get
            {
                BooruPost returnPost = null;
                foreach (var refPair in _bufferDict)
                {
                    if (!refPair.Value.IsAlive)
                        _bufferDict.Remove(refPair.Key);
                    else if (returnPost != null)
                    {
                        BooruPost retrievedPost = refPair.Value.Target as BooruPost;
                        if (retrievedPost != null)
                            returnPost = retrievedPost;
                        else _bufferDict.Remove(refPair.Key);
                    }
                }
                return returnPost;
            }
        }
    }
}
