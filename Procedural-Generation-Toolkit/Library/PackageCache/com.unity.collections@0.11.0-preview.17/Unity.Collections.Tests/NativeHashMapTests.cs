using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.Tests;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.TestTools;

internal class NativeHashMapTests : CollectionsTestFixture
{
#pragma warning disable 0649 // always default value
    struct NonBlittableStruct : IEquatable<NonBlittableStruct>
    {
        object o;

        public bool Equals(NonBlittableStruct other)
        {
            return Equals(o, other.o);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NonBlittableStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (o != null ? o.GetHashCode() : 0);
        }
    }
#pragma warning restore 0649

    [Test, DotsRuntimeIgnore]
    public void NativeHashMap_Non_Blittable_Throws()
    {
#pragma warning disable 0219 // assigned but its value is never used
        Assert.Throws<System.ArgumentException>(() => { var hashMap = new NativeHashMap<NonBlittableStruct, int>(16, Allocator.Temp); });
        Assert.Throws<System.ArgumentException>(() => { var hashMap = new NativeHashMap<int, NonBlittableStruct>(16, Allocator.Temp); });
#pragma warning restore 0219
    }

    static void ExpectedCount<TKey, TValue>(ref NativeHashMap<TKey, TValue> container, int expected)
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        Assert.AreEqual(expected == 0, container.IsEmpty);
        Assert.AreEqual(expected, container.Count());
    }

    static void ExpectedCount<TKey, TValue>(ref NativeMultiHashMap<TKey, TValue> container, int expected)
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        Assert.AreEqual(expected == 0, container.IsEmpty);
        Assert.AreEqual(expected, container.Count());
    }

    [Test]
    public void NativeHashMap_TryAdd_TryGetValue_Clear()
    {
        var hashMap = new NativeHashMap<int, int>(16, Allocator.Temp);
        ExpectedCount(ref hashMap, 0);

        int iSquared;
        // Make sure GetValue fails if hash map is empty
        Assert.IsFalse(hashMap.TryGetValue(0, out iSquared), "TryGetValue on empty hash map did not fail");

        // Make sure inserting values work
        for (int i = 0; i < 16; ++i)
            Assert.IsTrue(hashMap.TryAdd(i, i * i), "Failed to add value");
        ExpectedCount(ref hashMap, 16);

        // Make sure inserting duplicate keys fails
        for (int i = 0; i < 16; ++i)
            Assert.IsFalse(hashMap.TryAdd(i, i), "Adding duplicate keys did not fail");
        ExpectedCount(ref hashMap, 16);

        // Make sure reading the inserted values work
        for (int i = 0; i < 16; ++i)
        {
            Assert.IsTrue(hashMap.TryGetValue(i, out iSquared), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
        }

        // Make sure clearing removes all keys
        hashMap.Clear();
        ExpectedCount(ref hashMap, 0);

        for (int i = 0; i < 16; ++i)
            Assert.IsFalse(hashMap.TryGetValue(i, out iSquared), "Got value from hash table after clearing");

        hashMap.Dispose();
    }

    [Test, DotsRuntimeIgnore]
    public void NativeHashMap_Full_HashMap_Throws()
    {
        var hashMap = new NativeHashMap<int, int>(16, Allocator.Temp);
        // Fill the hash map
        for (int i = 0; i < 16; ++i)
            Assert.IsTrue(hashMap.TryAdd(i, i), "Failed to add value");
        // Make sure overallocating throws and exception if using the Concurrent version - normal hash map would grow
        var cHashMap = hashMap.AsParallelWriter();
        Assert.Throws<System.InvalidOperationException>(() => {cHashMap.TryAdd(100, 100); });
        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_Double_Deallocate_Throws()
    {
        var hashMap = new NativeHashMap<int, int>(16, Allocator.TempJob);
        hashMap.Dispose();
#if UNITY_2020_2_OR_NEWER
        Assert.Throws<ObjectDisposedException>(
#else
        Assert.Throws<InvalidOperationException>(
#endif
        () => { hashMap.Dispose(); });
    }

    [Test]
    public void NativeHashMap_Key_Collisions()
    {
        var hashMap = new NativeHashMap<int, int>(16, Allocator.Temp);
        int iSquared;
        // Make sure GetValue fails if hash map is empty
        Assert.IsFalse(hashMap.TryGetValue(0, out iSquared), "TryGetValue on empty hash map did not fail");
        // Make sure inserting values work
        for (int i = 0; i < 8; ++i)
            Assert.IsTrue(hashMap.TryAdd(i, i * i), "Failed to add value");
        // The bucket size is capacity * 2, adding that number should result in hash collisions
        for (int i = 0; i < 8; ++i)
            Assert.IsTrue(hashMap.TryAdd(i + 32, i), "Failed to add value with potential hash collision");
        // Make sure reading the inserted values work
        for (int i = 0; i < 8; ++i)
        {
            Assert.IsTrue(hashMap.TryGetValue(i, out iSquared), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
        }
        for (int i = 0; i < 8; ++i)
        {
            Assert.IsTrue(hashMap.TryGetValue(i + 32, out iSquared), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i, "Got the wrong value from the hash table");
        }
        hashMap.Dispose();
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe struct LargeKey : IEquatable<LargeKey>
    {
        [FieldOffset(0)]
        public int* Ptr;

        [FieldOffset(300)]
        int x;

        public bool Equals(LargeKey rhs)
        {
            return Ptr == rhs.Ptr;
        }
        public override int GetHashCode()
        {
            return (int)Ptr;
        }
    }

    [BurstCompile]
    struct HashMapTryAddAtomic : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<LargeKey> keys;

        [WriteOnly]
        public NativeHashMap<LargeKey, bool>.ParallelWriter hashMap;

        public void Execute(int index)
        {
            hashMap.TryAdd(keys[index & (keys.Length - 1)], false);
        }
    }

    [Test]
    public unsafe void NativeHashMap_Key_Collisions_FromJobs()
    {
//        Assert.True(false);

        var keys = new NativeArray<LargeKey>(4, Allocator.TempJob);
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = new LargeKey { Ptr = (int*)(((ulong)i) << 32) };
        }

        for (var spin = 0; spin < 1024; spin++)
        {
            var hashMap = new NativeHashMap<LargeKey, bool>(32, Allocator.TempJob);

            var jobHandle = new HashMapTryAddAtomic
            {
                keys = keys,
                hashMap = hashMap.AsParallelWriter(),
            }
            .Schedule(8, 1);

            jobHandle.Complete();

            Assert.AreEqual(keys.Length, hashMap.Count());

            for (var i = 0; i < keys.Length; ++i)
            {
                var key = new LargeKey { Ptr = (int*)(((ulong)i) << 32) };
                Assert.IsTrue(hashMap.ContainsKey(key));
            }

            hashMap.Dispose();
        }

        keys.Dispose();
    }

    [Test]
    public void NativeHashMap_HashMapSupportsAutomaticCapacityChange()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        int iSquared;
        // Make sure inserting values work and grows the capacity
        for (int i = 0; i < 8; ++i)
            Assert.IsTrue(hashMap.TryAdd(i, i * i), "Failed to add value");
        Assert.IsTrue(hashMap.Capacity >= 8, "Capacity was not updated correctly");
        // Make sure reading the inserted values work
        for (int i = 0; i < 8; ++i)
        {
            Assert.IsTrue(hashMap.TryGetValue(i, out iSquared), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
        }
        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_HashMapSameKey()
    {
        using (var hashMap = new NativeHashMap<int, int>(0, Allocator.Persistent))
        {
            Assert.DoesNotThrow(() => hashMap.Add(0, 0));
            Assert.Throws<ArgumentException>(() => hashMap.Add(0, 0));
        }

        using (var hashMap = new NativeHashMap<int, int>(0, Allocator.Persistent))
        {
            Assert.IsTrue(hashMap.TryAdd(0, 0));
            Assert.IsFalse(hashMap.TryAdd(0, 0));
        }
    }

    [Test]
    public void NativeHashMap_IsEmpty()
    {
        var hashMap = new NativeHashMap<int, int>(0, Allocator.Persistent);
        Assert.IsTrue(hashMap.IsEmpty);

        hashMap.TryAdd(0, 0);
        Assert.IsFalse(hashMap.IsEmpty);
        Assert.AreEqual(1, hashMap.Capacity);
        ExpectedCount(ref hashMap, 1);

        hashMap.Clear();
        Assert.IsTrue(hashMap.IsEmpty);

        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_HashMapEmptyCapacity()
    {
        var hashMap = new NativeHashMap<int, int>(0, Allocator.Persistent);
        hashMap.TryAdd(0, 0);
        Assert.AreEqual(1, hashMap.Capacity);
        ExpectedCount(ref hashMap, 1);
        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_Remove()
    {
        var hashMap = new NativeHashMap<int, int>(8, Allocator.Temp);
        int iSquared;
        // Make sure inserting values work
        for (int i = 0; i < 8; ++i)
            Assert.IsTrue(hashMap.TryAdd(i, i * i), "Failed to add value");
        Assert.AreEqual(8, hashMap.Capacity, "HashMap grew larger than expected");
        // Make sure reading the inserted values work
        for (int i = 0; i < 8; ++i)
        {
            Assert.IsTrue(hashMap.TryGetValue(i, out iSquared), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
        }
        for (int rm = 0; rm < 8; ++rm)
        {
            Assert.IsTrue(hashMap.Remove(rm));
            Assert.IsFalse(hashMap.TryGetValue(rm, out iSquared), "Failed to remove value from hash table");
            for (int i = rm + 1; i < 8; ++i)
            {
                Assert.IsTrue(hashMap.TryGetValue(i, out iSquared), "Failed get value from hash table");
                Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
            }
        }
        // Make sure entries were freed
        for (int i = 0; i < 8; ++i)
            Assert.IsTrue(hashMap.TryAdd(i, i * i), "Failed to add value");
        Assert.AreEqual(8, hashMap.Capacity, "HashMap grew larger than expected");
        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_RemoveOnEmptyMap_DoesNotThrow()
    {
        var hashMap = new NativeHashMap<int, int>(0, Allocator.Temp);
        Assert.DoesNotThrow(() => hashMap.Remove(0));
        Assert.DoesNotThrow(() => hashMap.Remove(-425196));
        hashMap.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_RemoveOnEmptyMap_DoesNotThrow()
    {
        var hashMap = new NativeMultiHashMap<int, int>(0, Allocator.Temp);

        Assert.DoesNotThrow(() => hashMap.Remove(0));
        Assert.DoesNotThrow(() => hashMap.Remove(-425196));
        Assert.DoesNotThrow(() => hashMap.Remove(0, 0));
        Assert.DoesNotThrow(() => hashMap.Remove(-425196, 0));

        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_RemoveFromMultiHashMap()
    {
        var hashMap = new NativeMultiHashMap<int, int>(16, Allocator.Temp);
        int iSquared;
        // Make sure inserting values work
        for (int i = 0; i < 8; ++i)
            hashMap.Add(i, i * i);
        for (int i = 0; i < 8; ++i)
            hashMap.Add(i, i);
        Assert.AreEqual(16, hashMap.Capacity, "HashMap grew larger than expected");
        // Make sure reading the inserted values work
        for (int i = 0; i < 8; ++i)
        {
            NativeMultiHashMapIterator<int> it;
            Assert.IsTrue(hashMap.TryGetFirstValue(i, out iSquared, out it), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i, "Got the wrong value from the hash table");
            Assert.IsTrue(hashMap.TryGetNextValue(out iSquared, ref it), "Failed get value from hash table");
            Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
        }
        for (int rm = 0; rm < 8; ++rm)
        {
            Assert.AreEqual(2, hashMap.Remove(rm));
            NativeMultiHashMapIterator<int> it;
            Assert.IsFalse(hashMap.TryGetFirstValue(rm, out iSquared, out it), "Failed to remove value from hash table");
            for (int i = rm + 1; i < 8; ++i)
            {
                Assert.IsTrue(hashMap.TryGetFirstValue(i, out iSquared, out it), "Failed get value from hash table");
                Assert.AreEqual(iSquared, i, "Got the wrong value from the hash table");
                Assert.IsTrue(hashMap.TryGetNextValue(out iSquared, ref it), "Failed get value from hash table");
                Assert.AreEqual(iSquared, i * i, "Got the wrong value from the hash table");
            }
        }
        // Make sure entries were freed
        for (int i = 0; i < 8; ++i)
            hashMap.Add(i, i * i);
        for (int i = 0; i < 8; ++i)
            hashMap.Add(i, i);
        Assert.AreEqual(16, hashMap.Capacity, "HashMap grew larger than expected");
        hashMap.Dispose();
    }

    void ExpectValues(NativeMultiHashMap<int, long> hashMap, int key, long[] expectedValues)
    {
        var list = new NativeList<long>(Allocator.TempJob);
        foreach (var value in hashMap.GetValuesForKey(key))
            list.Add(value);

        list.Sort();
        Assert.AreEqual(list.ToArray(), expectedValues);
        list.Dispose();
    }

    [Test]
    public void NativeHashMap_TryAddScalability()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Persistent);
        for (int i = 0; i != 1000 * 100; i++)
        {
            hashMap.TryAdd(i, i);
        }

        int value;
        Assert.IsFalse(hashMap.TryGetValue(-1, out value));
        Assert.IsFalse(hashMap.TryGetValue(1000 * 1000, out value));

        for (int i = 0; i != 1000 * 100; i++)
        {
            Assert.IsTrue(hashMap.TryGetValue(i, out value));
            Assert.AreEqual(i, value);
        }

        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_GetKeysEmpty()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        var keys = hashMap.GetKeyArray(Allocator.Temp);
        hashMap.Dispose();

        Assert.AreEqual(0, keys.Length);
        keys.Dispose();
    }

    [Test]
    public void NativeHashMap_GetKeys()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            hashMap.TryAdd(i, 2 * i);
        }
        var keys = hashMap.GetKeyArray(Allocator.Temp);
        hashMap.Dispose();

        Assert.AreEqual(30, keys.Length);
        keys.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(i, keys[i]);
        }
        keys.Dispose();
    }

    [Test]
    public void NativeHashMap_GetValues()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            hashMap.TryAdd(i, 2 * i);
        }
        var values = hashMap.GetValueArray(Allocator.Temp);
        hashMap.Dispose();

        Assert.AreEqual(30, values.Length);
        values.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(2 * i, values[i]);
        }
        values.Dispose();
    }

    [Test]
    public void NativeHashMap_GetKeysAndValues()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            hashMap.TryAdd(i, 2 * i);
        }
        var keysValues = hashMap.GetKeyValueArrays(Allocator.Temp);
        hashMap.Dispose();

        Assert.AreEqual(30, keysValues.Keys.Length);
        Assert.AreEqual(30, keysValues.Values.Length);

        // ensure keys and matching values are aligned
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(2 * keysValues.Keys[i], keysValues.Values[i]);
        }

        keysValues.Keys.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(i, keysValues.Keys[i]);
        }

        keysValues.Values.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(2 * i, keysValues.Values[i]);
        }

        keysValues.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_GetKeys()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            hashMap.Add(i, 2 * i);
            hashMap.Add(i, 3 * i);
        }
        var keys = hashMap.GetKeyArray(Allocator.Temp);
        hashMap.Dispose();

        Assert.AreEqual(60, keys.Length);
        keys.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(i, keys[i * 2 + 0]);
            Assert.AreEqual(i, keys[i * 2 + 1]);
        }
        keys.Dispose();
    }

