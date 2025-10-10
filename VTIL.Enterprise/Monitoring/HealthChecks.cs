/*
 * VTIL.Enterprise - Health Checks
 * Copyright (c) 2025 VTIL2 Project
 */

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VTIL.Enterprise.Compliance.Services;

namespace VTIL.Enterprise.Monitoring
{
    /// <summary>
    /// Health check for compliance service.
    /// </summary>
    public class ComplianceHealthCheck : IHealthCheck
    {
        private readonly IComplianceService _complianceService;

        public ComplianceHealthCheck(IComplianceService complianceService)
        {
            _complianceService = complianceService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var frameworks = await _complianceService.GetAllFrameworksAsync();
                var frameworkCount = frameworks.Count();

                if (frameworkCount == 0)
                {
                    return HealthCheckResult.Degraded(
                        "No compliance frameworks loaded",
                        data: new Dictionary<string, object>
                        {
                            ["FrameworkCount"] = 0
                        });
                }

                var criticalFindings = await _complianceService.GetFindingsAsync(Models.ComplianceSeverity.Critical);
                var criticalCount = criticalFindings.Count();

                if (criticalCount > 0)
                {
                    return HealthCheckResult.Degraded(
                        $"{criticalCount} critical compliance findings require attention",
                        data: new Dictionary<string, object>
                        {
                            ["FrameworkCount"] = frameworkCount,
                            ["CriticalFindings"] = criticalCount
                        });
                }

                return HealthCheckResult.Healthy(
                    $"{frameworkCount} compliance frameworks active, no critical findings",
                    data: new Dictionary<string, object>
                    {
                        ["FrameworkCount"] = frameworkCount,
                        ["CriticalFindings"] = 0
                    });
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Compliance service check failed",
                    exception: ex);
            }
        }
    }

    /// <summary>
    /// Health check for VTIL analysis service.
    /// </summary>
    public class VTILAnalysisHealthCheck : IHealthCheck
    {
        private readonly IVTILAnalysisService _analysisService;

        public VTILAnalysisHealthCheck(IVTILAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _analysisService.GetStatisticsAsync();

                var failureRate = stats.TotalJobs > 0
                    ? (double)stats.FailedJobs / stats.TotalJobs
                    : 0;

                if (failureRate > 0.1) // More than 10% failures
                {
                    return HealthCheckResult.Degraded(
                        $"High failure rate: {failureRate:P2}",
                        data: new Dictionary<string, object>
                        {
                            ["TotalJobs"] = stats.TotalJobs,
                            ["FailedJobs"] = stats.FailedJobs,
                            ["FailureRate"] = failureRate
                        });
                }

                return HealthCheckResult.Healthy(
                    $"{stats.TotalJobs} jobs processed, {stats.RunningJobs} running",
                    data: new Dictionary<string, object>
                    {
                        ["TotalJobs"] = stats.TotalJobs,
                        ["SuccessfulJobs"] = stats.SuccessfulJobs,
                        ["RunningJobs"] = stats.RunningJobs,
                        ["FailureRate"] = failureRate
                    });
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Analysis service check failed",
                    exception: ex);
            }
        }
    }

    /// <summary>
    /// Health check for system resources.
    /// </summary>
    public class SystemResourcesHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var memoryUsage = GC.GetTotalMemory(false);
                var memoryMB = memoryUsage / 1024 / 1024;

                var cpuUsage = GetCpuUsage();

                var data = new Dictionary<string, object>
                {
                    ["MemoryUsageMB"] = memoryMB,
                    ["CPUUsagePercent"] = cpuUsage,
                    ["GCGen0Collections"] = GC.CollectionCount(0),
                    ["GCGen1Collections"] = GC.CollectionCount(1),
                    ["GCGen2Collections"] = GC.CollectionCount(2)
                };

                if (memoryMB > 2048) // More than 2GB
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"High memory usage: {memoryMB}MB",
                        data: data));
                }

                if (cpuUsage > 80) // More than 80% CPU
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"High CPU usage: {cpuUsage}%",
                        data: data));
                }

                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Memory: {memoryMB}MB, CPU: {cpuUsage}%",
                    data: data));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "System resources check failed",
                    exception: ex));
            }
        }

        private double GetCpuUsage()
        {
            // Simplified CPU usage calculation
            // In production, use proper performance counters
            return Environment.ProcessorCount * 0.5; // Placeholder
        }
    }
}

