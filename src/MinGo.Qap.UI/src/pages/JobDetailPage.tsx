import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useJob, useUpdateJob, useDeleteJob, useTriggerJob, usePauseJob, useResumeJob } from '../hooks/useClusters';
import { Play, Pause, Square, Trash2, ArrowLeft, Clock, Calendar } from 'lucide-react';
import type { UpdateJobRequest, JobDetailDto } from '../types';

const JobDetailPage: React.FC = () => {
  const { clusterId, jobKey } = useParams<{ clusterId: string; jobKey: string }>();
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);
  const [editParams, setEditParams] = useState<Record<string, any>>({});

  const { data: job, isLoading, error } = useJob(clusterId || '', jobKey || '') as { data: JobDetailDto | undefined; isLoading: boolean; error: any };
  const updateJob = useUpdateJob(clusterId || '', jobKey || '');
  const deleteJob = useDeleteJob(clusterId || '');
  const triggerJob = useTriggerJob(clusterId || '');
  const pauseJob = usePauseJob(clusterId || '');
  const resumeJob = useResumeJob(clusterId || '');

  const handleDelete = async () => {
    if (!confirm(`Are you sure you want to delete job "${jobKey}"?`)) return;
    
    try {
      await deleteJob.mutateAsync(jobKey || '');
      navigate(`/clusters/${clusterId}/jobs`);
    } catch (err: any) {
      alert('Failed to delete job: ' + err.message);
    }
  };

  const handleSave = async () => {
    const request: UpdateJobRequest = {
      params: editParams,
    };

    try {
      await updateJob.mutateAsync(request);
      setIsEditing(false);
    } catch (err: any) {
      alert('Failed to update job: ' + err.message);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'normal': return 'bg-green-500';
      case 'paused': return 'bg-amber-500';
      case 'blocked': return 'bg-red-500';
      default: return 'bg-slate-500';
    }
  };

  if (isLoading) {
    return <div className="p-8 text-slate-400">Loading...</div>;
  }

  if (error) {
    return <div className="p-8 text-red-400">Error: {error.message}</div>;
  }

  if (!job) {
    return <div className="p-8 text-slate-400">Job not found</div>;
  }

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <Link 
          to={`/clusters/${clusterId}/jobs`} 
          className="flex items-center gap-1 text-sm text-blue-400 hover:text-blue-300 mb-2"
        >
          <ArrowLeft size={16} />
          Back to Jobs
        </Link>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-50 font-mono">{job.jobKey}</h1>
            <span className={`status-dot ${getStatusColor(job.status)}`} />
            <span className="text-sm text-slate-400 capitalize">{job.status}</span>
          </div>
          <div className="flex gap-2">
            {job.status === 'paused' ? (
              <button
                onClick={() => resumeJob.mutate(job.jobKey)}
                className="btn-primary flex items-center gap-2"
              >
                <Play size={16} />
                Resume
              </button>
            ) : (
              <button
                onClick={() => pauseJob.mutate(job.jobKey)}
                className="btn-secondary flex items-center gap-2"
              >
                <Pause size={16} />
                Pause
              </button>
            )}
            <button
              onClick={() => triggerJob.mutate(job.jobKey)}
              className="btn-primary flex items-center gap-2"
            >
              <Play size={16} />
              Trigger Now
            </button>
            <button
              onClick={handleDelete}
              className="btn-danger flex items-center gap-2"
            >
              <Trash2 size={16} />
              Delete
            </button>
          </div>
        </div>
      </div>

      {/* Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column - Info */}
        <div className="lg:col-span-2 space-y-6">
          {/* Basic Info */}
          <div className="card">
            <h3 className="text-lg font-semibold text-slate-50 mb-4">Job Information</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-slate-400">Type:</span>
                <span className="ml-2 text-slate-50">{job.jobType}</span>
              </div>
              <div>
                <span className="text-slate-400">Group:</span>
                <span className="ml-2 text-slate-50">{job.group}</span>
              </div>
              <div>
                <span className="text-slate-400">Description:</span>
                <span className="ml-2 text-slate-50">{job.description || '-'}</span>
              </div>
            </div>
          </div>

          {/* Schedule */}
          <div className="card">
            <h3 className="text-lg font-semibold text-slate-50 mb-4 flex items-center gap-2">
              <Calendar size={18} />
              Schedule
            </h3>
            <div className="space-y-3 text-sm">
              <div className="flex items-center justify-between py-2 border-b border-slate-700">
                <span className="text-slate-400">Type</span>
                <span className="text-slate-50 capitalize">{job.schedule.type}</span>
              </div>
              
              {job.schedule.cronExpression && (
                <div className="flex items-center justify-between py-2 border-b border-slate-700">
                  <span className="text-slate-400">Cron Expression</span>
                  <code className="text-slate-50 bg-slate-700 px-2 py-1 rounded font-mono">
                    {job.schedule.cronExpression}
                  </code>
                </div>
              )}
              
              {job.schedule.intervalSeconds && (
                <div className="flex items-center justify-between py-2 border-b border-slate-700">
                  <span className="text-slate-400">Interval</span>
                  <span className="text-slate-50">{job.schedule.intervalSeconds} seconds</span>
                </div>
              )}
              
              <div className="flex items-center justify-between py-2">
                <span className="text-slate-400 flex items-center gap-2">
                  <Clock size={14} />
                  Next Run
                </span>
                <span className="text-slate-50">
                  {job.nextFireTime 
                    ? new Date(job.nextFireTime).toLocaleString() 
                    : '-'}
                </span>
              </div>
              
              {job.previousFireTime && (
                <div className="flex items-center justify-between py-2 border-t border-slate-700">
                  <span className="text-slate-400">Previous Run</span>
                  <span className="text-slate-50">
                    {new Date(job.previousFireTime).toLocaleString()}
                  </span>
                </div>
              )}
            </div>
          </div>

          {/* Parameters */}
          <div className="card">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-slate-50">Parameters</h3>
              {!isEditing && (
                <button
                  onClick={() => {
                    setEditParams(job.params || {});
                    setIsEditing(true);
                  }}
                  className="text-sm text-blue-400 hover:text-blue-300"
                >
                  Edit
                </button>
              )}
            </div>
            
            {isEditing ? (
              <div className="space-y-3">
                {Object.entries(editParams).map(([key, value]) => (
                  <div key={key}>
                    <label className="block text-sm font-medium text-slate-300 mb-1">
                      {key}
                    </label>
                    <input
                      type="text"
                      value={value?.toString() || ''}
                      onChange={(e) => setEditParams({ ...editParams, [key]: e.target.value })}
                      className="input"
                    />
                  </div>
                ))}
                <div className="flex gap-2 pt-2">
                  <button onClick={handleSave} className="btn-primary">
                    Save
                  </button>
                  <button onClick={() => setIsEditing(false)} className="btn-secondary">
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div className="space-y-2">
                {Object.entries(job.params || {}).length === 0 ? (
                  <p className="text-slate-500">No parameters</p>
                ) : (
                  Object.entries(job.params).map(([key, value]) => (
                    <div key={key} className="flex justify-between py-2 border-b border-slate-700/50">
                      <span className="text-slate-400">{key}</span>
                      <span className="text-slate-50 font-mono text-sm">{value?.toString()}</span>
                    </div>
                  ))
                )}
              </div>
            )}
          </div>
        </div>

        {/* Right Column - Options */}
        <div className="space-y-6">
          <div className="card">
            <h3 className="text-lg font-semibold text-slate-50 mb-4">Options</h3>
            <div className="space-y-3 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-slate-400">Concurrent Execution</span>
                <span className={job.options.disallowConcurrentExecution ? 'text-red-400' : 'text-green-400'}>
                  {job.options.disallowConcurrentExecution ? 'Disabled' : 'Allowed'}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-slate-400">Misfire Policy</span>
                <span className="text-slate-50">{job.options.misfirePolicy}</span>
              </div>
            </div>
          </div>

          {/* Actions Card */}
          <div className="card bg-slate-800/50">
            <h3 className="text-lg font-semibold text-slate-50 mb-4">Quick Actions</h3>
            <div className="space-y-2">
              <button
                onClick={() => triggerJob.mutate(job.jobKey)}
                disabled={triggerJob.isPending}
                className="w-full btn-primary flex items-center justify-center gap-2"
              >
                <Play size={16} />
                {triggerJob.isPending ? 'Triggering...' : 'Trigger Now'}
              </button>
              
              {job.status === 'paused' ? (
                <button
                  onClick={() => resumeJob.mutate(job.jobKey)}
                  disabled={resumeJob.isPending}
                  className="w-full btn-secondary flex items-center justify-center gap-2"
                >
                  <Play size={16} />
                  {resumeJob.isPending ? 'Resuming...' : 'Resume'}
                </button>
              ) : (
                <button
                  onClick={() => pauseJob.mutate(job.jobKey)}
                  disabled={pauseJob.isPending}
                  className="w-full btn-secondary flex items-center justify-center gap-2"
                >
                  <Square size={16} />
                  {pauseJob.isPending ? 'Pausing...' : 'Pause'}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default JobDetailPage;