#if !UNITY_DOTSRUNTIME
    [Test]
    public void NativeMultiHashMap_GetUniqueKeysEmpty()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        var keys = hashMap.GetUniqueKeyArray(Allocator.Temp);

        Assert.AreEqual(0, keys.Item1.Length);
        Assert.AreEqual(0, keys.Item2);
    }

    [Test]
    public void NativeMultiHashMap_GetUniqueKeys()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            hashMap.Add(i, 2 * i);
            hashMap.Add(i, 3 * i);
        }
        var keys = hashMap.GetUniqueKeyArray(Allocator.Temp);
        hashMap.Dispose();
        Assert.AreEqual(30, keys.Item2);
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(i, keys.Item1[i]);
        }
        keys.Item1.Dispose();
    }

#endif

    [Test]
    public void NativeMultiHashMap_GetValues()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            hashMap.Add(i, 30 + i);
            hashMap.Add(i, 60 + i);
        }
        var values = hashMap.GetValueArray(Allocator.Temp);
        hashMap.Dispose();

        Assert.AreEqual(60, values.Length);
        values.Sort();
        for (int i = 0; i < 60; ++i)
        {
            Assert.AreEqual(30  + i, values[i]);
        }
        values.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_ForEach()
    {
        using (var container = new NativeMultiHashMap<int, int>(1, Allocator.Temp))
        {
            for (int i = 0; i < 30; ++i)
            {
                container.Add(i, 30 + i);
                container.Add(i, 60 + i);
            }

            var count = 0;
            foreach (var kv in container)
            {
                if (kv.Value < 60)
                {
                    Assert.AreEqual(kv.Key + 30, kv.Value);
                }
                else
                {
                    Assert.AreEqual(kv.Key + 60, kv.Value);
                }

                ++count;
            }

            Assert.AreEqual(60, count);
        }
    }

    [Test]
    public void NativeMultiHashMap_ForEach_Throws_When_Modified()
    {
        using (var container = new NativeMultiHashMap<int, int>(32, Allocator.TempJob))
        {
            for (int i = 0; i < 30; ++i)
            {
                container.Add(i, 30 + i);
                container.Add(i, 60 + i);
            }

#if UNITY_2020_2_OR_NEWER
            Assert.Throws<ObjectDisposedException>(() =>
#else
            Assert.Throws<InvalidOperationException>(() =>
#endif
            {
                foreach (var kv in container)
                {
                    container.Add(10, 10);
                }
            });

#if UNITY_2020_2_OR_NEWER
            Assert.Throws<ObjectDisposedException>(() =>
#else
            Assert.Throws<InvalidOperationException>(() =>
#endif
            {
                foreach (var kv in container)
                {
                    container.Remove(1);
                }
            });
        }
    }

    struct NativeMultiHashMap_ForEachIterator : IJob
    {
        [ReadOnly]
        public NativeMultiHashMap<int, int>.KeyValueEnumerator Iter;

        public void Execute()
        {
            while (Iter.MoveNext())
            {
            }
        }
    }

    [Test]
    public void NativeMultiHashMap_ForEach_Throws_Job_Iterator()
    {
        using (var container = new NativeMultiHashMap<int, int>(32, Allocator.TempJob))
        {
            var jobHandle = new NativeMultiHashMap_ForEachIterator
            {
                Iter = container.GetEnumerator()

            }.Schedule();

            Assert.Throws<InvalidOperationException>(() => { container.Add(1, 1); });

            jobHandle.Complete();
        }
    }

    struct ParallelWriteToMultiHashMapJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeMultiHashMap<int, int>.ParallelWriter Writer;

        public void Execute(int index)
        {
            Writer.Add(index, 0);
        }
    }

    [Test]
    public void NativeMultiHashMap_ForEach_Throws_When_Modified_From_Job()
    {
        using (var container = new NativeMultiHashMap<int, int>(32, Allocator.TempJob))
        {
            var iter = container.GetEnumerator();

            var jobHandle = new ParallelWriteToMultiHashMapJob
            {
                Writer = container.AsParallelWriter()

            }.Schedule(1, 2);

#if UNITY_2020_2_OR_NEWER
            Assert.Throws<ObjectDisposedException>(() =>
#else
            Assert.Throws<InvalidOperationException>(() =>
#endif
            {
                while (iter.MoveNext())
                {
                }
            });

            jobHandle.Complete();
        }
    }

    [Test]
    public void NativeMultiHashMap_GetKeysAndValues()
    {
        var container = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            container.Add(i, 30 + i);
            container.Add(i, 60 + i);
        }
        var keysValues = container.GetKeyValueArrays(Allocator.Temp);
        container.Dispose();

        Assert.AreEqual(60, keysValues.Keys.Length);
        Assert.AreEqual(60, keysValues.Values.Length);

        // ensure keys and matching values are aligned (though unordered)
        for (int i = 0; i < 30; ++i)
        {
            var k0 = keysValues.Keys[i * 2 + 0];
            var k1 = keysValues.Keys[i * 2 + 1];
            var v0 = keysValues.Values[i * 2 + 0];
            var v1 = keysValues.Values[i * 2 + 1];

            if (v0 > v1)
                (v0, v1) = (v1, v0);

            Assert.AreEqual(k0, k1);
            Assert.AreEqual(30 + k0, v0);
            Assert.AreEqual(60 + k0, v1);
        }

        keysValues.Keys.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(i, keysValues.Keys[i * 2 + 0]);
            Assert.AreEqual(i, keysValues.Keys[i * 2 + 1]);
        }

        keysValues.Values.Sort();
        for (int i = 0; i < 60; ++i)
        {
            Assert.AreEqual(30  + i, keysValues.Values[i]);
        }

        keysValues.Dispose();
    }

    public struct TestEntityGuid : IEquatable<TestEntityGuid>, IComparable<TestEntityGuid>
    {
        public ulong a;
        public ulong b;

        public bool Equals(TestEntityGuid other)
        {
            return a == other.a && b == other.b;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (a.GetHashCode() * 397) ^ b.GetHashCode();
            }
        }

        public int CompareTo(TestEntityGuid other)
        {
            var aComparison = a.CompareTo(other.a);
            if (aComparison != 0) return aComparison;
            return b.CompareTo(other.b);
        }
    }

    [Test]
    public void NativeHashMap_GetKeysGuid()
    {
        var hashMap = new NativeHashMap<TestEntityGuid, int>(1, Allocator.Temp);
        for (int i = 0; i < 30; ++i)
        {
            var didAdd = hashMap.TryAdd(new TestEntityGuid() { a = (ulong)i * 5, b = 3 * (ulong)i }, 2 * i);
            Assert.IsTrue(didAdd);
        }

        // Validate Hashtable has all the expected values
        ExpectedCount(ref hashMap, 30);
        for (int i = 0; i < 30; ++i)
        {
            int output;
            var exists = hashMap.TryGetValue(new TestEntityGuid() { a = (ulong)i * 5, b = 3 * (ulong)i }, out output);
            Assert.IsTrue(exists);
            Assert.AreEqual(2 * i, output);
        }

        // Validate keys array
        var keys = hashMap.GetKeyArray(Allocator.Temp);
        Assert.AreEqual(30, keys.Length);

        keys.Sort();
        for (int i = 0; i < 30; ++i)
        {
            Assert.AreEqual(new TestEntityGuid() { a = (ulong)i * 5, b = 3 * (ulong)i }, keys[i]);
        }

        hashMap.Dispose();
        keys.Dispose();
    }

    [Test]
    public void NativeHashMap_IndexerWorks()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        hashMap[5] = 7;
        Assert.AreEqual(7, hashMap[5]);

        hashMap[5] = 9;
        Assert.AreEqual(9, hashMap[5]);

        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_ContainsKeyHashMap()
    {
        var hashMap = new NativeHashMap<int, int>(1, Allocator.Temp);
        hashMap[5] = 7;

        Assert.IsTrue(hashMap.ContainsKey(5));
        Assert.IsFalse(hashMap.ContainsKey(6));

        hashMap.Dispose();
    }

    [Test]
    public void NativeHashMap_NativeKeyValueArrays_DisposeJob()
    {
        var container = new NativeHashMap<int, int>(1, Allocator.Persistent);
        Assert.True(container.IsCreated);
        Assert.DoesNotThrow(() => { container[0] = 0; });
        Assert.DoesNotThrow(() => { container[1] = 1; });
        Assert.DoesNotThrow(() => { container[2] = 2; });
        Assert.DoesNotThrow(() => { container[3] = 3; });

        var kv = container.GetKeyValueArrays(Allocator.Persistent);

        var disposeJob = container.Dispose(default);
        Assert.False(container.IsCreated);
#if UNITY_2020_2_OR_NEWER
        Assert.Throws<ObjectDisposedException>(
#else
        Assert.Throws<InvalidOperationException>(
#endif
            () => { container[0] = 2; });

        kv.Dispose(disposeJob);

        disposeJob.Complete();
    }

    [Test]
    public void NativeHashMap_ContainsKeyMultiHashMap()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        hashMap.Add(5, 7);

        hashMap.Add(6, 9);
        hashMap.Add(6, 10);

        Assert.IsTrue(hashMap.ContainsKey(5));
        Assert.IsTrue(hashMap.ContainsKey(6));
        Assert.IsFalse(hashMap.ContainsKey(4));

        hashMap.Dispose();
    }

    // These tests require:
    // - JobsDebugger support for static safety IDs (added in 2020.1)
    // - Asserting throws
