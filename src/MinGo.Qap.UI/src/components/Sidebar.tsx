/* eslint-disable react-hooks/set-state-in-effect */
import { useState, useEffect, useRef, useCallback } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Activity, LayoutDashboard, Layers, Server, Settings,
  ChevronDown, ChevronRight, Keyboard, PanelLeftClose, PanelLeft,
  Calendar
} from 'lucide-react';
import { agentApi } from '../api';
import { useLayout } from './LayoutContext';

const MAX_RECENT_AGENTS = 5;

interface RecentAgent {
  id: string;
  name: string;
  status: string;
}

function NavItem({ to, icon, label, active, collapsed, isActive }: {
  to: string; icon: React.ReactNode; label: string; active?: boolean; collapsed: boolean;
  isActive: (path: string) => boolean;
}) {
  return (
    <Link
      to={to}
      className={`flex items-center gap-3 px-3 py-2 rounded-md transition-colors group ${
        active ?? isActive(to) ? 'bg-slate-800 text-slate-50' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
      }`}
      title={collapsed ? label : undefined}
    >
      <span className="shrink-0">{icon}</span>
      {!collapsed && <span className="truncate">{label}</span>}
      {collapsed && (
        <span className="absolute left-full ml-2 px-2 py-1 bg-slate-800 text-xs text-slate-50 rounded
                     opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all
                     whitespace-nowrap z-50 shadow-lg">
          {label}
        </span>
      )}
    </Link>
  );
}

