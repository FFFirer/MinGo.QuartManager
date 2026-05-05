import React, { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Play, Pause, Trash2, Plus, RefreshCw } from 'lucide-react';
import toast from 'react-hot-toast';
import { jobApi } from '../api';
import CreateJobPanel from '../components/CreateJobPanel';
import StatusBadge from '../components/StatusBadge';
import DataTable from '../components/DataTable';
import PaginationBar from '../components/PaginationBar';
import ConfirmDialog from '../components/ConfirmDialog';
import PageHeader from '../components/PageHeader';
import type { JobSummaryDto } from '../types';

const JobsPage: React.FC = () => {
  const { schedulerName } = useParams<{ schedulerName: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [deleteConfirmJobId, setDeleteConfirmJobId] = useState<string | null>(null);
  // Batch selection state
  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(new Set());
  const [batchDeleteDialogOpen, setBatchDeleteDialogOpen] = useState(false);
  // Page size is now a stateful value (default 20)
  const [pageSize, setPageSize] = useState<number>(20);
  const decodedSchedulerName = schedulerName ? decodeURIComponent(schedulerName) : '';

  const { data: jobsResponse, isLoading, isFetching, error, refetch } = useQuery({
    queryKey: ['jobs', decodedSchedulerName, page, pageSize],
    queryFn: async () => {
      const response = await jobApi.getAll(decodedSchedulerName, page, pageSize);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!decodedSchedulerName,
  });

  const jobs = jobsResponse?.items ?? [];
  const totalItems = jobsResponse?.total ?? 0;
  const totalPages = jobsResponse?.totalPages ?? 1;

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

  // Batch operations across selected jobs
  const batchMutation = useMutation({
    mutationFn: async (action: 'trigger' | 'pause' | 'resume' | 'delete') => {
      const keys = Array.from(selectedKeys);
      if (keys.length === 0) return { total: 0, successes: 0, failures: 0 };
      const promises = keys.map((key) => {
        switch (action) {
          case 'trigger': return jobApi.trigger(decodedSchedulerName, key);
          case 'pause': return jobApi.pause(decodedSchedulerName, key);
          case 'resume': return jobApi.resume(decodedSchedulerName, key);
          case 'delete': return jobApi.delete(decodedSchedulerName, key);
        }
      });
      const results = await Promise.allSettled(promises);
      const successes = results.filter(r => r.status === 'fulfilled').length;
      const failures = results.filter(r => r.status === 'rejected').length;
      return { total: keys.length, successes, failures };
    },
    onSuccess: (payload: any) => {
      const { total, successes, failures } = payload || { total: 0, successes: 0, failures: 0 };
      if (total === 0) {
        return;
      }
      if (failures === 0) {
        toast.success(`Triggered ${successes} of ${total} jobs successfully`);
      } else if (successes > 0) {
        toast.warn(`Partial success: ${successes} of ${total} succeeded, ${failures} failed`);
      } else {
        toast.error(`Failed to perform operation on all ${total} jobs`);
      }
      queryClient.invalidateQueries({ queryKey: ['jobs', decodedSchedulerName] });
      setSelectedKeys(new Set());
      // Close potential delete confirmation dialog if open
      setBatchDeleteDialogOpen(false);
    },
    onError: (err: any) => {
      toast.error('Batch operation failed: ' + (err?.message ?? 'Unknown error'));
    },
  });

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPage(1);
  };

  if (isLoading) {
    return <div className="p-8 text-slate-400">Loading...</div>;
  }

  if (error) {
    return <div className="p-8 text-red-400">Error: {error.message}</div>;
  }

  const handleJobClick = (jobKey: string) => {
    navigate(`/schedulers/${encodeURIComponent(decodedSchedulerName)}/jobs/${encodeURIComponent(jobKey)}`);
  };

  // Columns definition. Insert a checkbox column as the FIRST column for batch selection.
  // We cast to `any` to allow a React element in the header and cells without touching DataTable.tsx.
  const columns: any[] = [
      {
        header: (
          <input
            type="checkbox"
            checked={!!jobs && jobs.length > 0 && selectedKeys.size === jobs.length}
            onChange={(e) => {
              e.stopPropagation();
              if (e.target.checked) {
                const keys = (jobs || []).map((j) => j.jobKey);
                setSelectedKeys(new Set<string>(keys));
              } else {
                setSelectedKeys(new Set<string>());
              }
            }}
            onClick={(e) => e.stopPropagation()}
          />
        ),
      accessor: (row: JobSummaryDto) => (
        <input
          type="checkbox"
          className="h-4 w-4"
          checked={selectedKeys.has(row.jobKey)}
          onChange={(e) => {
            e.stopPropagation();
            setSelectedKeys((prev) => {
              const next = new Set<string>(prev);
              if (next.has(row.jobKey)) next.delete(row.jobKey);
              else next.add(row.jobKey);
              return next;
            });
          }}
          onClick={(e) => e.stopPropagation()}
        />
      ),
      width: 'w-14',
    },
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
            onClick={(e) => { e.stopPropagation(); triggerJob.mutate(row.jobKey); }}
            className="p-1.5 text-blue-400 hover:text-blue-300 hover:bg-slate-700 rounded"
            title="Trigger"
          >
            <Play size={14} />
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); pauseJob.mutate(row.jobKey); }}
            className="p-1.5 text-amber-400 hover:text-amber-300 hover:bg-slate-700 rounded"
            title="Pause"
          >
            <Pause size={14} />
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); resumeJob.mutate(row.jobKey); }}
            className="p-1.5 text-green-400 hover:text-green-300 hover:bg-slate-700 rounded"
            title="Resume"
          >
            <Play size={14} />
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); setDeleteConfirmJobId(row.jobKey); }}
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
      <PageHeader
        title="Jobs"
        subtitle={`Scheduler: ${decodedSchedulerName}`}
        breadcrumbs={[
          { label: 'Schedulers', path: '/schedulers' },
          { label: decodedSchedulerName, path: `/schedulers/${encodeURIComponent(decodedSchedulerName)}` },
          { label: 'Jobs', active: true }
        ]}
        actions={
          <div className="flex items-center gap-2">
            <button
              onClick={() => refetch()}
              className="flex items-center gap-2 px-4 py-2 bg-slate-700 text-white rounded-lg hover:bg-slate-600 transition-colors"
              title="Refresh Jobs"
            >
              <RefreshCw size={16} className={isFetching ? 'animate-spin' : ''} />
              Refresh
            </button>
            <button
              onClick={() => setIsCreateModalOpen(true)}
              className="flex items-center gap-2 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
            >
              <Plus size={16} />
              Create Job
            </button>
          </div>
        }
      />

      {/* Batch actions bar - appears when items are selected */}
      {selectedKeys.size > 0 && (
        <div className="bg-slate-800 rounded-lg p-3 mb-3 flex items-center justify-between">
          <span className="text-sm text-slate-200">{selectedKeys.size} selected</span>
          <div className="flex items-center gap-2">
            <button onClick={() => batchMutation.mutate('trigger')} className="px-3 py-1.5 bg-blue-500 text-white rounded hover:bg-blue-600">Trigger</button>
            <button onClick={() => batchMutation.mutate('pause')} className="px-3 py-1.5 bg-slate-700 text-slate-200 rounded hover:bg-slate-600">Pause</button>
            <button onClick={() => batchMutation.mutate('resume')} className="px-3 py-1.5 bg-slate-700 text-slate-200 rounded hover:bg-slate-600">Resume</button>
            <button onClick={() => setBatchDeleteDialogOpen(true)} className="px-3 py-1.5 bg-red-600 text-white rounded hover:bg-red-500">Delete</button>
          </div>
        </div>
      )}

      {/* Batch Delete Confirmation for selected items */}
      {batchDeleteDialogOpen && (
        <ConfirmDialog
          title="Delete Selected Jobs"
          message={`Are you sure you want to delete ${selectedKeys.size} selected job(s)? This action cannot be undone.`}
          confirmLabel="Delete"
          onConfirm={() => {
            batchMutation.mutate('delete');
            setBatchDeleteDialogOpen(false);
          }}
          onCancel={() => setBatchDeleteDialogOpen(false)}
        />
      )}

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
      <PaginationBar
        page={page}
        pageSize={pageSize}
        totalItems={totalItems}
        totalPages={totalPages}
        onPageChange={setPage}
        onPageSizeChange={handlePageSizeChange}
      />

      {/* Create Job Panel */}
      {isCreateModalOpen && (
        <CreateJobPanel
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
