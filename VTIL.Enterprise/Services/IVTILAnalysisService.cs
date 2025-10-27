/*
 * VTIL.Enterprise - VTIL Analysis Service Interface
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VTIL.Compiler.Validation;
using VTIL.Enterprise.Controllers;

namespace VTIL.Enterprise.Services
{
    /// <summary>
    /// Service interface for VTIL analysis operations.
    /// </summary>
    public interface IVTILAnalysisService
    {
        Task<AnalysisResult> DeobfuscateBinaryAsync(AnalysisRequest request);
        Task<SimplificationResult> SimplifyExpressionAsync(SimplificationRequest request);
        Task<ValidationResult> ValidateRoutineAsync(ValidationRequest request);
        Task<AnalysisStatistics> GetStatisticsAsync();
        Task<IEnumerable<AnalysisJob>> GetJobsAsync(int page, int pageSize);
        Task<AnalysisJob?> GetJobAsync(Guid id);
        Task<bool> CancelJobAsync(Guid id);
    }
}