export default function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const { collapsed, toggleCollapsed } = useLayout();
  const [agentsOpen, setAgentsOpen] = useState(false);
  const [recentAgents, setRecentAgents] = useState<RecentAgent[]>(() => {
    const saved = localStorage.getItem('sidebar-recent-agents');
    if (saved) {
      try { return JSON.parse(saved); } catch { /* ignore */ }
    }
    return [];
  });
  const dropdownRef = useRef<HTMLLIElement>(null);

  const { data: allAgents } = useQuery({
    queryKey: ['sidebar-agents'],
    queryFn: async () => {
      const response = await agentApi.getAll();
      return response.data || [];
    },
  });

  const addRecentAgent = useCallback((agent: RecentAgent) => {
    setRecentAgents(prev => {
      const filtered = prev.filter(a => a.id !== agent.id);
      return [agent, ...filtered].slice(0, MAX_RECENT_AGENTS);
    });
  }, []);

  useEffect(() => {
    localStorage.setItem('sidebar-recent-agents', JSON.stringify(recentAgents));
  }, [recentAgents]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setAgentsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (location.pathname.startsWith('/agents/') && location.pathname !== '/agents') {
      const match = location.pathname.match(/^\/agents\/([^/]+)/);
      if (match) {
        const agentId = match[1];
        const agent = allAgents?.find(a => a.id === agentId);
        if (agent) {
          addRecentAgent({ id: agent.id, name: agent.name, status: agent.status });
        }
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname]);

  useEffect(() => {
    if (location.pathname === '/agents') setAgentsOpen(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname]);

  const handleAgentSelect = (agent: RecentAgent) => {
    addRecentAgent(agent);
    setAgentsOpen(false);
    navigate(`/agents/${agent.id}`);
  };

  const checkActive = (path: string) => location.pathname === path;
  const checkActiveStart = (path: string) => location.pathname.startsWith(path);

  return (
    <aside
      className={`bg-slate-950 border-r border-slate-800 flex flex-col transition-all duration-200 ${
        collapsed ? 'w-16' : 'w-64'
      }`}
    >
      {/* Brand */}
      <div className="p-4 border-b border-slate-800 flex items-center gap-2">
        {!collapsed ? (
          <>
            <Activity size={24} className="text-blue-500 shrink-0" />
            <span className="font-bold text-lg text-slate-50 truncate">MinGo.Qap</span>
          </>
        ) : (
          <Activity size={24} className="text-blue-500 mx-auto" />
        )}
      </div>

      {/* Keyboard hints — only show when expanded */}
      {!collapsed && (
        <div className="flex items-center gap-1 px-4 py-2 text-xs text-slate-500">
          <Keyboard size={12} />
          <span>Alt+D/A/S</span>
        </div>
      )}

      {/* Navigation */}
      <nav className="flex-1 p-3 overflow-y-auto">
        <ul className="space-y-1">
          <li>
            <NavItem to="/" icon={<LayoutDashboard size={18} />} label="Dashboard" collapsed={collapsed} isActive={checkActive} />
          </li>

          {/* Agents with dropdown */}
          <li ref={dropdownRef} className="relative">
            <button
              onClick={() => !collapsed && setAgentsOpen(!agentsOpen)}
              className={`w-full flex items-center gap-3 px-3 py-2 rounded-md transition-colors group ${
                checkActiveStart('/agents') ? 'bg-slate-800 text-slate-50' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
              }`}
              title={collapsed ? 'Agents' : undefined}
            >
              <Server size={18} className="shrink-0" />
              {!collapsed && (
                <>
                  <span className="flex-1 text-left truncate">Agents</span>
                  {agentsOpen ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                </>
              )}
              {collapsed && (
                <span className="absolute left-full ml-2 px-2 py-1 bg-slate-800 text-xs text-slate-50 rounded
                             opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all
                             whitespace-nowrap z-50 shadow-lg">
                  Agents
                </span>
              )}
            </button>

            {/* Dropdown — only show when expanded */}
            {!collapsed && agentsOpen && (
              <div className="mt-1 w-56 bg-slate-800 border border-slate-700 rounded-lg shadow-xl z-50">
                <div className="py-1">
                  {recentAgents.length > 0 ? (
                    <>
                      {recentAgents.map(agent => (
                        <button
                          key={agent.id}
                          onClick={() => handleAgentSelect(agent)}
                          className="w-full flex items-center gap-2 px-3 py-2 text-sm text-slate-300 hover:bg-slate-700"
                        >
                          <span className={`w-2 h-2 rounded-full shrink-0 ${
                            agent.status === 'Online' ? 'bg-green-500' :
                            agent.status === 'Warning' ? 'bg-amber-500' : 'bg-red-500'
                          }`} />
                          <span className="truncate">{agent.name}</span>
                        </button>
                      ))}
                      <div className="border-t border-slate-700 my-1" />
                    </>
                  ) : null}
                  <Link
                    to="/agents"
                    onClick={() => setAgentsOpen(false)}
                    className="flex items-center gap-2 px-3 py-2 text-sm text-slate-400 hover:bg-slate-700 hover:text-slate-50"
                  >
                    View All Agents
                  </Link>
                </div>
              </div>
            )}
          </li>

          <li>
            <NavItem to="/schedulers" icon={<Layers size={18} />} label="Schedulers" collapsed={collapsed} isActive={checkActive} />
          </li>
          <li>
            <NavItem to="/schedulers" icon={<Calendar size={18} />} label="Calendar" 
                     collapsed={collapsed} isActive={checkActive}
                     active={checkActiveStart('/schedulers/') && location.pathname.includes('calendar')} />
          </li>
          <li>
            <NavItem to="/settings" icon={<Settings size={18} />} label="Settings" collapsed={collapsed} isActive={checkActive} />
          </li>
        </ul>
      </nav>

      {/* Collapse toggle + version */}
      <div className="p-3 border-t border-slate-800">
        <button
          onClick={toggleCollapsed}
          className="w-full flex items-center justify-center gap-2 px-3 py-2 text-slate-400 hover:text-slate-50 hover:bg-slate-800 rounded-md transition-colors"
          title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {collapsed ? <PanelLeft size={18} /> : <PanelLeftClose size={18} />}
          {!collapsed && <span className="text-xs">Collapse</span>}
        </button>
        {!collapsed && (
          <div className="text-center text-xs text-slate-500 mt-2">v2.0.0</div>
        )}
      </div>
    </aside>
  );
}
