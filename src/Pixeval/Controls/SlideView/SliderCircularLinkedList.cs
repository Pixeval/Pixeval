#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SliderCircularLinkedList.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Controls.SlideView;

/// <summary>
/// A circular linked list (ring buffer) to loop through the slides and display them 
/// </summary>
/// <param name="Next"></param>
/// <param name="Element"></param>
public record SliderCircularLinkedList(SliderCircularLinkedList Next, object Element)
{
    public static SliderCircularLinkedList Create(IEnumerable<object> elements)
    {
        var enumerable = elements.ToList();
        var last = enumerable.Last();
        var lastNode = new SliderCircularLinkedList(null!, last);
        var headNode = lastNode;
        for (var i = enumerable.Count - 2; i >= 0; i--)
        {
            headNode = new SliderCircularLinkedList(headNode, enumerable[i]);
        }

        lastNode.Next = headNode;
        return headNode;
    }

    public SliderCircularLinkedList Next { get; set; } = Next;
}