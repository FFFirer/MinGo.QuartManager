import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Activity } from 'lucide-react';
import ClustersPage from './pages/ClustersPage';
import JobsPage from './pages/JobsPage';
import JobDetailPage from './pages/JobDetailPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="min-h-screen bg-slate-900 flex">
          {/* Sidebar */}
          <aside className="w-64 bg-slate-950 border-r border-slate-800 flex flex-col">
            <div className="p-4 border-b border-slate-800">
              <Link to="/" className="flex items-center gap-2 text-slate-50">
                <Activity size={24} className="text-blue-500" />
                <span className="font-bold text-lg">MinGo.Qap</span>
              </Link>
            </div>
            
            <nav className="flex-1 p-4">
              <ul className="space-y-2">
                <li>
                  <Link 
                    to="/" 
                    className="flex items-center gap-2 px-3 py-2 rounded-md text-slate-400 hover:bg-slate-800 hover:text-slate-50 transition-colors"
                  >
                    Clusters
                  </Link>
                </li>
              </ul>
            </nav>
            
            <div className="p-4 border-t border-slate-800 text-xs text-slate-500">
              v1.0.0
            </div>
          </aside>

          {/* Main Content */}
          <main className="flex-1 overflow-auto">
            <Routes>
              <Route path="/" element={<ClustersPage />} />
              <Route path="/clusters/:clusterId/jobs" element={<JobsPage />} />
              <Route path="/clusters/:clusterId/jobs/:jobKey" element={<JobDetailPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
