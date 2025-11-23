using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Kartverket.Tests.Helpers
{
    /// <summary>
    /// Test implementation of ISession for unit testing
    /// </summary>
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => "test-session-id";
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear()
        {
            _store.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _store[key] = value;
        }

        public bool TryGetValue(string key, out byte[]? value)
        {
            return _store.TryGetValue(key, out value);
        }

        // Helper methods for easier testing
        public void SetInt32(string key, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Set(key, bytes);
        }

        public int? GetInt32(string key)
        {
            if (TryGetValue(key, out var value) && value != null && value.Length == 4)
            {
                return BitConverter.ToInt32(value, 0);
            }
            return null;
        }
    }
}

