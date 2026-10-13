import { useEffect } from 'react';
import { cn } from '@/lib/utils';

export interface ToastProps {
  id: string;
  title?: string;
  description?: string;
  variant?: 'default' | 'destructive';
  onClose: (id: string) => void;
}

export function Toast({ id, title, description, variant = 'default', onClose }: ToastProps) {
  useEffect(() => {
    const timer = setTimeout(() => onClose(id), 5000);
    return () => clearTimeout(timer);
  }, [id, onClose]);

  return (
    <div
      className={cn(
        'fixed bottom-4 right-4 z-50 w-full max-w-sm rounded-lg border p-4 shadow-lg transition-all',
        {
          default: 'bg-card border-border',
          destructive: 'bg-destructive text-destructive-foreground border-destructive'
        }[variant]
      )}
    >
      {title && <div className="font-medium">{title}</div>}
      {description && <div className="text-sm text-muted-foreground">{description}</div>}
    </div>
  );
}
