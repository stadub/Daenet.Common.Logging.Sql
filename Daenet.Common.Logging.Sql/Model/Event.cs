using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Daenet.Common.Logging.Sql
{
    public class Event
    {
        private List<object> _properties = new List<object>();

        private static readonly Dictionary<string, int> propertiesMapping = new Dictionary<string, int>
        {
            {nameof(EventId), 0},
            {nameof(Type), 1},
            {nameof(Message), 2},
            {nameof(Timestamp), 3},
            {nameof(CategoryName), 4},
            {nameof(Exception), 5}
        };


        private T GetPropertyValue<T>([CallerMemberName] string propName = "")
        {
            var propIndex = propertiesMapping[propName];
            var propValue = _properties[propIndex];
            return (T)propValue;
        }

        private void SetPropertyValue<T>(T value, [CallerMemberName] string propName = "")
        {
            var propIndex = propertiesMapping[propName];
            _properties[propIndex] = value;
        }

        public static List<KeyValuePair<string, int>> PropertiesMapping => propertiesMapping.ToList();

        public int EventId
        {
            get => GetPropertyValue<int>(nameof(EventId));
            set => SetPropertyValue(value, nameof(EventId));
        }

        public string Type
        {
            get => GetPropertyValue<string>(nameof(Type));
            set => SetPropertyValue(value, nameof(Type));
        }

        public string Message
        {
            get => GetPropertyValue<string>(nameof(Message));
            set => SetPropertyValue(value, nameof(Message));
        }

        public DateTime Timestamp
        {
            get => GetPropertyValue<DateTime>(nameof(Timestamp));
            set => SetPropertyValue(value, nameof(Timestamp));
        }

        public string CategoryName
        {
            get => GetPropertyValue<string>(nameof(CategoryName));
            set => SetPropertyValue(value, nameof(CategoryName));
        }

        public string Exception
        {
            get => GetPropertyValue<string>(nameof(Exception));
            set => SetPropertyValue(value, nameof(Exception));
        }

        public List<object> DynamicProperties { get; } = new List<object>();


        public object this[int index]
        {
            get
            {
                if (index < 0 || index > _properties.Count + DynamicProperties.Count)
                    throw new ArgumentOutOfRangeException();


                if (index <= _properties.Count)
                {
                    return _properties[index];
                }

                return DynamicProperties[index - _properties.Count];



            }

        }

        public int Count => DynamicProperties.Count + _properties.Count;

    }
}