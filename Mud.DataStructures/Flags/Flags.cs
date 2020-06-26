﻿using Mud.Common;
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

        public string Map() => string.Join(",", _hashSet.OrderBy(x => x));

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
    }

    public abstract class Flags<TFlagValues> : IFlags<string, TFlagValues>
        where TFlagValues : IFlagValues<string>
    {
        //DependencyContainer.Current.RegisterInstance<ICharacterFlagValues>(new Rom24CharacterFlags()); // TODO: do this with reflection ?
        //DependencyContainer.Current.RegisterInstance<IRoomFlagValues>(new Rom24RoomFlags()); // TODO: do this with reflection ?
        private static readonly Lazy<TFlagValues> LazyFlagValues = new Lazy<TFlagValues>(() => (TFlagValues)DependencyContainer.Current.GetInstance(typeof(TFlagValues)));

        private TFlagValues FlagValues => LazyFlagValues.Value;

        private readonly HashSet<string> _hashSet;

        protected Flags()
        {
            _hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }

        protected Flags(string flags)
            : this()
        {
            if (!string.IsNullOrWhiteSpace(flags))
                Set(flags.Split(','));
        }

        protected Flags(params string[] flags)
            : this()
        {
            Set(flags);
        }

        #region IFlags

        public bool IsSet(string flag)
        {
            if (!CheckValues(flag))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.IsSet, flag.Yield());
            return _hashSet.Contains(flag);
        }
        public bool HasAny(params string[] flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAny, flags);
            return flags.Any(x => _hashSet.Contains(x));
        }
        public bool HasAny(IFlags<string, TFlagValues> flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAny, flags.Items);
            return flags.Items.Any(x => _hashSet.Contains(x));
        }
        public bool HasAll(params string[] flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAll, flags);
            return flags.All(x => _hashSet.Contains(x));
        }
        public bool HasAll(IFlags<string, TFlagValues> flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAll, flags.Items);
            return flags.Items.All(x => _hashSet.Contains(x));
        }

        public bool Set(string flag)
        {
            if (!CheckValues(flag))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.Set, flag.Yield());
            return _hashSet.Add(flag);
        }
        public void Set(params string[] flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.Set, flags);
            foreach (string flag in flags)
                _hashSet.Add(flag);
        }
        public void Set(IFlags<string, TFlagValues> flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.Set, flags.Items);
            foreach (string flag in flags.Items)
                _hashSet.Add(flag);
        }

        public bool Unset(string flag)
        {
            if (!CheckValues(flag))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.UnSet, flag.Yield());
            return _hashSet.Remove(flag);
        }
        public void Unset(params string[] flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.UnSet, flags);
            foreach (string flag in flags)
                _hashSet.Remove(flag);
        }
        public void Unset(IFlags<string, TFlagValues> flags)
        {
            if (!CheckValues(flags))
                FlagValues.OnUnknownValues(UnknownFlagValueContext.UnSet, flags.Items);
            foreach (string flag in flags.Items)
                _hashSet.Remove(flag);
        }

        public int Count => _hashSet.Count;

        public IEnumerable<string> Items => _hashSet;

        public string Map() => string.Join(",", _hashSet.OrderBy(x => x));

        #endregion

        private bool CheckValues(params string[] flags) => flags.All(x => FlagValues.AvailableValues.Contains(x));
        private bool CheckValues(IFlags<string, TFlagValues> flags) => flags.Items.All(x => FlagValues.AvailableValues.Contains(x));
    }
}
