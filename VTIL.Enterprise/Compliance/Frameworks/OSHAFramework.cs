/*
 * VTIL.Enterprise - OSHA Compliance Framework
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;
using VTIL.Enterprise.Compliance.Models;

namespace VTIL.Enterprise.Compliance.Frameworks
{
    /// <summary>
    /// OSHA (Occupational Safety and Health Administration) compliance framework.
    /// Focuses on workplace safety for development and operations teams.
    /// </summary>
    public static class OSHAFramework
    {
        /// <summary>
        /// Creates an OSHA compliance framework.
        /// </summary>
        public static ComplianceFramework Create()
        {
            var framework = new ComplianceFramework
            {
                Type = ComplianceFrameworkType.OSHA,
                Name = "OSHA Workplace Safety Standards",
                Description = "Occupational Safety and Health Administration standards for IT workplace safety",
                Version = "29 CFR 1910",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };

            framework.Controls = new List<ComplianceControl>
            {
                // General Duty Clause
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.5(a)",
                    Name = "General Duty - Safe Workplace",
                    Description = "Employer must furnish employment and place of employment free from recognized hazards",
                    Category = "General Duty",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Ergonomic workstations, proper lighting, climate control, and safety equipment",
                    Priority = 1,
                    Tags = new List<string> { "OSHA", "General Duty", "Workplace Safety" }
                },

                // Ergonomics
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.132",
                    Name = "Ergonomic Workstation Design",
                    Description = "Workstations must be designed to prevent musculoskeletal disorders",
                    Category = "Ergonomics",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Adjustable desks, ergonomic chairs, monitor arms, and keyboard trays provided to all developers",
                    Priority = 2,
                    Tags = new List<string> { "OSHA", "Ergonomics", "Workstation" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "OSHA-ERG-01",
                    Name = "Repetitive Strain Injury Prevention",
                    Description = "Implement measures to prevent repetitive strain injuries from computer use",
                    Category = "Ergonomics",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Medium,
                    Implementation = "Mandatory break reminders every 50 minutes, stretching programs, ergonomic training",
                    Priority = 3,
                    Tags = new List<string> { "OSHA", "Ergonomics", "RSI Prevention" }
                },

                // Electrical Safety
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.303",
                    Name = "Electrical Safety - General Requirements",
                    Description = "Electrical equipment must be free from recognized hazards",
                    Category = "Electrical Safety",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Regular electrical safety inspections, proper grounding, surge protection",
                    Priority = 2,
                    Tags = new List<string> { "OSHA", "Electrical", "Safety" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.305",
                    Name = "Electrical Safety - Wiring Methods",
                    Description = "Wiring methods and materials must be appropriate for the environment",
                    Category = "Electrical Safety",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Medium,
                    Implementation = "Professional electrical installations with documented compliance",
                    Priority = 3,
                    Tags = new List<string> { "OSHA", "Electrical", "Infrastructure" }
                },

                // Fire Safety
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.39",
                    Name = "Fire Prevention Plan",
                    Description = "Implement a written fire prevention plan",
                    Category = "Fire Safety",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Fire prevention plan with evacuation procedures, fire suppression systems in data centers",
                    Priority = 1,
                    Tags = new List<string> { "OSHA", "Fire Safety", "Emergency Response" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.157",
                    Name = "Portable Fire Extinguishers",
                    Description = "Provide portable fire extinguishers and mount, locate, and identify them properly",
                    Category = "Fire Safety",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Fire extinguishers accessible within 75 feet of all workstations, monthly inspections",
                    Priority = 2,
                    Tags = new List<string> { "OSHA", "Fire Safety", "Equipment" }
                },

                // Indoor Air Quality
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "OSHA-IAQ-01",
                    Name = "Indoor Air Quality",
                    Description = "Maintain acceptable indoor air quality in all work areas",
                    Category = "Environmental Health",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Medium,
                    Implementation = "HVAC system maintenance, air quality monitoring, ventilation standards",
                    Priority = 3,
                    Tags = new List<string> { "OSHA", "IAQ", "Environmental" }
                },

                // Hazard Communication
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.1200",
                    Name = "Hazard Communication Standard",
                    Description = "Ensure information about chemical and physical hazards is transmitted to employees",
                    Category = "Hazard Communication",
                    Status = ComplianceStatus.NotApplicable,
                    Severity = ComplianceSeverity.Low,
                    Implementation = "Not applicable - software development environment with no hazardous chemicals",
                    Priority = 5,
                    Tags = new List<string> { "OSHA", "Hazard Communication", "N/A" }
                },

                // Emergency Action Plan
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.38",
                    Name = "Emergency Action Plan",
                    Description = "Develop and implement an emergency action plan",
                    Category = "Emergency Response",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Written emergency action plan with evacuation procedures, assembly points, and drills",
                    Priority = 1,
                    Tags = new List<string> { "OSHA", "Emergency", "Response Plan" }
                },

                // Personal Protective Equipment
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1910.134",
                    Name = "Respiratory Protection",
                    Description = "Provide appropriate respiratory protection when necessary",
                    Category = "Personal Protective Equipment",
                    Status = ComplianceStatus.NotApplicable,
                    Severity = ComplianceSeverity.Low,
                    Implementation = "Not applicable - office environment with adequate ventilation",
                    Priority = 5,
                    Tags = new List<string> { "OSHA", "PPE", "N/A" }
                },

                // Recordkeeping
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "1904.1",
                    Name = "Recording and Reporting Occupational Injuries",
                    Description = "Maintain records of work-related injuries and illnesses",
                    Category = "Recordkeeping",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Incident tracking system with OSHA Form 300 generation and 5-year retention",
                    Priority = 2,
                    Tags = new List<string> { "OSHA", "Recordkeeping", "Injuries" }
                },

                // Training and Education
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "OSHA-TRAIN-01",
                    Name = "Safety Training Program",
                    Description = "Provide comprehensive safety training to all employees",
                    Category = "Training",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Annual safety training with documentation, ergonomics education, emergency procedures",
                    Priority = 2,
                    Tags = new List<string> { "OSHA", "Training", "Education" }
                }
            };

            framework.CalculateCompliancePercentage();
            return framework;
        }
    }
}

