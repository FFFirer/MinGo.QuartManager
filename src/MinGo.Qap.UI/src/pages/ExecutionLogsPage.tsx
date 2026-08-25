import React, { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { jobApi } from '../api';
import PageHeader from '../components/PageHeader';
import DataTable from '../components/DataTable';
import PaginationBar from '../components/PaginationBar';
import type { ExecutionLogEntryDto } from '../types';

const ExecutionLogsPage: React.FC = () => {
  const { schedulerName } = useParams<{ schedulerName: string }>();
  const decodedSchedulerName = schedulerName ? decodeURIComponent(schedulerName) : '';
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data: logsResponse, isLoading, error } = useQuery({
    queryKey: ['execution-logs', decodedSchedulerName, page, pageSize],
    queryFn: async () => {
      // Query all logs for the scheduler (no specific jobKey filter)
      const response = await jobApi.getLogs(decodedSchedulerName, '', '', page, pageSize);
      if (!response.success) throw new Error(response.errorMessage);
      return response.data;
    },
    enabled: !!decodedSchedulerName,
  });

  const logs = logsResponse?.items ?? [];
  const totalItems = logsResponse?.total ?? 0;
  const totalPages = logsResponse?.totalPages ?? 1;

  const columns = [
    {
      header: 'Job Key',
      accessor: (row: ExecutionLogEntryDto) => `${row.jobKey.group}.${row.jobKey.name}`,
      sortable: true,
    },
    {
      header: 'Start Time',
      accessor: (row: ExecutionLogEntryDto) => new Date(row.startTime).toLocaleString(),
      sortable: true,
    },
    {
      header: 'End Time',
      accessor: (row: ExecutionLogEntryDto) => row.endTime ? new Date(row.endTime).toLocaleString() : '-',
    },
    {
      header: 'Duration',
      accessor: (row: ExecutionLogEntryDto) => row.durationMs != null ? `${row.durationMs}ms` : '-',
    },
    {
      header: 'Status',
      accessor: (row: ExecutionLogEntryDto) => (
        <span className={`text-sm font-medium ${row.success ? 'text-green-400' : 'text-red-400'}`}>
          {row.success ? 'Success' : 'Failed'}
        </span>
      ),
    },
    {
      header: 'Agent',
      accessor: (row: ExecutionLogEntryDto) => row.agentId,
    },
    {
      header: 'Error',
      accessor: (row: ExecutionLogEntryDto) => row.errorMessage ? (
        <span className="text-xs text-red-400 truncate max-w-xs" title={row.errorMessage}>
          {row.errorMessage}
        </span>
      ) : '-',
    },
  ];

  if (isLoading) {
    return <div className="p-8 text-slate-400">Loading...</div>;
  }

  if (error) {
    return <div className="p-8 text-red-400">Error: {(error as Error).message}</div>;
  }

  return (
    <div className="p-6">
      <PageHeader
        title="Execution Logs"
        subtitle={`Scheduler: ${decodedSchedulerName}`}
        breadcrumbs={[
          { label: 'Schedulers', path: '/schedulers' },
          { label: decodedSchedulerName, path: `/schedulers/${encodeURIComponent(decodedSchedulerName)}` },
          { label: 'Execution Logs', active: true }
        ]}
      />

      <div className="bg-slate-800 rounded-lg border border-slate-700 overflow-hidden">
        <DataTable
          columns={columns}
          data={logs}
          emptyMessage="No execution logs found"
        />
      </div>

      <PaginationBar
        page={page}
        pageSize={pageSize}
        totalItems={totalItems}
        totalPages={totalPages}
        onPageChange={setPage}
        onPageSizeChange={(newSize) => { setPageSize(newSize); setPage(1); }}
      />
    </div>
  );
};

export default ExecutionLogsPage;
