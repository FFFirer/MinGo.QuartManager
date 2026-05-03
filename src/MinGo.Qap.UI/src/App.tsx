import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import AgentsPage from './pages/AgentsPage';
import AgentDetailPage from './pages/AgentDetailPage';
import SchedulersPage from './pages/SchedulersPage';
import SchedulerDetailPage from './pages/SchedulerDetailPage';
import JobsPage from './pages/JobsPage';
import JobDetailPage from './pages/JobDetailPage';
import PlatformDashboardPage from './pages/PlatformDashboardPage';
import CalendarPage from './pages/CalendarPage';
import ToastProvider from './components/ToastProvider';
import Sidebar from './components/Sidebar';
import StatusBar from './components/StatusBar';
import { LayoutProvider } from './components/LayoutContext';
import FloatingActionPalette from './components/FloatingActionPalette';
import GlobalSearch from './components/GlobalSearch';
import { SearchProvider } from './components/SearchContext';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

/** Handles global keyboard shortcuts */
function KeyboardShortcuts() {
  const navigate = useNavigate();

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.altKey && e.key === 'd') { e.preventDefault(); navigate('/'); }
      else if (e.altKey && e.key === 'a') { e.preventDefault(); navigate('/agents'); }
      else if (e.altKey && e.key === 's') { e.preventDefault(); navigate('/schedulers'); }
      else if (e.altKey && e.key === 'e') { e.preventDefault(); navigate('/executions'); }
      else if (e.altKey && e.key === 'c') { e.preventDefault(); navigate('/schedulers'); }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [navigate]);

  return null;
}

function AppLayout() {
  return (
    <div className="min-h-screen bg-slate-900 flex flex-col">
      <div className="flex flex-1 overflow-hidden">
        <Sidebar />
        <main className={`flex-1 overflow-auto transition-all duration-200`}>
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
        <FloatingActionPalette />
      </div>
      <StatusBar />
    </div>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider />
      <BrowserRouter>
        <SearchProvider>
          <LayoutProvider>
            <GlobalSearch />
            <KeyboardShortcuts />
            <AppLayout />
          </LayoutProvider>
        </SearchProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
