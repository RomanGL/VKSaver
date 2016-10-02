using System.Collections.ObjectModel;

namespace VKSaver.Core.ViewModels.Common
{
    public class JumpListGroup<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Key that represents the group of objects and used as group header.
        /// </summary>
        public object Key { get; set; }     
        
        public string Tag { get; set; }   
    }
}
