using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Saro.Pool
{
    public class ObjectPoolItem : TreeViewItem
    {
        public IObjectPool pool;

        public ObjectPoolItem(IObjectPool pool)
        {
            this.pool = pool;
        }

        public string PoolType => pool.GetType().FullName;
        public int CountAll => pool.CountAll;
        public int CountActive => pool.CountActive;
        public int CountInactive => pool.CountInactive;
        public int RentCount => pool.RentCount;
        public int ReturnCount => pool.ReturnCount;
    }
}
