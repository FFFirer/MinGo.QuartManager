import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useCluster } from '../hooks/useClusters';
import Calendar from 'react-calendar';
import { format, addMonths, subMonths, startOfMonth, endOfMonth, eachDayOfInterval, isSameDay, isSameMonth } from 'date-fns';
import { ChevronLeft, ChevronRight, List, Clock, Play, Eye, Copy, Calendar as CalendarIcon } from 'lucide-react';
import toast from 'react-hot-toast';
import 'react-calendar/dist/Calendar.css';
import StatusBadge from '../components/StatusBadge';
import ClusterTabs from '../components/ClusterTabs';

interface CalendarJob {
  jobKey: string;
  jobType: string;
  scheduleType: string;
  cronExpression?: string;
  scheduleDescription: string;
  fireTimes: string[];
}

interface CalendarData {
  year: number;
  month: number;
  jobs: CalendarJob[];
}

type ViewMode = 'month' | 'week' | 'list';

async function fetchCalendar(clusterId: string, year: number, month: number): Promise<CalendarData> {
  const response = await fetch(`/api/clusters/${clusterId}/calendar?year=${year}&month=${month}`);
  if (!response.ok) {
    throw new Error('Failed to fetch calendar');
  }
  const result = await response.json();
  return result.data;
}

