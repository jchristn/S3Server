namespace Test.Shared
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// In-memory collector for tests: subscribes a MeterListener and an ActivityListener to the given meter and
    /// activity source names and records everything they emit.  Thread-safe.
    /// </summary>
    public sealed class TelemetryCollector : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Captured measurements.
        /// </summary>
        public IReadOnlyList<RecordedMeasurement> Measurements
        {
            get { return _Measurements.ToArray(); }
        }

        /// <summary>
        /// Captured, stopped activities.
        /// </summary>
        public IReadOnlyList<Activity> Activities
        {
            get { return _Activities.ToArray(); }
        }

        #endregion

        #region Private-Members

        private readonly HashSet<string> _MeterNames;
        private readonly HashSet<string> _SourceNames;
        private readonly ConcurrentQueue<RecordedMeasurement> _Measurements = new ConcurrentQueue<RecordedMeasurement>();
        private readonly ConcurrentQueue<Activity> _Activities = new ConcurrentQueue<Activity>();
        private readonly MeterListener _MeterListener;
        private readonly ActivityListener _ActivityListener;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate and start listening.
        /// </summary>
        /// <param name="meterNames">Meter names to subscribe to.</param>
        /// <param name="sourceNames">Activity source names to subscribe to.</param>
        public TelemetryCollector(IEnumerable<string> meterNames, IEnumerable<string> sourceNames)
        {
            _MeterNames = new HashSet<string>(meterNames ?? Array.Empty<string>(), StringComparer.Ordinal);
            _SourceNames = new HashSet<string>(sourceNames ?? Array.Empty<string>(), StringComparer.Ordinal);

            _MeterListener = new MeterListener();
            _MeterListener.InstrumentPublished = (instrument, listener) =>
            {
                if (_MeterNames.Contains(instrument.Meter.Name)) listener.EnableMeasurementEvents(instrument);
            };
            _MeterListener.SetMeasurementEventCallback<long>((i, v, t, s) => Record(i, v, t));
            _MeterListener.SetMeasurementEventCallback<int>((i, v, t, s) => Record(i, v, t));
            _MeterListener.SetMeasurementEventCallback<double>((i, v, t, s) => Record(i, v, t));
            _MeterListener.Start();

            _ActivityListener = new ActivityListener();
            _ActivityListener.ShouldListenTo = source => _SourceNames.Contains(source.Name);
            _ActivityListener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded;
            _ActivityListener.ActivityStopped = activity => _Activities.Enqueue(activity);
            ActivitySource.AddActivityListener(_ActivityListener);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Poll observable instruments (gauges) so their current values are captured.
        /// </summary>
        public void CollectObservables()
        {
            _MeterListener.RecordObservableInstruments();
        }

        /// <summary>
        /// Measurements for one instrument that match the supplied attribute pairs.
        /// </summary>
        /// <param name="instrument">Instrument name.</param>
        /// <param name="tags">Attribute key and value pairs.</param>
        /// <returns>Matching measurements.</returns>
        public List<RecordedMeasurement> Find(string instrument, params string[] tags)
        {
            return _Measurements.Where(m => m.Instrument == instrument && m.Matches(tags)).ToList();
        }

        /// <summary>
        /// Wait until a measurement for the instrument with the supplied attributes is captured.
        /// Requests are completed after the response is sent, so measurements can arrive shortly after the client returns.
        /// </summary>
        /// <param name="instrument">Instrument name.</param>
        /// <param name="tags">Attribute key and value pairs.</param>
        /// <returns>The first matching measurement.</returns>
        /// <exception cref="TimeoutException">Thrown if no measurement arrives within five seconds.</exception>
        public async Task<RecordedMeasurement> WaitForAsync(string instrument, params string[] tags)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);

            while (DateTime.UtcNow < deadline)
            {
                RecordedMeasurement found = Find(instrument, tags).FirstOrDefault();
                if (found != null) return found;
                await Task.Delay(20).ConfigureAwait(false);
            }

            throw new TimeoutException("No " + instrument + " measurement with [" + String.Join(",", tags) + "]. Captured: "
                + String.Join("; ", _Measurements.Where(m => m.Instrument == instrument).Select(m => m.ToString())));
        }

        /// <summary>
        /// Wait until an activity with the supplied display name is captured.
        /// </summary>
        /// <param name="displayName">Span name.</param>
        /// <returns>The first matching activity.</returns>
        /// <exception cref="TimeoutException">Thrown if no activity arrives within five seconds.</exception>
        public async Task<Activity> WaitForActivityAsync(string displayName)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);

            while (DateTime.UtcNow < deadline)
            {
                Activity found = _Activities.FirstOrDefault(a => a.DisplayName == displayName);
                if (found != null) return found;
                await Task.Delay(20).ConfigureAwait(false);
            }

            throw new TimeoutException("No activity named " + displayName + ". Captured: "
                + String.Join("; ", _Activities.Select(a => a.Source.Name + ":" + a.DisplayName)));
        }

        /// <summary>
        /// Dispose of the listeners.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;
            _MeterListener.Dispose();
            _ActivityListener.Dispose();
        }

        #endregion

        #region Private-Methods

        private void Record<T>(Instrument instrument, T value, ReadOnlySpan<KeyValuePair<string, object?>> tags) where T : struct
        {
            RecordedMeasurement m = new RecordedMeasurement();
            m.Instrument = instrument.Name;
            m.Unit = instrument.Unit;
            m.Value = Convert.ToDouble(value);
            foreach (KeyValuePair<string, object?> tag in tags) m.Tags[tag.Key] = tag.Value;
            _Measurements.Enqueue(m);
        }

        #endregion
    }
}
