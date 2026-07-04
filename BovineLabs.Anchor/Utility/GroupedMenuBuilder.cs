// <copyright file="GroupedMenuBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using Unity.AppUI.UI;

    /// <summary> Extension helpers for building large AppUI menus with balanced submenus. </summary>
    public static class GroupedMenuBuilder
    {
        private const string FallbackGroupKey = "#";

        private static readonly IComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;

        /// <summary> Adds actions to a menu, grouping by first letter when the item count exceeds the configured menu cap. </summary>
        public static MenuBuilder AddGroupedActions<T>(
            this MenuBuilder builder,
            IReadOnlyList<T> items,
            Func<T, string> labelSelector,
            Action<T> callback,
            GroupedMenuBuilderOptions options = default)
        {
            return builder.AddGroupedActions(items, labelSelector, null, null, callback, options);
        }

        /// <summary> Adds actions to a menu, grouping first by a caller-owned key and then by label prefixes for oversized groups. </summary>
        public static MenuBuilder AddGroupedActions<T>(
            this MenuBuilder builder,
            IReadOnlyList<T> items,
            Func<T, string> labelSelector,
            Func<T, string> primaryGroupSelector,
            Action<T> callback,
            GroupedMenuBuilderOptions options = default)
        {
            return builder.AddGroupedActions(items, labelSelector, primaryGroupSelector, null, callback, options);
        }

        /// <summary> Adds actions to a menu, grouping first by a caller-owned key and using a caller-owned group order. </summary>
        public static MenuBuilder AddGroupedActions<T>(
            this MenuBuilder builder,
            IReadOnlyList<T> items,
            Func<T, string> labelSelector,
            Func<T, string> primaryGroupSelector,
            IComparer<string> primaryGroupComparer,
            Action<T> callback,
            GroupedMenuBuilderOptions options = default)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (labelSelector == null)
            {
                throw new ArgumentNullException(nameof(labelSelector));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (items.Count == 0)
            {
                return builder;
            }

            var groupComparer = primaryGroupComparer ?? DefaultComparer;
            var entries = CreateEntries(items, labelSelector, primaryGroupSelector, groupComparer);
            var maxItems = options.EffectiveMaxItemsPerMenu;

            if (entries.Count <= maxItems)
            {
                AddActions(builder, entries, callback);
                return builder;
            }

            var buckets = CreateBuckets(
                entries,
                static entry => entry.PrimaryGroupKey,
                entry => primaryGroupSelector == null ? GetNextDefaultPrefixLength(entry.PrimaryGroupKey) : 1,
                groupComparer);

            if (buckets.Count <= 1)
            {
                BuildPrefixGroups(builder, entries, callback, options, primaryGroupSelector == null ? GetNextDefaultPrefixLength(buckets[0].Key) : 1);
            }
            else
            {
                BuildBuckets(builder, buckets, callback, options, groupComparer);
            }

            return builder;
        }

        private static List<Entry<T>> CreateEntries<T>(
            IReadOnlyList<T> items,
            Func<T, string> labelSelector,
            Func<T, string> primaryGroupSelector,
            IComparer<string> groupComparer)
        {
            var entries = new List<Entry<T>>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var label = labelSelector(item) ?? string.Empty;
                var normalizedLabel = NormalizeLabel(label);
                var groupKey = primaryGroupSelector == null
                    ? GetDefaultPrimaryGroupKey(normalizedLabel)
                    : NormalizeGroupKey(primaryGroupSelector(item));

                entries.Add(new Entry<T>(item, i, label, normalizedLabel, groupKey));
            }

            entries.Sort(new EntryComparer<T>(groupComparer));
            return entries;
        }

        private static void BuildBuckets<T>(
            MenuBuilder builder,
            List<Bucket<T>> buckets,
            Action<T> callback,
            GroupedMenuBuilderOptions options,
            IComparer<string> groupComparer)
        {
            if (buckets.Count == 0)
            {
                return;
            }

            var maxItems = options.EffectiveMaxItemsPerMenu;
            if (buckets.Count == 1)
            {
                BuildBucket(builder, buckets[0], callback, options, groupComparer);
                return;
            }

            if (buckets.Count > maxItems)
            {
                var ranges = PartitionBuckets(buckets, maxItems);
                foreach (var range in ranges)
                {
                    if (range.Buckets.Count == 1)
                    {
                        BuildBucket(builder, range.Buckets[0], callback, options, groupComparer);
                        continue;
                    }

                    builder.PushSubMenu(GetGroupActionId(range.Label), range.Label, null);
                    BuildBuckets(builder, range.Buckets, callback, options, groupComparer);
                    builder.Pop();
                }

                return;
            }

            foreach (var bucket in buckets)
            {
                BuildBucket(builder, bucket, callback, options, groupComparer);
            }
        }

        private static void BuildBucket<T>(
            MenuBuilder builder,
            Bucket<T> bucket,
            Action<T> callback,
            GroupedMenuBuilderOptions options,
            IComparer<string> groupComparer)
        {
            if (bucket.Entries.Count == 1 && options.FlattenSingleItemGroups)
            {
                AddAction(builder, bucket.Entries[0], callback);
                return;
            }

            builder.PushSubMenu(GetGroupActionId(bucket.Key), bucket.Key, null);
            if (bucket.Entries.Count <= options.EffectiveMaxItemsPerMenu)
            {
                AddActions(builder, bucket.Entries, callback);
            }
            else
            {
                BuildPrefixGroups(builder, bucket.Entries, callback, options, bucket.NextPrefixLength, groupComparer);
            }

            builder.Pop();
        }

        private static void BuildPrefixGroups<T>(
            MenuBuilder builder,
            List<Entry<T>> entries,
            Action<T> callback,
            GroupedMenuBuilderOptions options,
            int prefixLength,
            IComparer<string> groupComparer = null)
        {
            if (entries.Count <= options.EffectiveMaxItemsPerMenu)
            {
                AddActions(builder, entries, callback);
                return;
            }

            var comparer = groupComparer ?? DefaultComparer;
            var length = Math.Max(1, prefixLength);
            var maxPrefixLength = options.EffectiveMaxPrefixLength;
            List<Bucket<T>> buckets;

            do
            {
                var capturedLength = length;
                buckets = CreateBuckets(entries, entry => GetLabelPrefix(entry.NormalizedLabel, capturedLength), _ => capturedLength + 1, comparer);
                if (buckets.Count > 1 || length >= maxPrefixLength)
                {
                    break;
                }

                length++;
            }
            while (true);

            if (buckets.Count <= 1)
            {
                AddActions(builder, entries, callback);
                return;
            }

            BuildBuckets(builder, buckets, callback, options, comparer);
        }

        private static List<Bucket<T>> CreateBuckets<T>(
            List<Entry<T>> entries,
            Func<Entry<T>, string> keySelector,
            Func<Entry<T>, int> nextPrefixLengthSelector,
            IComparer<string> comparer)
        {
            var buckets = new List<Bucket<T>>();
            foreach (var entry in entries)
            {
                var key = keySelector(entry);
                if (buckets.Count == 0 || comparer.Compare(buckets[buckets.Count - 1].Key, key) != 0)
                {
                    buckets.Add(new Bucket<T>(key, nextPrefixLengthSelector(entry)));
                }

                buckets[buckets.Count - 1].Entries.Add(entry);
            }

            return buckets;
        }

        private static List<BucketRange<T>> PartitionBuckets<T>(List<Bucket<T>> buckets, int maxItems)
        {
            var totalItems = 0;
            foreach (var bucket in buckets)
            {
                totalItems += bucket.Entries.Count;
            }

            var rangeCount = Math.Min(maxItems, Math.Max(2, (int)Math.Ceiling(totalItems / (double)maxItems)));
            rangeCount = Math.Min(rangeCount, buckets.Count);

            var ranges = new List<BucketRange<T>>(rangeCount);
            var bucketIndex = 0;
            var remainingItems = totalItems;

            for (var rangeIndex = 0; rangeIndex < rangeCount; rangeIndex++)
            {
                var remainingRanges = rangeCount - rangeIndex;
                var targetItems = (int)Math.Ceiling(remainingItems / (double)remainingRanges);
                var range = new BucketRange<T>();
                var rangeItems = 0;

                while (bucketIndex < buckets.Count)
                {
                    var bucketsLeftAfterThis = buckets.Count - bucketIndex - 1;
                    if (range.Buckets.Count > 0 && rangeItems >= targetItems && bucketsLeftAfterThis >= remainingRanges - 1)
                    {
                        break;
                    }

                    var bucket = buckets[bucketIndex++];
                    range.Buckets.Add(bucket);
                    rangeItems += bucket.Entries.Count;
                    remainingItems -= bucket.Entries.Count;

                    if (bucketsLeftAfterThis < remainingRanges - 1)
                    {
                        break;
                    }
                }

                range.Label = GetRangeLabel(range.Buckets);
                ranges.Add(range);
            }

            return ranges;
        }

        private static void AddActions<T>(MenuBuilder builder, List<Entry<T>> entries, Action<T> callback)
        {
            foreach (var entry in entries)
            {
                AddAction(builder, entry, callback);
            }
        }

        private static void AddAction<T>(MenuBuilder builder, Entry<T> entry, Action<T> callback)
        {
            builder.AddAction(entry.Index, entry.Label, null, _ => callback(entry.Item));
        }

        private static string NormalizeLabel(string label)
        {
            return string.IsNullOrWhiteSpace(label) ? string.Empty : label.Trim();
        }

        private static string NormalizeGroupKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? FallbackGroupKey : key.Trim();
        }

        private static string GetDefaultPrimaryGroupKey(string normalizedLabel)
        {
            if (string.IsNullOrEmpty(normalizedLabel))
            {
                return FallbackGroupKey;
            }

            var first = normalizedLabel[0];
            return char.IsLetter(first) ? char.ToUpperInvariant(first).ToString() : FallbackGroupKey;
        }

        private static int GetNextDefaultPrefixLength(string primaryGroupKey)
        {
            return primaryGroupKey.Length == 1 && char.IsLetter(primaryGroupKey[0]) ? 2 : 1;
        }

        private static string GetLabelPrefix(string normalizedLabel, int prefixLength)
        {
            if (string.IsNullOrEmpty(normalizedLabel))
            {
                return FallbackGroupKey;
            }

            var length = Math.Min(normalizedLabel.Length, prefixLength);
            var buffer = new char[length];
            for (var i = 0; i < length; i++)
            {
                var c = normalizedLabel[i];
                buffer[i] = char.IsLetter(c) ? char.ToUpperInvariant(c) : char.IsDigit(c) ? c : '#';
            }

            return new string(buffer);
        }

        private static string GetRangeLabel<T>(List<Bucket<T>> buckets)
        {
            if (buckets.Count == 0)
            {
                return FallbackGroupKey;
            }

            var first = buckets[0].Key;
            var last = buckets[buckets.Count - 1].Key;
            return first == last ? first : $"{first}-{last}";
        }

        private static int GetGroupActionId(string label)
        {
            unchecked
            {
                var hash = (int)2166136261;
                for (var i = 0; i < label.Length; i++)
                {
                    hash = (hash ^ label[i]) * 16777619;
                }

                return hash < 0 ? hash : -hash;
            }
        }

        private readonly struct Entry<T>
        {
            public Entry(T item, int index, string label, string normalizedLabel, string primaryGroupKey)
            {
                this.Item = item;
                this.Index = index;
                this.Label = label;
                this.NormalizedLabel = normalizedLabel;
                this.PrimaryGroupKey = primaryGroupKey;
            }

            public T Item { get; }

            public int Index { get; }

            public string Label { get; }

            public string NormalizedLabel { get; }

            public string PrimaryGroupKey { get; }
        }

        private sealed class EntryComparer<T> : IComparer<Entry<T>>
        {
            private readonly IComparer<string> groupComparer;

            public EntryComparer(IComparer<string> groupComparer)
            {
                this.groupComparer = groupComparer;
            }

            public int Compare(Entry<T> x, Entry<T> y)
            {
                var result = this.groupComparer.Compare(x.PrimaryGroupKey, y.PrimaryGroupKey);
                if (result != 0)
                {
                    return result;
                }

                result = StringComparer.OrdinalIgnoreCase.Compare(x.NormalizedLabel, y.NormalizedLabel);
                if (result != 0)
                {
                    return result;
                }

                result = StringComparer.Ordinal.Compare(x.NormalizedLabel, y.NormalizedLabel);
                return result != 0 ? result : x.Index.CompareTo(y.Index);
            }
        }

        private sealed class Bucket<T>
        {
            public Bucket(string key, int nextPrefixLength)
            {
                this.Key = key;
                this.NextPrefixLength = nextPrefixLength;
            }

            public string Key { get; }

            public int NextPrefixLength { get; }

            public List<Entry<T>> Entries { get; } = new();
        }

        private sealed class BucketRange<T>
        {
            public string Label { get; set; }

            public List<Bucket<T>> Buckets { get; } = new();
        }
    }
}