#if !UNITY_DOTSRUNTIME
    [Test,DotsRuntimeIgnore]
    public void NativeHashMap_UseAfterFree_UsesCustomOwnerTypeName()
    {
        var container = new NativeHashMap<int, int>(10, Allocator.TempJob);
        container[0] = 123;
        container.Dispose();
        Assert.That(() => container[0],
#if UNITY_2020_2_OR_NEWER
            Throws.Exception.TypeOf<ObjectDisposedException>()
#else
            Throws.InvalidOperationException
#endif
                .With.Message.Contains($"The {container.GetType()} has been deallocated"));
    }

    [BurstCompile(CompileSynchronously = true)]
    struct NativeHashMap_CreateAndUseAfterFreeBurst : IJob
    {
        public void Execute()
        {
            var container = new NativeHashMap<int, int>(10, Allocator.Temp);
            container[0] = 17;
            container.Dispose();
            container[1] = 42;
        }
    }

    [Test,DotsRuntimeIgnore]
    public void NativeHashMap_CreateAndUseAfterFreeInBurstJob_UsesCustomOwnerTypeName()
    {
        // Make sure this isn't the first container of this type ever created, so that valid static safety data exists
        var container = new NativeHashMap<int, int>(10, Allocator.TempJob);
        container.Dispose();

        var job = new NativeHashMap_CreateAndUseAfterFreeBurst
        {
        };

        // Two things:
        // 1. This exception is logged, not thrown; thus, we use LogAssert to detect it.
        // 2. Calling write operation after container.Dispose() emits an unintuitive error message. For now, all this test cares about is whether it contains the
        //    expected type name.
        job.Run();
        LogAssert.Expect(LogType.Exception,
            new Regex($"InvalidOperationException: The {Regex.Escape(container.GetType().ToString())} has been declared as \\[ReadOnly\\] in the job, but you are writing to it"));
    }
