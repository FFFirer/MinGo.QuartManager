import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useJobs, useTriggerJob, usePauseJob, useResumeJob, useDeleteJob } from '../hooks/useClusters';
import { Play, Pause, Trash2 } from 'lucide-react';
import CreateJobModal from '../components/CreateJobModal';
import StatusBadge from '../components/StatusBadge';
import PageHeader from '../components/PageHeader';
import DataTable from '../components/DataTable';
import ConfirmDialog from '../components/ConfirmDialog';

const JobsPage: React.FC = () => {
  const { clusterId } = useParams<{ clusterId: string }>();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [deleteConfirmJobId, setDeleteConfirmJobId] = useState<string | null>(null);
  const [deletingJobId, setDeletingJobId] = useState<string | null>(null);
  const pageSize = 20;

  const { data: jobs, isLoading, error } = useJobs(clusterId || '', page, pageSize);
  const triggerJob = useTriggerJob(clusterId || '');
  const pauseJob = usePauseJob(clusterId || '');
  const resumeJob = useResumeJob(clusterId || '');
  const deleteJob = useDeleteJob(clusterId || '');

  if (isLoading) {
    return <div className="p-8">Loading...</div>;
  }

  if (error) {
    return <div className="p-8">Error: {error.message}</div>;
  }

  return (
    <div className="p-6">
      <PageHeader 
        title="Jobs"
        subtitle={clusterId ? `Cluster: ${clusterId}` : undefined}
        backPath="/"
        actions={(
          <button 
            onClick={() => setIsCreateModalOpen(true)}
            className="btn-primary"
          >
            + Create Job
          </button>
        )}
      />

      <DataTable
        columns={[
          {
            header: 'Job Key',
            accessor: 'jobKey',
            width: 120,
            align: 'left'
          },
          {
            header: 'Type',
            accessor: 'jobType',
            width: 100,
            align: 'left'
          },
          {
            header: 'Status',
            accessor: (row: any) => (
              <StatusBadge status={row.status} size="sm" showLabel={true} variant="inline" />
            ),
            width: 100,
            align: 'center'
          },
          {
            header: 'Schedule',
            accessor: (row: any) => 
              row.scheduleType === 'cron' ? row.cronExpression : row.scheduleType,
            width: 120,
            align: 'left'
          },
          {
            header: 'Next Run',
            accessor: (row: any) => 
              row.nextFireTime ? new Date(row.nextFireTime).toLocaleString() : '-',
            width: 140,
            align: 'left'
          },
          {
            header: 'Actions',
            accessor: (row: any) => (
              <div className="flex gap-1" onClick={(e) => e.stopPropagation()}>
                <button
                  className="p-1 hover:bg-slate-700 rounded"
                  title="Trigger now"
                  onClick={() => triggerJob.mutate(row.jobKey)}
                >
                  <Play size={16} className="text-green-400" />
                </button>
                {row.status === 'paused' ? (
                  <button
                    className="p-1 hover:bg-slate-700 rounded"
                    title="Resume"
                    onClick={() => resumeJob.mutate(row.jobKey)}
                  >
                    <Play size={16} className="text-blue-400" />
                  </button>
                ) : (
                  <button
                    className="p-1 hover:bg-slate-700 rounded"
                    title="Pause"
                    onClick={() => pauseJob.mutate(row.jobKey)}
                  >
                    <Pause size={16} className="text-amber-400" />
                  </button>
                )}
                <button
                  className="p-1 hover:bg-slate-700 rounded"
                  title="Delete"
                  onClick={() => {
                    setDeleteConfirmJobId(row.jobKey);
                  }}
                >
                  <Trash2 size={16} className="text-red-400" />
                </button>
              </div>
            ),
            width: 120,
            align: 'center'
          }
        ]}
        data={jobs || []}
        loading={isLoading}
        emptyMessage="No jobs found. Create one to get started."
        onRowClick={(job: any) => navigate(`/clusters/${clusterId}/jobs/${job.jobKey}`)}
        showBorder
        showHeader
        className="w-full"
      />

      {jobs?.length === 0 && (
        <div className="text-center py-8 text-slate-400">
          No jobs found. Create one to get started.
        </div>
      )}

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