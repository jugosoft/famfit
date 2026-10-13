import { cn } from '@/lib/utils';

export interface AvatarProps {
  src?: string;
  fallback?: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export function Avatar({
  className,
  src,
  fallback = '?',
  size = 'md',
}: AvatarProps) {
  const sizeClasses = {
    sm: 'h-8 w-8 text-xs',
    md: 'h-10 w-10 text-sm',
    lg: 'h-14 w-14 text-lg'
  };

  if (src) {
    return (
      <div
        className={cn(
          'relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full',
          sizeClasses[size],
          className
        )}
      >
        <img src={src} alt="" className="aspect-square h-full w-full object-cover" />
      </div>
    );
  }

  return (
    <div
      className={cn(
        'flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary font-semibold',
        sizeClasses[size],
        className
      )}
    >
      {fallback}
    </div>
  );
}
