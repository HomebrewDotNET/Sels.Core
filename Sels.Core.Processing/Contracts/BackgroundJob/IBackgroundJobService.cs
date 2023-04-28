using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Service for scheduling background jobs and fetching state.
    /// </summary>
    public interface IBackgroundJobService
    {
        #region BackgroundJob
        /// <summary>
        /// Creates a transaction to queue a new background job of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the background job to queue</typeparam>
        /// <param name="input">Optional input for the background job. Should be serializable to json</param>
        /// <returns>A transaction for the job with an option to customize where and how a job is scheduled</returns>
        Task<IBackgroundJobTransaction> EnqueueAsync<T>(object input = null) where T : class, IBackgroundJob;
        /// <summary>
        /// Creates a transaction to queue a new background job of type <paramref name="backgroundJobType"/>.
        /// </summary>
        /// <param name="backgroundJobType">The type of the background job to queue</param>
        /// <param name="input">Optional input for the background job. Should be serializable to json</param>
        /// <returns>A transaction for the job with an option to customize where and how a job is scheduled</returns>
        Task<IBackgroundJobTransaction> EnqueueAsync(Type backgroundJobType, object input = null);
        /// <summary>
        /// Fetches the information and state of background job with <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the background job</param>
        /// <returns>The information and current state of the background job</returns>
        Task<IBackgroundJobInfo> GetJobInfoAsync(string id);
        #endregion

        #region RecurringJob
        /// <summary>
        /// Fetches the information about all recurring jobs.
        /// </summary>
        /// <param name="typeFilter">What type of recurring jobs to returns, when set to null all job types will be returned</param>
        /// <returns>All currently deployed recurring jobs</returns>
        Task<IRecurringJobInfo[]> GetAllRecurringJobs(RecurringJobType? typeFilter = null);
        /// <summary>
        /// Fetches the information and state of recurring background job with <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the recurring background job</param>
        /// <returns>The information and current state of the recurring background job</returns>
        Task<IRecurringJobInfo> GetRecurringJobInfoAsync(string id);
        /// <summary>
        /// Creates or updates a recurring job with id <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of the background job created by the recurring job</typeparam>
        /// <param name="id">The id of the recurring job</param>
        /// <param name="input">Optional input for the created background jobs</param>
        /// <param name="queue">The queue for jobs created by the recurring job</param>
        /// <param name="priority">Optional priority of jobs created by the recurring job</param>
        Task AddOrUpdateRecurringJobAsync<T>(string id, object input, string queue, int? priority = null) where T : class, IBackgroundJob;
        /// <summary>
        /// Creates or updates a recurring job with id <paramref name="id"/>.
        /// </summary>
        /// <param name="backgroundJobType">The type of the background job created by the recurring job</param>
        /// <param name="id">The id of the recurring job</param>
        /// <param name="input">Optional input for the created background jobs</param>
        /// <param name="queue">The queue for jobs created by the recurring job</param>
        /// <param name="priority">Optional priority of jobs created by the recurring job</param>
        Task AddOrUpdateRecurringJobAsync(Type backgroundJobType, string id, object input, string queue, int? priority = null);
        /// <summary>
        /// Deploys all recurring jobs in <paramref name="deploymentInfo"/> on the current node. 
        /// Recurring jobs on the current node not included in <paramref name="deploymentInfo"/> will be deleted, otherwise they will be created or updated.
        /// Recurring jobs will have a type of <see cref="RecurringJobType.Node"/>.
        /// </summary>
        /// <param name="deploymentInfo">Enumerator returning all recurring jobs to deploy</param>
        Task DeployRecurringJobsOnCurrentAsync(IEnumerable<RecurringJobDeploymentInfo> deploymentInfo);
        /// <summary>
        /// Deploys all recurring jobs in <paramref name="deploymentInfo"/>. 
        /// Recurring jobs with type <see cref="RecurringJobType.System"/> not included in <paramref name="deploymentInfo"/> will be deleted, otherwise they will be created or updated.
        /// Recurring jobs will have a type of <see cref="RecurringJobType.System"/>.
        /// </summary>
        /// <param name="deploymentInfo">Enumerator returning all recurring jobs to deploy</param>
        Task DeploySystemRecurringJobs(IEnumerable<RecurringJobDeploymentInfo> deploymentInfo);
        #endregion
    }
}
