export interface ProblemReference {
  id: string;
  name: string;
  isActive: boolean;
}

export type ProblemReferenceKind = "nature" | "element" | "actor" | "actionType" | "constraint";

/** Description d'un référentiel : route API, préfixe des codes d'erreur et libellés. */
export interface ProblemReferenceConfig {
  kind: ProblemReferenceKind;
  route: string;
  errorPrefix: string;
  /** Libellé de l'onglet (pluriel). */
  title: string;
  /** Libellé singulier, pour les titres de dialogue et les messages. */
  singular: string;
  /** Article + nom, pour les messages (« la nature »). */
  definite: string;
  feminine: boolean;
}
