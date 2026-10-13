import { cn } from '@/lib/utils';

export interface ProgressProps {
  value?: number;
  max?: number;
  className?: string;
}

export function Progress({
  className,
  value = 0,
  max = 100,
}: ProgressProps) {
  const percentage = Math.min(100, Math.max(0, (value / max) * 100));

  return (
    <div
      className={cn(
        'relative h-4 w-full overflow-hidden rounded-full bg-secondary',
        className
      )}
    >
      <div
        className="h-full bg-primary transition-all duration-300"
        style={{ width: `${percentage}%` }}
      />
    </div>
  );
}
