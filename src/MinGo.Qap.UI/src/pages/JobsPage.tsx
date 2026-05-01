import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Play, Pause, Trash2, Plus } from 'lucide-react';
import toast from 'react-hot-toast';
import { jobApi } from '../api';
import CreateJobModal from '../components/CreateJobModal';
import StatusBadge from '../components/StatusBadge';
import DataTable from '../components/DataTable';
import ConfirmDialog from '../components/ConfirmDialog';
import type { JobSummaryDto } from '../types';

const JobsPage: React.FC = () => {
  const { schedulerName } = useParams<{ schedulerName: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [deleteConfirmJobId, setDeleteConfirmJobId] = useState<string | null>(null);
  const pageSize = 20;
  const decodedSchedulerName = schedulerName ? decodeURIComponent(schedulerName) : '';

  const { data: jobs, isLoading, error } = useQuery({
    queryKey: ['jobs', decodedSchedulerName, page],
    queryFn: async () => {
      const response = await jobApi.getAll(decodedSchedulerName, page, pageSize);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data || [];
    },
    enabled: !!decodedSchedulerName,
  });

  const triggerJob = useMutation({
    mutationFn: (jobKey: string) => jobApi.trigger(decodedSchedulerName, jobKey),
    onSuccess: () => {
      toast.success('Job triggered successfully');
      queryClient.invalidateQueries({ queryKey: ['jobs', decodedSchedulerName] });
    },
    onError: (err: Error) => toast.error('Failed to trigger job: ' + err.message),
  });

  const pauseJob = useMutation({
    mutationFn: (jobKey: string) => jobApi.pause(decodedSchedulerName, jobKey),
    onSuccess: () => {
      toast.success('Job paused successfully');
      queryClient.invalidateQueries({ queryKey: ['jobs', decodedSchedulerName] });
    },
    onError: (err: Error) => toast.error('Failed to pause job: ' + err.message),
  });

  const resumeJob = useMutation({
    mutationFn: (jobKey: string) => jobApi.resume(decodedSchedulerName, jobKey),
    onSuccess: () => {
      toast.success('Job resumed successfully');
      queryClient.invalidateQueries({ queryKey: ['jobs', decodedSchedulerName] });
    },
    onError: (err: Error) => toast.error('Failed to resume job: ' + err.message),
  });

  const deleteJob = useMutation({
    mutationFn: (jobKey: string) => jobApi.delete(decodedSchedulerName, jobKey),
    onSuccess: () => {
      toast.success('Job deleted successfully');
      queryClient.invalidateQueries({ queryKey: ['jobs', decodedSchedulerName] });
    },
    onError: (err: Error) => toast.error('Failed to delete job: ' + err.message),
  });

  if (isLoading) {
    return <div className="p-8 text-slate-400">Loading...</div>;
  }

  if (error) {
    return <div className="p-8 text-red-400">Error: {error.message}</div>;
  }

  const handleJobClick = (jobKey: string) => {
    navigate(`/schedulers/${encodeURIComponent(decodedSchedulerName)}/jobs/${encodeURIComponent(jobKey)}`);
  };

  const columns = [
    {
      header: 'Job Key',
      accessor: (row: JobSummaryDto) => row.jobKey,
      sortable: true,
    },
    {
      header: 'Type',
      accessor: (row: JobSummaryDto) => row.jobType,
    },
    {
      header: 'Group',
      accessor: (row: JobSummaryDto) => row.group,
    },
    {
      header: 'Status',
      accessor: (row: JobSummaryDto) => <StatusBadge status={row.status} />,
    },
    {
      header: 'Schedule',
      accessor: (row: JobSummaryDto) => {
        if (row.scheduleType === 'cron' && row.cronExpression) {
          return <span className="text-xs font-mono">{row.cronExpression}</span>;
        }
        return row.scheduleType;
      },
    },
    {
      header: 'Next Fire',
      accessor: (row: JobSummaryDto) => {
        if (!row.nextFireTime) return '-';
        return new Date(row.nextFireTime).toLocaleString();
      },
    },
    {
      header: 'Actions',
      accessor: (row: JobSummaryDto) => (
        <div className="flex gap-1" onClick={(e) => e.stopPropagation()}>
          <button
            onClick={() => triggerJob.mutate(row.jobKey)}
            className="p-1.5 text-blue-400 hover:text-blue-300 hover:bg-slate-700 rounded"
            title="Trigger"
          >
            <Play size={14} />
          </button>
          <button
            onClick={() => pauseJob.mutate(row.jobKey)}
            className="p-1.5 text-amber-400 hover:text-amber-300 hover:bg-slate-700 rounded"
            title="Pause"
          >
            <Pause size={14} />
          </button>
          <button
            onClick={() => resumeJob.mutate(row.jobKey)}
            className="p-1.5 text-green-400 hover:text-green-300 hover:bg-slate-700 rounded"
            title="Resume"
          >
            <Play size={14} />
          </button>
          <button
            onClick={() => setDeleteConfirmJobId(row.jobKey)}
            className="p-1.5 text-red-400 hover:text-red-300 hover:bg-slate-700 rounded"
            title="Delete"
          >
            <Trash2 size={14} />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <Link to={`/schedulers/${encodeURIComponent(decodedSchedulerName)}`} className="text-sm text-blue-400 hover:text-blue-300">
              ← Back to Scheduler
            </Link>
          </div>
          <h1 className="text-2xl font-bold text-slate-50">Jobs</h1>
          <p className="text-slate-400 text-sm">Scheduler: {decodedSchedulerName}</p>
        </div>
        <button
          onClick={() => setIsCreateModalOpen(true)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
        >
          <Plus size={16} />
          Create Job
        </button>
      </div>

      {/* Jobs Table */}
      <div className="bg-slate-800 rounded-lg border border-slate-700 overflow-hidden">
        <DataTable
          columns={columns}
          data={jobs || []}
          onRowClick={(row) => handleJobClick(row.jobKey)}
          emptyMessage="No jobs found for this scheduler"
        />
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between mt-4">
        <button
          onClick={() => setPage(p => Math.max(1, p - 1))}
          disabled={page === 1}
          className="px-3 py-1.5 bg-slate-800 text-slate-300 rounded hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Previous
        </button>
        <span className="text-slate-400 text-sm">Page {page}</span>
        <button
          onClick={() => setPage(p => p + 1)}
          className="px-3 py-1.5 bg-slate-800 text-slate-300 rounded hover:bg-slate-700"
        >
          Next
        </button>
      </div>

      {/* Create Job Modal */}
      {isCreateModalOpen && (
        <CreateJobModal
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
          schedulerName={decodedSchedulerName}
        />
      )}

      {/* Delete Confirmation */}
      {deleteConfirmJobId && (
        <ConfirmDialog
          title="Delete Job"
          message={`Are you sure you want to delete job "${deleteConfirmJobId}"?`}
          confirmLabel="Delete"
          onConfirm={() => {
            deleteJob.mutate(deleteConfirmJobId);
            setDeleteConfirmJobId(null);
          }}
          onCancel={() => setDeleteConfirmJobId(null)}
        />
      )}
    </div>
  );
};

export default JobsPage;
