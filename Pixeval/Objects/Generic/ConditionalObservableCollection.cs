#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pixeval.Objects.Generic
{
    public interface ICollectionCondition<in T>
    {
        public bool CanAdd(T item);
    }
    
    public class ConditionalObservableCollection<T> : ObservableCollection<T>
    {
        private readonly List<T> awaits = new List<T>();
        
        public ICollectionCondition<T> Predicate { get; set; }

        protected override void InsertItem(int index, T item)
        {
            if (Predicate?.CanAdd(item) is true)
            {
                base.InsertItem(index, item);
            }
            else
            {
                awaits.Add(item);
            }
        }

        protected override void SetItem(int index, T item)
        {
            if (Predicate?.CanAdd(item) is true)
            {
                base.SetItem(index, item);
            }
            else
            {
                awaits.Add(item);
            }
        }

        // this method must stay synchronized to ensure the thread-safety between the operations of two lists
        [MethodImpl(MethodImplOptions.Synchronized)] 
        public void Refresh()
        {
            var currentList = this.ToList();
            // remove all the items that are used to be illegal but become legal now after predicate updates
            // and add them to current list
            foreach (var element in awaits.ToList().Where(Predicate.CanAdd))
            {
                awaits.Remove(element);
                Add(element);
            }
            
            // then remove all the items in the current list that are illegal after predicate updates
            // and add them to awaits list
            foreach (var element in currentList.Where(element => !Predicate.CanAdd(element)))
            {
                Remove(element);
                awaits.Add(element);
            }
        }
    }
}