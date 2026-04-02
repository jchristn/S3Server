namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Custom assertion helpers for test methods.
    /// Each method throws an Exception on failure with a descriptive message.
    /// </summary>
    public static class AssertHelper
    {
        /// <summary>
        /// Assert that a value is not null.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void IsNotNull(object value, string name)
        {
            if (value == null)
                throw new Exception($"Expected {name} to be non-null, but was null.");
        }

        /// <summary>
        /// Assert that a value is null.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void IsNull(object value, string name)
        {
            if (value != null)
                throw new Exception($"Expected {name} to be null, but was '{value}'.");
        }

        /// <summary>
        /// Assert that two values are equal.
        /// </summary>
        /// <typeparam name="T">Type of the values.</typeparam>
        /// <param name="expected">Expected value.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void AreEqual<T>(T expected, T actual, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception($"Expected {name} to be '{expected}', but was '{actual}'.");
        }

        /// <summary>
        /// Assert that two values are not equal.
        /// </summary>
        /// <typeparam name="T">Type of the values.</typeparam>
        /// <param name="expected">Value that actual should not equal.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void AreNotEqual<T>(T expected, T actual, string name)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception($"Expected {name} to differ from '{expected}', but they were equal.");
        }

        /// <summary>
        /// Assert that a condition is true.
        /// </summary>
        /// <param name="value">Condition to check.</param>
        /// <param name="message">Description of the assertion.</param>
        public static void IsTrue(bool value, string message)
        {
            if (!value)
                throw new Exception($"Assertion failed: {message}");
        }

        /// <summary>
        /// Assert that a condition is false.
        /// </summary>
        /// <param name="value">Condition to check.</param>
        /// <param name="message">Description of the assertion.</param>
        public static void IsFalse(bool value, string message)
        {
            if (value)
                throw new Exception($"Assertion failed (expected false): {message}");
        }

        /// <summary>
        /// Assert that a value is greater than a minimum.
        /// </summary>
        /// <param name="actual">Actual value.</param>
        /// <param name="minimum">Minimum exclusive bound.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void IsGreaterThan(long actual, long minimum, string name)
        {
            if (actual <= minimum)
                throw new Exception($"Expected {name} to be greater than {minimum}, but was {actual}.");
        }

        /// <summary>
        /// Assert that a value is greater than or equal to a minimum.
        /// </summary>
        /// <param name="actual">Actual value.</param>
        /// <param name="minimum">Minimum inclusive bound.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void IsGreaterThanOrEqual(long actual, long minimum, string name)
        {
            if (actual < minimum)
                throw new Exception($"Expected {name} to be >= {minimum}, but was {actual}.");
        }

        /// <summary>
        /// Assert that a string starts with a prefix.
        /// </summary>
        /// <param name="value">String to check.</param>
        /// <param name="prefix">Expected prefix.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void StartsWith(string value, string prefix, string name)
        {
            if (value == null || !value.StartsWith(prefix))
                throw new Exception($"Expected {name} to start with '{prefix}', but was '{value}'.");
        }

        /// <summary>
        /// Assert that a string contains a substring.
        /// </summary>
        /// <param name="actual">String to search.</param>
        /// <param name="substring">Expected substring.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void StringContains(string actual, string substring, string name)
        {
            IsNotNull(actual, name);
            if (!actual.Contains(substring))
                throw new Exception($"Expected {name} to contain '{substring}', but was '{actual}'.");
        }

        /// <summary>
        /// Assert that a string does not contain a substring.
        /// </summary>
        /// <param name="actual">String to search.</param>
        /// <param name="substring">Substring that should not be present.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void StringDoesNotContain(string actual, string substring, string name)
        {
            if (actual != null && actual.Contains(substring))
                throw new Exception($"Expected {name} to not contain '{substring}', but it did.");
        }

        /// <summary>
        /// Assert that a collection has a specific count.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="collection">Collection to check.</param>
        /// <param name="expected">Expected count.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void HasCount<T>(ICollection<T> collection, int expected, string name)
        {
            IsNotNull(collection, name);
            if (collection.Count != expected)
                throw new Exception($"Expected {name} to have {expected} items, but had {collection.Count}.");
        }

        /// <summary>
        /// Assert that a collection is not empty.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="collection">Collection to check.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void IsNotEmpty<T>(ICollection<T> collection, string name)
        {
            IsNotNull(collection, name);
            if (collection.Count == 0)
                throw new Exception($"Expected {name} to be non-empty, but was empty.");
        }

        /// <summary>
        /// Assert that a collection is empty.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="collection">Collection to check.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void IsEmpty<T>(ICollection<T> collection, string name)
        {
            IsNotNull(collection, name);
            if (collection.Count != 0)
                throw new Exception($"Expected {name} to be empty, but had {collection.Count} items.");
        }

        /// <summary>
        /// Assert that an HTTP response has the expected status code.
        /// </summary>
        /// <param name="expected">Expected status code.</param>
        /// <param name="actual">Actual status code.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void StatusCodeEquals(int expected, int actual, string name)
        {
            if (expected != actual)
                throw new Exception($"Expected {name} status code to be {expected}, but was {actual}.");
        }

        /// <summary>
        /// Assert that an HTTP response has the expected status code.
        /// </summary>
        /// <param name="expected">Expected status code.</param>
        /// <param name="response">HTTP response message.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void StatusCodeEquals(HttpStatusCode expected, HttpResponseMessage response, string name)
        {
            IsNotNull(response, name);
            if (expected != response.StatusCode)
                throw new Exception($"Expected {name} status code to be {(int)expected} ({expected}), but was {(int)response.StatusCode} ({response.StatusCode}).");
        }

        /// <summary>
        /// Assert that an async action throws a specific exception type.
        /// </summary>
        /// <typeparam name="TException">Expected exception type.</typeparam>
        /// <param name="action">Action to execute.</param>
        /// <param name="description">Description of the assertion.</param>
        public static async Task ThrowsAsync<TException>(Func<Task> action, string description) where TException : Exception
        {
            try
            {
                await action().ConfigureAwait(false);
                throw new Exception($"Expected {typeof(TException).Name} for: {description}");
            }
            catch (TException)
            {
                // expected
            }
        }

        /// <summary>
        /// Assert that a value is within a range.
        /// </summary>
        /// <param name="actual">Actual value.</param>
        /// <param name="min">Minimum inclusive bound.</param>
        /// <param name="max">Maximum inclusive bound.</param>
        /// <param name="name">Descriptive name for error messages.</param>
        public static void InRange(double actual, double min, double max, string name)
        {
            if (actual < min || actual > max)
                throw new Exception($"Expected {name} to be in range [{min}, {max}], but was {actual}.");
        }
    }
}
