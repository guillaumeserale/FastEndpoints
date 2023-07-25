﻿using System.Linq.Expressions;

namespace FastEndpoints;

/// <summary>
/// interface for defining the contract of a job storage provider
/// </summary>
/// <typeparam name="TStorageRecord">the type of job storage record of this storage provider</typeparam>
public interface IJobStorageProvider<TStorageRecord> where TStorageRecord : IJobStorageRecord
{
    /// <summary>
    /// store the job storage record however you please. ideally on a nosql database.
    /// </summary>
    /// <param name="job">the job storage record which contains the actual command object as well as some metadata</param>
    /// <param name="ct"></param>
    Task StoreJobAsync(TStorageRecord job, CancellationToken ct);

    /// <summary>
    /// fetch the next pending batch of job storage records that need to be processed, with the supplied match expression.
    /// </summary>
    /// <param name="match">a boolean lambda expression to use for matching and retrieving the next batch of pending records.</param>
    /// <param name="batchSize">how many records to fetch</param>
    /// <param name="ct">cancellation token</param>
    Task<IEnumerable<TStorageRecord>> GetNextBatchAsync(Expression<Func<TStorageRecord, bool>> match, int batchSize, CancellationToken ct);

    /// <summary>
    /// mark the job storage record as complete by either replacing the entity on storage with the supplied instance or
    /// simply update the <see cref="IJobStorageRecord.IsComplete"/> field to true with a partial update operation.
    /// </summary>
    /// <param name="job">the job storage record to mark as complete</param>
    /// <param name="ct">cancellation token</param>
    Task MarkJobAsCompleteAsync(TStorageRecord job, CancellationToken ct);

    /// <summary>
    /// this will only be triggered when the command handler (<see cref="ICommandHandler{TCommand}"/>) associated with the command
    /// throws an exception. If you've set an execution time limit for the command, the thrown exeception would be of type <see cref="OperationCanceledException"/>.
    /// <para>
    /// when a job/command execution fails, it will be retried immediately.
    /// the failed job will be fetched again with the next batch of pending jobs.
    /// if one or more jobs keep failing repeatedly, it may cause the whole queue to get stuck in a retry loop preventing it from progressing.
    /// to prevent this from happenning and allow other jobs in the queue to be given a chance at execution, you can reschedule failed jobs
    /// to be re-attempted at a future time instead. simply update the <see cref="IJobStorageRecord.ExecuteAfter"/> property to a future date/time
    /// and save the entity to the database (or do a partial update of only that property value).
    /// </para>
    /// </summary>
    /// <param name="job">the job that failed to execute succesfully</param>
    /// <param name="exception">the exeception that was thrown</param>
    /// <param name="ct">cancellation token</param>
    Task OnHandlerExecutionFailureAsync(TStorageRecord job, Exception exception, CancellationToken ct);

    /// <summary>
    /// this method will be called hourly. implement this method to delete stale records (completed or (incomplete and expired)) from storage.
    /// you can safely delete the completed records. the incomplete &amp; expired records can be moved to some other location (dead-letter-queue maybe) or for inspection by a human.
    /// or if you'd like to retry expired events, update the <see cref="IJobStorageRecord.ExpireOn"/> field to a future date/time.
    /// </summary>
    /// <param name="match">a boolean lambda expression to match stale records</param>
    /// <param name="ct">cancellation token</param>
    Task PurgeStaleJobsAsync(Expression<Func<TStorageRecord, bool>> match, CancellationToken ct);
}