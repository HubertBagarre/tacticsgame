using System;
using System.Collections.Generic;
using System.Linq;

// TODO - Optimize I guess xd
internal class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    public class Item : IComparable<Item>
    {
        public TElement Element { get; }
        public TPriority Priority { get; }

        public Item(TElement element, TPriority priority)
        {
            Element = element;
            Priority = priority;
        }

        public int CompareTo(Item other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Priority.CompareTo(other.Priority);
        }
    }
    
    private List<Item> Items { get; }
    public int Count => Items.Count;

    public PriorityQueue()
    {
        Items = new List<Item>();
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        Items.Add(new Item(element,priority));
        Items.Sort();
    }

    public TElement Dequeue()
    {
        var item = Items.First();
        Items.Remove(item);
        Items.Sort();
        return item.Element;
    }

    public void Clear()
    {
        Items.Clear();
    }
}