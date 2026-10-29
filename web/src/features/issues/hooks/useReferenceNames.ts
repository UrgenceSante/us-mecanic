import { useProblemReferences } from "../../problem-references/api/problemReferenceQueries";
import type { ProblemReferenceKind } from "../../problem-references/types/problemReference";

/** Résolution id -> nom incluant les valeurs désactivées, pour afficher ce qui est déjà utilisé. */
export function useReferenceNames() {
  const nature = useProblemReferences("nature", true);
  const element = useProblemReferences("element", true);
  const actor = useProblemReferences("actor", true);
  const actionType = useProblemReferences("actionType", true);
  const constraint = useProblemReferences("constraint", true);
  const lists = { nature, element, actor, actionType, constraint };

  return (kind: ProblemReferenceKind, id: string): string => {
    const reference = lists[kind].data?.find((r) => r.id === id);
    if (!reference) return "-";
    return reference.isActive ? reference.name : `${reference.name} (désactivé)`;
  };
}
