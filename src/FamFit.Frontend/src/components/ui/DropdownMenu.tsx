import { DropdownMenuHTMLAttributes, forwardRef, useState } from 'react';
import { MoreVertical, Check, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';

interface DropdownMenuItem {
  label: string;
  icon?: React.ReactNode;
  onClick: () => void;
  destructive?: boolean;
  separator?: boolean;
}

interface DropdownMenuProps {
  items: DropdownMenuItem[];
  trigger?: React.ReactNode;
}

export function DropdownMenu({ items, trigger }: DropdownMenuMenu) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative inline-block">
      <button
        className={cn(
          'rounded p-1 hover:bg-accent transition-colors',
          !trigger && 'h-8 w-8'
        )}
        onClick={() => setIsOpen(!isOpen)}
      >
        {trigger ?? <MoreVertical className="h-4 w-4" />}
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-10" onClick={() => setIsOpen(false)} />
          <div className="absolute right-0 z-20 mt-1 w-56 rounded-md border bg-popover shadow-lg">
            <div className="p-1">
              {items.map((item, index) => (
                <div key={index}>
                  {item.separator && (
                    <div className="my-1 h-px bg-border" />
                  )}
                  <button
                    className={cn(
                      'flex w-full items-center gap-2 rounded px-2 py-1.5 text-sm transition-colors',
                      item.destructive
                        ? 'text-destructive hover:bg-destructive/10'
                        : 'hover:bg-accent'
                    )}
                    onClick={() => {
                      item.onClick();
                      setIsOpen(false);
                    }}
                  >
                    {item.icon}
                    <span className="flex-1 text-left">{item.label}</span>
                    <ChevronRight className="h-4 w-4 text-muted-foreground" />
                  </button>
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
