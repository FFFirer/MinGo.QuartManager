import { BrowserRouter, Routes, Route, Link, useLocation, useNavigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Activity, LayoutDashboard, Layers, Server, Settings, ChevronDown, ChevronRight, Keyboard } from 'lucide-react';
import { useState, useEffect, createContext, useContext, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import AgentsPage from './pages/AgentsPage';
import AgentDetailPage from './pages/AgentDetailPage';
import SchedulersPage from './pages/SchedulersPage';
import SchedulerDetailPage from './pages/SchedulerDetailPage';
import JobsPage from './pages/JobsPage';
import JobDetailPage from './pages/JobDetailPage';
import PlatformDashboardPage from './pages/PlatformDashboardPage';
import CalendarPage from './pages/CalendarPage';
import ToastProvider from './components/ToastProvider';
import { agentApi } from './api';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

const MAX_RECENT_AGENTS = 5;

interface RecentAgent {
  id: string;
  name: string;
  status: string;
}

interface SidebarContextType {
  recentAgents: RecentAgent[];
  addRecentAgent: (agent: RecentAgent) => void;
}

const SidebarContext = createContext<SidebarContextType | null>(null);

export function useSidebar() {
  const context = useContext(SidebarContext);
  if (!context) {
    throw new Error('useSidebar must be used within SidebarProvider');
  }
  return context;
}

function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const [agentsOpen, setAgentsOpen] = useState(false);
  const [recentAgents, setRecentAgents] = useState<RecentAgent[]>([]);
  const dropdownRef = useRef<HTMLLIElement>(null);

  const { data: allAgents } = useQuery({
    queryKey: ['sidebar-agents'],
    queryFn: async () => {
      const response = await agentApi.getAll();
      return response.data || [];
    },
  });

  useEffect(() => {
    const saved = localStorage.getItem('sidebar-recent-agents');
    if (saved) {
      try {
        setRecentAgents(JSON.parse(saved));
      } catch {}
    }
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
          addRecentAgent({
            id: agent.id,
            name: agent.name,
            status: agent.status,
          });
        }
      }
    }
  }, [location.pathname, allAgents]);

  useEffect(() => {
    if (location.pathname === '/agents') {
      setAgentsOpen(false);
    }
  }, [location.pathname]);

  const addRecentAgent = (agent: RecentAgent) => {
    setRecentAgents(prev => {
      const filtered = prev.filter(a => a.id !== agent.id);
      const updated = [agent, ...filtered].slice(0, MAX_RECENT_AGENTS);
      return updated;
    });
  };

  const handleAgentSelect = (agent: RecentAgent) => {
    addRecentAgent(agent);
    setAgentsOpen(false);
    navigate(`/agents/${agent.id}`);
  };

  const isActive = (path: string) => location.pathname === path;
  const isActiveStart = (path: string) => location.pathname.startsWith(path);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && e.key === 'd') {
        e.preventDefault();
        navigate('/');
      }
      else if (e.altKey && e.key === 'a') {
        e.preventDefault();
        navigate('/agents');
      }
      else if (e.altKey && e.key === 's') {
        e.preventDefault();
        navigate('/schedulers');
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [navigate]);

  const contextValue: SidebarContextType = {
    recentAgents,
    addRecentAgent,
  };

  return (
    <SidebarContext.Provider value={contextValue}>
      <aside className="w-64 bg-slate-950 border-r border-slate-800 flex flex-col">
        <div className="p-4 border-b border-slate-800">
          <Link to="/" className="flex items-center gap-2 text-slate-50">
            <Activity size={24} className="text-blue-500" />
            <span className="font-bold text-lg">MinGo.Qap</span>
          </Link>
          <div className="flex items-center gap-1 mt-2 text-xs text-slate-500">
            <Keyboard size={12} />
            <span>Alt+D: Dashboard | Alt+A: Agents | Alt+S: Schedulers</span>
          </div>
        </div>
        
        <nav className="flex-1 p-4 overflow-y-auto">
          <ul className="space-y-1">
            <li>
              <Link 
                to="/" 
                className={`flex items-center gap-2 px-3 py-2 rounded-md transition-colors ${
                  isActive('/') ? 'bg-slate-800 text-slate-50' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
                }`}
              >
                <LayoutDashboard size={18} />
                <span>Dashboard</span>
              </Link>
            </li>

            <li ref={dropdownRef} className="relative">
              <button 
                onClick={() => setAgentsOpen(!agentsOpen)}
                className={`w-full flex items-center justify-between px-3 py-2 rounded-md transition-colors ${
                  isActiveStart('/agents') ? 'bg-slate-800 text-slate-50' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
                }`}
              >
                <div className="flex items-center gap-2">
                  <Server size={18} />
                  <span>Agents</span>
                </div>
                {agentsOpen ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
              </button>
              
              {agentsOpen && (
                <div className="absolute left-0 top-full mt-1 w-56 bg-slate-800 border border-slate-700 rounded-lg shadow-xl z-50">
                  <div className="py-1">
                    {recentAgents.length > 0 ? (
                      <>
                        {recentAgents.map(agent => (
                          <button
                            key={agent.id}
                            onClick={() => handleAgentSelect(agent)}
                            className="w-full flex items-center gap-2 px-3 py-2 text-sm text-slate-300 hover:bg-slate-700"
                          >
                            <span className={`w-2 h-2 rounded-full ${
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
                      className="flex items-center gap-2 px-3 py-2 text-sm text-slate-400 hover:bg-slate-700 hover:text-slate-50"
                    >
                      View All Agents
                    </Link>
                  </div>
                </div>
              )}
            </li>

            <li>
              <Link 
                to="/schedulers" 
                className={`flex items-center gap-2 px-3 py-2 rounded-md transition-colors ${
                  isActiveStart('/schedulers') ? 'bg-slate-800 text-slate-50' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
                }`}
              >
                <Layers size={18} />
                <span>Schedulers</span>
              </Link>
            </li>

            <li>
              <Link 
                to="/settings" 
                className="flex items-center gap-2 px-3 py-2 rounded-md text-slate-400 hover:bg-slate-800 hover:text-slate-50 transition-colors"
              >
                <Settings size={18} />
                <span>Settings</span>
              </Link>
            </li>
          </ul>
        </nav>
        
        <div className="p-4 border-t border-slate-800 text-xs text-slate-500">
          v2.0.0
        </div>
      </aside>
    </SidebarContext.Provider>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider />
      <BrowserRouter>
        <div className="min-h-screen bg-slate-900 flex">
          <Sidebar />
          <main className="flex-1 overflow-auto">
            <Routes>
              <Route path="/" element={<PlatformDashboardPage />} />
              <Route path="/agents" element={<AgentsPage />} />
              <Route path="/agents/:agentId" element={<AgentDetailPage />} />
              <Route path="/schedulers" element={<SchedulersPage />} />
              <Route path="/schedulers/:schedulerName" element={<SchedulerDetailPage />} />
              <Route path="/schedulers/:schedulerName/jobs" element={<JobsPage />} />
              <Route path="/schedulers/:schedulerName/jobs/:jobKey" element={<JobDetailPage />} />
              <Route path="/schedulers/:schedulerName/calendar" element={<CalendarPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
