using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pixeval.Util
{
    public class ConditionalObservableCollection<T> : ObservableCollection<T>
    {
        private readonly IList<T> _backup = new List<T>();

        private bool _filtered;

        public void UpdateCondition(Func<T?, bool> predicate)
        {
            // A lock on Type is generally considered as a bad practice
            // because it exposes the control of the lock to outside,
            // but this is the most convenient way if we want to lock
            // the whole object and prevent any other thread from accessing
            // it other than override any possibly stateful method and 
            // locks each of them
            lock (typeof(ConditionalObservableCollection<T>))
            {
                _filtered = true;
                _backup.Clear(); // insurance
                foreach (var t in this)
                {
                    _backup.Add(t); // add the whole list to _backup because we need to preserve the order
                }
                Clear();
                foreach (var t in _backup)
                {
                    if (predicate(t))
                    {
                        Add(t); // add directly without worrying about order issues, because the order in _backup is preserved
                    }
                }
            }
        }

        public void Restore()
        {
            lock (typeof(ConditionalObservableCollection<T>))
            {
                _filtered = false;
                Clear(); // insurance
                foreach (var t in _backup)
                {
                    Add(t); // add the whole list to _backup because we need to preserve the order
                }
                _backup.Clear();
            }
        }

        protected override void ClearItems()
        {
            if (_filtered)
            {
                _backup.Clear();
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (_filtered)
            {
                _backup.Insert(index, item);
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (_filtered)
            {
                var item = this[index];
                _backup.Remove(item);
            }
            base.RemoveItem(index);
        }
    }
}