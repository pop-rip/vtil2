/*
 * VTIL.Enterprise - Audit Service
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VTIL.Enterprise.Controllers;

namespace VTIL.Enterprise.Services
{
    /// <summary>
    /// Service interface for audit logging.
    /// </summary>
    public interface IAuditService
    {
        Task LogAnalysisRequestAsync(AnalysisRequest request, string user);
        Task LogAnalysisResultAsync(AnalysisResult result, string user);
        Task LogAnalysisErrorAsync(AnalysisRequest request, Exception exception, string user);
        Task LogComplianceEventAsync(string frameworkType, string eventType, string description, string user);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
        Task<AuditStatistics> GetAuditStatisticsAsync();
    }

    /// <summary>
    /// Implementation of audit service with comprehensive logging.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ConcurrentBag<AuditLog> _auditLogs = new();
        private static long _eventCounter = 0;

        public Task LogAnalysisRequestAsync(AnalysisRequest request, string user)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                EventId = System.Threading.Interlocked.Increment(ref _eventCounter),
                Timestamp = DateTime.UtcNow,
                User = user,
                EventType = "AnalysisRequest",
                Category = "Analysis",
                Severity = AuditSeverity.Informational,
                Description = $"Binary analysis requested for {request.Architecture} at entry point 0x{request.EntryPoint:X}",
                Details = new Dictionary<string, object>
                {
                    ["Architecture"] = request.Architecture,
                    ["EntryPoint"] = request.EntryPoint,
                    ["OptimizationLevel"] = request.OptimizationLevel,
                    ["TimeoutSeconds"] = request.TimeoutSeconds
                },
                SourceIP = "127.0.0.1", // Would come from HttpContext in real impl
                Success = true
            };

            _auditLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task LogAnalysisResultAsync(AnalysisResult result, string user)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                EventId = System.Threading.Interlocked.Increment(ref _eventCounter),
                Timestamp = DateTime.UtcNow,
                User = user,
                EventType = "AnalysisComplete",
                Category = "Analysis",
                Severity = result.Success ? AuditSeverity.Informational : AuditSeverity.Error,
                Description = result.Success 
                    ? $"Analysis completed successfully: {result.OptimizationsApplied} optimizations applied"
                    : $"Analysis failed: {result.ErrorMessage}",
                Details = new Dictionary<string, object>
                {
                    ["JobId"] = result.JobId,
                    ["BlockCount"] = result.BlockCount,
                    ["InstructionCount"] = result.InstructionCount,
                    ["OptimizationsApplied"] = result.OptimizationsApplied,
                    ["DurationMs"] = result.DurationMs
                },
                SourceIP = "127.0.0.1",
                Success = result.Success
            };

            _auditLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task LogAnalysisErrorAsync(AnalysisRequest request, Exception exception, string user)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                EventId = System.Threading.Interlocked.Increment(ref _eventCounter),
                Timestamp = DateTime.UtcNow,
                User = user,
                EventType = "AnalysisError",
                Category = "Analysis",
                Severity = AuditSeverity.Error,
                Description = $"Analysis failed with exception: {exception.Message}",
                Details = new Dictionary<string, object>
                {
                    ["Architecture"] = request.Architecture,
                    ["EntryPoint"] = request.EntryPoint,
                    ["ExceptionType"] = exception.GetType().Name,
                    ["ExceptionMessage"] = exception.Message,
                    ["StackTrace"] = exception.StackTrace ?? string.Empty
                },
                SourceIP = "127.0.0.1",
                Success = false
            };

            _auditLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task LogComplianceEventAsync(string frameworkType, string eventType, string description, string user)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                EventId = System.Threading.Interlocked.Increment(ref _eventCounter),
                Timestamp = DateTime.UtcNow,
                User = user,
                EventType = eventType,
                Category = "Compliance",
                Severity = AuditSeverity.Informational,
                Description = description,
                Details = new Dictionary<string, object>
                {
                    ["FrameworkType"] = frameworkType,
                    ["EventType"] = eventType
                },
                SourceIP = "127.0.0.1",
                Success = true
            };

            _auditLogs.Add(log);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter)
        {
            var query = _auditLogs.AsEnumerable();

            if (filter.StartDate.HasValue)
                query = query.Where(l => l.Timestamp >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(l => l.Timestamp <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.User))
                query = query.Where(l => l.User.Equals(filter.User, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(l => l.Category.Equals(filter.Category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(filter.EventType))
                query = query.Where(l => l.EventType.Equals(filter.EventType, StringComparison.OrdinalIgnoreCase));

            if (filter.MinSeverity.HasValue)
                query = query.Where(l => l.Severity >= filter.MinSeverity.Value);

            var results = query
                .OrderByDescending(l => l.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return Task.FromResult<IEnumerable<AuditLog>>(results);
        }

        public Task<AuditStatistics> GetAuditStatisticsAsync()
        {
            var stats = new AuditStatistics
            {
                TotalEvents = _auditLogs.Count,
                EventsByCategory = _auditLogs
                    .GroupBy(l => l.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsBySeverity = _auditLogs
                    .GroupBy(l => l.Severity)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                EventsByType = _auditLogs
                    .GroupBy(l => l.EventType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsPerHour = CalculateEventsPerHour(),
                MostActiveUsers = _auditLogs
                    .GroupBy(l => l.User)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),
                FirstEventTime = _auditLogs.Any() ? _auditLogs.Min(l => l.Timestamp) : DateTime.UtcNow,
                LastEventTime = _auditLogs.Any() ? _auditLogs.Max(l => l.Timestamp) : DateTime.UtcNow
            };

            return Task.FromResult(stats);
        }

        private Dictionary<string, int> CalculateEventsPerHour()
        {
            var now = DateTime.UtcNow;
            var last24Hours = Enumerable.Range(0, 24)
                .Select(i => now.AddHours(-i))
                .Reverse()
                .ToList();

            var result = new Dictionary<string, int>();
            foreach (var hour in last24Hours)
            {
                var hourKey = hour.ToString("yyyy-MM-dd HH:00");
                var count = _auditLogs.Count(l => 
                    l.Timestamp >= hour && 
                    l.Timestamp < hour.AddHours(1));
                result[hourKey] = count;
            }

            return result;
        }
    }

    /// <summary>
    /// Audit log entry.
    /// </summary>
    public class AuditLog
    {
        public Guid Id { get; set; }
        public long EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public AuditSeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
        public string SourceIP { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    /// <summary>
    /// Audit severity levels.
    /// </summary>
    public enum AuditSeverity
    {
        Verbose,
        Informational,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Filter for querying audit logs.
    /// </summary>
    public class AuditLogFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? User { get; set; }
        public string? Category { get; set; }
        public string? EventType { get; set; }
        public AuditSeverity? MinSeverity { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// Audit statistics.
    /// </summary>
    public class AuditStatistics
    {
        public int TotalEvents { get; set; }
        public Dictionary<string, int> EventsByCategory { get; set; } = new();
        public Dictionary<string, int> EventsBySeverity { get; set; } = new();
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsPerHour { get; set; } = new();
        public Dictionary<string, int> MostActiveUsers { get; set; } = new();
        public DateTime FirstEventTime { get; set; }
        public DateTime LastEventTime { get; set; }
    }
}

