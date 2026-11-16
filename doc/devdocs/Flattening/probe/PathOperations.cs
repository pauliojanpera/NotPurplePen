using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>The ways in which the probe unites the marks of a run into one path.</summary>
    public enum UnionWay
    {
        /// <summary>SKPath.OpBuilder: each mark added with the union operation, then one Resolve.</summary>
        Builder,

        /// <summary>One Op(Union) after the other, each mark united with the result so far.</summary>
        OneAfterTheOther,

        /// <summary>The marks united by pairs, then the results united by pairs, until one path is left.</summary>
        ByPairs
    }

    /// <summary>The result of the union of the marks of one run, with what it cost.</summary>
    public sealed class UnionResult
    {
        /// <summary>The union. Null when the way gave no result.</summary>
        public SKPath Path;

        /// <summary>Wall-clock seconds.</summary>
        public double Seconds;

        /// <summary>The count of operations that gave no result.</summary>
        public int Failures;

        /// <summary>True when the time limit stopped the way before its end.</summary>
        public bool IsStoppedByLimit;
    }

    /// <summary>One boolean path operation of the knockout, with its size and its time.</summary>
    public sealed class OperationRecord
    {
        /// <summary>The part of the knockout that the operation belongs to.</summary>
        public string Part;

        /// <summary>The index of the run that the operation cuts or adds.</summary>
        public int Run;

        /// <summary>The name of the operation of Skia.</summary>
        public string Operation;

        /// <summary>The counts of the verbs of the two paths of the operation.</summary>
        public int LeftVerbs, RightVerbs;

        /// <summary>Wall-clock seconds.</summary>
        public double Seconds;

        /// <summary>True when Skia gave a result.</summary>
        public bool HasResult;

        /// <summary>
        /// The count of the thick samples at which the result differs from what the samples of the two paths give.
        /// It is -1 when the result was not checked.
        /// </summary>
        public long ThickSamplesOfWrongResult = -1;
    }

    /// <summary>
    /// The boolean path operations of Skia as the probe uses them: the union of a run in three ways, and one
    /// operation with its time recorded.
    /// </summary>
    public static class PathOperations
    {
        /// <summary>
        /// Unites the marks of a run in one way. limitSeconds stops a way that the probe can stop: the two ways that
        /// are loops of single operations.
        /// </summary>
        public static UnionResult Unite(IReadOnlyList<SKPath> marks, UnionWay way, double limitSeconds)
        {
            UnionResult result = new UnionResult();
            Stopwatch stopwatch = Stopwatch.StartNew();
            switch (way) {
                case UnionWay.Builder:
                    using (SKPath.OpBuilder builder = new SKPath.OpBuilder()) {
                        foreach (SKPath mark in marks)
                            builder.Add(mark, SKPathOp.Union);
                        SKPath resolved = new SKPath();
                        if (builder.Resolve(resolved))
                            result.Path = resolved;
                        else
                            result.Failures = 1;
                    }
                    break;

                case UnionWay.OneAfterTheOther:
                    SKPath sum = new SKPath();
                    foreach (SKPath mark in marks) {
                        if (stopwatch.Elapsed.TotalSeconds > limitSeconds) {
                            result.IsStoppedByLimit = true;
                            break;
                        }
                        SKPath larger = sum.Op(mark, SKPathOp.Union);
                        if (larger == null) {
                            ++result.Failures;
                            continue;
                        }
                        sum.Dispose();
                        sum = larger;
                    }
                    result.Path = result.IsStoppedByLimit ? null : sum;
                    break;

                case UnionWay.ByPairs:
                    List<SKPath> level = new List<SKPath>(marks);
                    bool isFirstLevel = true;
                    while (level.Count > 1 && !result.IsStoppedByLimit) {
                        List<SKPath> next = new List<SKPath>((level.Count + 1) / 2);
                        for (int i = 0; i < level.Count; i += 2) {
                            if (stopwatch.Elapsed.TotalSeconds > limitSeconds) {
                                result.IsStoppedByLimit = true;
                                break;
                            }
                            SKPath pair;
                            if (i + 1 < level.Count) {
                                pair = level[i].Op(level[i + 1], SKPathOp.Union);
                            }
                            else if (isFirstLevel) {
                                pair = level[i].Simplify();
                            }
                            else {
                                pair = level[i];
                                level[i] = null;
                            }
                            if (pair == null) {
                                ++result.Failures;
                                pair = new SKPath(level[i]);
                            }
                            next.Add(pair);
                        }
                        if (!isFirstLevel)
                            foreach (SKPath path in level)
                                path?.Dispose();
                        level = next;
                        isFirstLevel = false;
                    }
                    if (!result.IsStoppedByLimit) {
                        result.Path = level.Count == 0 ? new SKPath() : isFirstLevel ? level[0].Simplify() : level[0];
                        if (result.Path == null)
                            ++result.Failures;
                    }
                    break;
            }
            result.Seconds = stopwatch.Elapsed.TotalSeconds;
            return result;
        }

        /// <summary>
        /// Unites the marks of a run with SKPath.OpBuilder on a thread of its own, and waits for it at most
        /// limitSeconds. The probe cannot stop Resolve. When the limit passes, the thread is left to run, and the
        /// result says that the limit stopped the way.
        /// </summary>
        public static UnionResult UniteWithBuilderWithinLimit(IReadOnlyList<SKPath> marks, double limitSeconds)
        {
            UnionResult result = null;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Thread thread = new Thread(() => { result = Unite(marks, UnionWay.Builder, double.MaxValue); });
            thread.IsBackground = true;
            thread.Start();
            if (thread.Join(TimeSpan.FromSeconds(limitSeconds)))
                return result;
            return new UnionResult { IsStoppedByLimit = true, Seconds = stopwatch.Elapsed.TotalSeconds };
        }

        /// <summary>Does one boolean operation and records its size and its time. Gives null when Skia gives no result.</summary>
        public static SKPath Operate(SKPath left, SKPath right, SKPathOp operation, string part, int run, OperationRecorder recorder)
        {
            OperationRecord record = new OperationRecord { Part = part, Run = run, Operation = operation.ToString(), LeftVerbs = left.VerbCount, RightVerbs = right.VerbCount };
            recorder.Begin(record);
            long start = Stopwatch.GetTimestamp();
            SKPath result = left.Op(right, operation);
            record.Seconds = Stopwatch.GetElapsedTime(start).TotalSeconds;
            record.HasResult = result != null;
            if (result != null)
                record.ThickSamplesOfWrongResult = recorder.CheckResult(left, right, operation, result);
            recorder.End(record);
            return result;
        }
    }

    /// <summary>
    /// Keeps the records of the operations of the knockout. With a table, it writes each record when its operation
    /// ends, and it writes the operation that runs to a file of its own, so that a process that is stopped from
    /// outside leaves what it measured and the operation that it did not end.
    /// </summary>
    public sealed class OperationRecorder
    {
        private readonly TsvTable table;
        private readonly string mapName;
        private readonly string fileOfRunningOperation;

        /// <summary>The records of the operations that ended.</summary>
        public List<OperationRecord> Records { get; } = new List<OperationRecord>();

        /// <summary>
        /// Counts the thick samples at which the result of an operation on two paths differs from what the samples of
        /// the two paths give. When it is null, the recorder does not check results.
        /// </summary>
        public Func<SKPath, SKPath, SKPathOp, SKPath, long> CountThickSamplesOfWrongResult { get; set; }

        /// <summary>The wall-clock seconds of the checks of results.</summary>
        public double CheckSeconds { get; private set; }

        /// <summary>Checks a result. It gives the count of the thick samples at which the result is wrong, or -1 when the recorder does not check results.</summary>
        public long CheckResult(SKPath left, SKPath right, SKPathOp operation, SKPath result)
        {
            if (CountThickSamplesOfWrongResult == null)
                return -1;
            long start = Stopwatch.GetTimestamp();
            long thickSamples = CountThickSamplesOfWrongResult(left, right, operation, result);
            CheckSeconds += Stopwatch.GetElapsedTime(start).TotalSeconds;
            return thickSamples;
        }

        /// <summary>Makes a recorder that keeps the records in memory only.</summary>
        public OperationRecorder()
        {
        }

        /// <summary>Makes a recorder that also writes to the table of the operations of a map.</summary>
        public OperationRecorder(string folder, string mapName, bool isAppended)
        {
            this.mapName = mapName;
            table = new TsvTable(System.IO.Path.Combine(folder, "q3-operations.tsv"), isAppended, "map", "part", "run", "operation", "left_verbs", "right_verbs", "seconds", "gave_result");
            fileOfRunningOperation = System.IO.Path.Combine(folder, "q3-running-operation.tsv");
        }

        /// <summary>Notes that an operation starts.</summary>
        public void Begin(OperationRecord record)
        {
            if (table != null)
                System.IO.File.WriteAllText(fileOfRunningOperation, string.Join("\t", mapName, record.Part, record.Run, record.Operation, record.LeftVerbs, record.RightVerbs, DateTime.UtcNow.ToString("o")) + "\n");
        }

        /// <summary>Keeps the record of an operation that ended.</summary>
        public void End(OperationRecord record)
        {
            Records.Add(record);
            if (table != null) {
                table.AddRow(mapName, record.Part, record.Run, record.Operation, record.LeftVerbs, record.RightVerbs, record.Seconds, record.HasResult);
                System.IO.File.Delete(fileOfRunningOperation);
            }
        }
    }
}
