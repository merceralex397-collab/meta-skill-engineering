using System;
using System.Collections.Generic;
using System.Linq;
using MetaSkillStudio.Models;
using MetaSkillStudio.Services.Interfaces;

namespace MetaSkillStudio.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IDialogService for testing.
    /// Tracks all dialog calls and allows setting return values.
    /// </summary>
    public class MockDialogService : IDialogService
    {
        // Tracking properties
        public List<(string Message, string Title, MessageType Type)> MessageBoxCalls { get; } = new();
        public List<Type> ShowDialogCalls { get; } = new();
        public int CreateSkillDialogCallCount { get; private set; }
        public int SkillSelectionDialogCallCount { get; private set; }
        public int InputDialogCallCount { get; private set; }
        public int BenchmarkDialogCallCount { get; private set; }
        public int SettingsDialogCallCount { get; private set; }
        public int AnalyticsDialogCallCount { get; private set; }
        public int RunDetailsDialogCallCount { get; private set; }

        // Configuration for return values
        public bool? NextShowDialogResult { get; set; } = true;
        public (bool? Result, string SkillBrief, TargetLibrary TargetLibrary) NextCreateSkillDialogResult { get; set; } 
            = (true, "Test Skill", TargetLibrary.LibraryWorkbench);
        public (bool? Result, SkillInfo? SelectedSkill, bool TestAllSkills) NextSkillSelectionDialogResult { get; set; }
            = (true, null, false);
        public (bool? Result, string ResponseText) NextInputDialogResult { get; set; } = (true, "Test Input");
        public (bool? Result, string SkillName, string BenchmarkGoal, int CaseCount) NextBenchmarkDialogResult { get; set; }
            = (true, "TestSkill", "Test benchmark", 8);
        public bool? NextSettingsDialogResult { get; set; } = true;

        // Lists for sequence-based results
        public List<(bool? Result, string SkillBrief, TargetLibrary TargetLibrary)> CreateSkillDialogResultsQueue { get; } = new();
        public List<(bool? Result, SkillInfo? SelectedSkill, bool TestAllSkills)> SkillSelectionDialogResultsQueue { get; } = new();
        public List<(bool? Result, string ResponseText)> InputDialogResultsQueue { get; } = new();

        // Last shown run details
        public RunInfo? LastShownRunDetails { get; private set; }

        public void ShowMessage(string message, string title, MessageType type = MessageType.Information)
        {
            MessageBoxCalls.Add((message, title, type));
        }

        public bool? ShowDialog<T>() where T : class
        {
            ShowDialogCalls.Add(typeof(T));
            return NextShowDialogResult;
        }

        public (bool? Result, string SkillBrief, TargetLibrary TargetLibrary) ShowCreateSkillDialog()
        {
            CreateSkillDialogCallCount++;
            
            if (CreateSkillDialogResultsQueue.Any())
            {
                var result = CreateSkillDialogResultsQueue[0];
                CreateSkillDialogResultsQueue.RemoveAt(0);
                return result;
            }
            
            return NextCreateSkillDialogResult;
        }

        public (bool? Result, SkillInfo? SelectedSkill, bool TestAllSkills) ShowSkillSelectionDialog(List<SkillInfo> skills, string description, bool allowTestAll = false)
        {
            SkillSelectionDialogCallCount++;
            
            if (SkillSelectionDialogResultsQueue.Any())
            {
                var result = SkillSelectionDialogResultsQueue[0];
                SkillSelectionDialogResultsQueue.RemoveAt(0);
                return result;
            }

            // If SelectedSkill is null in the configured result and we have skills, use the first one
            var configuredResult = NextSkillSelectionDialogResult;
            if (configuredResult.SelectedSkill == null && skills.Any())
            {
                return (configuredResult.Result, skills.First(), configuredResult.TestAllSkills);
            }

            return configuredResult;
        }

        public (bool? Result, string ResponseText) ShowInputDialog(string title, string message, string defaultResponse = "")
        {
            InputDialogCallCount++;
            
            if (InputDialogResultsQueue.Any())
            {
                var result = InputDialogResultsQueue[0];
                InputDialogResultsQueue.RemoveAt(0);
                return result;
            }

            return NextInputDialogResult;
        }

        public (bool? Result, string SkillName, string BenchmarkGoal, int CaseCount) ShowBenchmarkDialog()
        {
            BenchmarkDialogCallCount++;
            return NextBenchmarkDialogResult;
        }

        public bool? ShowSettingsDialog()
        {
            SettingsDialogCallCount++;
            return NextSettingsDialogResult;
        }

        public void ShowAnalyticsDialog()
        {
            AnalyticsDialogCallCount++;
        }

        public void ShowRunDetailsDialog(RunInfo runInfo)
        {
            RunDetailsDialogCallCount++;
            LastShownRunDetails = runInfo;
        }

        public void Reset()
        {
            MessageBoxCalls.Clear();
            ShowDialogCalls.Clear();
            CreateSkillDialogCallCount = 0;
            SkillSelectionDialogCallCount = 0;
            InputDialogCallCount = 0;
            BenchmarkDialogCallCount = 0;
            SettingsDialogCallCount = 0;
            AnalyticsDialogCallCount = 0;
            RunDetailsDialogCallCount = 0;
            LastShownRunDetails = null;
            CreateSkillDialogResultsQueue.Clear();
            SkillSelectionDialogResultsQueue.Clear();
            InputDialogResultsQueue.Clear();
        }

        public bool WasMessageShown(string messageSubstring) => 
            MessageBoxCalls.Any(m => m.Message.Contains(messageSubstring));

        public bool WasDialogShown<T>() => 
            ShowDialogCalls.Any(t => t == typeof(T));
    }
}
