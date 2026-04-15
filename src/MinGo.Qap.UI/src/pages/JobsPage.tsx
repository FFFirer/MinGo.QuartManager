import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useJobs, useTriggerJob, usePauseJob, useResumeJob, useDeleteJob } from '../hooks/useClusters';
import { Play, Pause, Square, Trash2 } from 'lucide-react';
import CreateJobModal from '../components/CreateJobModal';

const JobsPage: React.FC = () => {
  const { clusterId } = useParams<{ clusterId: string }>();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const pageSize = 20;

  const { data: jobs, isLoading, error } = useJobs(clusterId || '', page, pageSize);
  const triggerJob = useTriggerJob(clusterId || '');
  const pauseJob = usePauseJob(clusterId || '');
  const resumeJob = useResumeJob(clusterId || '');
  const deleteJob = useDeleteJob(clusterId || '');

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

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <Link to="/" className="text-sm text-blue-400 hover:text-blue-300 mb-1 block">
            ← Back to Clusters
          </Link>
          <h1 className="text-2xl font-bold text-slate-50">Jobs</h1>
        </div>
        <button 
          onClick={() => setIsCreateModalOpen(true)}
          className="btn-primary"
        >
          + Create Job
        </button>
      </div>

      <div className="card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-slate-700">
              <th className="table-header">Job Key</th>
              <th className="table-header">Type</th>
              <th className="table-header">Status</th>
              <th className="table-header">Schedule</th>
              <th className="table-header">Next Run</th>
              <th className="table-header">Actions</th>
            </tr>
          </thead>
          <tbody>
            {jobs?.map((job: any) => (
              <tr 
                key={job.jobKey} 
                className="border-b border-slate-700/50 hover:bg-slate-800/50 cursor-pointer"
                onClick={() => navigate(`/clusters/${clusterId}/jobs/${job.jobKey}`)}
              >
                <td className="table-cell font-mono text-xs">{job.jobKey}</td>
                <td className="table-cell">{job.jobType}</td>
                <td className="table-cell">
                  <span className="flex items-center">
                    <span className={`status-dot ${getStatusColor(job.status)}`} />
                    {job.status}
                  </span>
                </td>
                <td className="table-cell text-xs">
                  {job.scheduleType === 'cron' ? job.cronExpression : job.scheduleType}
                </td>
                <td className="table-cell text-xs">
                  {job.nextFireTime ? new Date(job.nextFireTime).toLocaleString() : '-'}
                </td>
                <td className="table-cell">
                  <div className="flex gap-1" onClick={(e) => e.stopPropagation()}>
                    <button
                      className="p-1 hover:bg-slate-700 rounded"
                      title="Trigger now"
                      onClick={() => triggerJob.mutate(job.jobKey)}
                    >
                      <Play size={16} className="text-green-400" />
                    </button>
                    {job.status === 'paused' ? (
                      <button
                        className="p-1 hover:bg-slate-700 rounded"
                        title="Resume"
                        onClick={() => resumeJob.mutate(job.jobKey)}
                      >
                        <Play size={16} className="text-blue-400" />
                      </button>
                    ) : (
                      <button
                        className="p-1 hover:bg-slate-700 rounded"
                        title="Pause"
                        onClick={() => pauseJob.mutate(job.jobKey)}
                      >
                        <Pause size={16} className="text-amber-400" />
                      </button>
                    )}
                    <button
                      className="p-1 hover:bg-slate-700 rounded"
                      title="Delete"
                      onClick={() => {
                        if (confirm(`Delete job ${job.jobKey}?`)) {
                          deleteJob.mutate(job.jobKey);
                        }
                      }}
                    >
                      <Trash2 size={16} className="text-red-400" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {jobs?.length === 0 && (
          <div className="text-center py-8 text-slate-400">
            No jobs found. Create one to get started.
          </div>
        )}
      </div>

      {/* Create Job Modal */}
      {clusterId && (
        <CreateJobModal
          clusterId={clusterId}
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
        />
      )}
    </div>
  );
};

export default JobsPage;