export function CalendarPage() {
  const { clusterId } = useParams<{ clusterId: string }>();
  const navigate = useNavigate();
  const { data: cluster } = useCluster(clusterId || '');
  const [currentDate, setCurrentDate] = useState(new Date());
  const [viewMode, setViewMode] = useState<ViewMode>('month');
  const [selectedJob, setSelectedJob] = useState<CalendarJob | null>(null);
  const [menuPosition, setMenuPosition] = useState<{ x: number; y: number } | null>(null);

  const year = currentDate.getFullYear();
  const month = currentDate.getMonth() + 1;

  const { data, isLoading, error } = useQuery({
    queryKey: ['cluster-calendar', clusterId, year, month],
    queryFn: () => fetchCalendar(clusterId!, year, month),
  });

  const jobFireTimesMap = useMemo(() => {
    const map = new Map<string, CalendarJob[]>();
    if (!data?.jobs) return map;
    
    data.jobs.forEach(job => {
      job.fireTimes.forEach(fireTime => {
        const dateKey = format(new Date(fireTime), 'yyyy-MM-dd');
        if (!map.has(dateKey)) {
          map.set(dateKey, []);
        }
        map.get(dateKey)!.push(job);
      });
    });
    
    return map;
  }, [data]);

  const handlePrevMonth = () => setCurrentDate(subMonths(currentDate, 1));
  const handleNextMonth = () => setCurrentDate(addMonths(currentDate, 1));
  const handleToday = () => setCurrentDate(new Date());

  const handleJobClick = (job: CalendarJob, event: React.MouseEvent) => {
    event.stopPropagation();
    setSelectedJob(job);
    setMenuPosition({ x: event.clientX, y: event.clientY });
  };

  const handleCloseMenu = () => {
    setSelectedJob(null);
    setMenuPosition(null);
  };

  const handleTrigger = () => {
    if (selectedJob) {
      toast.success(`Triggering job: ${selectedJob.jobKey}`);
      handleCloseMenu();
    }
  };

  const handleCopyKey = () => {
    if (selectedJob) {
      navigator.clipboard.writeText(selectedJob.jobKey);
      toast.success('Job key copied!');
      handleCloseMenu();
    }
  };

  const tileContent = ({ date, view }: { date: Date; view: string }) => {
    if (view !== 'month') return null;
    
    const dateKey = format(date, 'yyyy-MM-dd');
    const jobs = jobFireTimesMap.get(dateKey);
    
    if (!jobs || jobs.length === 0) return null;
    
    return (
      <div className="flex flex-wrap gap-1 justify-center mt-1">
        {jobs.slice(0, 3).map((job, idx) => (
          <div
            key={`${job.jobKey}-${idx}`}
            className="w-2 h-2 rounded-full bg-blue-500 cursor-pointer hover:bg-blue-400"
            onClick={(e) => handleJobClick(job, e)}
            title={job.jobKey}
          />
        ))}
        {jobs.length > 3 && (
          <span className="text-xs text-slate-400">+{jobs.length - 3}</span>
        )}
      </div>
    );
  };

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-500/10 border border-red-500/30 rounded-lg p-6 text-center">
          <h2 className="text-xl font-semibold text-slate-50 mb-2">Failed to load calendar</h2>
          <p className="text-slate-400">{error.message}</p>
        </div>
      </div>
    );
  }

  const daysWithJobs = Array.from(jobFireTimesMap.entries()).filter(([date]) => 
    isSameMonth(new Date(date), currentDate)
  );

  return (
    <div className="p-6" onClick={handleCloseMenu}>
      <ClusterTabs
        clusterName={clusterId || 'Cluster'}
        clusterStatus="Unknown"
      />

      <div className="flex justify-end mb-4">
        <Link
          to={`/clusters/${clusterId}/jobs`}
          className="flex items-center gap-2 px-4 py-2 bg-slate-800 text-slate-300 rounded-lg hover:bg-slate-700 transition-colors"
        >
          <Clock size={16} />
          View Jobs
        </Link>
      </div>

      {/* Calendar Controls */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <button
            onClick={handlePrevMonth}
            className="p-2 rounded-lg bg-slate-800 text-slate-400 hover:text-slate-50 hover:bg-slate-700"
          >
            <ChevronLeft size={20} />
          </button>
          <button
            onClick={handleToday}
            className="px-3 py-2 rounded-lg bg-slate-800 text-slate-400 hover:text-slate-50 hover:bg-slate-700 text-sm"
          >
            Today
          </button>
          <button
            onClick={handleNextMonth}
            className="p-2 rounded-lg bg-slate-800 text-slate-400 hover:text-slate-50 hover:bg-slate-700"
          >
            <ChevronRight size={20} />
          </button>
          <h2 className="text-xl font-semibold text-slate-50 ml-2">
            {format(currentDate, 'MMMM yyyy')}
          </h2>
        </div>
        
        <div className="flex items-center gap-1 bg-slate-800 rounded-lg p-1">
          <button
            onClick={() => setViewMode('month')}
            className={`px-3 py-1.5 rounded-md text-sm transition-colors ${
              viewMode === 'month' ? 'bg-slate-700 text-slate-50' : 'text-slate-400 hover:text-slate-50'
            }`}
          >
            Month
          </button>
          <button
            onClick={() => setViewMode('week')}
            className={`px-3 py-1.5 rounded-md text-sm transition-colors ${
              viewMode === 'week' ? 'bg-slate-700 text-slate-50' : 'text-slate-400 hover:text-slate-50'
            }`}
          >
            Week
          </button>
          <button
            onClick={() => setViewMode('list')}
            className={`px-3 py-1.5 rounded-md text-sm transition-colors ${
              viewMode === 'list' ? 'bg-slate-700 text-slate-50' : 'text-slate-400 hover:text-slate-50'
            }`}
          >
            List
          </button>
        </div>
      </div>

      {/* Calendar Grid */}
      <div className="bg-slate-800 rounded-lg border border-slate-700 p-4">
        {viewMode === 'month' && (
          <Calendar
            onChange={() => {}}
            value={currentDate}
            tileContent={tileContent}
            className="w-full"
            navigationLabel={null}
            prev2Label={null}
            next2Label={null}
            showNeighboringMonth={true}
            tileClassName={({ date, view }) => {
              if (view !== 'month') return '';
              const today = new Date();
              if (isSameDay(date, today)) return 'bg-blue-500/20 rounded-full';
              return '';
            }}
          />
        )}

        {viewMode === 'list' && (
          <div className="space-y-4">
            {daysWithJobs.length === 0 ? (
              <div className="text-center py-12 text-slate-400">
                <CalendarIcon size={48} className="mx-auto mb-4 opacity-50" />
                <p>No scheduled jobs this month</p>
              </div>
            ) : (
              daysWithJobs.map(([date, jobs]) => (
                <div key={date}>
                  <h3 className="text-lg font-semibold text-slate-50 mb-2">
                    {format(new Date(date), 'EEEE, MMMM d, yyyy')}
                  </h3>
                  <div className="space-y-2">
                    {jobs.map((job, idx) => (
                      <div
                        key={`${job.jobKey}-${idx}`}
                        className="flex items-center justify-between p-3 bg-slate-700/50 rounded-lg hover:bg-slate-700 cursor-pointer"
                        onClick={(e) => handleJobClick(job, e)}
                      >
                        <div className="flex items-center gap-3">
                          <Clock size={16} className="text-slate-400" />
                          <div>
                            <p className="text-sm font-medium text-slate-50">{job.jobKey}</p>
                            <p className="text-xs text-slate-400">{job.jobType} • {job.scheduleDescription}</p>
                          </div>
                        </div>
                        <div className="text-sm text-slate-400">
                          {job.fireTimes
                            .filter(ft => ft.startsWith(date))
                            .map(ft => format(new Date(ft), 'HH:mm'))
                            .join(', ')}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))
            )}
          </div>
        )}

        {viewMode === 'week' && (
          <div className="text-center py-12 text-slate-400">
            <p>Week view coming soon...</p>
          </div>
        )}
      </div>

      {/* Job Action Menu */}
      {selectedJob && menuPosition && (
        <div
          className="fixed bg-slate-800 border border-slate-700 rounded-lg shadow-xl py-2 z-50 min-w-[180px]"
          style={{ left: menuPosition.x, top: menuPosition.y }}
          onClick={(e) => e.stopPropagation()}
        >
          <div className="px-4 py-2 border-b border-slate-700">
            <p className="font-medium text-slate-50">{selectedJob.jobKey}</p>
            <p className="text-xs text-slate-400">{selectedJob.jobType}</p>
          </div>
          <button
            onClick={() => navigate(`/clusters/${clusterId}/jobs/${selectedJob.jobKey}`)}
            className="w-full px-4 py-2 text-left text-sm text-slate-300 hover:bg-slate-700 flex items-center gap-2"
          >
            <Eye size={14} />
            View Details
          </button>
          <button
            onClick={handleTrigger}
            className="w-full px-4 py-2 text-left text-sm text-slate-300 hover:bg-slate-700 flex items-center gap-2"
          >
            <Play size={14} />
            Trigger Now
          </button>
          <button
            onClick={handleCopyKey}
            className="w-full px-4 py-2 text-left text-sm text-slate-300 hover:bg-slate-700 flex items-center gap-2"
          >
            <Copy size={14} />
            Copy Job Key
          </button>
        </div>
      )}

      {/* Legend */}
      <div className="mt-4 flex items-center gap-4 text-sm text-slate-400">
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-blue-500" />
          <span>Job scheduled</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-blue-500/20 border-2 border-blue-500" />
          <span>Today</span>
        </div>
      </div>
    </div>
  );
}

export default CalendarPage;