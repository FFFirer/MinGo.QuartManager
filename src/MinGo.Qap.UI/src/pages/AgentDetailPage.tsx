import React from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { agentApi } from '../api';
import type { AgentDetailDto, AgentSchedulerDto } from '../types';
import { StatusBadge } from '../components/StatusBadge';
import { LoadingSkeleton } from '../components/LoadingSkeleton';
import { PageHeader } from '../components/PageHeader';

type ApiResponse<T> = {
  data: T;
  [key: string]: any;
};

export const AgentDetailPage: React.FC = () => {
  const { agentId } = useParams<{ agentId: string }>();
  const navigate = useNavigate();

  const { data: apiResp, isLoading, isError, error, refetch } = useQuery<ApiResponse<AgentDetailDto>, Error>(
    ['agent', agentId],
    () => agentApi.get(agentId as string),
    {
      enabled: !!agentId,
      refetchInterval: 30000, // 30 seconds auto-refresh
      refetchOnWindowFocus: false,
    }
  );

  // Normalize to AgentDetailDto when API returns { data: AgentDetailDto }
  const agent: AgentDetailDto | undefined = apiResp?.data;

  const formatDate = (value?: string | number) => {
    if (!value) return '-';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    return d.toLocaleString();
  };

  const renderInfoCard = (label: string, value: React.ReactNode) => (
    <div className="bg-slate-800 rounded-lg p-4 shadow" key={label}>
      <div className="text-sm text-slate-300 mb-1">{label}</div>
      <div className="text-base text-slate-50 font-semibold">{value}</div>
    </div>
  );

  if (isLoading) {
    return (
      <div className="min-h-screen bg-slate-900 text-slate-50 p-6">
        <LoadingSkeleton />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="min-h-screen bg-slate-900 text-slate-50 p-6 flex items-center justify-center">
        <div className="bg-slate-800 rounded-lg p-6 max-w-xl w-full shadow">
          <div className="text-lg font-semibold mb-2">Failed to load agent details.</div>
          <div className="text-sm text-slate-300">{error?.message ?? 'An unknown error occurred.'}</div>
          <button className="mt-4 px-4 py-2 bg-slate-700 rounded hover:bg-slate-600" onClick={() => refetch()}>
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-900 text-slate-50 p-6">
      {/* Back link */}
      <div className="mb-4">
        <Link to="/agents" className="inline-flex items-center text-sm text-slate-300 hover:text-slate-100">
          <span className="mr-2" aria-hidden="true">←</span> Back to Agents
        </Link>
      </div>

      {/* Agent header with status */}
      {agent && (
        <div className="mb-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-semibold text-slate-50">{agent.name}</h1>
              <StatusBadge status={agent.status} />
            </div>
          </div>
          <PageHeader title={agent.name} />
        </div>
      )}

      {/* Agent info cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-6">
        {agent ? (
          <>
            {renderInfoCard('ID', <span className="font-mono">{agent.id}</span>)}
            {renderInfoCard('URL', <a href={agent.url} target="_blank" rel="noreferrer" className="text-blue-400 hover:underline">{agent.url}</a>)}
            {renderInfoCard('Version', agent.agentVersion ?? '-')}
            {renderInfoCard('Started At', formatDate(agent.startedAt))}
            {renderInfoCard('Last Heartbeat', formatDate(agent.lastHeartbeat))}
            {renderInfoCard('Last Reported', formatDate(agent.lastReportedAt))}
          </>
        ) : (
          <>
            {renderInfoCard('ID', '-')}
            {renderInfoCard('URL', '-')}
            {renderInfoCard('Version', '-')}
            {renderInfoCard('Started At', '-')}
            {renderInfoCard('Last Heartbeat', '-')}
            {renderInfoCard('Last Reported', '-')}
          </>
        )}
      </div>

      {/* Schedulers list */}
      <div className="bg-slate-800 rounded-lg p-4 shadow mb-6">
        <div className="flex items-center justify-between mb-2">
          <h2 className="text-xl font-semibold text-slate-50">Associated Schedulers</h2>
        </div>
        {agent?.schedulers && agent.schedulers.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-slate-700">
              <thead>
                <tr>
                  <th className="px-4 py-2 text-left text-xs font-semibold text-slate-300 uppercase">Scheduler</th>
                  <th className="px-4 py-2 text-left text-xs font-semibold text-slate-300 uppercase">Instance</th>
                  <th className="px-4 py-2 text-left text-xs font-semibold text-slate-300 uppercase">Status</th>
                  <th className="px-4 py-2 text-left text-xs font-semibold text-slate-300 uppercase">Clustered</th>
                  <th className="px-4 py-2 text-left text-xs font-semibold text-slate-300 uppercase">Reported At</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-700">
                {agent.schedulers.map((sched: AgentSchedulerDto) => (
                  <tr
                    key={sched.schedulerInfoId}
                    className="hover:bg-slate-700 cursor-pointer"
                    onClick={() => navigate(`/schedulers/${encodeURIComponent(sched.schedulerName)}`)}
                  >
                    <td className="px-4 py-3 text-sm text-slate-50">{sched.schedulerName}</td>
                    <td className="px-4 py-3 text-sm text-slate-200">{sched.schedulerInstanceId}</td>
                    <td className="px-4 py-3 text-sm text-slate-200">{sched.status}</td>
                    <td className="px-4 py-3 text-sm text-slate-200">{sched.isClustered ? 'Yes' : 'No'}</td>
                    <td className="px-4 py-3 text-sm text-slate-200">{formatDate(sched.reportedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-sm text-slate-300">No schedulers associated with this agent.</div>
        )}
      </div>
    </div>
  );
};

export default AgentDetailPage;