#endif

    [Test]
    public void NativeHashMap_ForEach()
    {
        using (var container = new NativeHashMap<int, int>(32, Allocator.TempJob))
        {
            container.Add(0, 012);
            container.Add(1, 123);
            container.Add(2, 234);
            container.Add(3, 345);
            container.Add(4, 456);
            container.Add(5, 567);
            container.Add(6, 678);
            container.Add(7, 789);
            container.Add(8, 890);
            container.Add(9, 901);

            var count = 0;
            foreach (var kv in container)
            {
                int value;
                Assert.True(container.TryGetValue(kv.Key, out value));
                Assert.AreEqual(value, kv.Value);

                ++count;
            }

            Assert.AreEqual(container.Count(), count);
        }
    }

    [Test]
    public void NativeHashMap_ForEach_Throws_When_Modified()
    {
        using (var container = new NativeHashMap<int, int>(32, Allocator.TempJob))
        {
            container.Add(0, 012);
            container.Add(1, 123);
            container.Add(2, 234);
            container.Add(3, 345);
            container.Add(4, 456);
            container.Add(5, 567);
            container.Add(6, 678);
            container.Add(7, 789);
            container.Add(8, 890);
            container.Add(9, 901);

#if UNITY_2020_2_OR_NEWER
            Assert.Throws<ObjectDisposedException>(() =>
#else
            Assert.Throws<InvalidOperationException>(() =>
#endif
            {
                foreach (var kv in container)
                {
                    container.Add(10, 10);
                }
            });

#if UNITY_2020_2_OR_NEWER
            Assert.Throws<ObjectDisposedException>(() =>
#else
            Assert.Throws<InvalidOperationException>(() =>
#endif
            {
                foreach (var kv in container)
                {
                    container.Remove(1);
                }
            });
        }
    }

    struct NativeHashMap_ForEachIterator : IJob
    {
        [ReadOnly]
        public NativeHashMap<int, int>.Enumerator Iter;

        public void Execute()
        {
            while (Iter.MoveNext())
            {
            }
        }
    }

    [Test]
    public void NativeHashMap_ForEach_Throws_Job_Iterator()
    {
        using (var container = new NativeHashMap<int, int>(32, Allocator.TempJob))
        {
            var jobHandle = new NativeHashMap_ForEachIterator
            {
                Iter = container.GetEnumerator()

            }.Schedule();

            Assert.Throws<InvalidOperationException>(() => { container.Add(1, 1); });

            jobHandle.Complete();
        }
    }

    struct ParallelWriteToHashMapJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeHashMap<int, int>.ParallelWriter Writer;

        public void Execute(int index)
        {
            Writer.TryAdd(index, 0);
        }
    }

    [Test]
    public void NativeHashMap_ForEach_Throws()
    {
        using (var container = new NativeHashMap<int, int>(32, Allocator.TempJob))
        {
            var iter = container.GetEnumerator();

            var jobHandle = new ParallelWriteToHashMapJob
            {
                Writer = container.AsParallelWriter()

            }.Schedule(1, 2);

#if UNITY_2020_2_OR_NEWER
            Assert.Throws<ObjectDisposedException>(() =>
#else
            Assert.Throws<InvalidOperationException>(() =>
#endif
            {
                while (iter.MoveNext())
                {
                }
            });

            jobHandle.Complete();
        }
    }

    [Test]
    public void NativeMultiHashMap_IsEmpty()
    {
        var hashMap = new NativeMultiHashMap<int, int>(0, Allocator.Persistent);
        Assert.IsTrue(hashMap.IsEmpty);

        hashMap.Add(0, 0);
        Assert.IsFalse(hashMap.IsEmpty);
        Assert.AreEqual(1, hashMap.Capacity);
        ExpectedCount(ref hashMap, 1);

        hashMap.Clear();
        Assert.IsTrue(hashMap.IsEmpty);

        hashMap.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_CountValuesForKey()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        hashMap.Add(5, 7);
        hashMap.Add(6, 9);
        hashMap.Add(6, 10);

        Assert.AreEqual(1, hashMap.CountValuesForKey(5));
        Assert.AreEqual(2, hashMap.CountValuesForKey(6));
        Assert.AreEqual(0, hashMap.CountValuesForKey(7));

        hashMap.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_RemoveKeyAndValue()
    {
        var hashMap = new NativeMultiHashMap<int, long>(1, Allocator.Temp);
        hashMap.Add(10, 0);
        hashMap.Add(10, 1);
        hashMap.Add(10, 2);

        hashMap.Add(20, 2);
        hashMap.Add(20, 2);
        hashMap.Add(20, 1);
        hashMap.Add(20, 2);
        hashMap.Add(20, 1);

        hashMap.Remove(10, 1L);
        ExpectValues(hashMap, 10, new[] { 0L, 2L });
        ExpectValues(hashMap, 20, new[] { 1L, 1L, 2L, 2L, 2L });

        hashMap.Remove(20, 2L);
        ExpectValues(hashMap, 10, new[] { 0L, 2L });
        ExpectValues(hashMap, 20, new[] { 1L , 1L});

        hashMap.Remove(20, 1L);
        ExpectValues(hashMap, 10, new[] { 0L, 2L });
        ExpectValues(hashMap, 20, new long[0]);

        hashMap.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_ValueIterator()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp);
        hashMap.Add(5, 0);
        hashMap.Add(5, 1);
        hashMap.Add(5, 2);

        var list = new NativeList<int>(Allocator.TempJob);

        GCAllocRecorder.ValidateNoGCAllocs(() =>
        {
            list.Clear();
            foreach (var value in hashMap.GetValuesForKey(5))
                list.Add(value);
        });

        list.Sort();
        Assert.AreEqual(list.ToArray(), new int[] { 0, 1, 2 });

        foreach (var value in hashMap.GetValuesForKey(6))
            Assert.Fail();

        list.Dispose();
        hashMap.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_RemoveKeyValueDoesntDeallocate()
    {
        var hashMap = new NativeMultiHashMap<int, int>(1, Allocator.Temp) { { 5, 1 } };

        hashMap.Remove(5, 5);
        GCAllocRecorder.ValidateNoGCAllocs(() =>
        {
            hashMap.Remove(5, 1);
        });

        hashMap.Dispose();
    }

    [Test]
    public void NativeMultiHashMap_Double_Deallocate_Throws()
    {
        var hashMap = new NativeMultiHashMap<int, int>(16, Allocator.TempJob);
        hashMap.Dispose();
#if UNITY_2020_2_OR_NEWER
        Assert.Throws<ObjectDisposedException>(
#else
        Assert.Throws<InvalidOperationException>(
#endif
            () => { hashMap.Dispose(); });
    }
}