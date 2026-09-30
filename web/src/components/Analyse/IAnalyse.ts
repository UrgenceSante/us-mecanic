import type { IAction, IActionForm } from "./IAction";

export interface IAnalyse {
  Immatriculation: string | null;
  Crew: string | null;
  Analyze: string | null;
  AnalyzeBy: string | null;
  Concerning: IDisplayValue;
  LogId: number;
  Nature: IDisplayValue;
  ImmobilizeVehicle: boolean;
  Actions: IAction[];
  Date: string | null;
}

export interface IDisplayValue {
  Id: number;
  Label?: string;
  Value: string | null;
  requiresData?: boolean;
}

export type IAnalyseForm = {
  Analyze: string;
  ConcerningId: string;
  NatureId: string;
  ImmobilizeVehicle: boolean;
  Actions: IActionForm[];
};

export type AnalyseCmd = {
  Immatriculation: string;
  Crew: string;
  Analyze: string;
  AnalyzeBy: string;
  ConcerningId: number;
  LogId: number;
  NatureId: number;
  ImmobilizeVehicle: boolean;
};
