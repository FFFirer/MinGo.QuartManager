import React, { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { agentApi } from '../api';
import { AgentSummaryDto } from '../types';
import StatusBadge from '../components/StatusBadge';
import { LoadingSkeleton } from '../components/LoadingSkeleton';
import PageHeader from '../components/PageHeader';
import { AlertCircle } from 'lucide-react';

// Simple formatter for ISO dates to a readable string
function formatDate(iso?: string): string {
  if (!iso) return '';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

const AgentsPage: React.FC = () => {
  const { data, isLoading, isError, error } = useQuery<AgentSummaryDto[], Error>({
    queryKey: ['agents'],
    queryFn: () => agentApi.getAll(),
    // 30 seconds auto-refresh, don't refetch on window focus to avoid thrashing
    refetchInterval: 30000,
    refetchOnWindowFocus: false,
  });

  const agents = (data ?? []) as AgentSummaryDto[];
  const headerCols = useMemo(
    () => ['Name', 'URL', 'Status', 'Scheduler', 'Last Heartbeat', 'Started At'],
    []
  );

  return (
    <div className="min-h-screen bg-slate-900 text-slate-50">
      <PageHeader title="Agents" subtitle={`${agents.length} total`} />

      <div className="px-4 py-6">
        {/* Column header row */}
        <div className="grid grid-cols-6 gap-4 text-xs uppercase tracking-wider text-slate-300 border-b border-slate-700 py-2 px-2 bg-slate-900">
          {headerCols.map((col) => (
            <div key={col} className="px-2 py-1">
              {col}
            </div>
          ))}
        </div>

        {/* Loading state */}
        {isLoading && (
          <div className="mt-2 space-y-2">
            <LoadingSkeleton />
            <LoadingSkeleton />
            <LoadingSkeleton />
          </div>
        )}

        {/* Error state */}
        {isError && (
          <div className="mt-4 flex items-center text-sm text-amber-300">
            <AlertCircle className="mr-2" />
            Failed to load agents: {error?.message ?? 'Unknown error'}
          </div>
        )}

        {/* Empty state */}
        {!isLoading && agents.length === 0 && (
          <div className="flex items-center justify-center py-12 text-slate-400">
            <AlertCircle className="w-4 h-4 mr-2" />
            No agents found.
          </div>
        )}

        {/* Data rows */}
        {!isLoading && agents.length > 0 && (
          <div className="mt-2 border-t border-slate-700">
            {agents.map((agent) => (
              <Link
                to={`/agents/${agent.id}`}
                key={agent.id}
                className="grid grid-cols-6 items-center gap-4 py-3 px-2 hover:bg-slate-800 border-b border-slate-700"
              >
                <div className="truncate pr-2">{agent.name}</div>
                <div className="truncate pr-2">
                  <a href={agent.url} target="_blank" rel="noreferrer" className="text-slate-200 hover:underline">
                    {agent.url}
                  </a>
                </div>
                <div className="pr-2">
                  <StatusBadge status={agent.status as any} />
                </div>
                <div className="pr-2">{agent.schedulerCount ?? 0}</div>
                <div className="pr-2 text-slate-200">{formatDate(agent.lastHeartbeat)}</div>
                <div className="pr-2 text-slate-200">{formatDate(agent.startedAt)}</div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default AgentsPage;
