import { useEffect, useMemo, useRef, useState } from 'react';

// Public event type for ActivityFeed
export type StreamEvent = {
  id: string;
  type:
    | 'agent_online'
    | 'agent_offline'
    | 'agent_warning'
    | 'job_triggered'
    | 'job_completed'
    | 'job_failed'
    | 'job_paused';
  message: string;
  timestamp: string;
  resourceId?: string;
};

type ConnectionStatus = 'connected' | 'polling' | 'disconnected';

// Custom hook: SSE with polling fallback
export function useEventStream(): {
  events: StreamEvent[];
  isLive: boolean;
  connectionStatus: ConnectionStatus;
} {
  const [events, setEvents] = useState<StreamEvent[]>([]);
  const [isLive, setIsLive] = useState<boolean>(false);
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('disconnected');

  const esRef = useRef<EventSource | null>(null);
  const mounted = useRef(true);
  const pollingTimer = useRef<number | null>(null);
  const reconnectTimer = useRef<number | null>(null);
  const backoffMs = useRef<number>(1000);

  // Clean up on unmount
  useEffect(() => {
    return () => {
      mounted.current = false;
      if (esRef.current) {
        esRef.current.close();
        esRef.current = null;
      }
      if (pollingTimer.current) {
        window.clearInterval(pollingTimer.current);
        pollingTimer.current = null;
      }
      if (reconnectTimer.current) {
        window.clearTimeout(reconnectTimer.current);
        reconnectTimer.current = null;
      }
    };
  }, []);

  // Establish SSE connection
  useEffect(() => {
    let connected = false;

    const connectSSE = () => {
      if (!mounted.current) return;
      try {
        const es = new EventSource('/api/events');
        esRef.current = es;
        connected = true;
        setIsLive(true);
        setConnectionStatus('connected');

        es.onmessage = (e) => {
          try {
            const data = JSON.parse(e.data);
            let newEvents: StreamEvent[] = [];
            if (Array.isArray(data)) {
              newEvents = data as StreamEvent[];
            } else if ((data as any)?.id) {
              newEvents = [data as StreamEvent];
            } else if ((data as any)?.events && Array.isArray((data as any).events)) {
              newEvents = (data as any).events as StreamEvent[];
            }
            if (newEvents.length > 0) {
              setEvents((prev) => {
                const merged = [...prev, ...newEvents];
                // Keep the most recent 50 events
                if (merged.length > 50) {
                  return merged.slice(-50);
                }
                return merged;
              });
            }
          } catch {
            // ignore parsing errors
          }
        };

        es.onerror = () => {
          if (!mounted.current) return;
          // Clean up and fallback to polling with exponential backoff
          es.close();
          esRef.current = null;
          setConnectionStatus('polling');
          setIsLive(false);
          // Schedule reconnect attempt with backoff
          window.clearTimeout(reconnectTimer.current as number);
          const delay = backoffMs.current;
          reconnectTimer.current = window.setTimeout(() => {
            backoffMs.current = Math.min(backoffMs.current * 2, 30000);
            connectSSE();
          }, delay) as unknown as number;
        };
      } catch {
        // SSE not supported or failed to create
        setConnectionStatus('polling');
        setIsLive(false);
        startPollingFallback();
      }
    };

    const startPollingFallback = () => {
      if (pollingTimer.current) return;
      // Poll every 15 seconds with exponential backoff on failures
      let failureCount = 0;
      const tick = async () => {
        try {
          const res = await fetch('/api/events');
          if (res.ok) {
            const json = await res.json();
            let newEvents: StreamEvent[] = [];
            if (Array.isArray(json)) {
              newEvents = json as StreamEvent[];
            } else if (json?.events && Array.isArray(json.events)) {
              newEvents = json.events as StreamEvent[];
            }
            if (newEvents.length > 0) {
              setEvents((prev) => {
                const merged = [...prev, ...newEvents];
                if (merged.length > 50) return merged.slice(-50);
                return merged;
              });
            }
            failureCount = 0;
            setConnectionStatus('polling');
          } else {
            throw new Error('Polling response not ok');
          }
        } catch {
          failureCount++;
          // increase backoff on failures
          const backoff = Math.min(60000, 15000 * Math.pow(2, Math.min(failureCount, 5)));
          // reset interval with new backoff
          if (pollingTimer.current) {
            window.clearInterval(pollingTimer.current);
          }
          pollingTimer.current = window.setInterval(tick, backoff) as unknown as number;
          setConnectionStatus('polling');
        }
      };
      // initial register
      pollingTimer.current = window.setInterval(tick, 15000) as unknown as number;
      setIsLive(false);
      setConnectionStatus('polling');
    };

    connectSSE();

    // Cleanup when dependencies change
    return () => {
      // no-op here; outer cleanup handles unmount
    };
  }, []);

  return useMemo(() => ({ events, isLive, connectionStatus }), [events, isLive, connectionStatus]);
}
