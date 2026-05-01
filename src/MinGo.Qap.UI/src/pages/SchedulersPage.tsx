import React from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { schedulerApi } from '../api';
import { StatusBadge } from '../components/StatusBadge';
import { PageHeader } from '../components/PageHeader';
import { LoadingSkeleton } from '../components/LoadingSkeleton';
import { AlertCircle } from 'lucide-react';
import type { SchedulerSummaryDto, ApiResponse } from '../types';

const SchedulersPage: React.FC = () => {
  const { data, isLoading, isError, error, refetch } = useQuery<ApiResponse<SchedulerSummaryDto[]>, Error>({
    queryKey: ['schedulers'],
    queryFn: () => schedulerApi.getAll(),
    refetchInterval: 30000,
  });

  const schedulers: SchedulerSummaryDto[] = data?.data ?? [];
  const totalCount = schedulers.length;

  const formatDate = (iso?: string) => {
    if (!iso) return '-';
    return new Date(iso).toLocaleString();
  };

  if (isLoading) return <div className="p-6 space-y-4"><LoadingSkeleton /><LoadingSkeleton /><LoadingSkeleton /></div>;

  if (isError) {
    return (
      <div className="p-6">
        <div className="bg-red-500/10 border border-red-500/30 rounded-lg p-6 text-center">
          <AlertCircle size={48} className="mx-auto text-red-400 mb-4" />
          <h2 className="text-xl font-semibold text-slate-50 mb-2">Failed to load schedulers</h2>
          <p className="text-slate-400 mb-4">{error?.message}</p>
          <button onClick={() => refetch()} className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600">Retry</button>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageHeader title="Schedulers" subtitle={`Total: ${totalCount}`} />

      {schedulers.length === 0 ? (
        <div className="text-center py-12 text-slate-400">No schedulers found.</div>
      ) : (
        <div className="mt-4 overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-700">
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Name</th>
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Instance ID</th>
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Status</th>
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Clustered</th>
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Agents</th>
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Last Reported</th>
                <th className="text-left text-xs font-semibold text-slate-400 uppercase tracking-wider px-4 py-3">Running Since</th>
              </tr>
            </thead>
            <tbody>
              {schedulers.map((s) => (
                <tr key={s.id} className="border-b border-slate-800 hover:bg-slate-800/50 transition-colors">
                  <td className="px-4 py-3">
                    <Link to={`/schedulers/${encodeURIComponent(s.schedulerName)}`} className="text-blue-400 hover:text-blue-300">
                      {s.schedulerName}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-sm text-slate-300">{s.schedulerInstanceId || '-'}</td>
                  <td className="px-4 py-3"><StatusBadge status={s.status} /></td>
                  <td className="px-4 py-3 text-sm text-slate-300">{s.isClustered ? 'Yes' : 'No'}</td>
                  <td className="px-4 py-3 text-sm text-slate-300">{s.agentCount}</td>
                  <td className="px-4 py-3 text-sm text-slate-300">{formatDate(s.lastReportedAt)}</td>
                  <td className="px-4 py-3 text-sm text-slate-300">{formatDate(s.runningSince)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default SchedulersPage;
