using UnityEngine;

public class FIFOBuffer<T>
{
    public int Count => count;
    public int Capacity => buffer.Length;

    T[] buffer;
    int addingIndex = 0;
    int count = 0;

    public FIFOBuffer(int capacity)
    {
        buffer = new T[capacity];
    }

    public void Add(T obj)
    {
        buffer[addingIndex] = obj;
        addingIndex = (addingIndex + 1) % buffer.Length;

        if (count < buffer.Length)
            count++;
    }

    public T Get(int i)
    {
        // 0 = oldest, Count-1 = newest

        if (i < 0 || i >= count)
            throw new System.IndexOutOfRangeException($"Index ({i}) out of range: [0, {count}]");

        int index = (addingIndex - count + i + buffer.Length) % buffer.Length;
        return buffer[index];
    }

    public T this[int i]
    {
        get
        {
            return Get(i);
        }
    }

    public T[] ToArray()
    {
        T[] result = new T[count];
        for (int i = 0; i < count; i++)
            result[i] = Get(i);
        return result;
    }
}