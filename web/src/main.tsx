import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router';
import { ApiError } from './api/http';
import { App } from './App';
import { AuthProvider } from './auth/AuthContext';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // A client error will not change by asking again; only an unreachable or failing server might.
      retry: (failures, error) => failures < 1 && (!(error instanceof ApiError) || error.status === 0 || error.status >= 500),
      refetchOnWindowFocus: false,
    },
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      {/* Without transitions, so a checkbox held in the address does not spring back until the address catches up. */}
      <BrowserRouter useTransitions={false}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
);
