using Mud.Container;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.DataStructures.Flags
{
    public class Flags : IFlags<string>
    {
        private readonly HashSet<string> _hashSet;

        public Flags()
        {
            _hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public Flags(params string[] flags)
            : this()
        {
            Set(flags);
        }

        #region IFlags

        public bool IsSet(string flag) => _hashSet.Contains(flag);

        public bool HasAny(params string[] flags) => flags.Any(x => _hashSet.Contains(x));

        public bool HasAll(params string[] flags) => flags.All(x => _hashSet.Contains(x));

        public bool Set(string flag) => _hashSet.Add(flag);
        public void Set(params string[] flags)
        {
            foreach (string flag in flags)
                _hashSet.Add(flag);
        }

        public bool Unset(string flag) => _hashSet.Remove(flag);
        public void Unset(params string[] flags)
        {
            foreach (string flag in flags)
                _hashSet.Remove(flag);
        }

        public int Count => _hashSet.Count;

        public IEnumerable<string> Items => _hashSet;

        #endregion

        public bool HasAny(Flags flags) => flags.Items.Any(x => _hashSet.Contains(x));
        public bool HasAll(Flags flags) => flags.Items.All(x => _hashSet.Contains(x));
        public void Set(Flags flags)
        {
            _hashSet.UnionWith(flags.Items);
        }

        public static bool TryParse(string s, out Flags flags) // TryParse never fails :p
        {
            flags = new Flags();
            if (string.IsNullOrWhiteSpace(s))
                return true;

            flags.Set(s.Split(','));
            return true;
        }

        public static Flags Parse(string s)
        {
            Flags flags = new Flags();
            if (string.IsNullOrWhiteSpace(s))
                return flags;

            flags.Set(s.Split(','));
            return flags;
        }

        public override string ToString() => string.Join(",", _hashSet.OrderBy(x => x));
    }

    public class Flags<TFlagValues> : IFlags<string, TFlagValues>
        where TFlagValues : IFlagValues<string>
    {
        private static readonly Lazy<TFlagValues> LazyFlagValues = new Lazy<TFlagValues>(() => (TFlagValues)DependencyContainer.Current.GetInstance(typeof(TFlagValues)));

        private TFlagValues FlagValues => LazyFlagValues.Value;

        private readonly HashSet<string> _hashSet;

        public Flags()
        {
            _hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }
        
        public Flags(params string[] flags)
            : this()
        {
            Set(flags);
        }

        #region IFlags

        public bool IsSet(string flag)
        {
            if (!CheckValues(flag))
                throw new ArgumentException($"Flag '{flag}' not found in {typeof(TFlagValues).FullName}", nameof(flag));
            return _hashSet.Contains(flag);
        }
        public bool HasAny(params string[] flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            return flags.Any(x => _hashSet.Contains(x));
        }
        public bool HasAll(params string[] flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            return flags.All(x => _hashSet.Contains(x));
        }

        public bool Set(string flag)
        {
            if (!CheckValues(flag))
                throw new ArgumentException($"Flag '{flag}' not found in {typeof(TFlagValues).FullName}", nameof(flag));
            return _hashSet.Add(flag);
        }
        public void Set(params string[] flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            foreach (string flag in flags)
                _hashSet.Add(flag);
        }

        public bool Unset(string flag)
        {
            if (!CheckValues(flag))
                throw new ArgumentException($"Flag '{flag}' not found in {typeof(TFlagValues).FullName}", nameof(flag));
            return _hashSet.Remove(flag);
        }
        public void Unset(params string[] flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            foreach (string flag in flags)
                _hashSet.Remove(flag);
        }

        public int Count => _hashSet.Count;

        public IEnumerable<string> Items => _hashSet;

        #endregion

        public bool HasAny(Flags<TFlagValues> flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            return flags._hashSet.Any(x => _hashSet.Contains(x));
        }

        public bool HasAll(Flags<TFlagValues> flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            return flags._hashSet.All(x => _hashSet.Contains(x));
        }

        public void Set(Flags<TFlagValues> flags)
        {
            if (!CheckValues(flags))
                throw new ArgumentException($"Flags '{string.Join(",", flags)}' not found in {typeof(TFlagValues).FullName}", nameof(flags));
            foreach (string flag in flags._hashSet)
                _hashSet.Add(flag);
        }

        public static bool TryParse(string s, out Flags<TFlagValues> flags) // TryParse never fails :p
        {
            flags = new Flags<TFlagValues>();
            if (string.IsNullOrWhiteSpace(s))
                return true;

            flags.Set(s.Split(','));
            return true;
        }

        public static Flags<TFlagValues> Parse(string s)
        {
            Flags<TFlagValues> flags = new Flags<TFlagValues>();
            if (string.IsNullOrWhiteSpace(s))
                return flags;

            flags.Set(s.Split(','));
            return flags;
        }

        public override string ToString() => string.Join(",", _hashSet.OrderBy(x => x));

        private bool CheckValues(params string[] flags) => flags.All(x => FlagValues.AvailableValues.Contains(x));
        private bool CheckValues(Flags<TFlagValues> flags) => flags.Items.All(x => FlagValues.AvailableValues.Contains(x));
    }
}
