﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Daenet.Common.Logging.Sql
{
    class SqlLoggerScope
    {
        private readonly string _name;
        private readonly object _state;

        internal SqlLoggerScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public SqlLoggerScope Parent { get; private set; }
        public string[] ScopeInformation { get; set; }

        private static AsyncLocal<SqlLoggerScope> _value = new AsyncLocal<SqlLoggerScope>();
        public static SqlLoggerScope Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new SqlLoggerScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        internal object CurrentValue
        {
            get
            {
                return _state;
            }
        }

        public override string ToString()
        {
            return _state?.ToString();
        }

        internal string[] GetScopeInformation(ISqlLoggerSettings settings)
        {
            if (settings.ScopeColumnMapping != null || settings.ScopeColumnMapping.Count > 0)
            {
                string[] dictionary;
                if (ScopeInformation == null)
                {
                    dictionary = new string[settings.ScopeColumnMapping.Count()];
                    var builder = new StringBuilder();
                    var current = this;
                    var scopeLog = string.Empty;
                    var length = builder.Length;
                   
                    //Is adding scope path configured
                    var addScopePath = !string.IsNullOrEmpty(settings.ScopeColumnMapping.FirstOrDefault(k => k.Key == "SCOPEPATH").Key);
                   
                    while (current != null)
                    {
                        if (current.CurrentValue is IEnumerable<KeyValuePair<string, object>>)
                        {
                            foreach (var item in (IEnumerable<KeyValuePair<string, object>>)current.CurrentValue)
                            {
                                // TODO: For performance reasons we need to remove FirstOrDefault and additional IndexOf call and use only one call.
                                var map = settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == item.Key);
                                if (!String.IsNullOrEmpty(map.Key))
                                {
                                    dictionary[settings.ScopeColumnMapping.IndexOf(map)] = item.Value.ToString();
                                }
                            }
                        }
                        if (addScopePath)
                        {
                            if (length == builder.Length)
                            {
                                scopeLog = $"{settings.ScopeSeparator}{current}";
                            }
                            else
                            {
                                scopeLog = $"{settings.ScopeSeparator}{current} ";
                            }

                            builder.Insert(length, scopeLog);
                        }
                        current = current.Parent;
                    }
                    if (addScopePath)
                    {
                        var map = settings.ScopeColumnMapping.FirstOrDefault(a => a.Key == "SCOPEPATH");
                        dictionary[settings.ScopeColumnMapping.IndexOf(map)] = builder.ToString();
                    }

                    this.ScopeInformation = dictionary.ToArray();
                }
                else
                {
                    dictionary = ScopeInformation;
                }
            }
            else
                ScopeInformation = new string[settings.ScopeColumnMapping.Count()];

            return ScopeInformation;
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
