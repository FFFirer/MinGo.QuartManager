import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Play, Pause, Trash2 } from 'lucide-react';
import { jobApi, manifestApi } from '../api';
import ConfirmDialog from '../components/ConfirmDialog';
import PageHeader from '../components/PageHeader';
import { LoadingSkeleton } from '../components/LoadingSkeleton';
import JobParamsDisplay from '../components/JobParamsDisplay';
import type { JobDetailDto, ScheduleDto, QuartzOptionsDto, JobManifestDto } from '../types';

/** Safely parse a JSON string, falling back to default on failure */
function tryParseJson<T>(raw: string, fallback: T): T {
  if (typeof raw !== 'string') return raw as unknown as T;
  try {
    return JSON.parse(raw) as T;
  } catch {
    console.warn('Failed to parse JSON string:', raw);
    return fallback;
  }
}

const JobDetailPage: React.FC = () => {
  const { schedulerName, jobKey } = useParams<{ schedulerName: string; jobKey: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);

  const decodedSchedulerName = schedulerName ? decodeURIComponent(schedulerName) : '';
  const decodedJobKey = jobKey ? decodeURIComponent(jobKey) : '';

  const { data: job, isLoading, error } = useQuery({
    queryKey: ['job', decodedSchedulerName, decodedJobKey],
    queryFn: async () => {
      const response = await jobApi.get(decodedSchedulerName, decodedJobKey);
      if (!response.success) throw new Error(response.errorMessage);
      const dto = response.data!;
      // Deserialize string fields from JobDefinitionDto to match JobDetailDto shape
      return {
        jobKey: dto.jobKey,
        jobType: dto.jobType,
        group: '',
        status: dto.status,
        description: '',
        schedule: tryParseJson<ScheduleDto>(dto.schedule, {} as ScheduleDto),
        options: tryParseJson<QuartzOptionsDto>(dto.options, {} as QuartzOptionsDto),
        params: tryParseJson<Record<string, any>>(dto.params, {}),
        nextFireTime: undefined,
        previousFireTime: undefined,
      } as JobDetailDto;
    },
    enabled: !!decodedSchedulerName && !!decodedJobKey,
  });

  // Fetch manifest for parameter metadata
  const { data: manifest } = useQuery({
    queryKey: ['manifest', decodedSchedulerName],
    queryFn: async () => {
      const response = await manifestApi.get(decodedSchedulerName);
      if (!response.success) return null;
      return response.data;
    },
    enabled: !!decodedSchedulerName,
    staleTime: 60_000,
  });

  const triggerJob = useMutation({
    mutationFn: () => jobApi.trigger(decodedSchedulerName, decodedJobKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['job', decodedSchedulerName, decodedJobKey] });
    },
  });

  const pauseJob = useMutation({
    mutationFn: () => jobApi.pause(decodedSchedulerName, decodedJobKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['job', decodedSchedulerName, decodedJobKey] });
    },
  });

  const resumeJob = useMutation({
    mutationFn: () => jobApi.resume(decodedSchedulerName, decodedJobKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['job', decodedSchedulerName, decodedJobKey] });
    },
  });

  const deleteJob = useMutation({
    mutationFn: () => jobApi.delete(decodedSchedulerName, decodedJobKey),
    onSuccess: () => {
      navigate(`/schedulers/${encodeURIComponent(decodedSchedulerName)}/jobs`);
    },
    onError: (err: Error) => alert('Failed to delete job: ' + err.message),
  });

  const handleDelete = async () => {
    try {
      await deleteJob.mutateAsync();
    } catch (err: any) {
      alert('Failed to delete job: ' + err.message);
    }
  };

  if (isLoading) {
    return <div className="p-6"><LoadingSkeleton /></div>;
  }

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-500/10 border border-red-500/30 rounded-lg p-6 text-center">
          <p className="text-red-400">{error.message}</p>
          <Link
            to={`/schedulers/${encodeURIComponent(decodedSchedulerName)}/jobs`}
            className="mt-4 inline-block text-blue-400 hover:text-blue-300"
          >
            ← Back to Jobs
          </Link>
        </div>
      </div>
    );
  }

  if (!job) return null;

  const triggerStateDisplay = (() => {
    switch (job.status) {
      case 'normal': return { label: 'Normal', color: 'text-green-400' };
      case 'paused': return { label: 'Paused', color: 'text-amber-400' };
      case 'blocked': return { label: 'Blocked', color: 'text-red-400' };
      case 'complete': return { label: 'Complete', color: 'text-blue-400' };
      case 'error': return { label: 'Error', color: 'text-red-500' };
      default: return { label: job.status, color: 'text-slate-400' };
    }
  })();

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return 'N/A';
    return new Date(dateStr).toLocaleString();
  };

  const scheduleTypeDisplay = (() => {
    if (!job.schedule) return 'N/A';
    const type = (job.schedule.type || '').toLowerCase();
    switch (type) {
      case 'cron': return `Cron: ${job.schedule.cronExpression}`;
      case 'interval': return `Every ${job.schedule.intervalSeconds}s`;
      case 'once': return `Once at ${formatDate(job.schedule.runAt)}`;
      default: return job.schedule.type;
    }
  })();

  // Get parameter definitions for this job type from manifest
  const paramDefinitions = manifest?.jobs?.find(j => j.key === job.jobType)?.parameters;

  return (
    <div className="p-6">
      {/* Header */}
      <PageHeader
        title={decodedJobKey}
        subtitle={`Scheduler: ${decodedSchedulerName}`}
        backPath={`/schedulers/${encodeURIComponent(decodedSchedulerName)}/jobs`}
        breadcrumbs={[
          { label: 'Schedulers', path: '/schedulers' },
          { label: decodedSchedulerName, path: `/schedulers/${encodeURIComponent(decodedSchedulerName)}` },
          { label: 'Jobs', path: `/schedulers/${encodeURIComponent(decodedSchedulerName)}/jobs` },
          { label: decodedJobKey, active: true }
        ]}
      />
      <div className="flex gap-2 mb-6">
          <button
            onClick={() => triggerJob.mutate()}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-blue-500/20 text-blue-400 rounded-lg hover:bg-blue-500/30 transition-colors"
          >
            <Play size={14} /> Trigger
          </button>
          <button
            onClick={() => pauseJob.mutate()}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-500/20 text-amber-400 rounded-lg hover:bg-amber-500/30 transition-colors"
          >
            <Pause size={14} /> Pause
          </button>
          <button
            onClick={() => resumeJob.mutate()}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-green-500/20 text-green-400 rounded-lg hover:bg-green-500/30 transition-colors"
          >
            <Play size={14} /> Resume
          </button>
          <button
            onClick={() => setIsDeleteConfirmOpen(true)}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-red-500/20 text-red-400 rounded-lg hover:bg-red-500/30 transition-colors"
          >
            <Trash2 size={14} /> Delete
          </button>
        </div>

      {/* Job Info Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Job Type</div>
          <div className="text-sm text-slate-50">{job.jobType}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Group</div>
          <div className="text-sm text-slate-50">{job.group || 'default'}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Status</div>
          <div className={`text-sm font-medium ${triggerStateDisplay.color}`}>{triggerStateDisplay.label}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Schedule</div>
          <div className="text-sm text-slate-50">{scheduleTypeDisplay}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Next Fire Time</div>
          <div className="text-sm text-slate-50">{formatDate(job.nextFireTime)}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Previous Fire Time</div>
          <div className="text-sm text-slate-50">{formatDate(job.previousFireTime)}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Description</div>
          <div className="text-sm text-slate-50">{job.description || 'No description'}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Disallow Concurrent</div>
          <div className="text-sm text-slate-50">{job.options.disallowConcurrentExecution ? 'Yes' : 'No'}</div>
        </div>
        <div className="bg-slate-800 rounded-lg p-4 border border-slate-700">
          <div className="text-xs text-slate-500 mb-1">Misfire Policy</div>
          <div className="text-sm text-slate-50">{job.options.misfirePolicy}</div>
        </div>
      </div>

      {/* Job Parameters */}
      {job.params && Object.keys(job.params).length > 0 && (
        <JobParamsDisplay
          params={job.params}
          paramDefinitions={paramDefinitions}
          searchable={true}
        />
      )}

      {/* Delete Confirmation */}
      {isDeleteConfirmOpen && (
        <ConfirmDialog
          isOpen={isDeleteConfirmOpen}
          title="Delete Job"
          message={`Are you sure you want to delete "${decodedJobKey}"? This action cannot be undone.`}
          confirmLabel="Delete"
          onConfirm={handleDelete}
          onClose={() => setIsDeleteConfirmOpen(false)}
        />
      )}
    </div>
  );
};

export default JobDetailPage;
