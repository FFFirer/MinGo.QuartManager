import { BrowserRouter, Routes, Route, Link, useLocation, useNavigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Activity, LayoutDashboard, Layers, Settings, ChevronDown, ChevronRight, Keyboard, Plus } from 'lucide-react';
import { useState, useEffect, createContext, useContext, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import ClustersPage from './pages/ClustersPage';
import JobsPage from './pages/JobsPage';
import JobDetailPage from './pages/JobDetailPage';
import AgentInstancesPage from './pages/AgentInstancesPage';
import PlatformDashboardPage from './pages/PlatformDashboardPage';
import ClusterDashboardPage from './pages/ClusterDashboardPage';
import CalendarPage from './pages/CalendarPage';
import ToastProvider from './components/ToastProvider';
import { useClusters } from './hooks/useClusters';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

interface RecentCluster {
  id: string;
  name: string;
  status: string;
  env: string;
}

interface SidebarContextType {
  recentClusters: RecentCluster[];
  addRecentCluster: (cluster: RecentCluster) => void;
}

const SidebarContext = createContext<SidebarContextType | null>(null);

export function useSidebar() {
  const context = useContext(SidebarContext);
  if (!context) {
    throw new Error('useSidebar must be used within SidebarProvider');
  }
  return context;
}

const MAX_RECENT_CLUSTERS = 5;

function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const [clustersOpen, setClustersOpen] = useState(false);
  const [recentClusters, setRecentClusters] = useState<RecentCluster[]>([]);
  const dropdownRef = useRef<HTMLLIElement>(null);
  
  const { data: allClusters } = useClusters();

  useEffect(() => {
    const saved = localStorage.getItem('sidebar-recent-clusters');
    if (saved) {
      try {
        setRecentClusters(JSON.parse(saved));
      } catch {}
    }
  }, []);

  useEffect(() => {
    localStorage.setItem('sidebar-recent-clusters', JSON.stringify(recentClusters));
  }, [recentClusters]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setClustersOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  useEffect(() => {
    if (location.pathname.startsWith('/clusters/') && location.pathname !== '/clusters') {
      const match = location.pathname.match(/^\/clusters\/([^/]+)/);
      if (match) {
        const clusterId = match[1];
        const cluster = allClusters?.find(c => c.id === clusterId);
        if (cluster) {
          addRecentCluster({
            id: cluster.id,
            name: cluster.name,
            status: cluster.status,
            env: cluster.env,
          });
        }
      }
    }
  }, [location.pathname, allClusters]);

  useEffect(() => {
    if (location.pathname === '/clusters') {
      setClustersOpen(false);
    }
  }, [location.pathname]);

  const addRecentCluster = (cluster: RecentCluster) => {
    setRecentClusters(prev => {
      const filtered = prev.filter(c => c.id !== cluster.id);
      const updated = [cluster, ...filtered].slice(0, MAX_RECENT_CLUSTERS);
      return updated;
    });
  };

  const handleClusterSelect = (cluster: RecentCluster) => {
    addRecentCluster(cluster);
    setClustersOpen(false);
    navigate(`/clusters/${cluster.id}`);
  };

  const isActive = (path: string) => location.pathname === path;
  const isActiveStart = (path: string) => location.pathname.startsWith(path);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && e.key === 'd') {
        e.preventDefault();
        navigate('/');
      }
      else if (e.altKey && e.key === 'c') {
        e.preventDefault();
        navigate('/clusters');
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [navigate]);

  const contextValue: SidebarContextType = {
    recentClusters,
    addRecentCluster,
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
            <span>Alt+D: Dashboard | Alt+C: Clusters</span>
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
                onClick={() => setClustersOpen(!clustersOpen)}
                className={`w-full flex items-center justify-between px-3 py-2 rounded-md transition-colors ${
                  isActiveStart('/clusters') ? 'bg-slate-800 text-slate-50' : 'text-slate-400 hover:bg-slate-800 hover:text-slate-50'
                }`}
              >
                <div className="flex items-center gap-2">
                  <Layers size={18} />
                  <span>Clusters</span>
                </div>
                {clustersOpen ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
              </button>
              
              {clustersOpen && (
                <div className="absolute left-0 top-full mt-1 w-56 bg-slate-800 border border-slate-700 rounded-lg shadow-xl z-50">
                  <div className="py-1">
                    {recentClusters.length > 0 ? (
                      <>
                        {recentClusters.map(cluster => (
                          <button
                            key={cluster.id}
                            onClick={() => handleClusterSelect(cluster)}
                            className="w-full flex items-center gap-2 px-3 py-2 text-sm text-slate-300 hover:bg-slate-700"
                          >
                            <span className={`w-2 h-2 rounded-full ${
                              cluster.status === 'Online' ? 'bg-green-500' :
                              cluster.status === 'Warning' ? 'bg-amber-500' : 'bg-red-500'
                            }`} />
                            <span className="truncate">{cluster.name}</span>
                          </button>
                        ))}
                        <div className="border-t border-slate-700 my-1" />
                      </>
                    ) : null}
                    
                    <Link
                      to="/clusters"
                      className="flex items-center gap-2 px-3 py-2 text-sm text-slate-400 hover:bg-slate-700 hover:text-slate-50"
                    >
                      View All Clusters
                    </Link>
                    <Link
                      to="/clusters"
                      className="flex items-center gap-2 px-3 py-2 text-sm text-slate-400 hover:bg-slate-700 hover:text-slate-50"
                    >
                      <Plus size={14} />
                      Add New Cluster
                    </Link>
                  </div>
                </div>
              )}
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
          v1.0.0
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
              <Route path="/clusters" element={<ClustersPage />} />
              <Route path="/clusters/:clusterId" element={<ClusterDashboardPage />} />
              <Route path="/clusters/:clusterId/jobs" element={<JobsPage />} />
              <Route path="/clusters/:clusterId/jobs/:jobKey" element={<JobDetailPage />} />
              <Route path="/clusters/:clusterId/agents" element={<AgentInstancesPage />} />
              <Route path="/clusters/:clusterId/calendar" element={<CalendarPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;