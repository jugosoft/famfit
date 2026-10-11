import { useState, useCallback } from 'react';
import { Toast } from './Toast';

interface ToastItem {
  id: string;
  title?: string;
  description?: string;
  variant?: 'default' | 'destructive';
}

let toasts: ToastItem[] = [];

export const toast = {
  success: (title: string, description?: string) => {
    const id = crypto.randomUUID();
    toasts = [...toasts, { id, title, description, variant: 'default' }];
    window.dispatchEvent(new CustomEvent('toast:add', { detail: { id, title, description, variant: 'default' as const } }));
  },
  error: (title: string, description?: string) => {
    const id = crypto.randomUUID();
    toasts = [...toasts, { id, title, description, variant: 'destructive' as const }];
    window.dispatchEvent(new CustomEvent('toast:add', { detail: { id, title, description, variant: 'destructive' as const } }));
  },
  remove: (id: string) => {
    toasts = toasts.filter((t) => t.id !== id);
    window.dispatchEvent(new CustomEvent('toast:remove', { detail: { id } }));
  }
};

export function Toaster() {
  const [toastsState, setToasts] = useState<ToastItem[]>([]);

  const handleAdd = useCallback((e: Event) => {
    const { detail } = e as CustomEvent<{ id: string; title?: string; description?: string; variant?: 'default' | 'destructive' }>;
    setToasts((prev) => [...prev, detail]);
  }, []);

  const handleRemove = useCallback((e: Event) => {
    const { id } = e as CustomEvent<{ id: string }>;
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  useEffect(() => {
    window.addEventListener('toast:add', handleAdd);
    window.addEventListener('toast:remove', handleRemove);
    return () => {
      window.removeEventListener('toast:add', handleAdd);
      window.removeEventListener('toast:remove', handleRemove);
    };
  }, [handleAdd, handleRemove]);

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      {toastsState.map((t) => (
        <Toast
          key={t.id}
          id={t.id}
          title={t.title}
          description={t.description}
          variant={t.variant}
          onClose={(id) => toast.remove(id)}
        />
      ))}
    </div>
  );
}
