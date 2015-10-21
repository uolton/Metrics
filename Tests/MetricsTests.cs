﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RSG.MetricsTests
{
    public class MetricsTests
    {
        Metrics testObject;

        Mock<IMetricsEmitter> mockMetricsEmitter;

        void Init()
        {
            mockMetricsEmitter = new Mock<IMetricsEmitter>();

            testObject = new Metrics(mockMetricsEmitter.Object);
        }

        /// <summary>
        /// Helper function to check if two dictionaries are the same.
        /// </summary>
        bool DictionaryEquals<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
        {
            // Check that the two dictionaries are the same length.
            if (expected.Count != actual.Count)
            {
                return false;
            }

            // Check that each element is the same.
            foreach (var pair in expected)
            {
                TValue value;
                if (actual.TryGetValue(pair.Key, out value))
                {
                    // Check that each value is the same
                    if (!value.Equals(pair.Value))
                    {
                        return false;
                    }
                }
                else // Require key to be present.
                {
                    return false;
                }
            }

            return true;
        }

        [Fact]
        public void entry_with_string_emits_message()
        {
            Init();

            testObject.Entry("TestEntry", "Testing");

            mockMetricsEmitter
                .Verify(m => m.Emit(It.IsAny<IDictionary<string, string>>(), It.IsAny<Metric[]>()), Times.Once());
        }

        [Fact]
        public void entry_with_string_contains_entry_name()
        {
            Init();

            const string name = "TestEntry";

            string emittedEntryName = String.Empty;

            mockMetricsEmitter
                .Setup(m => m.Emit(It.IsAny<IDictionary<string, string>>(), It.IsAny<Metric[]>()))
                .Callback<IDictionary<string, string>, Metric[]>((properties, metrics) => {
                    var entry = metrics[0];
                    emittedEntryName = entry.Name;
                });

            testObject.Entry(name, "Test content");

            Assert.Equal(name, emittedEntryName);
        }

        [Fact]
        public void entry_with_string_contains_string()
        {
            Init();

            const string content = "Test content";

            string emittedData = String.Empty;

            mockMetricsEmitter
                .Setup(m => m.Emit(It.IsAny<IDictionary<string, string>>(), It.IsAny<Metric[]>()))
                .Callback<IDictionary<string, string>, Metric[]>((properties, metrics) => {
                    var entry = metrics[0];
                    emittedData = entry.Data;
                });

            testObject.Entry("TestEntry", content);

            Assert.Equal(content, emittedData);
        }

        [Fact]
        public void set_property_includes_property_on_first_message()
        {
            Init();

            const string propertyKey = "key";
            const string propertyValue = "value";

            testObject.SetProperty(propertyKey, propertyValue);

            testObject.Entry("TestEntry", "data");

            var properties = new Dictionary<string, string>();
            properties.Add(propertyKey, propertyValue);
            mockMetricsEmitter
                .Verify(m => m.Emit(
                    It.Is<IDictionary<string, string>>(p => DictionaryEquals<string, string>(properties, p)),
                    It.IsAny<Metric[]>()),
                    Times.Once());
        }

        [Fact]
        public void set_property_includes_property_on_subsequent_messages()
        {
            Init();

            const string propertyKey = "key";
            const string propertyValue = "value";

            testObject.SetProperty(propertyKey, propertyValue);

            testObject.Entry("TestEntry1", "data");
            testObject.Entry("TestEntry2", "data");
            testObject.Entry("TestEntry3", "data");

            var properties = new Dictionary<string, string>();
            properties.Add(propertyKey, propertyValue);
            mockMetricsEmitter
                .Verify(m => m.Emit(
                    It.Is<IDictionary<string, string>>(p => DictionaryEquals<string, string>(properties, p)),
                    It.IsAny<Metric[]>()),
                    Times.Exactly(3));
        }

        [Fact]
        public void set_property_updates_existing_property()
        {
            Init();

            const string propertyKey = "key";
            const string propertyValue1 = "value";
            const string propertyValue2 = "something else";

            testObject.SetProperty(propertyKey, propertyValue1);

            testObject.Entry("TestEntry1", "data");

            var properties1 = new Dictionary<string, string>();
            properties1.Add(propertyKey, propertyValue1);
            mockMetricsEmitter
                .Verify(m => m.Emit(
                    It.Is<IDictionary<string, string>>(p => DictionaryEquals<string, string>(properties1, p)),
                    It.IsAny<Metric[]>()),
                    Times.Once());

            testObject.SetProperty(propertyKey, propertyValue2);

            testObject.Entry("TestEntry2", "data");

            var properties2 = new Dictionary<string, string>();
            properties2.Add(propertyKey, propertyValue2);
            mockMetricsEmitter
                .Verify(m => m.Emit(
                    It.Is<IDictionary<string, string>>(p => DictionaryEquals<string, string>(properties2, p)),
                    It.IsAny<Metric[]>()));
        }

        [Fact]
        public void set_property_appends_to_included_properties()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void remove_property_stops_including_properties_on_messages()
        {
            throw new NotImplementedException();
        }
    }
}
