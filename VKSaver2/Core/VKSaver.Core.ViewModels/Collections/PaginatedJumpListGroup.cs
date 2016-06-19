using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.ViewModels.Collections
{
    public class PaginatedJumpListGroup<T> : PaginatedCollection<T>
    {
        public PaginatedJumpListGroup()
            : base() { }

        public PaginatedJumpListGroup(IEnumerable<T> collection)
            : base(collection) { }

        public PaginatedJumpListGroup(Func<uint, Task<IEnumerable<T>>> func)
            : base(func) { }

        public PaginatedJumpListGroup(IEnumerable<T> collection, Func<uint, Task<IEnumerable<T>>> func)
            : base(collection, func) { }

        public object Key { get; set; }

        public override void Refresh()
        {
            Clear();
            Reset();
        }
    }
}
