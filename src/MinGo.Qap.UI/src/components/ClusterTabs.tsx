import { Link, useParams, useLocation } from 'react-router-dom';
import { ArrowLeft, LayoutDashboard, Clock, Calendar, Server } from 'lucide-react';
import StatusBadge from './StatusBadge';

interface ClusterTabsProps {
  clusterName: string;
  clusterStatus: string;
  clusterEnv?: string;
}

interface Tab {
  label: string;
  icon: React.ReactNode;
  path: string;
}

export default function ClusterTabs({ clusterName, clusterStatus, clusterEnv }: ClusterTabsProps) {
  const { clusterId } = useParams<{ clusterId: string }>();
  const location = useLocation();
  
  const tabs: Tab[] = [
    { label: 'Dashboard', icon: <LayoutDashboard size={16} />, path: `/clusters/${clusterId}` },
    { label: 'Jobs', icon: <Clock size={16} />, path: `/clusters/${clusterId}/jobs` },
    { label: 'Calendar', icon: <Calendar size={16} />, path: `/clusters/${clusterId}/calendar` },
    { label: 'Agents', icon: <Server size={16} />, path: `/clusters/${clusterId}/agents` },
  ];

  const isActive = (path: string) => {
    if (path === `/clusters/${clusterId}`) {
      return location.pathname === path;
    }
    return location.pathname.startsWith(path);
  };

  return (
    <div className="flex gap-4 mb-6 border-b border-slate-700 pb-4">
      <Link
        to="/clusters"
        className="flex items-center gap-2 text-slate-400 hover:text-slate-50 transition-colors"
      >
        <ArrowLeft size={16} />
      </Link>
      
      <div className="flex items-center gap-3">
        <StatusBadge status={clusterStatus} size="sm" showLabel={false} />
        <span className="font-semibold text-slate-50">{clusterName}</span>
        {clusterEnv && (
          <span className="text-xs text-slate-400 bg-slate-700 px-2 py-1 rounded">
            {clusterEnv}
          </span>
        )}
      </div>

      <div className="flex-1" />

      <div className="flex items-center gap-1">
        {tabs.map(tab => (
          <Link
            key={tab.path}
            to={tab.path}
            className={`flex items-center gap-2 px-3 py-2 transition-colors ${
              isActive(tab.path)
                ? 'text-blue-400 border-b-2 border-blue-400'
                : 'text-slate-400 hover:text-slate-50'
            }`}
          >
            {tab.icon}
            <span>{tab.label}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}