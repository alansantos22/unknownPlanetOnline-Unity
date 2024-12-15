using System.Collections.Generic;

namespace GoPath.Navigation
{
    public class PriorityQueue<T> where T : NavigationNode
    {
        private List<T> heap = new List<T>();

        public int Count => heap.Count;

        public void Enqueue(T item)
        {
            heap.Add(item);
            SortUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            T firstItem = heap[0];
            int lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);

            if (heap.Count > 0)
                SortDown(0);

            return firstItem;
        }

        public bool Contains(T item)
        {
            return heap.Contains(item);
        }

        public void Remove(T item)
        {
            int index = heap.IndexOf(item);
            if (index != -1)
            {
                heap[index] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                if (index < heap.Count)
                    SortDown(index);
            }
        }

        private void SortUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (heap[index].FCost < heap[parentIndex].FCost)
                {
                    Swap(index, parentIndex);
                    index = parentIndex;
                }
                else
                    break;
            }
        }

        private void SortDown(int index)
        {
            while (true)
            {
                int smallest = index;
                int left = 2 * index + 1;
                int right = 2 * index + 2;

                if (left < heap.Count && heap[left].FCost < heap[smallest].FCost)
                    smallest = left;
                if (right < heap.Count && heap[right].FCost < heap[smallest].FCost)
                    smallest = right;

                if (smallest != index)
                {
                    Swap(index, smallest);
                    index = smallest;
                }
                else
                    break;
            }
        }

        private void Swap(int a, int b)
        {
            T temp = heap[a];
            heap[a] = heap[b];
            heap[b] = temp;
        }
    }
}
