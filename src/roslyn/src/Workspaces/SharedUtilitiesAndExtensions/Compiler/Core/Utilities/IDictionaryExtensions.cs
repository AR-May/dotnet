﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal static class IDictionaryExtensions
{
    // Copied from ConcurrentDictionary since IDictionary doesn't have this useful method
    public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<K, V> function)
        where K : notnull
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = function(key);
            dictionary.Add(key, value);
        }

        return value;
    }

    public static V GetOrAdd<K, V, TArg>(this IDictionary<K, V> dictionary, K key, Func<K, TArg, V> function, TArg arg)
        where K : notnull
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = function(key, arg);
            dictionary.Add(key, value);
        }

        return value;
    }

    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        return default!;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public static void MultiAdd<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value)
        where TKey : notnull
        where TCollection : ICollection<TValue>, new()
    {
        if (!dictionary.TryGetValue(key, out var collection))
        {
            collection = new TCollection();
            dictionary.Add(key, collection);
        }

        collection.Add(value);
    }

    public static void MultiAdd<TKey, TValue>(this IDictionary<TKey, ArrayBuilder<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var builder))
        {
            builder = ArrayBuilder<TValue>.GetInstance();
            dictionary.Add(key, builder);
        }

        builder.Add(value);
    }

    public static void MultiAddRange<TKey, TValue>(this IDictionary<TKey, ArrayBuilder<TValue>> dictionary, TKey key, IEnumerable<TValue> values)
        where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var builder))
        {
            builder = ArrayBuilder<TValue>.GetInstance();
            dictionary.Add(key, builder);
        }

        builder.AddRange(values);
    }

    public static bool MultiAdd<TKey, TValue>(this IDictionary<TKey, ImmutableHashSet<TValue>> dictionary, TKey key, TValue value, IEqualityComparer<TValue>? comparer = null)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var set))
        {
            var updated = set.Add(value);
            if (set == updated)
                return false;

            dictionary[key] = updated;
            return true;
        }
        else
        {
            dictionary[key] = ImmutableHashSet.Create(comparer, value);
            return true;
        }
    }

    public static void MultiAdd<TKey, TValue>(this IDictionary<TKey, ImmutableArray<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
        where TValue : IEquatable<TValue>
    {
        if (!dictionary.TryGetValue(key, out var existingArray))
        {
            existingArray = [];
        }

        dictionary[key] = existingArray.Add(value);
    }

    public static void MultiAdd<TKey, TValue>(this IDictionary<TKey, ImmutableArray<TValue>> dictionary, TKey key, TValue value, ImmutableArray<TValue> defaultArray)
        where TKey : notnull
        where TValue : IEquatable<TValue>
    {
        if (!dictionary.TryGetValue(key, out var existingArray))
        {
            existingArray = [];
        }

        dictionary[key] = existingArray.IsEmpty && value.Equals(defaultArray[0]) ? defaultArray : existingArray.Add(value);
    }

    public static void MultiRemove<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value)
        where TKey : notnull
        where TCollection : ICollection<TValue>
    {
        if (dictionary.TryGetValue(key, out var collection))
        {
            collection.Remove(value);

            if (collection.Count == 0)
            {
                dictionary.Remove(key);
            }
        }
    }

    public static ImmutableDictionary<TKey, ImmutableHashSet<TValue>> MultiRemove<TKey, TValue>(this ImmutableDictionary<TKey, ImmutableHashSet<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var collection))
        {
            collection = collection.Remove(value);
            if (collection.IsEmpty)
            {
                return dictionary.Remove(key);
            }
            else
            {
                return dictionary.SetItem(key, collection);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Private implementation we can delegate to for sets.
    /// This must be a different name as overloads are not resolved based on constraints
    /// and would conflict with <see cref="MultiRemove{TKey, TValue, TCollection}(IDictionary{TKey, TCollection}, TKey, TValue)"/>
    /// </summary>
    private static void MultiRemoveSet<TKey, TValue, TSet>(this IDictionary<TKey, TSet> dictionary, TKey key, TValue value)
        where TKey : notnull
        where TSet : IImmutableSet<TValue>
    {
        if (dictionary.TryGetValue(key, out var collection))
        {
            collection = (TSet)collection.Remove(value);
            if (collection.IsEmpty())
            {
                dictionary.Remove(key);
            }
            else
            {
                dictionary[key] = collection;
            }
        }
    }

    public static void MultiRemove<TKey, TValue>(this IDictionary<TKey, ImmutableHashSet<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        MultiRemoveSet(dictionary, key, value);
    }

    public static void MultiRemove<TKey, TValue>(this IDictionary<TKey, ImmutableSortedSet<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        MultiRemoveSet(dictionary, key, value);
    }

    public static void MultiRemove<TKey, TValue>(this IDictionary<TKey, ImmutableArray<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var collection))
        {
            if (collection.Length == 1 && EqualityComparer<TValue>.Default.Equals(collection[0], value))
            {
                dictionary.Remove(key);
            }
            else
            {
                dictionary[key] = collection.Remove(value);
            }
        }
    }

    /// <summary>
    /// Removes entries from a dictionary based on a specified condition. The condition is defined by a function that
    /// evaluates each key-value pair.
    /// </summary>
    public static void RemoveAll<TKey, TValue, TArg>(this Dictionary<TKey, TValue> dictionary, Func<TKey, TValue, TArg, bool> predicate, TArg arg)
        where TKey : notnull
    {
#if NET
        // .NET supports removing while enumerating:
        foreach (var entry in dictionary)
        {
            if (predicate(entry.Key, entry.Value, arg))
            {
                dictionary.Remove(entry.Key);
            }
        }
#else
        if (dictionary.Count == 0)
        {
            return;
        }

        using var _ = ArrayBuilder<TKey>.GetInstance(out var keysToRemove);
        foreach (var entry in dictionary)
        {
            if (predicate(entry.Key, entry.Value, arg))
            {
                keysToRemove.Add(entry.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
#endif
    }
}
