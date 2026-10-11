import { RouterProvider as RouterProviderInner, router } from './router';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProviderInner />
  </React.StrictMode>
);
