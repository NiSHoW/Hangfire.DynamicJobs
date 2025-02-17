// This file is part of the Hangfire Core extension set. Copyright © 2023 Hangfire OÜ.
// Please see the LICENSE file for the licensing details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Storage;

namespace Hangfire
{
    public static class DynamicJobRecurringJobManagerExtensions
    {
        public static void AddOrUpdateDynamic(
            [NotNull] IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] Job job,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (recurringJobId == null) throw new ArgumentNullException(nameof(recurringJobId));
            if (job == null) throw new ArgumentNullException(nameof(job));
            if (cronExpression == null) throw new ArgumentNullException(nameof(cronExpression));

            manager.AddOrUpdate(
                recurringJobId,
                ToDynamicJob(job, options?.Filters),
                cronExpression,
                options ?? new RecurringJobOptions());
        }

        public static void AddOrUpdateDynamic(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] Expression<Action> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            var job = Job.FromExpression(methodCall);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] string queue,
            [NotNull] Expression<Action> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            var job = Job.FromExpression(methodCall, queue);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic<T>(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] Expression<Action<T>> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            var job = Job.FromExpression(methodCall);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic<T>(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] string queue,
            [NotNull] Expression<Action<T>> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            var job = Job.FromExpression(methodCall, queue);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] Expression<Func<Task>> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            var job = Job.FromExpression(methodCall);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] string queue,
            [NotNull] Expression<Func<Task>> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            var job = Job.FromExpression(methodCall, queue);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic<T>(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] Expression<Func<T, Task>> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            var job = Job.FromExpression(methodCall);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        public static void AddOrUpdateDynamic<T>(
            [NotNull] this IRecurringJobManager manager,
            [NotNull] string recurringJobId,
            [NotNull] string queue,
            [NotNull] Expression<Func<T, Task>> methodCall,
            [NotNull] string cronExpression,
            [CanBeNull] DynamicRecurringJobOptions options = null)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            var job = Job.FromExpression(methodCall, queue);
            AddOrUpdateDynamic(manager, recurringJobId, job, cronExpression, options);
        }

        private static Job ToDynamicJob([NotNull] Job job, [CanBeNull] IEnumerable<JobFilterAttribute> filters)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            var invocationData = InvocationData.SerializeJob(job);

            //get display name form original job
            var jobDisplayNameAttribute = job.Method.GetCustomAttributes<JobDisplayNameAttribute>(inherit: true).FirstOrDefault();
            //TODO: maybe is better resolve name here and serialize as string?

            //get queue filter from original job            
            ////    Job.Method.GetMethodFilterAttributes() inaccessibile
            ////    ReflectedAttributeCache.GetMethodFilterAttributes(job.Method); inaccessible
            var typeFiltes = job.Type.GetTypeInfo().GetCustomAttributes(typeof(QueueAttribute), inherit: true).Cast<QueueAttribute>();
            var methodFiltes = job.Method.GetCustomAttributes(typeof(QueueAttribute), inherit: true).Cast<QueueAttribute>();

            //generate list of filters for dynamicjob
            var dynamicfilters = new List<JobFilterAttribute>(filters?.ToArray() ?? Enumerable.Empty<JobFilterAttribute>());
            dynamicfilters.AddRange(typeFiltes);
            dynamicfilters.AddRange(methodFiltes);

            var dynamicJob = new DynamicJob(invocationData.Type,
                invocationData.Method,
                !String.IsNullOrEmpty(invocationData.ParameterTypes) ? invocationData.ParameterTypes : null,
                invocationData.Arguments,
                dynamicfilters?.ToArray(),
                jobDisplayNameAttribute);

            return Job.FromExpression(() => DynamicJob.Execute(dynamicJob, default));
        }
    }
}