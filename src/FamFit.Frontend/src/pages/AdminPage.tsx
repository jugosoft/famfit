import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Plus } from 'lucide-react';
import { Alert } from '@/components/ui/Alert';
import { Tabs } from '@/components/ui/Tabs';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/contexts/AuthContext';
import {
  getActivityTypes,
  getActivityUnits,
  getActivityAttributeDefinitions,
  getUsers,
} from '@/api/client';
import type { ActivityUnitDto } from '@/api/types';
import { ActivityTypesTab } from './ActivityTypesTab';
import { ActivityUnitsTab } from './ActivityUnitsTab';
import { UsersTab } from './UsersTab';

// Helper to map ActivityTypeDto to ActivityTypeItem
function mapActivityType(dto: any) {
  return {
    id: dto.id,
    name: dto.name,
    activityUnitId: dto.activityUnitId || dto.unitId || '',
    unitSymbol: dto.unitSymbol || null,
    isActive: dto.isActive !== false,
    attributes: (dto.attributes || []).map((attr: any) => ({
      id: attr.id || crypto.randomUUID(),
      definitionName: attr.definitionName,
      displayName: attr.displayName,
      dataType: attr.dataType,
      unit: attr.unit || null,
      value: attr.value
    }))
  };
}

export function AdminPage() {
  const { user } = useAuth();

  const [activeTab, setActiveTab] = useState('activity-types');
  const [pendingCreateTab, setPendingCreateTab] = useState<string | null>(null);

  const { data: activityUnits = [] } = useQuery<ActivityUnitDto[]>({
    queryKey: ['activity-units'],
    queryFn: getActivityUnits,
  });

  const { data: rawActivityTypes, isLoading: isLoadingTypes } = useQuery({
    queryKey: ['activity-types'],
    queryFn: getActivityTypes
  });

  const activityTypes = (rawActivityTypes || []).map(mapActivityType);

  const { data: attributeDefinitions = [] } = useQuery({
    queryKey: ['attribute-definitions'],
    queryFn: getActivityAttributeDefinitions
  });

  const { data: users = [] } = useQuery({
    queryKey: ['users'],
    queryFn: getUsers,
  });

  const tabItems = [
    { value: 'activity-types', label: 'Управление типами активностей' },
    { value: 'activity-units', label: 'Управление единицами измерения' },
    { value: 'users', label: 'Реестр пользователей' },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Админ-панель</h1>
          <p className="text-muted-foreground mt-1">Управление системой</p>
        </div>
        {activeTab === 'activity-types' && (
          <Button onClick={() => setPendingCreateTab('activity-types')}>
            <Plus className="mr-2 h-4 w-4" />
            Добавить тип
          </Button>
        )}
        {activeTab === 'activity-units' && (
          <Button onClick={() => setPendingCreateTab('activity-units')}>
            <Plus className="mr-2 h-4 w-4" />
            Добавить единицу
          </Button>
        )}
      </div>

      {/* Role Check */}
      {user?.role !== 'Admin' && (
        <Alert
          variant="destructive"
          title="Доступ запрещён"
          description="У вас нет прав для доступа к админ-панели."
        />
      )}

      {/* Tabs */}
      <Tabs items={tabItems} value={activeTab} onValueChange={setActiveTab} />

      {/* Tab Content */}
      {activeTab === 'activity-types' && (
        <ActivityTypesTab
          activityTypes={activityTypes}
          activityUnits={activityUnits}
          attributeDefinitions={attributeDefinitions}
          isLoading={isLoadingTypes}
          pendingCreateTab={pendingCreateTab}
          onCreate={() => setPendingCreateTab(null)}
        />
      )}

      {activeTab === 'activity-units' && (
        <ActivityUnitsTab
          activityUnits={activityUnits}
          pendingCreateTab={pendingCreateTab}
          onCreate={() => setPendingCreateTab(null)}
        />
      )}

      {activeTab === 'users' && (
        <UsersTab
          users={users}
        />
      )}
    </div>
  );
}
