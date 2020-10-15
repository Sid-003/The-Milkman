using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pahoe.Search;

namespace The_Milkman.Collections
{
    public class MusicQueue
    {
        private readonly List<LavalinkTrack> _internalList = new List<LavalinkTrack>();
        

        public void Add(LavalinkTrack track)
        {
            lock (_internalList)
            {
                _internalList.Add(track);
            }
        }

        public void Insert(int index, LavalinkTrack track)
        {
            lock (_internalList)
            {
                _internalList.Insert(index, track);
            }
        }
        
        public bool TryPopAt(int index, out LavalinkTrack track)
        {
            lock (_internalList)
            {
                track = null;
                if (_internalList.Count < 0 || index > _internalList.Count - 1)
                    return false;

                track = _internalList[index];
                _internalList.Remove(track);
                return true;
            }
        }

        public bool Contains(LavalinkTrack track)
        {
            lock (_internalList)
            {
                return _internalList.Contains(track);
            }
        }

        public int GetTotalLength()
        {
            lock (_internalList)
            {
                return _internalList.Sum(x => (int)x.Length.TotalSeconds);
            }
        }

        public override string ToString()
        {
            lock (_internalList)
            {
                return  string.Join("\n", _internalList.Select((x, i) => $"{i}. [{x.Title}]({x.Uri})"));
            }
        }


        public LavalinkTrack this[int index]
        {
            get
            {
                lock (_internalList)
                {
                    return _internalList[index];
                }
            }
            set
            {
                lock (_internalList)
                {
                    _internalList[index] = value;
                }
            }
        }
    }
}