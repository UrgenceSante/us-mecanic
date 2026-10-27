import { apiClient } from "../../../api/client";
import type { DeadlineThresholds, DeadlineType, DeadlineTypeInput } from "../types/deadlineType";

const TYPES = "api/deadline-types";
const THRESHOLDS = "api/deadline-thresholds";

export async function listDeadlineTypes(includeInactive: boolean): Promise<DeadlineType[]> {
  const { data } = await apiClient.get<DeadlineType[]>(TYPES, { params: { includeInactive } });
  return data;
}

export async function createDeadlineType(input: DeadlineTypeInput): Promise<DeadlineType> {
  const { data } = await apiClient.post<DeadlineType>(TYPES, input);
  return data;
}

export async function updateDeadlineType(id: string, input: DeadlineTypeInput): Promise<DeadlineType> {
  const { data } = await apiClient.put<DeadlineType>(`${TYPES}/${id}`, input);
  return data;
}

export async function deactivateDeadlineType(id: string): Promise<DeadlineType> {
  const { data } = await apiClient.post<DeadlineType>(`${TYPES}/${id}/deactivate`);
  return data;
}

export async function getDeadlineThresholds(): Promise<DeadlineThresholds> {
  const { data } = await apiClient.get<DeadlineThresholds>(THRESHOLDS);
  return data;
}

export async function updateDeadlineThresholds(thresholds: DeadlineThresholds): Promise<DeadlineThresholds> {
  const { data } = await apiClient.put<DeadlineThresholds>(THRESHOLDS, thresholds);
  return data;
}
