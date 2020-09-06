using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daenet.Common.Logging.Sql
{
    /// <summary>
    /// Handles scopes.
    /// </summary>
    internal class ScopeManager
    {
        internal static readonly AsyncLocal<List<DisposableScope>> AsyncSopes = new AsyncLocal<List<DisposableScope>>();

        private object _state;

        internal ScopeManager(object state)
        {
            _state = state;
        }

        public string Current
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in AsyncSopes.Value)
                {
                    sb.Append($"/{item}");
                }

                return sb.ToString();
            }
        }

        public IDisposable Push(object state)
        {
            lock ("scope")
            {
                if (AsyncSopes.Value == null)
                    AsyncSopes.Value = new List<DisposableScope>();

                var newScope = new DisposableScope(state.ToString(), this);

                AsyncSopes.Value.Add(newScope);

                return newScope;
            }
        }

        public override string ToString()
        {
            return _state?.ToString();
        }

        internal class DisposableScope : IDisposable
        {
            private ScopeManager _scopeMgr;
            private string _scopeName;

            public DisposableScope(string scopeName, ScopeManager scopeMgr)
            {
                _scopeName = scopeName;
                _scopeMgr = scopeMgr;
            }

            public void Dispose()
            {
               // lock ("scope")
                //{
                    var me = AsyncSopes.Value.FirstOrDefault(s => s == this);
                    if (me == null)
                    {
                        throw new InvalidOperationException("This should never happen!");
                    }

                    AsyncSopes.Value.Remove(me);
               // }
            }

            public override string ToString()
            {
                return _scopeName;
            }
        }
    }
}
