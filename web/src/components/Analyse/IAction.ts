import type { IDisplayValue } from "./IAnalyse";

export interface IAction {
  Id: number;
  Actor: IDisplayValue;
  ActionType: IDisplayValue;
  Constraint: IDisplayValue;
  Rappel: string | null;
  DueDate: string | null;
  Creation: string | null;
  comment: string | null;
  Closed: boolean;
  CallBack: string | null;
  AnalyzeId: number;
}

export interface IActionForm {
  ActionTypeId: string;
  ActorId: string;
  comment: string;
  ConstraintId: string;
  Id: string;
  DueDate: string;
  RequiresDate?: boolean;
}

