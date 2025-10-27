/*
 * VTIL.Enterprise - Metrics Collector
 * Copyright (c) 2025 VTIL2 Project
 */

using Prometheus;
using System;

namespace VTIL.Enterprise.Monitoring
{
    /// <summary>
    /// Prometheus metrics collector for VTIL operations.
    /// </summary>
    public static class MetricsCollector
    {
        // Counters
        private static readonly Counter AnalysisRequestsTotal = Metrics.CreateCounter(
            "vtil_analysis_requests_total",
            "Total number of analysis requests",
            new CounterConfiguration
            {
                LabelNames = new[] { "architecture", "status" }
            });

        private static readonly Counter OptimizationsAppliedTotal = Metrics.CreateCounter(
            "vtil_optimizations_applied_total",
            "Total number of optimizations applied",
            new CounterConfiguration
            {
                LabelNames = new[] { "pass_name" }
            });

        private static readonly Counter ComplianceViolationsTotal = Metrics.CreateCounter(
            "vtil_compliance_violations_total",
            "Total number of compliance violations detected",
            new CounterConfiguration
            {
                LabelNames = new[] { "framework", "severity" }
            });

        // Gauges
        private static readonly Gauge ActiveAnalysisJobs = Metrics.CreateGauge(
            "vtil_active_analysis_jobs",
            "Number of currently running analysis jobs");

        private static readonly Gauge CompliancePercentage = Metrics.CreateGauge(
            "vtil_compliance_percentage",
            "Overall compliance percentage",
            new GaugeConfiguration
            {
                LabelNames = new[] { "framework" }
            });

        private static readonly Gauge CacheHitRate = Metrics.CreateGauge(
            "vtil_cache_hit_rate",
            "Expression simplification cache hit rate");

        // Histograms
        private static readonly Histogram AnalysisDuration = Metrics.CreateHistogram(
            "vtil_analysis_duration_seconds",
            "Duration of analysis operations in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "architecture" },
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) // 0.01s to 5.12s
            });

        private static readonly Histogram SimplificationDuration = Metrics.CreateHistogram(
            "vtil_simplification_duration_ms",
            "Duration of expression simplification in milliseconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10) // 0.1ms to 51.2ms
            });

        private static readonly Histogram RoutineSize = Metrics.CreateHistogram(
            "vtil_routine_size_instructions",
            "Size of routines in instructions",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(10, 2, 12) // 10 to 20,480 instructions
            });

        // Summary
        private static readonly Summary ApiLatency = Metrics.CreateSummary(
            "vtil_api_latency_seconds",
            "API endpoint latency",
            new SummaryConfiguration
            {
                LabelNames = new[] { "endpoint", "method" },
                Objectives = new[]
                {
                    new QuantileEpsilonPair(0.5, 0.05),  // p50
                    new QuantileEpsilonPair(0.9, 0.01),  // p90
                    new QuantileEpsilonPair(0.95, 0.01), // p95
                    new QuantileEpsilonPair(0.99, 0.001) // p99
                }
            });

        /// <summary>
        /// Records an analysis request.
        /// </summary>
        public static void RecordAnalysisRequest(string architecture, string status)
        {
            AnalysisRequestsTotal.WithLabels(architecture, status).Inc();
        }

        /// <summary>
        /// Records optimizations applied.
        /// </summary>
        public static void RecordOptimizationsApplied(string passName, int count)
        {
            OptimizationsAppliedTotal.WithLabels(passName).Inc(count);
        }

        /// <summary>
        /// Records a compliance violation.
        /// </summary>
        public static void RecordComplianceViolation(string framework, string severity)
        {
            ComplianceViolationsTotal.WithLabels(framework, severity).Inc();
        }

        /// <summary>
        /// Sets the number of active analysis jobs.
        /// </summary>
        public static void SetActiveAnalysisJobs(int count)
        {
            ActiveAnalysisJobs.Set(count);
        }

        /// <summary>
        /// Sets compliance percentage for a framework.
        /// </summary>
        public static void SetCompliancePercentage(string framework, double percentage)
        {
            CompliancePercentage.WithLabels(framework).Set(percentage);
        }

        /// <summary>
        /// Sets cache hit rate.
        /// </summary>
        public static void SetCacheHitRate(double rate)
        {
            CacheHitRate.Set(rate);
        }

        /// <summary>
        /// Observes analysis duration.
        /// </summary>
        public static void ObserveAnalysisDuration(string architecture, double seconds)
        {
            AnalysisDuration.WithLabels(architecture).Observe(seconds);
        }

        /// <summary>
        /// Observes simplification duration.
        /// </summary>
        public static void ObserveSimplificationDuration(double milliseconds)
        {
            SimplificationDuration.Observe(milliseconds);
        }

        /// <summary>
        /// Observes routine size.
        /// </summary>
        public static void ObserveRoutineSize(int instructionCount)
        {
            RoutineSize.Observe(instructionCount);
        }

        /// <summary>
        /// Observes API latency.
        /// </summary>
        public static void ObserveApiLatency(string endpoint, string method, double seconds)
        {
            ApiLatency.WithLabels(endpoint, method).Observe(seconds);
        }
    }
}

